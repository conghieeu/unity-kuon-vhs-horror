using System.Collections;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using ThunderWire.Attributes;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;
using Random = UnityEngine.Random;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(SphereCollider))]
    public class EMFAnomaly : MonoBehaviour, ISaveable
    {
        [Serializable]
        public struct EMFSpike
        {
            public float Milligauss;
            [Range(0f, 1f)]
            public float Probability;
        }

        public enum AnomalyDetect { Once, Random, Always, Event }

        public AnomalyDetect AnomalyDetection = AnomalyDetect.Once;
        public EMFSpike[] EMFSpikes;

        [Range(0f, 1f)] public float Weight = 1f;
        [Range(0f, 1f)] public float StartingWeight = 1f;

        public float SpikeRate;
        public float MinDistance;
        public float NoiseAmount;
        public float NoiseSpeed;

        [Range(0f, 1f)]
        public float TimerStartRange;
        public float WeightFadeSpeed;

        public MinMax EMFDetectionTime;
        public MinMax AnomalyResetTime;

        public UnityEvent OnAnomalyStart;
        public UnityEvent OnAnomalyEnd;
        public UnityEvent OnOutOfRange;
        public UnityEvent OnTimerStart;

        public int SpikesCount;
        public MinMax MilligaussRange;
        [MinMaxRange(0f, 1f)]
        public Vector2 ProbabilityRange;

        public float AnomalyWeight => weight;

        private bool detectionStarted;
        private bool timerStarted;
        private bool anomalyEnded;

        private float weight = 1f;
        private float targetWeight;

        private float milligauss;
        private float spikeRate;

        private void Awake()
        {
            if (!SaveGameManager.GameWillLoad && AnomalyDetection == AnomalyDetect.Event)
                weight = StartingWeight;
        }

        public float GetMilligauss()
        {
            if (anomalyEnded)
                return 0f;

            if (!detectionStarted)
            {
                weight = 1f;
                OnAnomalyStart?.Invoke();
                detectionStarted = true;
            }

            float noise = Mathf.PerlinNoise1D(Time.time * NoiseSpeed) * NoiseAmount;
            return milligauss + noise;
        }

        public void OutOfRange()
        {
            OnOutOfRange?.Invoke();
        }

        public void StartTimer()
        {
            if (timerStarted || AnomalyDetection == AnomalyDetect.Always)
                return;

            StartCoroutine(AnomalyTime());
            OnTimerStart?.Invoke();
            timerStarted = true;
        }

        /// <summary>
        /// Call this if you want to show the anomaly.
        /// </summary>
        public void ShowAnomaly()
        {
            if (AnomalyDetection != AnomalyDetect.Event)
                return;

            StopAllCoroutines();
            StartCoroutine(SetAnomaly(true));

            anomalyEnded = false;
        }

        /// <summary>
        /// Call this if you want to hide the anomaly.
        /// </summary>
        public void HideAnomaly()
        {
            if (AnomalyDetection != AnomalyDetect.Event)
                return;

            StopAllCoroutines();
            StartCoroutine(SetAnomaly(false));

            OnAnomalyEnd?.Invoke();
        }

        private void Update()
        {
            if (!detectionStarted)
                return;

            if (spikeRate <= 0)
            {
                milligauss = GetRandomSpike() * weight * Weight;
                spikeRate = SpikeRate;
            }
            else spikeRate -= Time.deltaTime;
        }

        IEnumerator SetAnomaly(bool show)
        {
            targetWeight = show ? 1f : 0f;

            while (Mathf.Abs(targetWeight - weight) > 0.01f)
            {
                weight = Mathf.MoveTowards(weight, targetWeight, Time.deltaTime * WeightFadeSpeed);
                yield return null;
            }

            weight = targetWeight;

            if (!show)
            {
                anomalyEnded = true;
                detectionStarted = false;
            }
        }

        IEnumerator AnomalyTime()
        {
            float detectionTime = EMFDetectionTime.Random();
            yield return new WaitForSeconds(detectionTime);

            targetWeight = 0f;
            while (Mathf.Abs(targetWeight - weight) > 0.01f)
            {
                weight = Mathf.MoveTowards(weight, targetWeight, Time.deltaTime * WeightFadeSpeed);
                yield return null;
            }

            weight = targetWeight;
            OnAnomalyEnd?.Invoke();
            anomalyEnded = true;
            detectionStarted = false;

            yield return ResetAnomaly();
        }

        IEnumerator ResetAnomaly()
        {
            if (AnomalyDetection == AnomalyDetect.Random)
            {
                float resetTime = AnomalyResetTime.Random();
                yield return new WaitForSeconds(resetTime);

                anomalyEnded = false;
                targetWeight = 1f;

                while (Mathf.Abs(targetWeight - weight) > 0.01f)
                {
                    weight = Mathf.MoveTowards(weight, targetWeight, Time.deltaTime * WeightFadeSpeed);
                    yield return null;
                }

                weight = targetWeight;
                timerStarted = false;
            }
        }

        public float GetRandomSpike()
        {
            float totalProbability = EMFSpikes.Sum(x => x.Probability);
            float randomValue = Random.Range(0f, totalProbability);
            float cumulativeProbability = 0f;

            foreach (var spike in EMFSpikes)
            {
                cumulativeProbability += spike.Probability;
                if (randomValue <= cumulativeProbability)
                {
                    return spike.Milligauss;
                }
            }

            return 0f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green.Alpha(0.25f);
            Gizmos.DrawWireSphere(transform.position, MinDistance);
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(targetWeight), targetWeight },
                { nameof(anomalyEnded), anomalyEnded },
                { nameof(timerStarted), timerStarted },
                { nameof(detectionStarted), detectionStarted }
            };
        }

        public void OnLoad(JToken data)
        {
            targetWeight = (float)data[nameof(targetWeight)];
            anomalyEnded = (bool)data[nameof(anomalyEnded)];
            timerStarted = (bool)data[nameof(timerStarted)];
            detectionStarted = (bool)data[nameof(detectionStarted)];
            weight = targetWeight;

            if(AnomalyDetection == AnomalyDetect.Random && timerStarted)
            {
                if (detectionStarted) StartCoroutine(AnomalyTime());
                else StartCoroutine(ResetAnomaly());
            }
        }
    }
}