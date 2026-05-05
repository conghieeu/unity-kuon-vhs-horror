using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Saves UI Loader")]
    [Summary("Tự động quét và hiển thị danh sách các tệp đã lưu (Saves) lên giao diện người dùng, đồng thời xử lý việc Load/Delete.")]
    public class SavesUILoader : MonoBehaviour
    {
        [Tooltip("Tham chiếu đến BackgroundFader để xử lý chuyển cảnh.")]
        public BackgroundFader BackgroundFader;
        [Tooltip("Nút bấm để chơi tiếp tệp lưu gần nhất.")]
        public Button ContinueButton;
        [Tooltip("Nút bấm để load tệp lưu đang được chọn.")]
        public Button LoadButton;
        [Tooltip("Nút bấm để xóa tệp lưu đang được chọn.")]
        public Button DeleteButton;

        [Header("Save Slot")]
        [Tooltip("Transform cha chứa danh sách các ô Save Slot.")]
        public Transform SaveSlotsParent;
        [Tooltip("Prefab của ô Save Slot.")]
        public GameObject SaveSlotPrefab;

        [Header("Settings")]
        [Tooltip("Tự động fade out hình nền khi bắt đầu.")]
        public bool FadeOutAtStart;
        [Tooltip("Tự động tải danh sách saves khi bắt đầu.")]
        public bool LoadAtStart;
        [Tooltip("Tốc độ fade.")]
        public float FadeSpeed;

        [Header("Events")]
        [Tooltip("Sự kiện gọi khi bắt đầu quá trình tải saves.")]
        public UnityEvent OnSavesBeingLoaded;
        [Tooltip("Sự kiện gọi khi tải xong danh sách saves.")]
        public UnityEvent OnSavesLoaded;
        [Tooltip("Sự kiện gọi khi không tìm thấy tệp lưu nào.")]
        public UnityEvent OnSavesEmpty;

        private readonly Dictionary<GameObject, SavedGameInfo> saveSlots = new();
        private SavedGameInfo? lastSave;
        private SavedGameInfo? selected;

        private bool isLoading;

        private async void Start()
        {
            if (LoadAtStart)
            {
                // load saves process
                await LoadAllSaves();

                // enable or disable continue button when last save exists
                if(ContinueButton != null)
                    ContinueButton.gameObject.SetActive(lastSave.HasValue);

                if (FadeOutAtStart) 
                    StartCoroutine(BackgroundFader.StartBackgroundFade(true, fadeSpeed: FadeSpeed));
            }
        }

        /// <summary>
        /// Manually load all saved games.
        /// </summary>
        public async void LoadSavedGames()
        {
            foreach (var slot in saveSlots)
            {
                Destroy(slot.Key);
            }

            saveSlots.Clear();
            OnSavesBeingLoaded?.Invoke();

            await LoadAllSaves();
        }

        /// <summary>
        /// Load last saved game.
        /// </summary>
        public void LoadLastSave()
        {
            if (!lastSave.HasValue || isLoading) 
                return;

            SaveGameManager.SetLoadGameState(lastSave.Value.Scene, lastSave.Value.Foldername);
            StartCoroutine(FadeAndLoadGame());

            selected = null;
            isLoading = true;
        }

        /// <summary>
        /// Load selected save.
        /// </summary>
        public void LoadSelectedSave()
        {
            if (!selected.HasValue || isLoading)
                return;

            SaveGameManager.SetLoadGameState(selected.Value.Scene, selected.Value.Foldername);
            StartCoroutine(FadeAndLoadGame());

            selected = null;
            isLoading = true;
        }

        /// <summary>
        /// Remove selected save.
        /// </summary>
        public async void RemoveSelectedSave()
        {
            if (!selected.HasValue || isLoading)
                return;

            // remove save from disk
            await SaveGameManager.SaveGameReader.RemoveSave(selected.Value.Foldername);

            // remove save from UI
            foreach (var save in saveSlots)
            {
                if(save.Value.Foldername == selected.Value.Foldername)
                {
                    Destroy(save.Key);
                    saveSlots.Remove(save.Key);

                    // select new last save
                    if (save.Value.Id == lastSave.Value.Id)
                    {
                        if (saveSlots.Count > 0)
                        {
                            lastSave = saveSlots.ElementAt(0).Value;
                            if (ContinueButton != null) 
                                ContinueButton.gameObject.SetActive(true);
                        }
                        else if(ContinueButton != null)
                        {
                            lastSave = null;
                            ContinueButton.gameObject.SetActive(false);
                        }
                    }

                    break;
                }
            }

            // reset selected save
            if (LoadButton != null) LoadButton.gameObject.SetActive(false);
            if (DeleteButton != null) DeleteButton.gameObject.SetActive(false);
            selected = null;
        }

        /// <summary>
        /// Reset all save slots.
        /// </summary>
        public void ResetSaves()
        {
            foreach (var slot in saveSlots)
            {
                UIButton slotButton = slot.Key.GetComponent<UIButton>();
                slotButton.DeselectButton();
            }

            if (LoadButton != null) LoadButton.gameObject.SetActive(false);
            if (DeleteButton != null) DeleteButton.gameObject.SetActive(false);
            selected = null;
        }

        private async Task LoadAllSaves()
        {
            // load saves in another thread
            var savedGames = await SaveGameManager.SaveGameReader.ReadAllSaves();

            // set last saved game
            if(savedGames.Length > 0)
                lastSave = savedGames[0];

            // instantiate saves in main thread
            for (int i = 0; i < savedGames.Length; i++)
            {
                SavedGameInfo saveInfo = savedGames[i];
                GameObject slotGO = Instantiate(SaveSlotPrefab, SaveSlotsParent);
                slotGO.name = "Slot" + i.ToString();

                LoadGameSlot loadGameSlot = slotGO.GetComponent<LoadGameSlot>();
                loadGameSlot.Initialize(i, saveInfo);

                UIButton loadButton = slotGO.GetComponent<UIButton>();
                loadButton.OnClick.AddListener((_) => 
                {
                    selected = saveInfo;
                    if (LoadButton != null) LoadButton.gameObject.SetActive(true);
                    if (DeleteButton != null) DeleteButton.gameObject.SetActive(true);
                });

                saveSlots.Add(slotGO, saveInfo);
            }

            if(savedGames.Length > 0)
            {
                OnSavesLoaded?.Invoke();
            }
            else
            {
                OnSavesEmpty?.Invoke();
            }
        }

        IEnumerator FadeAndLoadGame()
        {
            if(BackgroundFader != null) 
                yield return BackgroundFader.StartBackgroundFade(false, fadeSpeed: FadeSpeed);
            SceneManager.LoadScene(SaveGameManager.LMS);
        }
    }
}