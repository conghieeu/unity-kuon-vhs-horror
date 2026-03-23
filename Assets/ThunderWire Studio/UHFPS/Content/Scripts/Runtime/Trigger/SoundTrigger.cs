using UnityEngine;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Sound Trigger")]
    public class SoundTrigger : MonoBehaviour, IInteractStart, ISaveable
    {
        public enum TriggerTypeEnum { Interact, Trigger, Event }
        public enum UseTypeEnum { Once, MoreTimes }

        public TriggerTypeEnum TriggerType;
        public UseTypeEnum UseType;

        [Header("Sound")]
        public SoundClip TriggerSound;

        private bool isPlayed = false;

        public void InteractStart()
        {
            if(TriggerType == TriggerTypeEnum.Interact)
            {
                PlaySound();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Trigger && other.CompareTag("Player"))
            {
                PlaySound();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (TriggerType == TriggerTypeEnum.Trigger && UseType == UseTypeEnum.MoreTimes && other.CompareTag("Player"))
            {
                isPlayed = false;
            }
        }

        public void PlaySound()
        {
            if (isPlayed)
                return;

            GameTools.PlayOneShot3D(transform.position, TriggerSound);
            isPlayed = true;
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isPlayed), isPlayed }
            };
        }

        public void OnLoad(JToken data)
        {
            isPlayed = (bool)data[nameof(isPlayed)];
        }
    }
}