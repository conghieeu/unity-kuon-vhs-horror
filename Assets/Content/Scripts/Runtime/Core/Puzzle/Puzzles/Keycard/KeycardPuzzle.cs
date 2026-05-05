using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Tools;
using System.Linq;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(AudioSource))]
    [Summary("Hệ thống giải đố Máy quét thẻ từ (Keycard Reader). Hỗ trợ kiểm tra quyền truy cập thông qua Level Custom Data của vật phẩm thẻ.")]
    public class KeycardPuzzle : PuzzleBaseSimple, IInventorySelector, ISaveable
    {
        public const string LEVEL_KEY = "level";

        [Tooltip("Thẻ từ yêu cầu (Nếu chỉ sử dụng 1 loại thẻ duy nhất).")]
        public ItemProperty KeycardItem;

        [Tooltip("Danh sách các ID Thẻ từ được phép quét (Ví dụ: thẻ đỏ, thẻ xanh, thẻ quản lý).")]
        public ItemGuid[] UsableKeycards;

        [Tooltip("Chỉ sử dụng 1 thẻ duy nhất (Bỏ qua danh sách UsableKeycards).")]
        public bool SingleKeycard = false;

        [Tooltip("Tự động quét thẻ nếu có trong túi đồ khi tương tác (Không mở Inventory Selector).")]
        public bool UseInteract = false;

        [Tooltip("Xóa thẻ từ khỏi túi sau khi dùng thành công.")]
        public bool RemoveKeycardAfterUse = false;

        [Tooltip("Khoảng thời gian nhấp nháy đèn báo (tính bằng giây).")]
        public float AccessUpdateTime = 0.1f;

        [Tooltip("Kiểm tra giá trị 'level' trong CustomData của thẻ từ (VD: Cần thẻ cấp độ 'yellow').")]
        public bool CheckKeycardLevel = true;

        [Tooltip("Cấp độ (Level) yêu cầu để mở máy quét (VD: 'yellow').")]
        public string RequiredLevel = "yellow";

        [Tooltip("Sử dụng đèn sáng (Light Component) để báo trạng thái.")]
        public bool UseLight = true;
        public Light KeycardLight;
        public Color GrantedColor = Color.green;
        public Color DeniedColor = Color.red;

        [Tooltip("Sử dụng hiệu ứng Material Emission để báo trạng thái.")]
        public bool UseEmission = true;
        public MeshRenderer KeycardRenderer;
        public string EmissionShaderKey = "_EmissionOn";
        public string GrantedShaderKey = "_Granted";

        [Tooltip("Âm thanh khi thẻ hợp lệ.")]
        public SoundClip AccessGrantedSound;

        [Tooltip("Âm thanh khi thẻ bị từ chối (Sai cấp độ).")]
        public SoundClip AccessDeniedSound;

        [Tooltip("Sự kiện gọi ra khi thẻ hợp lệ (Mở cửa).")]
        public UnityEvent OnAccessGranted;

        [Tooltip("Sự kiện gọi ra khi thẻ không đủ cấp độ.")]
        public UnityEvent OnAccessDenied;

        [Tooltip("Sự kiện gọi ra khi dùng sai vật phẩm (Không phải thẻ từ).")]
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