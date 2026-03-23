using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class LookAtTrigger : MonoBehaviour, ISaveable
    {
        public enum TriggerTypeEnum { Once, MoreTimes }

        public TriggerTypeEnum TriggerType;
        public LayerMask CullMask;
        public Vector2 ViewportOffset = Vector2.one;

        public bool LookAwayViewport = false;
        public bool UseDistance = false;

        public bool CallEventOutsideDistance = false;
        public bool VisualizeDistance = false;
        public float TriggerDistance = 5f;

        public UnityEvent OnLookAt;
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