using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Thành phần hỗ trợ cho Levers Puzzle (Order mode). Quản lý chuỗi đèn báo hiệu bật sáng tuần tự khi gạt đòn bẩy.")]
    public class LeversPuzzleOrderLights : MonoBehaviour, ISaveable
    {
        [Serializable]
        public struct OrderLight
        {
            public Light Light;
            public RendererMaterial LightMaterial;
        }

        [Tooltip("Tham chiếu đến hệ thống Puzzle quản lý nhóm đòn bẩy.")]
        public LeversPuzzle LeversPuzzle;

        [Tooltip("Danh sách cấu hình đèn báo tương ứng với từng bước trong thứ tự.")]
        public List<OrderLight> OrderLights = new();

        [Tooltip("Từ khóa Shader để bật hiệu ứng Emission (Mặc định: _EMISSION).")]
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Chỉ số thứ tự đèn hiện tại đang được bật.")]
        public int OrderIndex = 0;

        public void OnSetLever()
        {
            if (OrderIndex < LeversPuzzle.Levers.Count)
                SetLightState(OrderLights[OrderIndex++], true);
        }

        public void ResetLights()
        {
            foreach (var item in OrderLights)
            {
                SetLightState(item, false);
            }

            OrderIndex = 0;
        }

        private void SetLightState(OrderLight light, bool state)
        {
            light.Light.enabled = state;
            if (state) light.LightMaterial.ClonedMaterial.EnableKeyword(EmissionKeyword);
            else light.LightMaterial.ClonedMaterial.DisableKeyword(EmissionKeyword);
        }

        public StorableCollection OnSave()
        {
            StorableCollection storableCollection = new StorableCollection();

            for (int i = 0; i < OrderLights.Count; i++)
            {
                string name = "light_" + i;
                bool lightState = OrderLights[i].Light.enabled;
                storableCollection.Add(name, lightState);
            }

            storableCollection.Add("orderIndex", OrderIndex);
            return storableCollection;
        }

        public void OnLoad(JToken data)
        {
            for (int i = 0; i < OrderLights.Count; i++)
            {
                string name = "light_" + i;
                bool lightState = (bool)data[name];
                SetLightState(OrderLights[i], lightState);
            }

            OrderIndex = (int)data["orderIndex"];
        }
    }
}