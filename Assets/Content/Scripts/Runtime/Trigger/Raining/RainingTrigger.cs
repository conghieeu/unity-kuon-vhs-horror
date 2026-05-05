using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Raining Trigger")]
    [Summary("Bật/tắt hiệu ứng mưa khi người chơi bước vào hoặc thoát khỏi vùng Trigger (Ví dụ: bước vào nhà thì tạnh mưa).")]
    public class RainingTrigger : MonoBehaviour
    {
        public enum TriggerTypeEnum { Enter, Exit, Stay }

        [Tooltip("Khi nào thì thay đổi: Khi bước vào (Enter), thoát ra (Exit) hoặc khi ở trong vùng (Stay).")]
        public TriggerTypeEnum TriggerType = TriggerTypeEnum.Enter;
        [Tooltip("Thời gian chuyển đổi (mờ dần) hiệu ứng mưa.")]
        public float BlendTime = 1f;
        [Tooltip("Trạng thái hiệu ứng mưa mong muốn (Bật = true, Tắt = false).")]
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