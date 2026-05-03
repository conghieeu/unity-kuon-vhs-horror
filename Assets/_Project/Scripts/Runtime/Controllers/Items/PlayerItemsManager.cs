using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UHFPS.Input;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/player-items")]
    public class PlayerItemsManager : PlayerComponent
    {
        public List<PlayerItemBehaviour> PlayerItems = new();
        public float AntiSpamDelay = 0.5f;
        public bool IsItemsUsable = true;

        private PlayerItemBehaviour currentItem;
        private PlayerItemBehaviour previousItem;
        private PlayerItemBehaviour nextItem;

        private bool canSwitch = true;
        private bool wasDeactivated = false;

        public bool CanInteract => isEnabled && IsItemsUsable;
        public bool IsAnyEquipped => CurrentItemIndex != -1;

        public PlayerItemBehaviour CurrentItem => currentItem;
        public int CurrentItemIndex => PlayerItems.IndexOf(currentItem);

        public PlayerItemBehaviour PreviousItem => previousItem;
        public int PreviousItemIndex => PlayerItems.IndexOf(previousItem);

        private void Awake()
        {
            InputManager.Performed(Controls.ITEM_UNEQUIP, _ => DeselectCurrent());
            canSwitch = true;
        }

        /// <summary>
        /// Switch or select a player item.
        /// </summary>
        /// <param name="itemID">Index of the player item in the PlayerItems list.</param>
        public void SwitchPlayerItem(int itemID)
        {
            if (currentItem != null && currentItem.IsBusy() || !canSwitch || !IsItemsUsable || ExamineController.IsExamining)
                return;

            StopAllCoroutines();
            nextItem = PlayerItems[itemID];
            wasDeactivated = false;
            canSwitch = false;

            if (nextItem != currentItem)
            {
                if (currentItem != null && currentItem.IsEquipped())
                {
                    StartCoroutine(SwitchItem());
                }
                else
                {
                    previousItem = currentItem;
                    currentItem = nextItem;
                    nextItem = null;

                    StartCoroutine(SelectItem());
                }
            }
            else
            {
                DeselectCurrent();
                nextItem = null;
            }
        }

        /// <summary>
        /// Activate player item.
        /// </summary>
        /// <param name="itemID">Index of the player item in the PlayerItems list.</param>
        public void ActivateItem(int itemID)
        {
            nextItem = PlayerItems[itemID];

            if (nextItem == currentItem)
                return;

            wasDeactivated = currentItem != null;
            if (currentItem != null)
            {
                previousItem = currentItem;
                previousItem.OnItemDeactivate();
            }

            currentItem = nextItem;
            nextItem = null;

            currentItem.OnItemActivate();
            canSwitch = true;
        }

        /// <summary>
        /// Activate a player item that was previously deselected.
        /// </summary>
        public void ActivatePreviousItem()
        {
            if (previousItem == null)
                return;

            var current = currentItem;
            currentItem = previousItem;
            previousItem = current;

            currentItem.OnItemActivate();
            wasDeactivated = false;
            canSwitch = true;
        }

        /// <summary>
        /// Select a previously deselected player item.
        /// </summary>
        public void SelectPreviousItem()
        {
            if (previousItem == null)
                return;

            var current = currentItem;
            currentItem = previousItem;
            previousItem = current;

            StopAllCoroutines();
            StartCoroutine(SelectItem());
            wasDeactivated = false;
            canSwitch = false;
        }

        /// <summary>
        /// Activate a player item that was previously deactivated.
        /// </summary>
        public void ActivatePreviouslyDeactivatedItem()
        {
            if (previousItem == null || !wasDeactivated)
                return;

            var current = currentItem;
            currentItem = previousItem;
            previousItem = current;

            currentItem.OnItemActivate();
            wasDeactivated = false;
            canSwitch = true;
        }

        /// <summary>
        /// Select a player item that was previously deactivated.
        /// </summary>
        public void SelectPreviouslyDeactivatedItem()
        {
            if (previousItem == null || !wasDeactivated)
                return;

            var current = currentItem;
            currentItem = previousItem;
            previousItem = current;

            StopAllCoroutines();
            StartCoroutine(SelectItem());
            wasDeactivated = false;
            canSwitch = false;
        }

        /// <summary>
        /// Deselect the currently equipped player item.
        /// </summary>
        public void DeselectCurrent()
        {
            if (currentItem == null)
                return;

            previousItem = currentItem;

            StopAllCoroutines();
            StartCoroutine(DeselectItem());
        }

        /// <summary>
        /// Deactivate the currently equipped player item.
        /// </summary>
        public void DeactivateCurrentItem()
        {
            if (currentItem == null)
                return;

            previousItem = currentItem;
            currentItem = null;
            wasDeactivated = true;
            canSwitch = true;

            StopAllCoroutines();
            previousItem.OnItemDeactivate();
        }

        /// <summary>
        /// Register the current item as a previously deactivated item.
        /// </summary>
        public void RegisterPreviousItem()
        {
            if (currentItem == null)
                return;

            previousItem = currentItem;
            wasDeactivated = true;
        }

        /// <summary>
        /// Get PlayerItemBehaviour reference by item name.
        /// </summary>
        public PlayerItemBehaviour GetItemByName(string name)
        {
            foreach (var item in PlayerItems)
            {
                if(item.Name == name)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Get player item reference by item name.
        /// </summary>
        public T GetItemByName<T>(string name) where T : PlayerItemBehaviour
        {
            foreach (var item in PlayerItems)
            {
                if (item.Name == name)
                    return item as T;
            }

            return default;
        }

        IEnumerator SwitchItem()
        {
            currentItem.OnItemDeselect();
            yield return new WaitUntil(() => !currentItem.IsEquipped());

            previousItem = currentItem;
            currentItem = nextItem;
            nextItem = null;

            currentItem.OnItemSelect();
            yield return AntiSpam();
        }

        IEnumerator SelectItem()
        {
            currentItem.OnItemSelect();
            yield return AntiSpam();
        }

        IEnumerator DeselectItem()
        {
            currentItem.OnItemDeselect();
            yield return new WaitUntil(() => !currentItem.IsEquipped());
            yield return AntiSpam();

            currentItem = null;
            nextItem = null;
        }

        IEnumerator AntiSpam()
        {
            yield return new WaitForSeconds(AntiSpamDelay);
            canSwitch = true;
        }
    }
}