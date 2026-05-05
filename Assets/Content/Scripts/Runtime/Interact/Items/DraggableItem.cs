using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
    [Summary("Component cho phép vật thể có thể bị người chơi cầm nắm và kéo đi.")]
    public class DraggableItem : SaveableBehaviour, IOnDragStart, IOnDragEnd
    {
        [Tooltip("Khoảng cách tối thiểu và tối đa mà vật thể có thể được đưa lại gần/ra xa.")]
        public MinMax ZoomDistance;
        [Tooltip("Khoảng cách cầm tối đa. Nếu vật thể vượt quá khoảng cách này (ví dụ bị kẹt), vật thể sẽ tự động bị thả ra.")]
        public float MaxHoldDistance = 4f;

        [Tooltip("Bật/tắt âm thanh khi vật thể va chạm.")]
        public bool EnableImpactSound = true;
        [Tooltip("Mảng chứa các âm thanh va chạm.")]
        public AudioClip[] ImpactSounds;
        [Tooltip("Âm lượng va chạm tối thiểu và tối đa. Âm thanh sẽ phát nếu âm lượng tính toán lớn hơn mức tối thiểu.")]
        public MinMax ImpactVolume;
        [Tooltip("Hệ số nhân cho âm lượng va chạm. Giá trị càng cao, âm thanh va chạm càng lớn.")]
        public float VolumeModifier;
        [Tooltip("Thời gian (giây) để giới hạn giữa các lần phát hiện va chạm liên tiếp.")]
        public float NextImpact = 0.1f;

        [Tooltip("Bật/tắt âm thanh khi vật thể trượt trên bề mặt.")]
        public bool EnableSlidingSound = true;
        [Tooltip("Góc tối thiểu giữa va chạm và chuyển động để phát hiện trượt. Gần 0 = trượt, Lớn hơn 0 = tĩnh.")]
        public float MinSlidingFactor = 5f;
        [Tooltip("Phạm vi tốc độ để tính toán âm lượng trượt. Giá trị càng cao, vật thể cần di chuyển càng nhanh để đạt âm lượng 1.")]
        public float SlidingVelocityRange = 5f;
        [Tooltip("Hệ số nhân cho âm lượng trượt. Giá trị càng cao, âm lượng trượt càng lớn.")]
        public float SlidingVolumeModifier = 5f;
        [Tooltip("Tốc độ giảm dần âm lượng khi vật thể ngừng trượt.")]
        public float VolumeFadeOffSpeed = 5f;

        [Tooltip("Sự kiện gọi ra khi người chơi bắt đầu kéo/cầm vật thể.")]
        public UnityEvent OnDragStarted;
        [Tooltip("Sự kiện gọi ra khi thả vật thể.")]
        public UnityEvent OnDragEnded;

        [Tooltip("Trạng thái hiện tại xem vật thể có đang va chạm hay không.")]
        public bool Collision;

        private Rigidbody rigid;
        private AudioSource audioSource;

        private float impactTime;
        private int lastImpact;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = 0f;
            audioSource.loop = true;
            audioSource.spatialBlend = 1f;
            audioSource.playOnAwake = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Collision = true;
            if (!EnableImpactSound) return;

            float newVolume = collision.relativeVelocity.magnitude / VolumeModifier;
            if (newVolume < ImpactVolume.RealMin) return;

            newVolume = Mathf.Clamp(newVolume, ImpactVolume.RealMin, ImpactVolume.RealMax);
            if (impactTime <= 0) OnObjectImpact(newVolume);
        }

        private void OnCollisionExit(Collision collision)
        {
            Collision = false;
        }

        private void OnCollisionStay(Collision collision)
        {
            Collision = true;
        }

        private void Update()
        {
            if (impactTime > 0) impactTime -= Time.deltaTime;
            if (!EnableSlidingSound) return;

            float velMagnitude = rigid.linearVelocity.magnitude;
            float velMagnitudeNormalized = rigid.linearVelocity.normalized.magnitude;

            if (Collision && velMagnitudeNormalized > MinSlidingFactor)
            {
                float slidingVolume = Mathf.InverseLerp(0f, SlidingVelocityRange, velMagnitude);
                if (!audioSource.isPlaying) audioSource.Play();
                audioSource.volume = Mathf.Clamp01(slidingVolume * SlidingVolumeModifier);
            }
            else
            {
                audioSource.volume = Mathf.MoveTowards(audioSource.volume, 0f, Time.deltaTime * VolumeFadeOffSpeed);
                if (audioSource.isPlaying && audioSource.volume <= 0) audioSource.Stop();
            }
        }

        private void OnObjectImpact(float volume)
        {
            lastImpact = GameTools.RandomUnique(0, ImpactSounds.Length, lastImpact);
            AudioClip audioClip = ImpactSounds[lastImpact];
            AudioSource.PlayClipAtPoint(audioClip, transform.position, volume);
        }

        public void OnDragStart()
        {
            OnDragStarted?.Invoke();
        }

        public void OnDragEnd()
        {
            OnDragEnded?.Invoke();
        }

        public override StorableCollection OnSave()
        {
            return new StorableCollection().WithTransform(transform);
        }

        public override void OnLoad(JToken data)
        {
            data.LoadTransform(transform);
        }
    }
}