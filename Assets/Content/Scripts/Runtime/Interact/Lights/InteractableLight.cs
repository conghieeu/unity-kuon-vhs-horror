using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using UnityEngine;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class InteractableLight : MonoBehaviour, IPowerConsumer, IInteractStart, ISaveable
    {
        public sealed class LightComponent
        {
            public Light light;
            public float intensity;
            public float current;
        }

        [field: SerializeField]
        public float ConsumeWattage { get; set; }

        public bool IsSwitchedOn;
        public bool UseEnergy;

        public List<Light> LightComponents = new();
        public bool SmoothLight;
        public float SmoothDuration;

        public RendererMaterial LightMaterial;
        public bool EnableEmission = true;
        public string EmissionKeyword = "_EMISSION";

        public SoundClip LightSwitchOn;
        public SoundClip LightSwitchOff;

        public UnityEvent OnLightOn;
        public UnityEvent OnLightOff;

        private LightComponent[] lightComponents;
        private bool powerState;
        private bool prevState;

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

            if(IsSwitchedOn = !IsSwitchedOn)
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

            prevState = IsSwitchedOn;
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
            if (!prevState)
                return;

            SetLightEnabled(state);
            IsSwitchedOn = state;
        }

        private void SetLightEnabled(bool state)
        {
            if (!SmoothLight) LightComponents.ForEach(x => x.enabled = state);
            else
            {
                StopAllCoroutines();
                StartCoroutine(SwitchLightSmoothly(state));
            }

            if (LightMaterial.IsAssigned && EnableEmission)
            {
                if (state) LightMaterial.ClonedMaterial.EnableKeyword(EmissionKeyword);
                else LightMaterial.ClonedMaterial.DisableKeyword(EmissionKeyword);
            }
        }

        IEnumerator SwitchLightSmoothly(bool state)
        {
            float elapsedTime = 0;

            // set current light intensity
            foreach (var light in lightComponents)
            {
                light.current = light.light.intensity;
                if(state)
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
                { "lightState", IsSwitchedOn },
                { "prevState", prevState }
            };
        }

        public void OnLoad(JToken data)
        {
            bool lightState = (bool)data["lightState"];
            prevState = (bool)data["prevState"];
            SetLightState(lightState);
        }
    }
}