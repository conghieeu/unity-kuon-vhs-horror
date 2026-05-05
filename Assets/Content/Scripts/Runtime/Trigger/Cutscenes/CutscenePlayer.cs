using UnityEngine;
using Unity.Cinemachine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Script điều khiển camera và các thành phần liên quan của người chơi trong suốt quá trình diễn ra cutscene.")]
    public class CutscenePlayer : MonoBehaviour
    {
        [Tooltip("Camera chính đại diện cho góc nhìn của người chơi trong cutscene.")]
        public CinemachineCamera HeadCamera;

        private PlayerPresenceManager playerPresence;
        private PlayerPresenceManager PlayerPresence
        {
            get
            {
                if (playerPresence == null)
                    playerPresence = PlayerPresenceManager.Instance;

                return playerPresence;
            }
        }

        private CharacterController characterController;
        private CharacterController CharacterController
        {
            get
            {
                if (PlayerPresence == null)
                    characterController = null;
                else if (characterController == null)
                    characterController = PlayerPresence.Player.GetComponent<CharacterController>();

                return characterController;
            }
        }

        public void SetCutsceneActive(bool state)
        {
            HeadCamera.gameObject.SetActive(state);
        }

        private void OnDrawGizmosSelected()
        {
            if (CharacterController == null)
                return;

            float offset = 0.6f;
            float height = (CharacterController.height + offset) / 2f;
            float radius = CharacterController.radius;

            Vector3 origin = transform.position + Vector3.up * (offset / 2f);
            Vector3 p2 = origin + Vector3.up * height;
            Vector3 p1 = origin;

            Gizmos.color = Color.cyan;
            GizmosE.DrawWireCapsule(p1, p2, radius);

            Gizmos.color = Color.red;
            GizmosE.DrawGizmosArrow(transform.position, transform.forward * 0.5f);
        }
    }
}