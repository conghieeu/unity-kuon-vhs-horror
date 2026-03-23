using System;
using UHFPS.Input;
using UnityEngine;
using UnityEngine.UI;

namespace UHFPS.Runtime
{
    public class HotspotKey : MonoBehaviour
    {
        public InputReference UseKey;
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