using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using System.Linq;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    public class KeycardPuzzle : PuzzleBaseSimple, IInventorySelector, ISaveable
    {
        public const string LEVEL_KEY = "level";

        public ItemProperty KeycardItem;
        public ItemGuid[] UsableKeycards;

        public bool SingleKeycard = false;
        public bool UseInteract = false;
        public bool RemoveKeycardAfterUse = false;
        public float AccessUpdateTime = 0.1f;

        public bool CheckKeycardLevel = true;
        public string RequiredLevel = "yellow";

        public bool UseLight = true;
        public Light KeycardLight;
        public Color GrantedColor = Color.green;
        public Color DeniedColor = Color.red;

        public bool UseEmission = true;
        public MeshRenderer KeycardRenderer;
        public string EmissionShaderKey = "_EmissionOn";
        public string GrantedShaderKey = "_Granted";

        public SoundClip AccessGrantedSound;
        public SoundClip AccessDeniedSound;

        public UnityEvent OnAccessGranted;
        public UnityEvent OnAccessDenied;
        public UnityEvent OnWrongItem;

        public bool AccessGranted => accessGranted;

        private bool accessGranted;
        private bool notUsable;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        public override void InteractStart()
        {
            if (accessGranted || notUsable) 
                return;

            if (!UseInteract)
            {
                Inventory.Instance.OpenItemSelector(this);
            }
            else
            {
                InventoryItem keycardItem = Inventory.Instance.GetInventoryItem(KeycardItem);
                if (keycardItem != null) UseInventoryItem(keycardItem);
            }
        }

        public void OnInventoryItemSelect(Inventory inventory, InventoryItem selectedItem)
        {
            if (SingleKeycard)
            {
                if (selectedItem.ItemGuid != KeycardItem)
                    return;
            }
            else if (!UsableKeycards.Any(x => x == selectedItem.ItemGuid))
            {
                OnWrongItem?.Invoke();
                return;
            }

            UseInventoryItem(selectedItem);
        }

        private void UseInventoryItem(InventoryItem item)
        {
            if (CheckKeycardLevel)
            {
                string keycardLevel = item.CustomData.GetValue<string>(LEVEL_KEY);
                if (!string.IsNullOrEmpty(keycardLevel))
                {
                    bool state = keycardLevel == RequiredLevel;
                    UpdateKeycardState(item, state);
                }
                else
                {
                    UpdateKeycardState(item, false);
                }
            }
            else
            {
                UpdateKeycardState(item, true);
            }
        }

        private void UpdateKeycardState(InventoryItem item, bool state)
        {
            if (state)
            {
                DisableInteract();

                if (UseLight || UseEmission)
                {
                    StopAllCoroutines();
                    StartCoroutine(AccessUpdate(true));
                    notUsable = true;
                }

                audioSource.PlayOneShotSoundClip(AccessGrantedSound);
                if (RemoveKeycardAfterUse) Inventory.Instance.RemoveItem(item);
                OnAccessGranted?.Invoke();
                accessGranted = true;
            }
            else
            {
                if (UseLight || UseEmission)
                {
                    StopAllCoroutines();
                    StartCoroutine(AccessUpdate(false));
                    notUsable = true;
                }

                audioSource.PlayOneShotSoundClip(AccessDeniedSound);
                OnAccessDenied?.Invoke();
                accessGranted = false;
            }
        }

        IEnumerator AccessUpdate(bool state)
        {
            SetAccessState(state, true);
            yield return new WaitForSeconds(AccessUpdateTime);
            SetAccessState(state, false);
            yield return new WaitForSeconds(AccessUpdateTime);

            SetAccessState(state, true);
            yield return new WaitForSeconds(AccessUpdateTime);
            SetAccessState(state, false);
            yield return new WaitForSeconds(AccessUpdateTime);

            SetAccessState(state, true);
            notUsable = false;

            yield return new WaitForSeconds(1f);
            SetAccessState(false, false);
        }

        private void SetAccessState(bool state, bool enabled)
        {
            if (UseLight)
            {
                KeycardLight.enabled = enabled;
                if (enabled) KeycardLight.color = state ? GrantedColor : DeniedColor;
            }

            if (UseEmission)
            {
                KeycardRenderer.material.SetFloat(GrantedShaderKey, state ? 1f : 0f);
                KeycardRenderer.material.SetFloat(EmissionShaderKey, enabled ? 1f : 0f);
            }
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(accessGranted), accessGranted }
            };
        }

        public void OnLoad(JToken data)
        {
            accessGranted = (bool)data[nameof(accessGranted)];
            if (accessGranted) DisableInteract();
        }
    }
}