using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;
using static UHFPS.Input.InputManager;

namespace UHFPS.Runtime
{
    [System.Serializable]
    public sealed class ControlsContext
    {
        public InputReference InputAction;
        public GString InteractName;

        public void SubscribeGloc()
        {
            InteractName.SubscribeGloc();
        }
    }

    [InspectorHeader("Interact Info Panel")]
    public class ControlsInfoPanel : MonoBehaviour
    {
        public InteractButton[] InteractButtons;

        private readonly Stack<ControlsContext[]> contextsQueue = new();
        private BindingPath[] bindingPaths;

        public void ShowInfo(ControlsContext[] contexts)
        {
            // add contexts to the queue
            contextsQueue.Push(contexts);

            // initialize binding paths
            if (bindingPaths == null || bindingPaths.Length <= 0 || bindingPaths.Length < contexts.Length)
                bindingPaths = new BindingPath[contexts.Length];

            // interact buttons
            if (InteractButtons != null)
            {
                for (int i = 0; i < InteractButtons.Length; i++)
                {
                    var button = InteractButtons[i];
                    if (button == null) continue;

                    if (i < contexts.Length)
                    {
                        var context = contexts[i];
                        if (context != null)
                        {
                            if (bindingPaths[i] == null)
                                bindingPaths[i] = GetBindingPath(context.InputAction.ActionName, context.InputAction.BindingIndex);

                            string name = context.InteractName;
                            if (name != null && bindingPaths[i] != null)
                            {
                                var glyph = bindingPaths[i].inputGlyph;
                                button.SetButton(name, glyph.GlyphSprite, glyph.GlyphScale);
                            }
                        }
                    }
                    else
                    {
                        button.HideButton();
                    }
                }
            }

            gameObject.SetActive(true);
        }

        public void HideInfo()
        {
            if (contextsQueue.Count > 0)
            {
                contextsQueue.Pop();

                if(contextsQueue.Count > 0)
                {
                    var contexts = contextsQueue.Pop();
                    ShowInfo(contexts);
                }
                else
                {
                    bindingPaths = new BindingPath[0];
                    gameObject.SetActive(false);
                }
            }
            else
            {
                bindingPaths = new BindingPath[0];
                gameObject.SetActive(false);
            }
        }
    }
}
