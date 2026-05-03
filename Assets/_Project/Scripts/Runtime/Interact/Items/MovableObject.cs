using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class MovableObject : MonoBehaviour, IStateInteract
    {
        public enum MoveDirectionEnum { LeftRight, ForwardBackward, AllDirections }

        public AudioSource AudioSource;
        public Rigidbody Rigidbody;
        public Axis ForwardAxis;
        public bool DrawGizmos = true;

        public MoveDirectionEnum MoveDirection;
        public LayerMask CollisionMask;
        public Vector3 HoldOffset;
        public bool AllowRotation = true;

        public float HoldDistance = 2f;
        public float ObjectWeight = 20f;
        public float PlayerRadius = 0.3f;
        public float PlayerHeight = 1.8f;
        public float PlayerFeetOffset = 0f;

        public float WalkMultiplier = 1f;
        public float LookMultiplier = 1f;

        [Range(0f, 1f)]
        public float SlideVolume = 1f;
        public float VolumeFadeSpeed = 1f;

        public bool UseMouseLimits;
        public MinMax MouseVerticalLimits;

        public Transform RootMovable => Rigidbody.transform;

        public MeshRenderer Renderer => RootMovable.GetComponent<MeshRenderer>();

        private void Awake()
        {
            if(Rigidbody != null) Rigidbody.mass = ObjectWeight;
            if(AudioSource != null)
            {
                AudioSource.playOnAwake = false;
                AudioSource.spatialBlend = 1f;
                AudioSource.loop = true;
                AudioSource.Stop();
            }
        }

        public void FadeSoundOut()
        {
            StartCoroutine(FadeSound());
        }

        IEnumerator FadeSound()
        {
            while(Mathf.Approximately(AudioSource.volume, 0f))
            {
                AudioSource.volume = Mathf.MoveTowards(AudioSource.volume, 0f, Time.deltaTime * SlideVolume * 10);
                yield return null;
            }

            AudioSource.volume = 0f;
            AudioSource.Stop();
        }

        public StateParams OnStateInteract()
        {
            if (!CheckOverlapping())
            {
                StopAllCoroutines();
                return new StateParams()
                {
                    stateKey = PlayerStateMachine.PUSHING_STATE,
                    stateData = new StorableCollection()
                    {
                        { "reference", this }
                    }
                };
            }

            return null;
        }

        private bool CheckOverlapping()
        {
            Vector3 forwardGlobal = ForwardAxis.Convert();
            float height = PlayerHeight - 0.6f;

            Vector3 position = RootMovable.TransformPoint((-forwardGlobal * HoldDistance) + HoldOffset);
            Vector3 bottomPos = new(position.x, Renderer.bounds.min.y, position.z);

            Vector3 playerBottom = bottomPos;
            playerBottom.y += PlayerFeetOffset;

            Vector3 p1 = new Vector3(position.x, playerBottom.y, position.z);
            Vector3 p2 = new Vector3(position.x, playerBottom.y + height, position.z);

            return Physics.CheckCapsule(p1, p2, PlayerRadius, CollisionMask);
        }

        private void OnDrawGizmosSelected()
        {
            if (!DrawGizmos || Rigidbody == null || RootMovable == null) 
                return;

            Vector3 forwardGlobal = ForwardAxis.Convert();
            Vector3 forwardLocal = RootMovable.Direction(ForwardAxis);
            float radius = 0.5f;

            Vector3 position = RootMovable.TransformPoint((-forwardGlobal * HoldDistance) + HoldOffset);
            Vector3 bottomPos = new(position.x, Renderer.bounds.min.y, position.z);

            GizmosE.DrawDisc(bottomPos, radius, Color.green, Color.green.Alpha(0.01f));
            GizmosE.DrawGizmosArrow(bottomPos, forwardLocal * radius);

            float height = PlayerHeight - 0.6f;
            Vector3 playerBottom = bottomPos;
            playerBottom.y += PlayerFeetOffset;

            Vector3 p1 = new(position.x, playerBottom.y, position.z);
            Vector3 p2 = new(position.x, playerBottom.y + height, position.z);

            Gizmos.color = Color.green;
            GizmosE.DrawWireCapsule(p1, p2, PlayerRadius);
        }
    }
}