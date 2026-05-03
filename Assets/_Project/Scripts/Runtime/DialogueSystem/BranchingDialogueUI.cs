using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace Custom.DialogueSystem
{
    public class BranchingDialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject DialoguePanel;
        public TMP_Text CharacterNameText;
        public TMP_Text DialogueText;
        public Transform OptionsParent;
        public GameObject OptionButtonPrefab;

        private List<GameObject> activeOptionButtons = new List<GameObject>();

        private void Start()
        {
            ShowDialoguePanel(false);
        }

        public void ShowDialoguePanel(bool show)
        {
            if (DialoguePanel != null)
                DialoguePanel.SetActive(show);
        }

        public void DisplayNode(BranchingDialogueNode node)
        {
            if (CharacterNameText != null)
                CharacterNameText.text = node.CharacterName;
                
            if (DialogueText != null)
                DialogueText.text = node.DialogueText;

            ClearOptions();

            for (int i = 0; i < node.Options.Count; i++)
            {
                var option = node.Options[i];
                GameObject btnObj = Instantiate(OptionButtonPrefab, OptionsParent);
                activeOptionButtons.Add(btnObj);

                var btn = btnObj.GetComponent<Button>();
                var txt = btnObj.GetComponentInChildren<TMP_Text>();

                if (txt != null)
                    txt.text = option.OptionText;

                int nextIndex = option.NextNodeIndex;
                var onSelect = option.OnOptionSelected;

                btn.onClick.AddListener(() =>
                {
                    BranchingDialogueManager.Instance.OptionSelected(nextIndex, onSelect);
                });
            }
        }

        private void ClearOptions()
        {
            foreach (var btn in activeOptionButtons)
            {
                Destroy(btn);
            }
            activeOptionButtons.Clear();
        }
    }
}
