using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("CRT Simple Monitor")]
    public class CRTSimpleMonitor : MonoBehaviour
    {
        [System.Serializable]
        public struct DisplayTexture
        {
            public string Name;
            public Texture2D Texture;
        }

        [Header("Material Setup")]
        public RendererMaterial Display;
        public Material PoweredOnMaterial;
        public Material PoweredOffMaterial;
        public string MaterialProperty = "_MainTex";

        [Header("Display Setup")]
        public DisplayTexture[] DisplayTextures;

        public bool IsPoweredOn = false;

        private RenderTexture inputTexture;
        private Texture2D displayTexture;
        private string displayTextureName;

        private void Start()
        {
            if (IsPoweredOn)
                SetPower(true);
        }

        public void PowerOnOff()
        {
            SetPower(!IsPoweredOn);
        }

        public void SetPower(bool power)
        {
            if (IsPoweredOn = power)
            {
                Display.ClonedMaterial = PoweredOnMaterial;
                if (inputTexture == null && displayTexture == null) SetNamedTexture(displayTextureName);
                else if (inputTexture == null && displayTexture != null) SetTexture(displayTexture);
                else SetVideoInput(inputTexture);
            }
            else Display.ClonedMaterial = PoweredOffMaterial;
        }

        public void SetVideoInput(RenderTexture texture)
        {
            if (IsPoweredOn) 
                Display.ClonedMaterial.SetTexture(MaterialProperty, texture);

            inputTexture = texture;
            displayTexture = null;
        }

        public void SetNamedTexture(string name)
        {
            Texture2D texture = null;

            foreach (var displayTexture in DisplayTextures)
            {
                if (displayTexture.Name.Equals(name))
                {
                    texture = displayTexture.Texture;
                    break;
                }
            }

            if(texture != null && IsPoweredOn)
                Display.ClonedMaterial.SetTexture(MaterialProperty, texture);

            displayTextureName = name;
            displayTexture = texture;
            inputTexture = null;
        }

        public void SetTexture(Texture2D texture)
        {
            if (IsPoweredOn) Display.ClonedMaterial.SetTexture(MaterialProperty, texture);
            displayTexture = texture;
            inputTexture = null;
        }
    }
}