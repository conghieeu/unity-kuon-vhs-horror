using System;
using System.Text;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class PadlockPuzzle : PuzzleBase, IInventorySelector, IDynamicUnlock, ISaveable
    {
        public enum PadlockTypeEnum { NumberPadlock, KeyPadlock }

        public PadlockTypeEnum PadlockType;
        public bool UseInteract = true;
        public PadlockPuzzleDigit[] PadlockDigits = new PadlockPuzzleDigit[1];

        public string UnlockCode = "0";
        public ItemGuid UnlockKeyItem;

        public Animator Animator;
        public string UnlockAnimation = "Unlock";

        public SoundClip UnlockSound;
        public UnityEvent OnPadlockUnlock;

        public bool isUnlocked;

        private DynamicObject dynamicObject;
        private string currentCode;

        private void Reset()
        {
            PadlockDigits = new PadlockPuzzleDigit[1];
            UnlockCode = "0";
        }

        public override void InteractStart()
        {
            if (UseInteract)
            {
                if (PadlockType == PadlockTypeEnum.NumberPadlock)
                {
                    base.InteractStart();
                }
                else
                {
                    Inventory.Instance.OpenItemSelector(this);
                }
            }
        }

        public void OnInventoryItemSelect(Inventory inventory, InventoryItem selectedItem)
        {
            if (selectedItem.ItemGuid != UnlockKeyItem)
                return;

            inventory.RemoveItem(selectedItem);
            StartCoroutine(OnUnlock());
        }

        public void OnTryUnlock(DynamicObject dynamicObject)
        {
            this.dynamicObject = dynamicObject;

            if (PadlockType == PadlockTypeEnum.NumberPadlock)
            {
                base.InteractStart();
            }
            else
            {
                Inventory.Instance.OpenItemSelector(this);
            }
        }

        public void SetDigit(PadlockPuzzleDigit digit, int number)
        {
            if (PadlockType == PadlockTypeEnum.NumberPadlock)
            {
                int index = Array.IndexOf(PadlockDigits, digit);
                StringBuilder unlockCodeBuilder = new(currentCode);
                unlockCodeBuilder.Length = PadlockDigits.Length;
                unlockCodeBuilder[index] = (char)(number + 48);
                currentCode = unlockCodeBuilder.ToString();

                if (currentCode == UnlockCode)
                {
                    StartCoroutine(OnUnlock());
                }
                else
                {
                    StopAllCoroutines();
                }
            }
        }

        IEnumerator OnUnlock()
        {
            if (PadlockType == PadlockTypeEnum.NumberPadlock)
            {
                canManuallySwitch = false;
                yield return new WaitForSeconds(1f);
            }

            foreach (var collider in GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            GameTools.PlayOneShot3D(transform.position, UnlockSound, "PadlockUnlock");

            if (Animator != null)
            {
                Animator.SetTrigger(UnlockAnimation);
                yield return new WaitForAnimatorClip(Animator, UnlockAnimation);
            }

            if (dynamicObject != null)
                dynamicObject.TryUnlockResult(true);

            OnPadlockUnlock?.Invoke();
            isUnlocked = true;

            if (PadlockType == PadlockTypeEnum.NumberPadlock)
            {
                switchColliders = false;
                SwitchBack();
            }
        }

        public override void OnBackgroundFade()
        {
            base.OnBackgroundFade();

            if(!isActive && isUnlocked)
            {
                gameObject.SetActive(false);
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isUnlocked), isUnlocked }
            };
        }

        public void OnLoad(JToken data)
        {
            isUnlocked = (bool)data[nameof(isUnlocked)];
            if (isUnlocked)
            {
                gameObject.SetActive(false);
            }
        }
    }
}