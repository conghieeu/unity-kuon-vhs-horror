using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Đại diện cho một ô hiển thị thông tin tệp đã lưu (Save Slot) trong menu Load Game.")]
    public class LoadGameSlot : MonoBehaviour
    {
        [Tooltip("Số thứ tự của tệp lưu.")]
        public TMP_Text IndexText;
        [Tooltip("Hình ảnh xem trước (Screenshot) tại thời điểm lưu.")]
        public RawImage Thumbnail;
        [Tooltip("Loại tệp lưu (Tự động hoặc Thủ công).")]
        public TMP_Text SaveTypeText;
        [Tooltip("Tên màn chơi (Scene) đã lưu.")]
        public TMP_Text SceneNameText;
        [Tooltip("Thời gian thực hiện lưu game.")]
        public TMP_Text TimeSavedText;
        [Tooltip("Tổng thời gian đã chơi trong tệp lưu này.")]
        public TMP_Text PlaytimeText;

        public void Initialize(int index, SavedGameInfo info)
        {
            IndexText.text = index.ToString();
            Thumbnail.texture = info.Thumbnail;
            SaveTypeText.text = info.IsAutosave ? "Autosave" : "Manual Save";
            SceneNameText.text = info.Scene;
            TimeSavedText.text = info.TimeSaved.ToString("dd/MM/yyyy HH:mm:ss");
            PlaytimeText.text = info.TimePlayed.ToString(@"hh\:mm\:ss");
        }
    }
}