using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("UI Button Tabs")]
    [HelpBox("This component is used to manage the UIButton Tabs. It deselects the other tabs when a tab is selected.")]
    public class UIButtonTabs : MonoBehaviour
    {
        [Space]
        public bool SelectFirstTab = true;
        public GameObject[] Tabs;
        private UIButton[] uiButtons;

        private void Awake()
        {
            uiButtons = transform.GetComponentsInChildren<UIButton>();
        }

        private void Start()
        {
            if (!SelectFirstTab)
                return;

            SelectTab(0);
            uiButtons[0].SelectButton();
        }

        public void DeselectOthers(UIButton current)
        {
            foreach (var button in uiButtons)
            {
                if (button != current)
                    button.DeselectButton();
            }
        }

        public void SelectTab(int index)
        {
            for (int i = 0; i < Tabs.Length; i++)
            {
                var tab = Tabs[i];
                tab.SetActive(i == index);
            }
        }

        public void SelectTabWthButton(int index)
        {
            for (int i = 0; i < Tabs.Length; i++)
            {
                var tab = Tabs[i];
                tab.SetActive(i == index);

                if (i == index) uiButtons[i].SelectButton();
                else uiButtons[i].DeselectButton();
            }
        }

        public void SelectTabWthButtonForceColor(int index)
        {
            for (int i = 0; i < Tabs.Length; i++)
            {
                var tab = Tabs[i];
                tab.SetActive(i == index);

                if (i == index) uiButtons[i].SelectButton(true);
                else uiButtons[i].DeselectButton(true);
            }
        }
    }
}