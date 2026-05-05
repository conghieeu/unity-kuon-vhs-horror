using System;
using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;
using UHFPS.Input;

namespace UHFPS.Runtime
{
    [Summary("Hiển thị biểu tượng phím bấm tương ứng cho một Hotspot (điểm tương tác) cụ thể.")]
    public class HotspotKey : MonoBehaviour
    {
        [Tooltip("Tham chiếu phím bấm sẽ được hiển thị.")]
        public InputReference UseKey;
        [Tooltip("Image UI sẽ hiển thị icon của phím bấm.")]
        public Image HotspotSprite;

        private IDisposable disposable;

        private void Awake()
        {
            if (!InputManager.HasReference)
                return;

            disposable = InputManager.GetBindingPath(UseKey.ActionName, UseKey.BindingIndex)
                .GlyphSpriteObservable.Subscribe(icon => HotspotSprite.sprite = icon);
        }

        private void OnDestroy()
        {
            disposable.Dispose();
        }
    }
}