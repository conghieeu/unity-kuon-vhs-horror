using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using static UHFPS.Scriptable.DialogueAsset;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Component điều khiển giao diện UI hiển thị các Lựa chọn (Options) khi hội thoại rẽ nhánh.")]
    public class DialogueOptionsUI : MonoBehaviour
    {
        [Tooltip("Panel UI chứa toàn bộ danh sách các Lựa chọn.")]
        public GameObject OptionsPanel;

        [Tooltip("Prefab của Button lựa chọn (Bao gồm Text).")]
        public GameObject OptionButtonPrefab;

        [Tooltip("Transform chứa các Button lựa chọn được sinh ra (Thường là một Layout Group).")]
        public Transform OptionsContainer;

        private List<GameObject> activeButtons = new();
        private List<TMP_Text> optionTexts = new();
        private List<int> jumpIndices = new();

        private bool isOptionsActive = false;
        private int selectedIndex = 0;

        private float inputDelayTimer = 0f;

        public void OnShowOptions(List<DialogueOption> options)
        {
            ClearButtons();

            isOptionsActive = false;
            selectedIndex = 0;
            inputDelayTimer = 0.2f;

            foreach (var option in options)
            {
                GameObject btnObj = Instantiate(OptionButtonPrefab, OptionsContainer);
                btnObj.SetActive(true);
                activeButtons.Add(btnObj);

                TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = (string)option.OptionText;
                    optionTexts.Add(btnText);
                    jumpIndices.Add(option.JumpToIndex);
                }
            }

            OptionsPanel.SetActive(true);
            UpdateVisuals();

            StartCoroutine(ActivateOptionsNextFrame());
        }

        private System.Collections.IEnumerator ActivateOptionsNextFrame()
        {
            yield return null;
            isOptionsActive = true;
        }

        private void Update()
        {
            if (!isOptionsActive || activeButtons.Count == 0) return;

            if (inputDelayTimer > 0f)
            {
                inputDelayTimer -= Time.deltaTime;
                return;
            }

            bool selectionChanged = false;

            // Mouse Scroll
            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (scroll > 0f)
                {
                    selectedIndex--;
                    selectionChanged = true;
                }
                else if (scroll < 0f)
                {
                    selectedIndex++;
                    selectionChanged = true;
                }
            }

            // Keyboard Arrows
            if (Keyboard.current != null)
            {
                if (Keyboard.current.upArrowKey.wasPressedThisFrame)
                {
                    selectedIndex--;
                    selectionChanged = true;
                }
                else if (Keyboard.current.downArrowKey.wasPressedThisFrame)
                {
                    selectedIndex++;
                    selectionChanged = true;
                }
            }

            if (selectionChanged)
            {
                if (selectedIndex < 0) selectedIndex = activeButtons.Count - 1;
                if (selectedIndex >= activeButtons.Count) selectedIndex = 0;
                UpdateVisuals();
            }

            // Confirm Selection (E, Enter, Left Click)
            bool confirm = false;
            if (Keyboard.current != null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                confirm = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                confirm = true;

            if (confirm)
            {
                OnOptionSelected(selectedIndex);
            }
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < optionTexts.Count; i++)
            {
                var textObj = optionTexts[i];
                if (i == selectedIndex)
                {
                    // Highlighted
                    textObj.color = new Color(1f, 0.8f, 0.2f, 1f); // Gold/Yellow
                    textObj.fontStyle = FontStyles.Bold;
                    
                    // Add > prefix if not already there, remove spaces
                    string currentText = textObj.text;
                    if (currentText.StartsWith("  ")) currentText = currentText.Substring(2);
                    if (!currentText.StartsWith("> "))
                    {
                        textObj.text = "> " + currentText;
                    }
                }
                else
                {
                    // Normal
                    textObj.color = new Color(1f, 1f, 1f, 0.6f); // Faded White
                    textObj.fontStyle = FontStyles.Normal;

                    // Remove > prefix if there, add spaces
                    string currentText = textObj.text;
                    if (currentText.StartsWith("> ")) currentText = currentText.Substring(2);
                    if (!currentText.StartsWith("  "))
                    {
                        textObj.text = "  " + currentText;
                    }
                }
            }
        }

        public void OnOptionSelected(int index)
        {
            if (index < 0 || index >= jumpIndices.Count) return;

            isOptionsActive = false;
            DialogueSystem.Instance.SelectOption(jumpIndices[index]);
            OptionsPanel.SetActive(false);
            
            // Clean up old ones, but not the prefab
            foreach (Transform child in OptionsContainer)
            {
                if (child.gameObject != OptionButtonPrefab)
                {
                    Destroy(child.gameObject);
                }
            }
            activeButtons.Clear();
            optionTexts.Clear();
            jumpIndices.Clear();
        }

        public void OnDialogueEnd()
        {
            isOptionsActive = false;
            OptionsPanel.SetActive(false);
            ClearButtons();
        }

        private void ClearButtons()
        {
            foreach (var btn in activeButtons)
            {
                if (btn != null && btn != OptionButtonPrefab)
                {
                    Destroy(btn);
                }
            }
            activeButtons.Clear();
            optionTexts.Clear();
            jumpIndices.Clear();
        }
    }
}
