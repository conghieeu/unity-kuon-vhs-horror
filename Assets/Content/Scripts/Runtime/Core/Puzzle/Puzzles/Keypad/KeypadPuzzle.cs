using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using TMPro;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    [Summary("Hệ thống giải đố Bảng mã số (Keypad). Hỗ trợ bấm trực tiếp (UseInteract) hoặc chuyển góc nhìn Camera (PuzzleBase).")]
    public class KeypadPuzzle : PuzzleBase, ISaveable
    {
        public enum Button { Number0, Number1, Number2, Number3, Number4, Number5, Number6, Number7, Number8, Number9, Remove, Confirm }

        [Tooltip("Mã PIN (mật khẩu) đúng để mở khóa.")]
        public string AccessCode = "0000";

        [Tooltip("Số lượng chữ số tối đa có thể nhập.")]
        public uint MaxCodeLength = 4;

        [Tooltip("Bật: Nhấn trực tiếp các nút trên Keypad ngoài màn hình chơi. Tắt: Phải tương tác để chuyển sang góc nhìn cận cảnh mới bấm được.")]
        public bool UseInteract = false;

        [Tooltip("Thời gian chờ (giây) sau khi bấm Confirm để reset lại chữ trên màn hình.")]
        public float AccessUpdateWaitTime = 1f;

        [Tooltip("Thời gian (giây) không tương tác trước khi Keypad tự tắt đèn màn hình (Chỉ dùng khi UseInteract = true).")]
        public float SleepWaitTime = 10f;

        [Tooltip("Text 3D (TextMeshPro) trên mô hình để hiển thị số vừa bấm.")]
        public TextMeshPro DisplayTextMesh;

        [Tooltip("Dòng chữ hiển thị khi nhập ĐÚNG mã.")]
        public string GrantedText = "ACCESS GRANTED";

        [Tooltip("Dòng chữ hiển thị khi nhập SAI mã.")]
        public string DeniedText = "ACCESS DENIED";

        [Tooltip("Kích thước chữ của thông báo Granted/Denied.")]
        public float TextFontSize = 20;

        [Tooltip("Kích thước chữ của các con số khi đang nhập mã.")]
        public float CodeFontSize = 25;

        [Tooltip("Màu chữ mặc định khi nhập số.")]
        public Color DefaultColor = Color.white;

        [Tooltip("Màu chữ khi nhập đúng mã.")]
        public Color GrantedColor = Color.green;

        [Tooltip("Màu chữ khi nhập sai mã.")]
        public Color DeniedColor = Color.red;

        [Tooltip("Sử dụng nguồn sáng (Light Component) để báo trạng thái.")]
        public bool UseLights = true;

        [Tooltip("Đèn Light tham chiếu trên mô hình.")]
        public Light KeypadLight;

        [Tooltip("Màu đèn báo khi đúng mã.")]
        public Color GrantedLightColor = Color.green;

        [Tooltip("Màu đèn báo khi sai mã.")]
        public Color DeniedLightColor = Color.red;

        [Tooltip("Bật/tắt vật liệu phát sáng (Emission) của mô hình khi đang nhập mã.")]
        public bool UseEmission = true;

        [Tooltip("Mesh Renderer của mô hình để đổi Material Emission.")]
        public MeshRenderer KeypadRenderer;

        [Tooltip("Tên tham số Shader để kích hoạt Emission (Mặc định: _EMISSION).")]
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Âm thanh khi bấm một phím bất kỳ trên bảng.")]
        public SoundClip ButtonPressSound;

        [Tooltip("Âm thanh báo mở khóa thành công.")]
        public SoundClip AccessGrantedSound;

        [Tooltip("Âm thanh báo nhập sai mã.")]
        public SoundClip AccessDeniedSound;

        [Tooltip("Sự kiện gọi ra khi nhập mã đúng (VD: Mở cửa, Thêm item).")]
        public UnityEvent OnAccessGranted;

        [Tooltip("Sự kiện gọi ra khi nhập mã sai.")]
        public UnityEvent OnAccessDenied;

        [Tooltip("Sự kiện gọi ra mỗi khi bấm một phím số (Truyền ra số vừa bấm).")]
        public UnityEvent<int> OnButtonPressed;

        /// <summary>
        /// Granted status of the code lock.
        /// </summary>
        public bool AccessGranted => accessGranted;

        private AudioSource audioSource;
        private string displayText = "";
        private bool accessGranted;
        private bool notUsable;

        private bool confirmPressed;
        private float sleepTime;

        public override void Awake()
        {
            base.Awake();
            audioSource = GetComponent<AudioSource>();
            DisplayTextMesh.color = DefaultColor;
            DisplayTextMesh.fontSize = CodeFontSize;
            DisplayTextMesh.text = "";
            SetRendererEmission(false);

            if (UseInteract)
            {
                foreach (var collider in CollidersEnable)
                {
                    collider.enabled = true;
                }
            }
        }

        public void OnPressButton(Button button)
        {
            if (accessGranted || notUsable)
                return;

            audioSource.PlayOneShotSoundClip(ButtonPressSound);
            confirmPressed = false;

            if (button == Button.Confirm)
            {
                if(displayText == AccessCode)
                {
                    SetAccessGranted();
                    accessGranted = true;
                }
                else
                {
                    SetAccessDenied();
                    displayText = "";
                }

                confirmPressed = true;
            }
            else if(button == Button.Remove)
            {
                if (displayText.Length > 0)
                {
                    displayText = displayText.Remove(displayText.Length - 1);
                    DisplayTextMesh.text = displayText;
                }
            }
            else if(displayText.Length < MaxCodeLength)
            {
                displayText += (int)button;
                DisplayTextMesh.text = displayText;
                OnButtonPressed?.Invoke((int)button);
            }

            if (UseInteract)
            {
                sleepTime = SleepWaitTime;
                SetRendererEmission(true);
            }
        }

        public override void InteractStart()
        {
            if(!UseInteract && !accessGranted) 
                base.InteractStart();
        }

        public override void Update()
        {
            if (!UseInteract) base.Update();
            else if (!isActive && confirmPressed)
            {
                if(sleepTime > 0) sleepTime -= Time.deltaTime;
                else
                {
                    displayText = "";
                    DisplayTextMesh.text = "";
                    SetRendererEmission(false);
                    confirmPressed = false;
                }
            }
        }

        public override void OnBackgroundFade()
        {
            base.OnBackgroundFade();

            if (!UseInteract && UseEmission)
            {
                if(isActive) SetRendererEmission(true);
                else SetRendererEmission(false);
            }
        }

        public void SetAccessGranted()
        {
            audioSource.PlayOneShotSoundClip(AccessGrantedSound);
            OnAccessGranted?.Invoke();

            DisplayTextMesh.text = GrantedText;
            DisplayTextMesh.color = GrantedColor;
            DisplayTextMesh.fontSize = TextFontSize;

            if (UseLights)
            {
                KeypadLight.color = GrantedLightColor;
                KeypadLight.enabled = true;
            }

            notUsable = true;
            StartCoroutine(OnAccessUpdated(true));
        }

        public void SetAccessDenied()
        {
            audioSource.PlayOneShotSoundClip(AccessDeniedSound);
            OnAccessDenied?.Invoke();

            DisplayTextMesh.text = DeniedText;
            DisplayTextMesh.color = DeniedColor;
            DisplayTextMesh.fontSize = TextFontSize;

            if (UseLights)
            {
                KeypadLight.color = DeniedLightColor;
                KeypadLight.enabled = true;
            }

            notUsable = true;
            StartCoroutine(OnAccessUpdated(false));
        }

        private void SetRendererEmission(bool state)
        {
            if (!UseEmission) 
                return;

            if(state) KeypadRenderer.material.EnableKeyword(EmissionKeyword);
            else KeypadRenderer.material.DisableKeyword(EmissionKeyword);
        }

        IEnumerator OnAccessUpdated(bool granted)
        {
            yield return new WaitForSeconds(AccessUpdateWaitTime);

            if (UseLights) KeypadLight.enabled = false;

            displayText = "";
            DisplayTextMesh.text = "";
            DisplayTextMesh.color = DefaultColor;
            DisplayTextMesh.fontSize = CodeFontSize;

            if (granted)
            {
                if (!UseInteract) SwitchBack();
                else
                {
                    SetRendererEmission(false);
                    confirmPressed = false;
                    sleepTime = 0f;
                }

                DisableInteract();
            }

            notUsable = false;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(accessGranted), accessGranted }
            };
        }

        public void OnLoad(JToken data)
        {
            accessGranted = (bool)data[nameof(accessGranted)];
            if(accessGranted) DisableInteract();

            if (UseLights) KeypadLight.enabled = false;
            SetRendererEmission(false);
            DisplayTextMesh.text = "";
            displayText = "";
        }
    }
}