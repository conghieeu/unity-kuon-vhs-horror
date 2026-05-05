using System;
using System.Collections;
using UnityEngine;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Quản lý Radio dò đài, hỗ trợ nhiều kênh (Channel) tương ứng với từng tần số/vị trí dò (Tuner).")]
    public class Radio : MonoBehaviour, ISaveable
    {
        public enum ChannelTypeEnum { Once, Loop }

        [Serializable]
        public sealed class RadioChannel
        {
            public ChannelTypeEnum ChannelType;
            public float TunerPosition;
            public AudioClip ChannelAudio;
            [Range(0f, 1f)] public float Volume = 1f;

            [NonSerialized, HideInInspector]
            public bool isPlayed = false;
            [NonSerialized, HideInInspector]
            public float playbackTime = 0f;
        }

        [Tooltip("Danh sách cấu hình các đài phát (Tần số, Âm thanh, v.v.).")]
        public RadioChannel[] RadioChannels;

        [Tooltip("Thành phần xử lý tương tác núm vặn dò đài.")]
        public RadioTuner RadioTuner;

        [Tooltip("Thanh kim (Rod) chỉ định vị trí rà đài.")]
        public Transform TunerRod;

        [Tooltip("Trục không gian mà thanh kim di chuyển.")]
        public Axis TunerMoveAxis;

        [Tooltip("Giới hạn di chuyển tối thiểu/tối đa của thanh kim.")]
        public MinMax TunerLimits;

        [Tooltip("Khoảng dung sai tần số (Range) mà một kênh phát sóng có thể được nhận.")]
        public float TuneRange = 0.005f;

        [Tooltip("Tên tham số làm sáng màn hình Radio.")]
        public string EmissionKeyword = "_EMISSION";

        [Tooltip("Nguồn phát âm thanh chính của Radio.")]
        public AudioSource AudioSource;

        [Tooltip("Âm thanh nhiễu (Static) khi không có tín hiệu đài.")]
        public SoundClip RadioStatic;

        [Tooltip("Danh sách tiếng lách cách rít lên ngẫu nhiên khi vặn núm dò đài.")]
        public AudioClip[] TuneSounds;
        [Range(0f, 1f)] public float TuneVolume = 1f;

        private MeshRenderer meshRenderer;
        private RadioChannel lastChannel;

        private int lastTune;
        private bool isSwitched;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            lastChannel = null;
        }

        public void SwitchRadio(bool state)
        {
            isSwitched = state;

            if (!state)
            {
                if (lastChannel != null)
                    lastChannel.playbackTime = AudioSource.time;

                StopAllCoroutines();
                AudioSource.Pause();
                meshRenderer.material.DisableKeyword(EmissionKeyword);
            }
            else
            {
                HandleTunerPosition();

                if (lastChannel == null)
                {
                    AudioSource.SetSoundClip(RadioStatic);
                    AudioSource.loop = true;
                    AudioSource.Play();
                }
                else
                {
                    PlayChannel(lastChannel, false);
                }

                meshRenderer.material.EnableKeyword(EmissionKeyword);
            }
        }

        public void UpdateTuner(float t)
        {
            Vector3 position = TunerRod.localPosition;
            float nextMove = Mathf.Lerp(TunerLimits.min, TunerLimits.max, t);

            position = position.SetComponent(TunerMoveAxis, nextMove);
            TunerRod.localPosition = position;

            if (!isSwitched)
                return;

            bool flag = false;
            foreach (var channel in RadioChannels)
            {
                float min = channel.TunerPosition - TuneRange;
                float max = channel.TunerPosition + TuneRange;

                if(nextMove > min && nextMove < max)
                {
                    if(PlayChannel(channel, true))
                    {
                        lastChannel = channel;
                        flag = true;
                    }

                    break;
                }
            }

            if (!flag)
            {
                lastTune = -1;
                if(lastChannel != null)
                {
                    StopAllCoroutines();
                    lastChannel.playbackTime = AudioSource.time;
                    lastChannel = null;
                }

                if (AudioSource.clip != RadioStatic.audioClip)
                {
                    AudioSource.SetSoundClip(RadioStatic);
                    AudioSource.loop = true;
                    AudioSource.Play();
                }
            }
        }

        private void HandleTunerPosition()
        {
            Vector3 position = TunerRod.localPosition;
            float tunePos = position.Component(TunerMoveAxis);

            bool flag = false;
            foreach (var channel in RadioChannels)
            {
                float min = channel.TunerPosition - TuneRange;
                float max = channel.TunerPosition + TuneRange;

                if (tunePos > min && tunePos < max)
                {
                    lastChannel = channel;
                    flag = true;
                    break;
                }
            }

            if (!flag) lastChannel = null;
        }

        private bool PlayChannel(RadioChannel channel, bool tune)
        {
            if (channel == null)
                return false;

            if (AudioSource.clip != channel.ChannelAudio)
            {
                StopAllCoroutines();
                if (channel.ChannelType == ChannelTypeEnum.Once)
                {
                    if (channel.isPlayed) return false;
                    else StartCoroutine(SetPlayed(channel));
                }

                if (tune)
                {
                    lastTune = GameTools.RandomUnique(0, TuneSounds.Length, lastTune);
                    AudioClip tuneSound = TuneSounds[lastTune];
                    AudioSource.PlayClipAtPoint(tuneSound, transform.position, TuneVolume);
                }

                AudioSource.clip = channel.ChannelAudio;
                AudioSource.volume = channel.Volume;
                AudioSource.time = channel.playbackTime;
                AudioSource.loop = channel.ChannelType == ChannelTypeEnum.Loop;
                AudioSource.Play();
            }
            else if(!AudioSource.isPlaying)
            {
                AudioSource.UnPause();
            }

            return true;
        }

        IEnumerator SetPlayed(RadioChannel channel)
        {
            yield return new WaitForEndOfFrame();
            yield return new WaitUntil(() => AudioSource.isPlaying);

            float clipTime = channel.ChannelAudio.length - 0.01f;
            yield return new WaitUntil(() => AudioSource.time >= clipTime);

            channel.isPlayed = true;
        }

        public StorableCollection OnSave()
        {
            StorableCollection channelData = new();
            for (int i = 0; i < RadioChannels.Length; i++)
            {
                var channel = RadioChannels[i];
                channelData.Add("channel_" + i, channel.isPlayed);
            }

            return new StorableCollection()
            {
                { "channelData", channelData },
                { "tunerAngle", RadioTuner.TunerAngle }
            };
        }

        public void OnLoad(JToken data)
        {
            for (int i = 0; i < RadioChannels.Length; i++)
            {
                string name = "channel_" + i;
                bool isPlayed = data["channelData"][name].ToObject<bool>();
                RadioChannels[i].isPlayed = isPlayed;
            }

            float tunerAngle = (float)data["tunerAngle"];
            RadioTuner.TunerAngle = tunerAngle;
        }
    }
}