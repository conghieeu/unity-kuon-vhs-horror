using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Look At Trigger")]
    [Summary("Kích hoạt sự kiện khi người chơi nhìn thẳng vào (Look At) hoặc nhìn hướng khác (Look Away) đối với vật thể này.")]
    public class LookAtTrigger : MonoBehaviour, ISaveable
    {
        public enum TriggerTypeEnum { Once, MoreTimes }

        [Tooltip("Loại kích hoạt: Một lần (Once) hay Nhiều lần (MoreTimes).")]
        public TriggerTypeEnum TriggerType;
        [Tooltip("LayerMask dùng để kiểm tra vật cản che khuất tầm nhìn (Raycast).")]
        public LayerMask CullMask;
        [Tooltip("Phạm vi khung nhìn (Viewport) mà người chơi phải hướng tới (1,1 là toàn màn hình, thấp hơn là phải nhìn gần vào tâm vật thể hơn).")]
        public Vector2 ViewportOffset = Vector2.one;

        [Tooltip("Sử dụng ViewportOffset để xác định hành động Look Away (người chơi nhìn ra khỏi phạm vi Viewport).")]
        public bool LookAwayViewport = false;
        [Tooltip("Bật kiểm tra khoảng cách (người chơi phải ở trong khoảng cách TriggerDistance).")]
        public bool UseDistance = false;

        [Tooltip("Có gọi sự kiện Look Away khi người chơi vượt ra ngoài khoảng cách cho phép không?")]
        public bool CallEventOutsideDistance = false;
        [Tooltip("Hiển thị vòng tròn khoảng cách trong Scene view (chỉ trên Editor).")]
        public bool VisualizeDistance = false;
        [Tooltip("Khoảng cách tối đa người chơi có thể nhìn thấy và kích hoạt vật thể.")]
        public float TriggerDistance = 5f;

        [Tooltip("Sự kiện gọi ra khi người chơi nhìn vào vật thể.")]
        public UnityEvent OnLookAt;
        [Tooltip("Sự kiện gọi ra khi người chơi rời mắt khỏi vật thể.")]
        public UnityEvent OnLookAway;

        private PlayerPresenceManager playerPresence;
        private bool isLookedOnce = false;
        private bool resetLook = false;

        private void Awake()
        {
            playerPresence = PlayerPresenceManager.Instance;
        }

        private void Update()
        {
            Camera playerCamera = playerPresence.PlayerCamera;
            Transform cameraTransform = playerCamera.transform;
            bool inDistance = true;

            if (UseDistance)
            {
                Vector3 playerPos = playerPresence.Player.transform.position;
                float distance = Vector3.Distance(transform.position, playerPos);
                inDistance = distance <= TriggerDistance;
            }

            if(inDistance && !Physics.Linecast(transform.position, cameraTransform.position, CullMask))
            {
                Vector3 screenPoint = playerCamera.WorldToViewportPoint(transform.position);
                if(screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0)
                {
                    float xMin = 1 - Remap(ViewportOffset.x);
                    float xMax = Remap(ViewportOffset.x);

                    float yMin = 1 - Remap(ViewportOffset.y);
                    float yMax = Remap(ViewportOffset.y);

                    if (screenPoint.x >= xMin && screenPoint.x <= xMax && screenPoint.y >= yMin && screenPoint.y <= yMax)
                    {
                        if (!isLookedOnce)
                        {
                            OnLookAt?.Invoke();
                            isLookedOnce = true;
                            resetLook = false;
                        }
                    }
                    else if (LookAwayViewport && isLookedOnce && !resetLook)
                    {
                        OnLookAway?.Invoke();
                        resetLook = true;

                        if (TriggerType == TriggerTypeEnum.MoreTimes)
                        {
                            isLookedOnce = false;
                        }
                    }
                }
                else if(!LookAwayViewport && isLookedOnce && !resetLook)
                {
                    OnLookAway?.Invoke();
                    resetLook = true;

                    if (TriggerType == TriggerTypeEnum.MoreTimes)
                    {
                        isLookedOnce = false;
                    }
                }
            }
            else if (TriggerType == TriggerTypeEnum.MoreTimes)
            {
                if (CallEventOutsideDistance && isLookedOnce)
                    OnLookAway?.Invoke();

                isLookedOnce = false;
                resetLook = true;
            }
        }

        private float Remap(float value)
        {
            return (value - 0) / (1 - 0) * (1 - 0.5f) + 0.5f;
        }

        private void OnDrawGizmosSelected()
        {
            if (!UseDistance || !VisualizeDistance) 
                return;

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, TriggerDistance);
#endif
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isLookedOnce), isLookedOnce },
                { nameof(resetLook), resetLook }
            };
        }

        public void OnLoad(JToken data)
        {
            isLookedOnce = (bool)data["isLookedOnce"];
            resetLook = (bool)data["resetLook"];
        }
    }
}