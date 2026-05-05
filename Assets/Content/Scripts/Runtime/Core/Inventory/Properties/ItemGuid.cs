using System;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Trường tham chiếu đến ID của một Item trong Inventory Database, thường được dùng để cấu hình chìa khóa mở cửa, vật phẩm nhận được...")]
    public class ItemGuid : ItemField { }
}