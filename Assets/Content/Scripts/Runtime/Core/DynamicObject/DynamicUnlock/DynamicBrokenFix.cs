using UnityEngine;
using ThunderWire.Attributes;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [InspectorHeader("Dynamic Broken Fix")]
    [Summary("Quản lý việc sửa chữa một Dynamic Object bị hỏng bằng cách yêu cầu người chơi chọn đúng vật phẩm từ Inventory.")]
    public class DynamicBrokenFix : MonoBehaviour, IDynamicUnlock, IInventorySelector, ISaveable
    {
        [Tooltip("Renderer (mô hình) của bộ phận sẽ được hiển thị sau khi đã sửa chữa xong.")]
        public MeshRenderer DisabledRenderer;
        [Tooltip("ID của vật phẩm cần thiết trong Inventory để sửa chữa đối tượng này.")]
        public ItemGuid FixableItem;

        [Header("Hint Text")]
        [Tooltip("Hiển thị thông báo khi người chơi chọn sai vật phẩm.")]
        public bool ShowHintText;
        [Tooltip("Nội dung thông báo (có hỗ trợ Localization) khi người chơi chọn sai vật phẩm.")]
        public GString NoFitHintText;
        [Tooltip("Thời gian hiển thị thông báo trên màn hình (giây).")]
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