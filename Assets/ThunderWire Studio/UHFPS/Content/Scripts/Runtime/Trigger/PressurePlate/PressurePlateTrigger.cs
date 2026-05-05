using UnityEngine.Events;
using UnityEngine;
using System;

namespace UHFPS.Runtime
{
    public class PressurePlateTrigger : MonoBehaviour, ICharacterControllerHit
    {
        [Flags]
        public enum WeightTypeEnum
        {
            None = 0,
            Player = 1 << 0,
            Objects = 1 << 2
        }

        public WeightTypeEnum WeightType = WeightTypeEnum.Player | WeightTypeEnum.Objects;
        public float TriggerWeight = 10f;

        public UnityEvent OnWeightTrigger;
        public UnityEvent OnWeightChange;
        public UnityEvent OnWeightRelease;

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