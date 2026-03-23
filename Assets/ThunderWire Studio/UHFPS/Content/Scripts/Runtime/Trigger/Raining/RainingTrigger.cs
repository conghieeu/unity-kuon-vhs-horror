using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Raining Trigger")]
    public class RainingTrigger : MonoBehaviour
    {
        public enum TriggerTypeEnum { Enter, Exit, Stay }

        public TriggerTypeEnum TriggerType = TriggerTypeEnum.Enter;
        public float BlendTime = 1f;
        public bool RainingState = true;

        private RainingModule raining;

        private void Awake()
        {
            raining = GameManager.Module<RainingModule>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Exit)
                return;

            if (other.CompareTag("Player"))
            {
                if (TriggerType == TriggerTypeEnum.Enter)
                    raining.FadeRaindrop(RainingState, BlendTime);
                else if (TriggerType == TriggerTypeEnum.Stay)
                    raining.FadeRaindrop(RainingState, BlendTime);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Enter)
                return;

            if (other.CompareTag("Player"))
            {
                if (TriggerType == TriggerTypeEnum.Exit)
                    raining.FadeRaindrop(RainingState, BlendTime);
                else if(TriggerType == TriggerTypeEnum.Stay)
                    raining.FadeRaindrop(!RainingState, BlendTime);
            }
        }
    }
}