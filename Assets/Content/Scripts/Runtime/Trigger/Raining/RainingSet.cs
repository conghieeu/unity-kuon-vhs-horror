using UnityEngine;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;
using UHFPS.Rendering;

namespace UHFPS.Runtime
{
    [InspectorHeader("Raining Set")]
    [HelpBox("Set the raindrop volume reference and set the default raining state.")]
    public class RainingSet : MonoBehaviour, ISaveable
    {
        public VolumeComponentReferecne RaindropReference;
        public bool DefaultState = true;

        private RainingModule rainingModule;
        private Raindrop raindrop;

        private void Awake()
        {
            rainingModule = GameManager.Module<RainingModule>();
            raindrop = (Raindrop)RaindropReference.GetVolumeComponent();
        }

        private void Start()
        {
            if (SaveGameManager.GameWillLoad && SaveGameManager.GameStateExist)
                return;

            rainingModule.SetRaining(raindrop, DefaultState);
        }

        public StorableCollection OnSave()
        {
            bool raining = raindrop.Raining.value >= 0.5f;
            return new StorableCollection()
            {
                { "raining", raining }
            };
        }

        public void OnLoad(JToken data)
        {
            bool raining = (bool)data["raining"];
            rainingModule.SetRaining(raindrop, raining);
        }
    }
}