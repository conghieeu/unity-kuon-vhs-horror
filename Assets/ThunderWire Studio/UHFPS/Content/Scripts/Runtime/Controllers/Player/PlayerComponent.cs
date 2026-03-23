using System;
using UnityEngine;
using Unity.Cinemachine;

namespace UHFPS.Runtime
{
    public abstract class PlayerComponent : MonoBehaviour
    {
        protected bool isEnabled = true;

        public virtual void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }

        [NonSerialized]
        private PlayerManager playerManager;
        public PlayerManager PlayerManager
        {
            get
            {
                if (playerManager == null)
                {
                    Transform currentTransform = transform;
                    while (currentTransform != null)
                    {
                        if (currentTransform.TryGetComponent(out playerManager)) 
                            break;

                        currentTransform = currentTransform.parent;
                    }
                }

                return playerManager;
            }
        }

        public CharacterController PlayerCollider => PlayerManager.PlayerCollider;
        public PlayerStateMachine PlayerStateMachine => PlayerManager.PlayerStateMachine;
        public LookController LookController => PlayerManager.LookController;
        public ExamineController ExamineController => PlayerManager.ExamineController;

        public Camera MainCamera => PlayerManager.MainCamera;
        public CinemachineCamera VirtualCamera => PlayerManager.MainVirtualCamera;
    }
}