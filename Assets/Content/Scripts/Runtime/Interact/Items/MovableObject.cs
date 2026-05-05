using System.Collections;
using UnityEngine;
using UHFPS.Input;
using UHFPS.Tools;
using UHFPS.Runtime.States;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý một vật thể có thể được người chơi kéo/đẩy (Ví dụ: Thùng gỗ lớn, Tủ đồ).")]
    public class MovableObject : MonoBehaviour, IStateInteract
    {
        public enum MoveDirectionEnum { LeftRight, ForwardBackward, AllDirections }

        [Tooltip("Âm thanh phát ra khi vật thể trượt trên mặt đất.")]
        public AudioSource AudioSource;

        [Tooltip("Thành phần vật lý của vật thể.")]
        public Rigidbody Rigidbody;

        [Tooltip("Trục không gian hướng về phía trước của vật thể.")]
        public Axis ForwardAxis;

        [Tooltip("Hiển thị hình vẽ (Gizmos) trong Editor.")]
        public bool DrawGizmos = true;

        [Tooltip("Hướng di chuyển cho phép (Trái/Phải, Tiến/Lùi, hay Tất cả).")]
        public MoveDirectionEnum MoveDirection;

        [Tooltip("Lớp (Layer) của chướng ngại vật cần kiểm tra va chạm khi di chuyển.")]
        public LayerMask CollisionMask;

        [Tooltip("Độ lệch (Offset) vị trí cầm nắm so với điểm gốc.")]
        public Vector3 HoldOffset;

        [Tooltip("Cho phép người chơi xoay (Rotate) vật thể khi di chuyển.")]
        public bool AllowRotation = true;

        [Tooltip("Khoảng cách tối đa người chơi có thể với tới để giữ vật.")]
        public float HoldDistance = 2f;

        [Tooltip("Trọng lượng của vật thể (Ảnh hưởng lực kéo/đẩy).")]
        public float ObjectWeight = 20f;

        [Tooltip("Bán kính vùng va chạm mô phỏng của người chơi.")]
        public float PlayerRadius = 0.3f;

        [Tooltip("Chiều cao vùng va chạm mô phỏng của người chơi.")]
        public float PlayerHeight = 1.8f;

        [Tooltip("Độ lệch (Offset) phần chân người chơi khi kiểm tra va chạm.")]
        public float PlayerFeetOffset = 0f;

        [Tooltip("Hệ số làm chậm tốc độ di chuyển của người chơi khi kéo vật.")]
        public float WalkMultiplier = 1f;

        [Tooltip("Hệ số làm chậm tốc độ xoay Camera khi kéo vật.")]
        public float LookMultiplier = 1f;

        [Range(0f, 1f)]
        [Tooltip("Âm lượng tối đa tiếng trượt của vật thể.")]
        public float SlideVolume = 1f;

        [Tooltip("Tốc độ mờ/giảm dần âm lượng khi dừng kéo.")]
        public float VolumeFadeSpeed = 1f;

        [Tooltip("Bật chế độ giới hạn góc xoay dọc của Camera (Chuột).")]
        public bool UseMouseLimits;

        [Tooltip("Giới hạn góc xoay dọc của Camera khi kéo vật.")]
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