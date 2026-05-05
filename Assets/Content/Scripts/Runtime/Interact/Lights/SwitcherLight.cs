using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;
using static UHFPS.Runtime.InteractableLight;

namespace UHFPS.Runtime
{
    [Summary("Tương tự InteractableLight nhưng hỗ trợ cấu hình đa MeshRenderer để bật tắt Emission cho nhiều phần của đèn.")]
    public class SwitcherLight : MonoBehaviour, IPowerConsumer, IInteractStart, ISaveable
    {
        [field: SerializeField]
        [Tooltip("Lượng điện năng (Watt) tiêu thụ khi được bật (Nếu có máy phát điện).")]
        public float ConsumeWattage { get; set; }

        [Tooltip("Trạng thái hiện tại của đèn (Đang bật hay tắt).")]
        public bool IsSwitchedOn;

        [Tooltip("Cần có hệ thống điện (Máy phát điện) để hoạt động.")]
        public bool UseEnergy;

        [Tooltip("Danh sách các Component Light (Nguồn sáng) vật lý.")]
        public List<Light> LightComponents = new();

        [Tooltip("Danh sách các đối tượng 3D (MeshRenderer) cần phát sáng bề mặt khi đèn bật.")]
        public List<MeshRenderer> MeshRenderers = new();

        [Tooltip("Bật chế độ sáng dần/mờ đi từ từ.")]
        public bool SmoothLight;

        [Tooltip("Thời gian (giây) để hoàn tất việc làm mờ/sáng.")]
        public float SmoothDuration;

        [Tooltip("Cho phép đổi trạng thái Emission của Material.")]
        public bool EnableEmission = true;

        [Tooltip("Tên tham số phát sáng trong Material (thường là _EMISSION).")]
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Âm thanh bật đèn.")]
        public SoundClip LightSwitchOn;

        [Tooltip("Âm thanh tắt đèn.")]
        public SoundClip LightSwitchOff;

        [Tooltip("Sự kiện gọi ra khi đèn được bật.")]
        public UnityEvent OnLightOn;

        [Tooltip("Sự kiện gọi ra khi đèn bị tắt.")]
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