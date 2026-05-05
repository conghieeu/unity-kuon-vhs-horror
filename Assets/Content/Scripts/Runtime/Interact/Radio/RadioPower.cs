using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Radio Power")]
    [Summary("Quản lý núm xoay/gạt (theo trục dọc) để bật hoặc tắt nguồn Radio.")]
    public class RadioPower : MonoBehaviour, IExamineDragVertical
    {
        [Tooltip("Khối (Transform) của công tắc nguồn trên Radio.")]
        public Transform Switch;

        [Header("Settings")]
        [Tooltip("Trục xoay của công tắc.")]
        public Axis SwitchAxis;

        [Tooltip("Giới hạn góc khi Tắt/Bật.")]
        public MinMax SwitchLimits;

        [Tooltip("Tốc độ chuyển động của công tắc.")]
        public float SwitchSpeed;

        [Tooltip("Ngưỡng kéo chuột dọc tối thiểu (Drag Delta) để kích hoạt bật/tắt.")]
        public float SwitchDelta;

        [Header("Sounds")]
        [Tooltip("Âm thanh công tắc (Tách).")]
        public SoundClip SwitchSound;

        private Radio radio;
        private bool isSwitched;
        private float currPos;

        private void Awake()
        {
            currPos = Switch.localPosition.Component(SwitchAxis);
            radio = GetComponentInParent<Radio>();
        }

        public void OnExamineDragVertical(float dragDelta)
        {
            if(!isSwitched && dragDelta >= SwitchDelta)
            {
                isSwitched = true;
                radio.SwitchRadio(true);
                GameTools.PlayOneShot3D(transform.position, SwitchSound, "Radio Switch");
            }
            else if(isSwitched && dragDelta <= -SwitchDelta)
            {
                isSwitched = false;
                radio.SwitchRadio(false);
                GameTools.PlayOneShot3D(transform.position, SwitchSound, "Radio Switch");
            }
        }

        private void Update()
        {
            Vector3 position = Switch.localPosition;
            float nextPos = isSwitched ? SwitchLimits.max : SwitchLimits.min;

            currPos = Mathf.MoveTowards(currPos, nextPos, Time.deltaTime * SwitchSpeed);
            position = position.SetComponent(SwitchAxis, currPos);
            Switch.localPosition = position;
        }
    }
}