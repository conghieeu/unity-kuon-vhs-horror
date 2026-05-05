using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý giao diện hiển thị các mức tín hiệu/chỉ báo trên màn hình máy quay (Camcorder).")]
    [InspectorHeader("Camcorder Channels")]
    public class CamcorderChannels : MonoBehaviour
    {
        public enum Channel { Left, Right }

        [Tooltip("Transform chứa các vạch hiển thị bên trái.")]
        public Transform LeftChannel;
        [Tooltip("Transform chứa các vạch hiển thị bên phải.")]
        public Transform RightChannel;

        [Header("Settings")]
        [Tooltip("Chỉ số phần tử bắt đầu hiển thị màu cảnh báo mức trung bình.")]
        public uint MediumColorIndex = 10;
        [Tooltip("Chỉ số phần tử bắt đầu hiển thị màu cảnh báo mức cao.")]
        public uint HighColorIndex = 18;

        [Header("Colors")]
        [Tooltip("Màu khi vạch chưa được kích hoạt.")]
        public Color DisabledColor = Color.gray;
        [Tooltip("Màu ở mức tín hiệu bình thường.")]
        public Color NormalColor = Color.white;
        [Tooltip("Màu ở mức tín hiệu trung bình.")]
        public Color MediumColor = Color.yellow;
        [Tooltip("Màu ở mức tín hiệu cao (nguy hiểm).")]
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