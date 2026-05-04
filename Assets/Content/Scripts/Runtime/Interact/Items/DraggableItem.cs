using UnityEngine;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
    public class DraggableItem : SaveableBehaviour, IOnDragStart, IOnDragEnd
    {
        [Tooltip("Minimum and maximum distance to which the object can be zoomed.")]
        public MinMax ZoomDistance;
        [Tooltip("Maximum hold distance at which the object will be out of range and will be dropped.")]
        public float MaxHoldDistance = 4f;

        public bool EnableImpactSound = true;
        [Tooltip("Array of the impact sounds.")]
        public AudioClip[] ImpactSounds;
        [Tooltip("Minimum and maximum impact volume. The impact will be played if the calculated volume is greater than the minimum impact volume.")]
        public MinMax ImpactVolume;
        [Tooltip("Modifier that is multiplied with the impact volume. Higher value = louder impact volume")]
        public float VolumeModifier;
        [Tooltip("Time at which the next impact will be detected.")]
        public float NextImpact = 0.1f;

        public bool EnableSlidingSound = true;
        [Tooltip("Minimum angle between the collision and the motion at which the sliding is detected. Near 0 = sliding, More than 0 = static")]
        public float MinSlidingFactor = 5f;
        [Tooltip("Velocity range at which the sliding volume is calculated. Higher value = faster movement is required to achieve volume 1")]
        public float SlidingVelocityRange = 5f;
        [Tooltip("Modifier that is multiplied with the sliding volume. Higher value = louder sliding volume")]
        public float SlidingVolumeModifier = 5f;
        [Tooltip("Speed at which the volume is faded when the sliding stops.")]
        public float VolumeFadeOffSpeed = 5f;

        public UnityEvent OnDragStarted;
        public UnityEvent OnDragEnded;

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