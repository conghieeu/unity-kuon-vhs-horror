using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace UHFPS.Runtime
{
    public class CutsceneModule : ManagerModule
    {
        private PlayableDirector currentCutscene;
        private CutsceneTrigger currentTrigger;

        private CinemachineBrain cinemachineBrain;
        private CinemachineBlendDefinition defaultBlend;
        private CinemachineBlenderSettings defaultBlendAsset;

        public override string Name => "Cutscene";

        public override void OnAwake()
        {
            cinemachineBrain = PlayerPresence.PlayerManager.MainCamera.GetComponent<CinemachineBrain>();
        }

        public void PlayCutscene(CutsceneTrigger cutsceneTrigger)
        {
            currentCutscene = cutsceneTrigger.Cutscene;
            currentTrigger = cutsceneTrigger;

            // freeze player and disable game panels
            PlayerPresence.FreezePlayer(true);
            PlayerPresence.PlayerIsUnlocked = false;
            PlayerPresence.PlayerManager.PlayerItems.DeactivateCurrentItem();
            GameManager.DisableAllGamePanels();

            if (cutsceneTrigger.CutsceneType == CutsceneTrigger.CutsceneTypeEnum.CameraCutscene)
            {
                RunCoroutine(OnPlayCameraCutscene());
            }
            else
            {
                defaultBlend = cinemachineBrain.DefaultBlend;
                defaultBlendAsset = cinemachineBrain.CustomBlends;

                cinemachineBrain.DefaultBlend = cutsceneTrigger.BlendDefinition;
                cinemachineBrain.CustomBlends = cutsceneTrigger.CustomBlendAsset;

                if (currentTrigger.BlendDefinition.Style != CinemachineBlendDefinition.Styles.Cut)
                {
                    // activate player cutscene camera and disable player
                    cutsceneTrigger.CutscenePlayer.SetCutsceneActive(true);
                    PlayerPresence.Player.SetActive(false);

                    // start cutscene
                    RunCoroutine(OnPlayPlayerCutscene(true));
                }
                else
                {
                    // start cutscene
                    RunCoroutine(OnPlayCutCutscene(true));
                }

                currentCutscene.stopped += OnPlayerCutsceneStopped;
            }

            cutsceneTrigger.OnCutsceneStart?.Invoke();
        }

        IEnumerator OnPlayPlayerCutscene(bool blendIn)
        {
            if (currentTrigger.BlendDefinition.Style == CinemachineBlendDefinition.Styles.Cut)
                yield break;

            if (blendIn && !currentTrigger.WaitForBlendIn)
                currentCutscene.Play();
            else if (!blendIn)
            {
                PlayerPresence.Player.SetActive(true);
                currentTrigger.CutscenePlayer.SetCutsceneActive(false);
            }

            yield return new WaitUntil(() => cinemachineBrain.ActiveBlend != null);
            CinemachineBlend blend = cinemachineBrain.ActiveBlend;

            float blendTarget = blendIn ? currentTrigger.WaitForBlendIn ? currentTrigger.BlendInOffset : 1f : 1f;
            while (blend != null && !blend.IsComplete)
            {
                if (blend.BlendWeight >= blendTarget)
                    break;

                yield return null;
            }

            if (blendIn)
            {
                if (currentTrigger.WaitForBlendIn) currentCutscene.Play();
            }
            else
            {
                cinemachineBrain.DefaultBlend = defaultBlend;
                cinemachineBrain.CustomBlends = defaultBlendAsset;
                OnCutsceneEnd();
            }
        }

        IEnumerator OnPlayCutCutscene(bool blendIn)
        {
            if (currentTrigger.BlendDefinition.Style != CinemachineBlendDefinition.Styles.Cut)
                yield break;

            yield return GameManager.StartBackgroundFade(false, fadeSpeed: currentTrigger.CutFadeOutSpeed);

            if (blendIn)
            {
                // activate player cutscene camera and disable player
                currentTrigger.CutscenePlayer.SetCutsceneActive(true);
                PlayerPresence.Player.SetActive(false);
            }
            else
            {
                // reset player cutscene camera and enable player
                PlayerPresence.Player.SetActive(true);
                currentTrigger.CutscenePlayer.SetCutsceneActive(false);
            }

            yield return GameManager.StartBackgroundFade(true, fadeSpeed: currentTrigger.CutFadeInSpeed);

            if (blendIn)
            {
                currentCutscene.Play();
            }
            else
            {
                cinemachineBrain.DefaultBlend = defaultBlend;
                cinemachineBrain.CustomBlends = defaultBlendAsset;
                OnCutsceneEnd();
            }
        }

        IEnumerator OnPlayCameraCutscene()
        {
            GameObject cutsceneCamera = currentTrigger.CutsceneCamera.gameObject;
            yield return PlayerPresence.SwitchCamera(cutsceneCamera, currentTrigger.CutsceneFadeSpeed);

            currentCutscene.Play();

            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds((float)currentCutscene.duration);
            yield return PlayerPresence.SwitchCamera(null, currentTrigger.CutsceneFadeSpeed);

            OnCutsceneEnd();
        }

        private void OnPlayerCutsceneStopped(PlayableDirector _)
        {
            if (currentTrigger.BlendDefinition.Style != CinemachineBlendDefinition.Styles.Cut)
            {
                // apply player position and look
                ApplyPlayerTransform(null);

                // start blend out player cutscene
                cinemachineBrain.DefaultBlend.Time = currentTrigger.BlendOutTime;
                RunCoroutine(OnPlayPlayerCutscene(false));
            }
            else
            {
                ApplyPlayerTransform(currentTrigger.CutEndTransform);
                RunCoroutine(OnPlayCutCutscene(false));
            }

            // dispose stopped event
            if(currentCutscene != null)
                currentCutscene.stopped -= OnPlayerCutsceneStopped;
        }

        private void ApplyPlayerTransform(Transform endTransform)
        {
            Vector3 newPosition = currentTrigger.CutscenePlayer.transform.position;
            Vector3 currentLook = currentTrigger.CutscenePlayer.HeadCamera.transform.eulerAngles;
            Vector3 lookForward = currentTrigger.CutscenePlayer.HeadCamera.transform.forward;

            if (endTransform != null)
            {
                newPosition = endTransform.position;
                currentLook = endTransform.eulerAngles;
            }
            else
            {
                newPosition += lookForward.normalized * 0.5f;
            }

            Vector2 newLook = new(currentLook.y, 0f);
            PlayerPresence.SetPlayerPositionAndLook(newPosition, newLook);
        }

        private void OnCutsceneEnd()
        {
            // reset player
            PlayerPresence.PlayerManager.InteractController.ResetInteract();
            PlayerPresence.PlayerManager.MotionController.ResetMotions();
            PlayerPresence.StateMachine.ToStandingPose();

            // unfreeze player and show main panel
            PlayerPresence.FreezePlayer(false);
            PlayerPresence.PlayerIsUnlocked = true;
            GameManager.ShowPanel(GameManager.PanelType.MainPanel);

            currentTrigger.OnCutsceneEnd?.Invoke();
            currentCutscene = null;
            currentTrigger = null;
        }
    }
}