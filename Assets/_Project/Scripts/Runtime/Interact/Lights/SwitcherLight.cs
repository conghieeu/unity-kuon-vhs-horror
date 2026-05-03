using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using UnityEngine;
using UHFPS.Tools;
using static UHFPS.Runtime.InteractableLight;

namespace UHFPS.Runtime
{
    public class SwitcherLight : MonoBehaviour, IPowerConsumer, IInteractStart, ISaveable
    {
        [field: SerializeField]
        public float ConsumeWattage { get; set; }

        public bool IsSwitchedOn;
        public bool UseEnergy;

        public List<Light> LightComponents = new();
        public List<MeshRenderer> MeshRenderers = new();

        public bool SmoothLight;
        public float SmoothDuration;

        public bool EnableEmission = true;
        public string EmissionKeyword = "_EMISSION";

        public SoundClip LightSwitchOn;
        public SoundClip LightSwitchOff;

        public UnityEvent OnLightOn;
        public UnityEvent OnLightOff;

        private LightComponent[] lightComponents;
        private bool powerState;

        public BehaviorSubject<bool> IsTurnedOn { get; set; } = new(false);

        private void Awake()
        {
            lightComponents = new LightComponent[LightComponents.Count];
            for (int i = 0; i < LightComponents.Count; i++)
            {
                lightComponents[i] = new LightComponent()
                {
                    light = LightComponents[i],
                    intensity = LightComponents[i].intensity,
                    current = LightComponents[i].intensity
                };
            }

            SetLightState(IsSwitchedOn);
        }

        public void InteractStart()
        {
            if (UseEnergy && !powerState)
                return;

            if (IsSwitchedOn = !IsSwitchedOn)
            {
                SetLightState(true);
                GameTools.PlayOneShot3D(transform.position, LightSwitchOn, "Lamp On");
                OnLightOn?.Invoke();
            }
            else
            {
                SetLightState(false);
                GameTools.PlayOneShot3D(transform.position, LightSwitchOff, "Lamp Off");
                OnLightOff?.Invoke();
            }
        }

        public void SetLightState(bool state)
        {
            IsSwitchedOn = state;
            SetLightEnabled(state);
            IsTurnedOn.OnNext(state);
        }

        public void OnPowerState(bool state)
        {
            powerState = state;
            if (!IsSwitchedOn)
                return;

            SetLightEnabled(state);
        }

        private void SetLightEnabled(bool state)
        {
            if (!SmoothLight) LightComponents.ForEach(x => x.enabled = state);
            else
            {
                StopAllCoroutines();
                StartCoroutine(SwitchLightSmoothly(state));
            }

            foreach (var renderer in MeshRenderers)
            {
                if (state) renderer.material.EnableKeyword(EmissionKeyword);
                else renderer.material.DisableKeyword(EmissionKeyword);
            }
        }

        IEnumerator SwitchLightSmoothly(bool state)
        {
            float elapsedTime = 0;

            // set current light intensity
            foreach (var light in lightComponents)
            {
                light.current = light.light.intensity;
                if (state)
                {
                    light.light.intensity = 0f;
                    light.light.enabled = true;
                }
            }

            // lerp light intensity smoothly
            while (elapsedTime < SmoothDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / SmoothDuration);

                foreach (var light in lightComponents)
                {
                    float target = state ? light.intensity : 0f;
                    light.light.intensity = Mathf.Lerp(light.current, target, t);
                }

                yield return null;
            }

            // disable light after lerping
            if (!state)
            {
                foreach (var light in lightComponents)
                {
                    light.current = 0f;
                    light.light.enabled = false;
                }
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "lightState", IsSwitchedOn }
            };
        }

        public void OnLoad(JToken data)
        {
            bool lightState = (bool)data["lightState"];
            SetLightState(lightState);
        }
    }
}