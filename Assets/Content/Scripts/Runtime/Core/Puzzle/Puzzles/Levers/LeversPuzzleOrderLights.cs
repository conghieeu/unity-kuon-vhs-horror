using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class LeversPuzzleOrderLights : MonoBehaviour, ISaveable
    {
        [Serializable]
        public struct OrderLight
        {
            public Light Light;
            public RendererMaterial LightMaterial;
        }

        public LeversPuzzle LeversPuzzle;
        public List<OrderLight> OrderLights = new();
        public string EmissionKeyword = "_EMISSION";
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