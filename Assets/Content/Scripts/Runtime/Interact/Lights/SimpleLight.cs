using UnityEngine;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [InspectorHeader("Simple Light")]
    [Summary("Quản lý một bóng đèn đơn giản, chỉ bật/tắt tự động hoặc qua sự kiện (Không tương tác trực tiếp bằng tay).")]
    public class SimpleLight : MonoBehaviour, ISaveable
    {
        [Tooltip("Component Light (Nguồn sáng) vật lý.")]
        public Light Light;

        [Header("Settings")]
        [Tooltip("Bật chế độ thay đổi Emission (Phát sáng bề mặt) của đèn.")]
        public bool UseEmission = true;

        [Tooltip("Trạng thái đèn hiện tại (Sáng/Tắt).")]
        public bool LightState = false;

        [Header("Emission")]
        [Tooltip("Renderer chứa Material cần phát sáng.")]
        public RendererMaterial LightRenderer;

        [Tooltip("Tên tham số phát sáng trong Material (Thường là _EMISSION).")]
        public string EmissionKeyword = "_EMISSION";

        private void Awake()
        {
            if (LightState && !SaveGameManager.GameWillLoad) 
                SetLightState(true);
        }

        public void SetLightState(bool state)
        {
            if (state)
            {
                if (Light) Light.enabled = true;
                if (UseEmission) LightRenderer.ClonedMaterial.EnableKeyword(EmissionKeyword);
            }
            else
            {
                if (Light) Light.enabled = false;
                if (UseEmission) LightRenderer.ClonedMaterial.DisableKeyword(EmissionKeyword);
            }

            LightState = state;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "lightState", LightState }
            };
        }

        public void OnLoad(JToken data)
        {
            bool lightState = (bool)data["lightState"];
            SetLightState(lightState);
        }
    }
}