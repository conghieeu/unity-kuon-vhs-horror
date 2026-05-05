using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Electrical Circuit Lights")]
    [Summary("Quản lý hệ thống đèn báo hiệu và dây dẫn phát sáng cho Electrical Circuit Puzzle.")]
    public class ElectricalCircuitLights : MonoBehaviour
    {
        [Serializable]
        public sealed class CircuitLight
        {
            public uint PowerID;
            public Light[] Lights;
            public RendererMaterial[] Renderers;
            public bool isPowered;
        }

        [Tooltip("Danh sách cấu hình các cụm đèn báo theo mã dòng điện (Power ID).")]
        public List<CircuitLight> CircuitLights = new List<CircuitLight>();
        [Space]
        [Tooltip("Vật liệu dây dẫn sẽ phát sáng khi có dòng điện chạy qua.")]
        public RendererMaterial WireMaterial;

        [Header("Shader Keywords")]
        [Tooltip("Từ khóa Shader bật hiệu ứng sáng (Mặc định: _EMISSION).")]
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Bật/Tắt đổi Emission thay vì chỉ bật/tắt component Light.")]
        public bool useEmission = false;

        [Header("Light Colors")]
        [Tooltip("Màu đèn khi có điện.")]
        public Color PoweredOn = Color.green;

        [Tooltip("Màu đèn khi mất điện.")]
        public Color PoweredOff = Color.red;

        [Tooltip("Sử dụng đổi màu đèn (Light component) thay vì bật/tắt.")]
        public bool useLightColors = false;

        [Header("Settings")]
        [Tooltip("Đánh dấu đây là nguồn cấp điện đầu ra (Sẽ luôn sáng từ lúc bắt đầu).")]
        public bool isOutputLight = false;

        private void Awake()
        {
            if(isOutputLight)
            {
                foreach (var light in CircuitLights)
                {
                    SetCircuitLight(light, true);
                }

                if (WireMaterial.IsAssigned)
                {
                    WireMaterial.ClonedMaterial.EnableKeyword(EmissionKeyword);
                }
            }
        }

        public void OnConnected(int powerID)
        {
            SetCircuitLight(powerID, true);
        }

        public void OnDisconnected(int powerID)
        {
            SetCircuitLight(powerID, false);
        }

        public void SetCircuitLight(int powerID, bool state)
        {
            foreach (var circuitLight in CircuitLights)
            {
                if(circuitLight.PowerID == powerID)
                {
                    SetCircuitLight(circuitLight, state);
                }
            }

            if (WireMaterial.IsAssigned)
            {
                if (CircuitLights.Any(x => x.isPowered))
                {
                    WireMaterial.ClonedMaterial.EnableKeyword(EmissionKeyword);
                }
                else
                {
                    WireMaterial.ClonedMaterial.DisableKeyword(EmissionKeyword);
                }
            }
        }

        private void SetCircuitLight(CircuitLight circuitLight, bool state)
        {
            foreach (var light in circuitLight.Lights)
            {
                if (state)
                {
                    if(useLightColors) light.color = PoweredOn;
                    else light.enabled = true;
                }
                else
                {
                    if (useLightColors) light.color = PoweredOff;
                    else light.enabled = false;
                }
            }

            if (useEmission)
            {
                foreach (var renderer in circuitLight.Renderers)
                {
                    if(state) renderer.ClonedMaterial.EnableKeyword(EmissionKeyword);
                    else renderer.ClonedMaterial.DisableKeyword(EmissionKeyword);
                }
            }

            circuitLight.isPowered = state;
        }
    }
}