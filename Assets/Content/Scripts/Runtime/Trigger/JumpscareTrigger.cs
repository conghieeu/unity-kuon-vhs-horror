using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/jumpscares")]
    [Summary("Quản lý việc kích hoạt Jumpscare (trực tiếp qua hình ảnh/mô hình hoặc gián tiếp qua hoạt ảnh).")]
    public class JumpscareTrigger : MonoBehaviour, ISaveable
    {
        public enum JumpscareTypeEnum { Direct, Indirect, Audio }
        public enum DirectTypeEnum { Image, Model }
        public enum TriggerTypeEnum { Event, TriggerEnter, TriggerExit }

        [Tooltip("Loại Jumpscare: Direct (Trực tiếp), Indirect (Qua hoạt ảnh), Audio (Chỉ âm thanh).")]
        public JumpscareTypeEnum JumpscareType = JumpscareTypeEnum.Direct;
        [Tooltip("Hình thức trực tiếp: Bằng Hình ảnh (Image) hay Mô hình 3D (Model).")]
        public DirectTypeEnum DirectType = DirectTypeEnum.Image;
        [Tooltip("Cách thức kích hoạt: Bước vào (TriggerEnter), Thoát ra (TriggerExit) hoặc kích hoạt ngoài (Event).")]
        public TriggerTypeEnum TriggerType = TriggerTypeEnum.Event;

        [Tooltip("Hình ảnh Jumpscare (Nếu chọn DirectType là Image).")]
        public Sprite JumpscareImage;
        [Tooltip("Mã ID của mô hình Jumpscare (Nếu chọn DirectType là Model).")]
        public string JumpscareModelID = "scare_zombie";
        [Tooltip("Âm thanh Jumpscare.")]
        public SoundClip JumpscareSound;

        [Tooltip("Animator dùng cho Jumpscare gián tiếp (Indirect).")]
        public Animator Animator;
        [Tooltip("Tên trạng thái (State) chứa hoạt ảnh Jumpscare trong Animator.")]
        public string AnimatorStateName = "Jumpscare";
        [Tooltip("Tên biến Trigger trong Animator để kích hoạt hoạt ảnh.")]
        public string AnimatorTrigger = "Jumpscare";

        [Tooltip("Bật hiệu ứng hoảng sợ (Fear) sau Jumpscare (Xúc tu/Mờ viền màn hình).")]
        public bool InfluenceFear;
        [Range(0f, 1f)] [Tooltip("Độ rùng rợn của hiệu ứng xúc tu (0-1).")] public float TentaclesIntensity = 0f;
        [Range(0.1f, 3f)] [Tooltip("Tốc độ chuyển động của hiệu ứng xúc tu.")] public float TentaclesSpeed = 1f;
        [Range(0f, 1f)] [Tooltip("Độ mạnh của hiệu ứng làm mờ/tối viền màn hình (Vignette).")] public float VignetteStrength = 0f;

        [Tooltip("Bắt buộc người chơi nhìn về hướng Jumpscare.")]
        public bool LookAtJumpscare;
        [Tooltip("Mục tiêu mà người chơi sẽ bị bắt nhìn vào.")]
        public Transform LookAtTarget;
        [Tooltip("Thời gian ép góc nhìn (giây).")]
        public float LookAtDuration;
        [Tooltip("Khóa di chuyển/camera của người chơi trong lúc xảy ra Jumpscare.")]
        public bool LockPlayer;
        [Tooltip("Jumpscare sẽ không tự kết thúc mà chờ một sự kiện bên ngoài gọi hàm TriggerJumpscareEnded().")]
        public bool EndJumpscareWithEvent;

        [Tooltip("Tạo hiệu ứng rung lắc (Wobble) camera khi Jumpscare.")]
        public bool InfluenceWobble;
        [Tooltip("Biên độ rung lắc.")]
        public float WobbleAmplitudeGain = 1f;
        [Tooltip("Tần số rung lắc.")]
        public float WobbleFrequencyGain = 1f;

        [Tooltip("Thời gian rung lắc (giây).")]
        public float WobbleDuration = 0.2f;
        [Tooltip("Thời gian hiển thị Jumpscare trực tiếp (Image/Model).")]
        public float DirectDuration = 1f;
        [Tooltip("Thời gian duy trì trạng thái hoảng sợ (Fear) sau Jumpscare.")]
        public float FearDuration = 1f;

        [Tooltip("Sự kiện cơ bản của Unity khi bước vào vùng Trigger.")]
        public UnityEvent TriggerEnter;
        [Tooltip("Sự kiện cơ bản của Unity khi thoát ra vùng Trigger.")]
        public UnityEvent TriggerExit;

        [Tooltip("Sự kiện gọi ra khi Jumpscare bắt đầu.")]
        public UnityEvent OnJumpscareStarted;
        [Tooltip("Sự kiện gọi ra khi Jumpscare kết thúc.")]
        public UnityEvent OnJumpscareEnded;

        private bool jumpscareStarted;
        private bool triggerEntered;

        private JumpscareManager jumpscareManager;

        private void Awake()
        {
            jumpscareManager = JumpscareManager.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Event)
                return;

            if (other.CompareTag("Player") && !jumpscareStarted && !triggerEntered)
            {
                TriggerEnter?.Invoke();

                if (TriggerType == TriggerTypeEnum.TriggerEnter)
                {
                    TriggerJumpscare();
                }
                else if (TriggerType == TriggerTypeEnum.TriggerExit)
                {
                    triggerEntered = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Event)
                return;

            if (other.CompareTag("Player") && !jumpscareStarted && triggerEntered)
            {
                TriggerExit?.Invoke();

                if (TriggerType == TriggerTypeEnum.TriggerExit)
                {
                    TriggerJumpscare();
                }
            }
        }

        public void TriggerJumpscare()
        {
            if (jumpscareStarted)
                return;

            OnJumpscareStarted?.Invoke();

            if(JumpscareType == JumpscareTypeEnum.Indirect)
            {
                Animator.SetTrigger(AnimatorTrigger);
                StartCoroutine(IndirectJumpscare());
            }

            jumpscareManager.StartJumpscareEffect(this);
            GameTools.PlayOneShot2D(transform.position, JumpscareSound, "Jumpscare Sound");

            jumpscareStarted = true;
        }

        public void TriggerJumpscareEnded()
        {
            if (EndJumpscareWithEvent) jumpscareManager.EndJumpscareEffect();
        }

        IEnumerator IndirectJumpscare()
        {
            yield return new WaitForAnimatorClip(Animator, AnimatorStateName);
            if (!EndJumpscareWithEvent) jumpscareManager.EndJumpscareEffect();
            OnJumpscareEnded?.Invoke();
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(jumpscareStarted), jumpscareStarted }
            };
        }

        public void OnLoad(JToken data)
        {
            jumpscareStarted = (bool)data[nameof(jumpscareStarted)];
        }
    }
}