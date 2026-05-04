using UnityEngine;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Autosave Trigger")]
    [HelpBox("When the player walks through a trigger that has this component attached. The game will automatically be saved.")]
    public class AutosaveTrigger : MonoBehaviour, ISaveable
    {
        private bool isSaved;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !isSaved)
            {
                TriggerAutosave();
            }
        }

        public void TriggerAutosave()
        {
            if (isSaved || SaveGameManager.GameWillLoad) 
                return;

            isSaved = true;
            SaveGameManager.SaveGame(true);
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isSaved), isSaved }
            };
        }

        public void OnLoad(JToken data)
        {
            isSaved = (bool)data[nameof(isSaved)];
        }
    }
}