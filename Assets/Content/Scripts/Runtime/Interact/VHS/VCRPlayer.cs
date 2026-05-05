using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý đầu máy phát băng video (VCR/VHS). Nhận băng, phát video lên CRTMonitor và xử lý các thao tác như Play, Pause, Rewind, Fast Forward.")]
    public class VCRPlayer : MonoBehaviour, IInventorySelector, ISaveable
    {
        public enum DisplayText { Play, Pause, Stop, Eject, FastForwad, Rewind, None }

        [Tooltip("Định danh của vật phẩm cuộn băng (Băng VHS) trong Inventory.")]
        public ItemProperty VHSItem;

        [Tooltip("Animator xử lý hoạt ảnh đóng/mở khay băng.")]
        public Animator animator;

        [Tooltip("Nguồn phát âm thanh cơ học của máy VCR.")]
        public AudioSource audioSource;

        [Tooltip("Đối tượng cuộn băng sẽ hiển thị sau khi lắp vào.")]
        public InteractableItem VHSTape;

        [Tooltip("Vùng va chạm để tương tác bỏ băng vào.")]
        public Collider insertCollider;

        [Tooltip("Thuộc tính Material để đổi nhãn dán của băng.")]
        public string tapeMaterialProperty = "_MainTex";

        [Tooltip("Material phát sáng (Màn hình LED) của máy VCR.")]
        public RendererMaterial emissionMaterial;

        [Tooltip("Tên tham số phát sáng trên Material.")]
        public string emissionKeyword = "_EMISSION";

        [Tooltip("UI hiển thị thời gian phát.")]
        public TMP_Text timeText;

        [Tooltip("UI hiển thị trạng thái (Play, Stop...).")]
        public TMP_Text stateText;

        [Tooltip("Khối cha chứa màn hình hiển thị UI của VCR.")]
        public GameObject displayParent;

        [Tooltip("Biểu tượng (Icon) hiện lên khi băng đã được đưa vào.")]
        public GameObject VHSIcon;

        [Tooltip("Định dạng hiển thị thời gian (Phút:Giây).")]
        public string displayFormat = "<mspace=0.5em>{0:D2}:{1:D2}</mspace>";

        [Tooltip("Màn hình ti-vi (CRT) nhận tín hiệu từ đầu phát này.")]
        public CRTMonitor monitor;

        [Tooltip("Kích thước Texture xuất ra màn hình (Độ phân giải Video).")]
        public Vector2Int outputTextureSize = new Vector2Int(500, 350);

        [Tooltip("Tốc độ tua ngược (Rewind).")]
        public float rewindSpeed;

        [Tooltip("Tốc độ tua nhanh (Fast Forward).")]
        public float fastForwardSpeed;

        [Tooltip("Tốc độ khởi động máy quay (Winding) trước khi đạt vận tốc tối đa.")]
        public float windingStartupSpeed;

        [Tooltip("Thời gian chờ trước khi tua bắt đầu.")]
        public float timeBeforeWinding;

        [Tooltip("Trigger kích hoạt hoạt ảnh đưa băng vào.")]
        public string insertTrigger = "Insert";

        [Tooltip("Trigger kích hoạt hoạt ảnh đẩy băng ra.")]
        public string ejectTrigger = "Eject";

        [Tooltip("Trigger kích hoạt đóng cửa khay băng.")]
        public string closeCoverTrigger = "Close";

        [Tooltip("Ký tự đại diện cho trạng thái Tua đi (Fast Forward) trên Text UI.")]
        public string fastForwardSymbol = ">";

        [Tooltip("Ký tự đại diện cho Tua lại (Rewind).")]
        public string rewindSymbol = "<";

        [Tooltip("Ký tự đại diện cho Phát (Play).")]
        public string playSymbol = "€";

        [Tooltip("Ký tự đại diện cho Dừng (Stop).")]
        public string stopSymbol = "€";

        [Tooltip("Ký tự đại diện cho Tạm dừng (Pause).")]
        public string pauseSymbol = "™";

        [Tooltip("Ký tự đại diện cho Đẩy băng (Eject).")]
        public string ejectSymbol = "®";

        [Tooltip("Âm thanh đưa băng vào.")]
        public SoundClip tapeInsert;

        [Tooltip("Âm thanh đẩy băng ra.")]
        public SoundClip tapeEject;

        [Tooltip("Âm thanh đang phát băng.")]
        public SoundClip play;

        [Tooltip("Âm thanh dừng băng.")]
        public SoundClip stop;

        [Tooltip("Âm thanh tua băng.")]
        public SoundClip rewind;

        private RenderTexture outputTexture;
        private Collider tapeCollider;
        private string tapeCustomData;

        private double tapeDuration;
        private double currentTime;
        private float windingMod;

        public bool isStarted;
        public bool isPoweredOn;

        public bool IsPlaying => isStarted && !isPaused;

        private bool canInsert;
        private bool canEject;
        private bool isPaused;
        private bool isEnded;
        private bool isWinding;

        private void Awake()
        {
            tapeCollider = VHSTape.GetComponent<Collider>();
            canInsert = true;
            canEject = false;

            outputTexture = new RenderTexture(outputTextureSize.x, outputTextureSize.y, 16);
            outputTexture.Create();
        }

        private void Start()
        {
            SetPower(isPoweredOn);
        }

        private void Update()
        {
            if (isStarted && !isWinding)
            {
                UpdateClipTime(monitor.videoPlayer.time);
                if ((tapeDuration - currentTime) <= 0.5f && !isEnded)
                {
                    monitor.videoPlayer.Pause();
                    SetDisplayText(DisplayText.Stop);
                    monitor.SetDisplayTexture(DisplayTexture.Stop);
                    audioSource.SetSoundClip(stop, play: true);
                    isEnded = true;
                }
            }
        }

        private void UpdateClipTime(double time, bool setCurrent = true)
        {
            int seconds = ((int)time) % 60;
            int minutes = Mathf.FloorToInt((int)time / 60f);
            timeText.text = string.Format(displayFormat, minutes, seconds);
            if(setCurrent) currentTime = time;
        }

        public void PowerOnOff()
        {
            if (isWinding) return;
            SetPower(!isPoweredOn);
        }

        public void SetPower(bool power)
        {
            if (isWinding) return;
            if (isPoweredOn = power)
            {
                if (canEject)
                {
                    SetDisplayText(DisplayText.Stop, true);
                    monitor.SetDisplayTexture(DisplayTexture.Stop);
                }
                else
                {
                    SetDisplayText(DisplayText.None, true);
                    monitor.SetDisplayTexture(DisplayTexture.NoTape);
                }

                insertCollider.gameObject.SetActive(canInsert);
                if(emissionMaterial.IsAssigned)
                    emissionMaterial.ClonedMaterial.EnableKeyword(emissionKeyword);
            }
            else
            {
                monitor.SetDisplayTexture(DisplayTexture.NoSignal);
                monitor.videoPlayer.Pause();
                isPaused = isStarted;

                if (emissionMaterial.IsAssigned)
                    emissionMaterial.ClonedMaterial.DisableKeyword(emissionKeyword);
            }

            displayParent.SetActive(isPoweredOn);
        }

        public void SetDisplayText(DisplayText? display, bool resetTimer = false)
        {
            stateText.gameObject.SetActive(display.HasValue);
            stateText.text = display switch
            {
                DisplayText.Play => playSymbol,
                DisplayText.Pause => pauseSymbol,
                DisplayText.Stop => stopSymbol,
                DisplayText.Eject => ejectSymbol,
                DisplayText.Rewind => rewindSymbol,
                DisplayText.FastForwad => fastForwardSymbol,
                DisplayText.None => "",
                _ => ""
            };

            if (resetTimer) timeText.text = "-- : --";
        }

        public void StartPausePlayback()
        {
            if ((tapeDuration - currentTime) <= 0.5f || !canEject || isEnded)
                return;

            if (isWinding)
            {
                StopAllCoroutines();
                SetDisplayText(DisplayText.Stop);
                monitor.SetDisplayTexture(DisplayTexture.Stop);
                audioSource.SetSoundClip(stop, play: true);
                monitor.videoPlayer.time = currentTime;
                monitor.videoPlayer.Prepare();
                isWinding = false;
                isPaused = true;
            }
            else if (!isStarted || isPaused)
            {
                SetDisplayText(DisplayText.Play);
                monitor.SetVideoInput(outputTexture);
                audioSource.SetSoundClip(play, play: true);
                monitor.videoPlayer.Play();
                isStarted = true;
                isPaused = false;
            }
            else if(!isPaused)
            {
                SetDisplayText(DisplayText.Pause);
                monitor.videoPlayer.Pause();
                isPaused = true;
            }
        }

        public void PlayPlayback()
        {
            if ((tapeDuration - currentTime) <= 0.5f || !canEject || isEnded || isWinding)
                return;

            if (!isStarted || isPaused)
            {
                SetDisplayText(DisplayText.Play);
                monitor.SetVideoInput(outputTexture);
                audioSource.SetSoundClip(play, play: true);
                monitor.videoPlayer.Play();
                isStarted = true;
                isPaused = false;
            }
        }

        public void PausePlayback()
        {
            if ((tapeDuration - currentTime) <= 0.5f || !canEject || isEnded)
                return;

            if (isWinding)
            {
                StopAllCoroutines();
                SetDisplayText(DisplayText.Stop);
                monitor.SetDisplayTexture(DisplayTexture.Stop);
                audioSource.SetSoundClip(stop, play: true);
                monitor.videoPlayer.time = currentTime;
                monitor.videoPlayer.Prepare();
                isWinding = false;
                isPaused = true;
            }
            else if (!isPaused)
            {
                SetDisplayText(DisplayText.Pause);
                monitor.videoPlayer.Pause();
                isPaused = true;
            }
        }

        public void Rewind()
        {
            if(isStarted && !isWinding && (tapeDuration - currentTime) < tapeDuration - 0.5f)
            {
                monitor.videoPlayer.Pause();
                StartCoroutine(OnRewind());
                isWinding = true;
                isEnded = false;
            }
        }

        public void FastForward()
        {
            if (isStarted && !isWinding && (tapeDuration - currentTime) > 0.5f)
            {
                monitor.videoPlayer.Pause();
                StartCoroutine(OnFastForward());
                isWinding = true;
            }
        }

        public void InsertVHSTape()
        {
            if (isPoweredOn && canInsert)
                Inventory.Instance.OpenItemSelector(this);
        }

        public void EjectVHSTape()
        {
            if (isPoweredOn && canEject && !isWinding)
            {
                monitor.videoPlayer.Pause();
                StartCoroutine(OnEject());
                isWinding = true;
            }
        }

        public void OnInventoryItemSelect(Inventory inventory, InventoryItem selectedItem)
        {
            if(selectedItem.ItemGuid == VHSItem)
            {
                var customData = selectedItem.CustomData.GetJson();
                if(customData.TryGetValue("texture", out JToken texture))
                {
                    string texturePath = texture.ToString();
                    Texture2D tapeTexture = Resources.Load<Texture2D>(texturePath);
                    MeshRenderer tapeRenderer = VHSTape.GetComponentInChildren<MeshRenderer>();
                    tapeRenderer.material.SetTexture(tapeMaterialProperty, tapeTexture);
                }

                if (customData.TryGetValue("video", out JToken video))
                {
                    string videoPath = video.ToString();
                    VideoClip videoClip = Resources.Load<VideoClip>(videoPath);
                    monitor.PrepareVideo(videoClip, outputTexture);
                    tapeDuration = videoClip.length;
                }

                VHSTape.gameObject.SetActive(true);
                insertCollider.gameObject.SetActive(false);
                tapeCollider.enabled = false;

                tapeCustomData = selectedItem.CustomData.JsonData;
                inventory.RemoveItem(selectedItem);

                canInsert = false;
                canEject = false;

                StartCoroutine(OnInsert());
            }
            else
            {
                Debug.Log("Selected item is not a VHS Tape!");
            }
        }

        public void OnTakeVHSTape()
        {
            insertCollider.gameObject.SetActive(true);
            VHSTape.gameObject.SetActive(false);
            animator.SetTrigger(closeCoverTrigger);
            canInsert = true;
        }

        #region Enumerators
        IEnumerator OnInsert()
        {
            animator.SetTrigger(insertTrigger);
            audioSource.SetSoundClip(tapeInsert, play: true);

            yield return new WaitForAnimatorClip(animator, insertTrigger);

            SetDisplayText(DisplayText.Stop);
            monitor.SetDisplayTexture(DisplayTexture.Stop);

            VHSIcon.SetActive(true);
            VHSTape.gameObject.SetActive(false);
            canEject = true;
        }

        IEnumerator OnEject()
        {
            yield return OnRewind();

            SetDisplayText(DisplayText.Eject, true);
            monitor.SetDisplayTexture(DisplayTexture.Stop);
            yield return new WaitForSeconds(1f);

            VHSTape.gameObject.SetActive(true);
            tapeCollider.enabled = false;
            animator.SetTrigger(ejectTrigger);
            audioSource.SetSoundClip(tapeEject, play: true);

            yield return new WaitForAnimatorClip(animator, ejectTrigger);

            SetDisplayText(null);
            monitor.SetDisplayTexture(null);
            monitor.SetDisplayTexture(DisplayTexture.NoTape);

            VHSIcon.SetActive(false);
            VHSTape.ItemCustomData.JsonData = tapeCustomData;
            tapeCustomData = null;
            tapeCollider.enabled = true;

            canInsert = true;
            canEject = false;
            isStarted = false;
            isWinding = false;
            isPaused = false;
            isEnded = false;
        }

        IEnumerator OnRewind()
        {
            if (currentTime > 0)
            {
                windingMod = 0;
                monitor.SetDisplayTexture(DisplayTexture.Rewind);
                SetDisplayText(DisplayText.Rewind);
                audioSource.SetSoundClip(rewind, play: true);
                yield return new WaitForSeconds(timeBeforeWinding);

                while (currentTime > 0)
                {
                    UpdateClipTime(currentTime, false);
                    windingMod = Mathf.MoveTowards(windingMod, 1, Time.deltaTime * windingStartupSpeed);
                    currentTime -= Time.deltaTime * rewindSpeed * windingMod;
                    yield return null;
                }

                audioSource.SetSoundClip(stop, play: true);
            }

            monitor.SetDisplayTexture(DisplayTexture.Stop);
            SetDisplayText(DisplayText.Stop);

            currentTime = 0;
            monitor.videoPlayer.time = currentTime;
            isStarted = false;
            isWinding = false;
            isPaused = true;
        }

        IEnumerator OnFastForward()
        {
            if (currentTime < tapeDuration)
            {
                windingMod = 0;
                monitor.SetDisplayTexture(DisplayTexture.FastForward);
                SetDisplayText(DisplayText.FastForwad);
                audioSource.SetSoundClip(rewind, play: true);
                yield return new WaitForSeconds(timeBeforeWinding);

                while (currentTime < tapeDuration)
                {
                    UpdateClipTime(currentTime, false);
                    windingMod = Mathf.MoveTowards(windingMod, 1, Time.deltaTime * windingStartupSpeed);
                    currentTime += Time.deltaTime * fastForwardSpeed * windingMod;
                    yield return null;
                }
            }

            monitor.SetDisplayTexture(DisplayTexture.Stop);
            SetDisplayText(DisplayText.Stop);
            audioSource.SetSoundClip(stop, play: true);

            currentTime = tapeDuration;
            monitor.videoPlayer.time = currentTime;
            isWinding = false;
            isPaused = true;
        }
        #endregion

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isPoweredOn), isPoweredOn },
                { nameof(canInsert), canInsert },
                { nameof(canEject), canEject },
                { nameof(isEnded), isEnded },
                { "playtime", currentTime },
                { "customData", !string.IsNullOrEmpty(tapeCustomData) 
                    ? JObject.Parse(tapeCustomData) : new JObject() }
            };
        }

        public void OnLoad(JToken data)
        {
            bool isPowered = (bool)data[nameof(isPoweredOn)];
            SetPower(isPowered);
            isPaused = isPowered;

            canInsert = (bool)data[nameof(canInsert)];
            canEject = (bool)data[nameof(canEject)];
            isEnded = (bool)data[nameof(isEnded)];
            currentTime = (double)data["playtime"];
            tapeCustomData = data["customData"].ToString();

            if (!string.IsNullOrEmpty(tapeCustomData) && data["customData"]["texture"] != null && data["customData"]["video"] != null)
            {
                string texturePath = data["customData"]["texture"].ToString();
                string videoPath = data["customData"]["video"].ToString();

                Texture2D tapeTexture = Resources.Load<Texture2D>(texturePath);
                VideoClip videoClip = Resources.Load<VideoClip>(videoPath);
                MeshRenderer tapeRenderer = VHSTape.GetComponentInChildren<MeshRenderer>();

                if (tapeTexture) tapeRenderer.material.SetTexture(tapeMaterialProperty, tapeTexture);
                if (videoClip)
                {
                    tapeDuration = videoClip.length;
                    monitor.PrepareVideo(videoClip, outputTexture);
                }
            }

            UpdateClipTime(currentTime);
            SetDisplayText(DisplayText.Stop);
            monitor.SetDisplayTexture(DisplayTexture.Stop);
        }
    }
}
