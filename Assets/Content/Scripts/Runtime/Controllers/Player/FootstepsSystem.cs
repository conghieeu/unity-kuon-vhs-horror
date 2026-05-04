using UnityEngine;
using UHFPS.Scriptable;
using UHFPS.Tools;
using static UHFPS.Scriptable.SurfaceDefinitionSet;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class FootstepsSystem : PlayerComponent
    {
        public enum FootstepStyleEnum { Timed, HeadBob, Animation }

        public SurfaceDefinitionSet SurfaceDefinitionSet;
        public FootstepStyleEnum FootstepStyle;
        public SurfaceDetection SurfaceDetection;
        public LayerMask FootstepsMask;

        public float StepPlayerVelocity = 0.1f;
        public float JumpStepAirTime = 0.1f;

        public float WalkStepTime = 1f;
        public float RunStepTime = 1f;
        public float LandStepTime = 1f;
        [Range(-1f, 1f)]
        public float HeadBobStepWave = -0.9f;

        [Range(0, 1)] public float WalkingVolume = 1f;
        [Range(0, 1)] public float RunningVolume = 1f;
        [Range(0, 1)] public float LandVolume = 1f;

        public SurfaceDefinition CurrentSurface;

        private AudioSource audioSource;
        private Collider surfaceUnder;

        private bool isWalking;
        private bool isRunning;

        private int lastStep;
        private int lastLandStep;

        private float stepTime;
        private bool waveStep;

        private float airTime;
        private bool wasInAir;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            surfaceUnder = FootstepsMask.CompareLayer(hit.gameObject.layer)
                ? hit.collider : null;
        }

        private void Update()
        {
            if (!isEnabled)
                return;

            if(stepTime > 0f)
                stepTime -= Time.deltaTime;

            if (PlayerStateMachine.StateGrounded)
            {
                if (surfaceUnder != null)
                {
                    CurrentSurface = SurfaceDefinitionSet.GetSurface(surfaceUnder.gameObject, transform.position, SurfaceDetection);
                    if (FootstepStyle != FootstepStyleEnum.Animation && CurrentSurface != null)
                        EvaluateFootsteps(CurrentSurface);
                }
            }
            else
            {
                airTime += Time.deltaTime;
                wasInAir = true;
            }
        }

        private void EvaluateFootsteps(SurfaceDefinition surface)
        {
            float playerVelocity = PlayerCollider.velocity.magnitude;
            bool isCrouching = PlayerStateMachine.IsCurrent(PlayerStateMachine.CROUCH_STATE);
            isWalking = PlayerStateMachine.IsCurrent(PlayerStateMachine.WALK_STATE);
            isRunning = PlayerStateMachine.IsCurrent(PlayerStateMachine.RUN_STATE);

            if (isCrouching)
                return;

            if (FootstepStyle == FootstepStyleEnum.Timed)
            {
                if(wasInAir)
                {
                    if(airTime >= LandStepTime) 
                        PlayFootstep(surface, true);

                    airTime = 0;
                    wasInAir = false;
                }
                else if((isWalking || isRunning) && playerVelocity > StepPlayerVelocity && stepTime <= 0)
                {
                    PlayFootstep(surface, false);
                    stepTime = isWalking ? WalkStepTime : isRunning ? RunStepTime : 0f;
                }
            }
            else if (FootstepStyle == FootstepStyleEnum.HeadBob)
            {
                if (wasInAir)
                {
                    if (airTime >= LandStepTime)
                        PlayFootstep(surface, true);

                    airTime = 0;
                    wasInAir = false;
                }
                else if (playerVelocity > StepPlayerVelocity)
                {
                    float yWave = PlayerManager.MotionController.BobWave;
                    if (yWave < HeadBobStepWave && !waveStep)
                    {
                        PlayFootstep(surface, false);
                        waveStep = true;
                    }
                    else if (yWave > HeadBobStepWave && waveStep)
                    {
                        waveStep = false;
                    }
                }
            }
        }

        private void PlayFootstep(SurfaceDefinition surface, bool isLand)
        {
            if (!isLand && surface.SurfaceFootsteps.Count > 0)
            {
                lastStep = GameTools.RandomUnique(0, surface.SurfaceFootsteps.Count, lastStep);
                AudioClip footstep = surface.SurfaceFootsteps[lastStep];

                float volume = surface.FootstepsVolume;
                float volumeScale = (isWalking ? WalkingVolume : isRunning ? RunningVolume : 0f) * volume;

                audioSource.PlayOneShot(footstep, volumeScale);
            }
            else if (surface.SurfaceLandSteps.Count > 0)
            {
                lastLandStep = GameTools.RandomUnique(0, surface.SurfaceLandSteps.Count, lastLandStep);
                AudioClip landStep = surface.SurfaceLandSteps[lastLandStep];

                float volume = surface.LandStepsVolume;
                float volumeScale = LandVolume * volume;

                audioSource.PlayOneShot(landStep, volumeScale);
            }
        }

        public void PlayFootstep(bool runningStep)
        {
            if (surfaceUnder == null)
                return;

            CurrentSurface = SurfaceDefinitionSet.GetSurface(surfaceUnder.gameObject, transform.position, SurfaceDetection);
            if (CurrentSurface != null)
            {
                isWalking = !runningStep;
                isRunning = runningStep;
                PlayFootstep(CurrentSurface, false);
            }
        }

        public void PlayLandSteps()
        {
            if (surfaceUnder == null)
                return;

            CurrentSurface = SurfaceDefinitionSet.GetSurface(surfaceUnder.gameObject, transform.position, SurfaceDetection);
            if (CurrentSurface != null) PlayFootstep(CurrentSurface, true);
        }
    }
}