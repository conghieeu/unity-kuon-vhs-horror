using UnityEngine;
using ThunderWire.Attributes;
using TMPro;
using static UHFPS.Input.InputManager;

namespace UHFPS.Runtime
{
    public struct InteractInfo
    {
        public string ObjectName;
        public InteractContext[] Contexts;
    }

    public sealed class InteractContext
    {
        public InputReference InputAction;
        public string InteractName;
    }

    [InspectorHeader("Interact Info Panel")]
    [Summary("Bảng hiển thị thông tin tương tác khi người chơi nhìn vào một vật thể có thể tương tác (IInteractable).")]
    public class InteractInfoPanel : MonoBehaviour
    {
        [Tooltip("CanvasGroup để xử lý việc fade hiện/ẩn bảng.")]
        public CanvasGroup CanvasGroup;
        [Tooltip("Text hiển thị tên của vật thể đang tương tác.")]
        public TMP_Text InteractName;
        [Tooltip("Danh sách các nút bấm tương tác khả dụng.")]
        public InteractButton[] InteractButtons;

        [Header("Fading")]
        [Tooltip("Tốc độ fade hiện/ẩn.")]
        public float FadeSpeed = 5f;

        private BindingPath[] bindingPaths;
        private bool fadeState;

        private void Update()
        {
            CanvasGroup.alpha = Mathf.MoveTowards(CanvasGroup.alpha, fadeState ? 1 : 0, Time.deltaTime * FadeSpeed);
        }

        public void ShowInfo(InteractInfo interactInfo)
        {
            // initialize binding paths
            if (bindingPaths == null || bindingPaths.Length <= 0)
                bindingPaths = new BindingPath[interactInfo.Contexts.Length];

            // interact name
            if (!string.IsNullOrEmpty(interactInfo.ObjectName))
                InteractName.text = interactInfo.ObjectName;

            // interact buttons
            for (int i = 0; i < interactInfo.Contexts.Length; i++)
            {
                var context = interactInfo.Contexts[i];
                var button = InteractButtons[i];

                if(context != null)
                {
                    if (bindingPaths[i] == null)
                        bindingPaths[i] = GetBindingPath(context.InputAction.ActionName, context.InputAction.BindingIndex);

                    string name = context.InteractName;
                    var glyph = bindingPaths[i].inputGlyph;
                    button.SetButton(name, glyph.GlyphSprite, glyph.GlyphScale);
                }
                else
                {
                    button.HideButton();
                }
            }

            // show info
            fadeState = true;
        }

        public void HideInfo()
        {
            bindingPaths = new BindingPath[0];
            fadeState = false;
        }
    }
}