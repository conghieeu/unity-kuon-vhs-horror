using UnityEngine;
using Newtonsoft.Json.Linq;
using ThunderWire.Attributes;
using UHFPS.Rendering;

namespace UHFPS.Runtime
{
    [InspectorHeader("Raining Set")]
    [HelpBox("Set the raindrop volume reference and set the default raining state.")]
    [Summary("Thiết lập trạng thái hiệu ứng mưa mặc định cho Scene.")]
    public class RainingSet : MonoBehaviour, ISaveable
    {
        [Tooltip("Tham chiếu đến Volume Component của hiệu ứng giọt mưa (Raindrop).")]
        public VolumeComponentReferecne RaindropReference;
        [Tooltip("Trạng thái mặc định (có mưa hay không) khi bắt đầu Scene.")]
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