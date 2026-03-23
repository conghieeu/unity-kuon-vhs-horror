using System;
using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Feature Disable Trigger")]
    public class FeatureDisableTrigger : MonoBehaviour
    {
        private GameManager gameManager;
        private PlayerStateMachine player;

        [Flags]
        public enum Features
        {
            None = 0,
            SaveGame = 1 << 0,
            LoadGame = 1 << 1,
            Jump = 1 << 2,
            Run = 1 << 3,
            Crouch = 1 << 4
        }

        public Features FeaturesToDisable = Features.None;

        private void Awake()
        {
            gameManager = GameManager.Instance;
            player = gameManager.PlayerPresence.StateMachine;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetFeature(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetFeature(true);
            }
        }

        private void SetFeature(bool state)
        {
            bool saveDisable = FeaturesToDisable.HasFlag(Features.SaveGame);
            bool loadDisable = FeaturesToDisable.HasFlag(Features.LoadGame);

            if(saveDisable || loadDisable)
            {
                bool saveState = !saveDisable || state;
                bool loadState = !loadDisable || state;
                gameManager.SetSaveInteractable(saveState, loadState);
            }

            if (FeaturesToDisable.HasFlag(Features.Jump))
                player.SetStateEnabled(PlayerStateMachine.JUMP_STATE, state);

            if (FeaturesToDisable.HasFlag(Features.Run))
                player.SetStateEnabled(PlayerStateMachine.RUN_STATE, state);

            if (FeaturesToDisable.HasFlag(Features.Crouch))
                player.SetStateEnabled(PlayerStateMachine.CROUCH_STATE, state);
        }
    }
}