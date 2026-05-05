using System;
using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/interactions#changing-interact-reticle")]
    [Summary("Ghi đè biểu tượng (Reticle) ở giữa màn hình khi người chơi nhìn vào hoặc tương tác với vật thể này.")]
    public class CustomInteractReticle : MonoBehaviour, IReticleProvider
    {
        [Tooltip("Biểu tượng (Reticle) thay thế khi nhìn vào vật thể.")]
        public Reticle OverrideReticle;

        [Tooltip("Biểu tượng (Reticle) thay thế khi đang giữ nút tương tác.")]
        public Reticle HoldReticle;

        [Tooltip("Sử dụng biểu tượng Hold động dựa trên giá trị của một biến (ReflectionField).")]
        public bool DynamicHoldReticle;

        [Tooltip("Tham chiếu tới biến bool để xác định trạng thái đang giữ (Hold).")]
        public ReflectionField DynamicHold;

        public (Type, Reticle, bool) OnProvideReticle()
        {
            bool hold = DynamicHoldReticle && DynamicHold.Value;
            Reticle reticle = hold ? HoldReticle : OverrideReticle;
            return (null, reticle, hold);
        }
    }
}