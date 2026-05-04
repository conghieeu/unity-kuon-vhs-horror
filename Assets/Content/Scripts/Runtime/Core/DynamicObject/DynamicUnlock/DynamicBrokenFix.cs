using UnityEngine;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [InspectorHeader("Dynamic Broken Fix")]
    public class DynamicBrokenFix : MonoBehaviour, IDynamicUnlock, IInventorySelector, ISaveable
    {
        public MeshRenderer DisabledRenderer;
        public ItemGuid FixableItem;

        [Header("Hint Text")]
        public bool ShowHintText;
        public GString NoFitHintText;
        public float HintTime = 2f;

        private DynamicObject dynamicObject;
        private bool isFixed;

        private GameManager gameManager;

        private void Awake()
        {
            gameManager = GameManager.Instance;
        }

        private void Start()
        {
            NoFitHintText.SubscribeGloc();
        }

        public void OnTryUnlock(DynamicObject dynamicObject)
        {
            Inventory.Instance.OpenItemSelector(this);
            this.dynamicObject = dynamicObject;
        }

        public void OnInventoryItemSelect(Inventory inventory, InventoryItem selectedItem)
        {
            if (selectedItem.ItemGuid == FixableItem)
            {
                DisabledRenderer.enabled = true;
                inventory.RemoveItem(selectedItem);
                dynamicObject.TryUnlockResult(true);
                isFixed = true;
            }
            else if(ShowHintText)
            {
                gameManager.ShowHintMessage(NoFitHintText, HintTime);
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isFixed), isFixed }
            };
        }

        public void OnLoad(JToken data)
        {
            isFixed = (bool)data[nameof(isFixed)];
            if(isFixed) DisabledRenderer.enabled = true;
        }
    }
}