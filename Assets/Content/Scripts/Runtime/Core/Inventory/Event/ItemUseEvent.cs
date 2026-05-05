using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Struct truyền dữ liệu khi một vật phẩm được Use, bao gồm thông tin vật phẩm và Data custom đi kèm.
    /// </summary>
    public struct ItemUseEvent
    {
        public InventoryItem Item;
        public ItemCustomData UseData;
    }
}