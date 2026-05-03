using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;
using UHFPS.Rendering;
using ThunderWire.Attributes;
using static UHFPS.Runtime.JumpscareTrigger;

namespace UHFPS.Runtime
{
    [InspectorHeader("Jumpscare Manager")]
    [Docs("https://docs.twgamesdev.com/uhfps/guides/jumpscares")]
    public class JumpscareManager : Singleton<JumpscareManager>
    {
        public Image DirectImage;

        [Header("Direct Jumpscare Settings")]
        [Range(1f, 2f)] public float ImageMaxScale = 2f;
        [Range(0f, 1f)] public float ImageScaleTime = 0.5f;

        [Header("Fear Effect Settings")]
        [Range(0f, 1f)] public float FearIntensityDuration = 0.2f;
        [Range(0f, 1f)] public float FearSpeedDuration = 0.2f;

        [Header("Tentacles Default Settings")]
        [Range(0.1f, 3f)] public float TentaclesDefaultSpeed = 1f;
        [Range(-0.2f, 0.2f)] public float TentaclesDefaultPosition = 0f;

        [Header("Tentacles Animation Settings")]
        public float TentaclesMoveSpeed = 1f;
        public float TentaclesAnimationSpeed = 1f;
        public float TentaclesFadeInSpeed = 1f;
        public float TentaclesFadeOutSpeed = 1f;

        [Header("Camera Wobble Settings")]
        public float WobbleLossRate = 0.5f;

        private PlayerPresenceManager playerPresence;
        private GameManager gameManager;

        private PlayerManager playerManager;
        private LookController lookController;
        private JumpscareDirect jumpscareDirect;

        private WobbleMotion wobbleMotion;
        private FearTentancles fearTentancles;
        private GameObject directModel;

        private bool isDirectJumpscare;
        private bool isPlayerLocked;
        private bool influenceFear;
        private bool tentaclesFaded;
        private bool showTentacles;

        private float directDuration;
        private float directTimer;
        private float fearDuration;
        private float fearTimer;

        private void Awake()
        {
            playerPresence = GetComponent<PlayerPresenceManager>();
            gameManager = GetComponent<GameManager>();

            playerManager = playerPresence.PlayerManager;
            lookController = playerPresence.LookController;
            jumpscareDirect = playerManager.GetComponent<JumpscareDirect>();
            fearTentancles = gameManager.GetStack<FearTentancles>();
        }

        private void Start()
        {
            wobbleMotion = playerManager.MotionController.GetDefaultMotion<WobbleMotion>();
        }

        private void Update()
        {
            if (isDirectJumpscare)
            {
                directTimer -= Time.deltaTime;

                if (directTimer <= 0f)
                {
                    if(directModel != null)
                    {
                        directModel.SetActive(false);
                        directModel = null;
                    }
                    else
                    {
                        DirectImage.gameObject.SetActive(false);
                        DirectImage.rectTransform.localScale = Vector3.one;
                    }

                    isDirectJumpscare = false;
                }
                else if(directModel == null)
                {
                    float directTimeOffset = directDuration * ImageScaleTime;
                    float directValue = Mathf.InverseLerp(directDuration - directTimeOffset, 0f, directTimer);
                    float directScale = Mathf.Lerp(1f, ImageMaxScale, directValue);
                    DirectImage.rectTransform.localScale = Vector3.one * directScale;
                }
            }

            if (influenceFear && showTentacles)
            {
                if (fearTentancles.EffectFade.value < 1f && !tentaclesFaded)
                {
                    float fade = fearTentancles.EffectFade.value;
                    fearTentancles.EffectFade.value = Mathf.MoveTowards(fade, 1f, Time.deltaTime * TentaclesFadeInSpeed);
                }
                else if(fearTimer > 0f)
                {
                    float fearSpeedOffset = fearDuration - fearDuration * FearSpeedDuration;
                    float fearIntensityOffset = fearDuration - fearDuration * FearIntensityDuration;
                    fearTimer -= Time.deltaTime;

                    if(fearTimer <= fearSpeedOffset)
                    {
                        float speed = fearTentancles.TentaclesSpeed.value;
                        fearTentancles.TentaclesSpeed.value = Mathf.Lerp(speed, TentaclesDefaultSpeed, Time.deltaTime * TentaclesAnimationSpeed);
                    }

                    if (fearTimer <= fearIntensityOffset)
                    {
                        float position = fearTentancles.TentaclesPosition.value;
                        fearTentancles.TentaclesPosition.value = Mathf.Lerp(position, TentaclesDefaultPosition, Time.deltaTime * TentaclesMoveSpeed);
                    }

                    tentaclesFaded = true;
                }
                else if(tentaclesFaded)
                {
                    if(fearTentancles.EffectFade.value > 0f)
                    {
                        float fade = fearTentancles.EffectFade.value;
                        fearTentancles.EffectFade.value = Mathf.MoveTowards(fade, 0f, Time.deltaTime * TentaclesFadeOutSpeed);
                    }
                    else
                    {
                        fearTimer = 0f;
                        fearDuration = 0f;
                        fearTentancles.EffectFade.value = 0f;
                        tentaclesFaded = false;
                        showTentacles = false;
                        influenceFear = false;
                    }
                }
            }
        }

        public void StartJumpscareEffect(JumpscareTrigger jumpscare)
        {
            if (jumpscare.InfluenceWobble)
            {
                float amplitude = jumpscare.WobbleAmplitudeGain;
                float frequency = jumpscare.WobbleFrequencyGain;
                float duration = jumpscare.WobbleDuration;

                wobbleMotion.ApplyWobble(amplitude, frequency, duration);
            }

            if(jumpscare.JumpscareType == JumpscareTypeEnum.Direct)
            {
                if (jumpscare.DirectType == DirectTypeEnum.Image)
                {
                    DirectImage.sprite = jumpscare.JumpscareImage;
                    DirectImage.gameObject.SetActive(true);

                    directDuration = jumpscare.DirectDuration;
                    directTimer = jumpscare.DirectDuration;
                    isDirectJumpscare = true;
                }
                else if (jumpscare.DirectType == DirectTypeEnum.Model)
                {
                    jumpscareDirect.ShowDirectJumpscare(jumpscare.JumpscareModelID, jumpscare.DirectDuration);
                }
            }
            else if((jumpscare.JumpscareType == JumpscareTypeEnum.Indirect || jumpscare.JumpscareType == JumpscareTypeEnum.Audio) && jumpscare.LookAtJumpscare)
            {
                isPlayerLocked = jumpscare.LockPlayer;
                lookController.LerpRotation(jumpscare.LookAtTarget, jumpscare.LookAtDuration, isPlayerLocked);
                if (isPlayerLocked) gameManager.FreezePlayer(true);
            }

            if (jumpscare.InfluenceFear)
            {
                fearTentancles.TentaclesPosition.value = Mathf.Lerp(0f, 0.2f, jumpscare.TentaclesIntensity);
                fearTentancles.TentaclesSpeed.value = jumpscare.TentaclesSpeed;
                fearTentancles.VignetteStrength.value = jumpscare.VignetteStrength;
                fearDuration = jumpscare.FearDuration;
                fearTimer = jumpscare.FearDuration;
                tentaclesFaded = false;
                showTentacles = true;
                influenceFear = true;
            }
        }

        public void EndJumpscareEffect()
        {
            if (!isPlayerLocked)
                return;

            gameManager.FreezePlayer(false);
            lookController.LookLocked = false;
            isPlayerLocked = false;
        }
    }
}