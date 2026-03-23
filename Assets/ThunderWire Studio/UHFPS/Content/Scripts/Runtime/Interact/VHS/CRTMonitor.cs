using UnityEngine;
using UnityEngine.Video;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public enum DisplayTexture { NoSignal, NoTape, Stop, Rewind, FastForward }

    [InspectorHeader("CRT Monitor (for VCR Player)")]
    public class CRTMonitor : MonoBehaviour, ISaveable
    {
        public VideoPlayer videoPlayer;
        public AudioSource videoAudio;

        [Header("Materials")]
        public RendererMaterial display;
        public Material poweredOnMaterial;
        public Material poweredOffMaterial;
        public string materialProperty = "_MainTex";

        [Header("Display Textures")]
        public Texture2D noSignal;
        public Texture2D insertTape;
        public Texture2D stop;
        public Texture2D rewind;
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