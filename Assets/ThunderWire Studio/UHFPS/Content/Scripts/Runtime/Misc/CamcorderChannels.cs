using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Camcorder Channels")]
    public class CamcorderChannels : MonoBehaviour
    {
        public enum Channel { Left, Right }

        public Transform LeftChannel;
        public Transform RightChannel;

        [Header("Settings")]
        public uint MediumColorIndex = 10;
        public uint HighColorIndex = 18;

        [Header("Colors")]
        public Color DisabledColor = Color.gray;
        public Color NormalColor = Color.white;
        public Color MediumColor = Color.yellow;
        public Color HighColor = Color.red;

        private Image[] leftChannelParts;
        private Image[] rightChannelParts;

        private void Awake()
        {
            leftChannelParts = LeftChannel.transform.GetComponentsInChildren<Image>();
            rightChannelParts = RightChannel.transform.GetComponentsInChildren<Image>();
        }

        public void SetChannelValue(Channel channel, float value)
        {
            if (!gameObject.activeInHierarchy)
                return;

            Image[] parts = channel == Channel.Left ? leftChannelParts : rightChannelParts;
            int activeParts = Mathf.CeilToInt(Mathf.Lerp(0, parts.Length, value));

            for (int i = 0; i < parts.Length; i++)
            {
                Image part = parts[i];
                if(i < activeParts)
                {
                    if (i >= HighColorIndex) part.color = HighColor;
                    else if(i >= MediumColorIndex) part.color = MediumColor;
                    else part.color = NormalColor;
                }
                else
                {
                    part.color = DisabledColor;
                }
            }
        }
    }
}