using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Electrical Circuit Lights")]
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

        public List<CircuitLight> CircuitLights = new List<CircuitLight>();
        [Space]
        public RendererMaterial WireMaterial;

        [Header("Shader Keywords")]
        public string EmissionKeyword = "_EMISSION";
        public bool useEmission = false;

        [Header("Light Colors")]
        public Color PoweredOn = Color.green;
        public Color PoweredOff = Color.red;
        public bool useLightColors = false;

        [Header("Settings")]
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