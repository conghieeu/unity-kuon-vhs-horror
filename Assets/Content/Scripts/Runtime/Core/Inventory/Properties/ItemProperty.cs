using System;

using ThunderWire.Attributes;

namespace UHFPS.Runtime 
{
    [Serializable]
    [Summary("Trường tham chiếu đến một property (thuộc tính) cụ thể của Item, thường đi kèm với ItemGuid.")]
    public class ItemProperty : ItemField { }
}