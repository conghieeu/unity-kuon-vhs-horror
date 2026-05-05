using UnityEngine.Events;
using UnityEngine;
using System;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Kích hoạt sự kiện khi có đủ trọng lượng đè lên bàn đạp (Pressure Plate).")]
    public class PressurePlateTrigger : MonoBehaviour, ICharacterControllerHit
    {
        [Flags]
        public enum WeightTypeEnum
        {
            None = 0,
            Player = 1 << 0,
            Objects = 1 << 2
        }

        [Tooltip("Loại đối tượng có thể tạo ra trọng lượng đè lên bàn đạp (Người chơi, Vật thể, hoặc cả hai).")]
        public WeightTypeEnum WeightType = WeightTypeEnum.Player | WeightTypeEnum.Objects;
        [Tooltip("Trọng lượng tối thiểu cần thiết để kích hoạt bàn đạp.")]
        public float TriggerWeight = 10f;

        [Tooltip("Sự kiện gọi ra khi tổng trọng lượng đạt hoặc vượt ngưỡng TriggerWeight.")]
        public UnityEvent OnWeightTrigger;
        [Tooltip("Sự kiện gọi ra mỗi khi trọng lượng thay đổi.")]
        public UnityEvent OnWeightChange;
        [Tooltip("Sự kiện gọi ra khi tổng trọng lượng giảm xuống dưới ngưỡng TriggerWeight.")]
        public UnityEvent OnWeightRelease;

        [Tooltip("Tổng trọng lượng hiện tại đang đè lên bàn đạp.")]
        public float totalWeight;
        private float playerWeight;
        private bool isTriggered;

        public void OnCharacterControllerEnter(CharacterController controller)
        {
            if (!WeightType.HasFlag(WeightTypeEnum.Player))
                return;

            var player = controller.gameObject.GetComponent<PlayerStateMachine>();
            playerWeight = player.PlayerControllerSettings.PlayerWeight;
            totalWeight += playerWeight;

            CheckWeight();
        }

        public void OnCharacterControllerExit()
        {
            if (!WeightType.HasFlag(WeightTypeEnum.Player))
                return;

            totalWeight -= playerWeight;
            playerWeight = 0f;

            CheckWeight();
        }

        public void OnWeightObjectStack(float weightChange)
        {
            if (!WeightType.HasFlag(WeightTypeEnum.Objects))
                return;

            totalWeight += weightChange;
            CheckWeight();
        }

        private void CheckWeight()
        {
            OnWeightChange?.Invoke();

            if(totalWeight >= TriggerWeight)
            {
                if (!isTriggered)
                {
                    OnWeightTrigger?.Invoke();
                    isTriggered = true;
                }
            }
            else if(isTriggered)
            {
                OnWeightRelease?.Invoke();
                isTriggered = false;
            }
        }
    }
}