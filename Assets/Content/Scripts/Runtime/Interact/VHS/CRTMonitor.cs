using UnityEngine;
using UnityEngine.Video;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public enum DisplayTexture { NoSignal, NoTape, Stop, Rewind, FastForward }

    [InspectorHeader("CRT Monitor (for VCR Player)")]
    [Summary("Màn hình ti-vi (CRT) đi kèm với đầu phát VCR. Hiển thị thông báo tĩnh hoặc nhận tín hiệu Video.")]
    public class CRTMonitor : MonoBehaviour, ISaveable
    {
        [Tooltip("Trình phát Video (VideoPlayer) truyền hình ảnh lên màn hình.")]
        public VideoPlayer videoPlayer;

        [Tooltip("Nguồn phát âm thanh của Video.")]
        public AudioSource videoAudio;

        [Header("Materials")]
        [Tooltip("Lớp vật liệu màn hình của TV.")]
        public RendererMaterial display;

        [Tooltip("Material khi Bật màn hình (Màn sáng).")]
        public Material poweredOnMaterial;

        [Tooltip("Material khi Tắt màn hình (Màn đen).")]
        public Material poweredOffMaterial;

        [Tooltip("Tham số Textures trên Material.")]
        public string materialProperty = "_MainTex";

        [Header("Display Textures")]
        [Tooltip("Ảnh hiển thị: Không có tín hiệu.")]
        public Texture2D noSignal;

        [Tooltip("Ảnh hiển thị: Yêu cầu đút băng.")]
        public Texture2D insertTape;

        [Tooltip("Ảnh hiển thị: Băng đang dừng.")]
        public Texture2D stop;

        [Tooltip("Ảnh hiển thị: Tua lùi.")]
        public Texture2D rewind;

        [Tooltip("Ảnh hiển thị: Tua đi.")]
        public Texture2D fastForward;

        private RenderTexture inputTexture;
        private DisplayTexture prevTexture;
        private bool isPoweredOn;

        public bool IsPoweredOn => isPoweredOn;

        private void Awake()
        {
            videoPlayer.playOnAwake = false;
            videoAudio.playOnAwake = false;
            videoAudio.spatialBlend = 1;
        }

        public void PowerOnOff()
        {
            SetPower(!isPoweredOn);
        }

        public void SetPower(bool power)
        {
            if (isPoweredOn = power)
            {
                display.ClonedMaterial = poweredOnMaterial;
                if (!inputTexture) SetDisplayTexture(prevTexture);
                else SetVideoInput(inputTexture);
                videoAudio.enabled = true;
            }
            else
            {
                display.ClonedMaterial = poweredOffMaterial;
                videoAudio.enabled = false;
            }
        }

        public void PrepareVideo(VideoClip clip, RenderTexture outputTexture)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.isLooping = false;

            videoPlayer.clip = clip;
            videoPlayer.SetTargetAudioSource(0, videoAudio);
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.controlledAudioTrackCount = 1;
            videoPlayer.targetTexture = outputTexture;
            videoPlayer.Prepare();
        }

        public void SetDisplayTexture(DisplayTexture? displayTex)
        {
            if (!displayTex.HasValue)
            {
                inputTexture = null;
            }
            else
            {
                prevTexture = displayTex.Value;
                if (isPoweredOn)
                {
                    switch (displayTex)
                    {
                        case DisplayTexture.NoSignal:
                            display.ClonedMaterial.SetTexture(materialProperty, noSignal);
                            break;
                        case DisplayTexture.NoTape:
                            display.ClonedMaterial.SetTexture(materialProperty, insertTape);
                            break;
                        case DisplayTexture.Stop:
                            display.ClonedMaterial.SetTexture(materialProperty, stop);
                            break;
                        case DisplayTexture.Rewind:
                            display.ClonedMaterial.SetTexture(materialProperty, rewind);
                            break;
                        case DisplayTexture.FastForward:
                            display.ClonedMaterial.SetTexture(materialProperty, fastForward);
                            break;
                    }
                }
            }
        }

        public void SetVideoInput(RenderTexture texture)
        {
            display.ClonedMaterial.SetTexture(materialProperty, texture);
            prevTexture = texture != null ? DisplayTexture.Stop : DisplayTexture.NoTape;
            inputTexture = texture;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isPoweredOn), isPoweredOn },
                { "displayState", (int)prevTexture }
            };
        }

        public void OnLoad(JToken data)
        {
            bool isPowered = (bool)data[nameof(isPoweredOn)];
            DisplayTexture displayTexture = (DisplayTexture)(int)data["displayState"];
            prevTexture = displayTexture;
            SetPower(isPowered);
        }
    }
}