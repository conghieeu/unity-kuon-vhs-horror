using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    [Summary("Hệ thống giải đố Bảng mã điện tử (Keypad) cho két sắt nhỏ. Hỗ trợ hiển thị đèn báo hiệu và kiểm tra mã pin 4 số.")]
    public class SafeKeypadPuzzle : PuzzleBase, ISaveable
    {
        public enum Button { Number0, Number1, Number2, Number3, Number4, Number5, Number6, Number7, Number8, Number9, Cancel, Confirm }
        private const int MAX_INDICATORS = 4;

        [Tooltip("Mã PIN đúng để mở khóa két (Mặc định 4 số).")]
        public string AccessCode = "0000";

        [Tooltip("Animator của két sắt để thực hiện Animation mở cửa.")]
        public Animator Animator;

        [Tooltip("Tên Trigger gọi animation mở cửa.")]
        public string UnlockTrigger = "Unlock";

        [Tooltip("Tên Trigger gọi animation đóng/reset lại lúc load game.")]
        public string ResetTrigger = "Reset";

        [Tooltip("Sử dụng các đèn LED nhỏ để báo hiệu số lượng phím đã bấm.")]
        public bool UseIndicators = true;

        [Tooltip("Danh sách Renderer của 4 đèn LED báo hiệu.")]
        public MeshRenderer[] Indicators;
        public string EmissionKeyword = "_EMISSION";
        public string EmissionColor = "_EmissionColor";

        [Tooltip("Màu đèn báo lúc chưa nhập hoặc nhập sai (Thường là đỏ).")]
        public Color DefaultLightColor = Color.red;

        [Tooltip("Màu đèn báo khi đã nhập phím (Thường là xanh).")]
        public Color EnterLightColor = Color.green;

        [Tooltip("Âm thanh khi nhấn nút trên keypad.")]
        public SoundClip ButtonPressSound;

        [Tooltip("Âm thanh khi nhập đúng mã.")]
        public SoundClip AccessGrantedSound;

        [Tooltip("Âm thanh báo lỗi khi nhập sai mã.")]
        public SoundClip AccessDeniedSound;

        [Tooltip("Có gọi sự kiện OnAccessGranted khi load game đã giải xong không?")]
        public bool LoadCallEvent;

        [Tooltip("Sự kiện gọi ra khi mở két thành công.")]
        public UnityEvent OnAccessGranted;

        [Tooltip("Sự kiện gọi ra khi nhập sai mã.")]
        public UnityEvent OnAccessDenied;

        [Tooltip("Sự kiện gọi ra mỗi khi một phím số được bấm (truyền ra số vừa bấm).")]
        public UnityEvent<int> OnButtonPressed;

        private AudioSource audioSource;
        private string enteredCode = "";

        private bool isUnlocked;
        private bool notUsable;

        public override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
        }

        public void OnPressButton(Button button)
        {
            if (isUnlocked || notUsable)
                return;

            if (button == Button.Confirm)
            {
                if (enteredCode == AccessCode)
                {
                    SetAccessGranted();
                    isUnlocked = true;
                }
                else
                {
                    SetAccessDenied();
                    enteredCode = "";
                }
            }
            else if(button == Button.Cancel)
            {
                enteredCode = "";
                SetIndicator();
            }
            else if (enteredCode.Length < 4)
            {
                enteredCode += (int)button;
                OnButtonPressed?.Invoke((int)button);
                SetIndicator();
            }

            audioSource.PlayOneShotSoundClip(ButtonPressSound);
        }

        public override void OnBackgroundFade()
        {
            base.OnBackgroundFade();

            if (isActive) gameObject.layer = DisabledLayer;
            else if (!isUnlocked)
            {
                gameObject.layer = InteractLayer;
                enteredCode = "";

                for (int i = 0; i < MAX_INDICATORS; i++)
                {
                    Indicators[i].material.SetColor(EmissionColor, DefaultLightColor);
                }
            }
            else if(isUnlocked)
            {
                Animator.SetTrigger(UnlockTrigger);
            }
        }

        private void SetIndicator()
        {
            if (!UseIndicators)
                return;

            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Color color = i < enteredCode.Length ? EnterLightColor : DefaultLightColor;
                Indicators[i].material.SetColor(EmissionColor, color);
            }
        }

        public void SetAccessGranted()
        {
            audioSource.PlayOneShotSoundClip(AccessGrantedSound);
            OnAccessGranted?.Invoke();

            notUsable = true;
            StartCoroutine(OnAccessUpdated(true));
        }

        public void SetAccessDenied()
        {
            audioSource.PlayOneShotSoundClip(AccessDeniedSound);
            OnAccessDenied?.Invoke();

            notUsable = true;
            StartCoroutine(OnAccessUpdated(false));
        }

        IEnumerator OnAccessUpdated(bool granted)
        {
            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Indicators[i].material.DisableKeyword(EmissionKeyword);
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Color color = granted ? EnterLightColor : DefaultLightColor;
                Indicators[i].material.SetColor(EmissionColor, color);
                Indicators[i].material.EnableKeyword(EmissionKeyword);
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Indicators[i].material.DisableKeyword(EmissionKeyword);
            }

            yield return new WaitForSeconds(0.1f);

            for (int i = 0; i < MAX_INDICATORS; i++)
            {
                Indicators[i].material.EnableKeyword(EmissionKeyword);
            }

            yield return new WaitForSeconds(0.5f);

            if (notUsable = granted)
                SwitchBack();
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isUnlocked), isUnlocked }
            };
        }

        public void OnLoad(JToken data)
        {
            isUnlocked = (bool)data[nameof(isUnlocked)];

            if (isUnlocked)
            {
                gameObject.layer = DisabledLayer;
                if (Animator != null) Animator.SetTrigger(ResetTrigger);
                if (LoadCallEvent) OnAccessGranted?.Invoke();

                for (int i = 0; i < MAX_INDICATORS; i++)
                {
                    Indicators[i].material.SetColor(EmissionColor, EnterLightColor);
                }
            }
        }
    }
}