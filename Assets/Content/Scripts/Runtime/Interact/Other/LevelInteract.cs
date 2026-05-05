using UnityEngine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/save-load-manager/previous-scene-persistency")]
    [Summary("Quản lý việc chuyển đổi màn chơi (Level) hoặc nạp trạng thái thế giới (WorldState) thông qua tương tác hoặc vùng Trigger.")]
    public class LevelInteract : MonoBehaviour, IInteractStart
    {
        public enum TriggerTypeEnum { Trigger, Interact, Event }
        public enum LevelTypeEnum { NextLevel, WorldState, PlayerData }

        [Tooltip("Cách kích hoạt (Trigger: Đi vào vùng, Interact: Bấm nút tương tác, hoặc Gọi qua sự kiện).")]
        public TriggerTypeEnum TriggerType = TriggerTypeEnum.Interact;

        [Tooltip("Loại dữ liệu nạp: Màn mới hoàn toàn, Trạng thái thế giới, hoặc Nạp dữ liệu người chơi.")]
        public LevelTypeEnum LevelType = LevelTypeEnum.NextLevel;

        [Tooltip("Tên Scene tiếp theo cần nạp.")]
        public string NextLevelName;

        [Tooltip("Sử dụng vị trí tùy chỉnh để đặt người chơi khi qua màn mới.")]
        public bool CustomTransform;

        [Tooltip("Vị trí điểm dịch chuyển (Spawn Point) ở màn tiếp theo.")]
        public Transform TargetTransform;

        [Tooltip("Góc nhìn dọc (Lên/Xuống) của camera khi qua màn.")]
        public float LookUpDown;

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType != TriggerTypeEnum.Trigger)
                return;

            if (other.CompareTag("Player"))
                SwitchLevel();
        }

        public void InteractStart()
        {
            if (TriggerType != TriggerTypeEnum.Interact)
                return;

            SwitchLevel();
        }

        public void SwitchLevel()
        {
            if (LevelType == LevelTypeEnum.PlayerData)
            {
                SaveGameManager.SavePlayer();
                GameManager.Instance.LoadNextLevel(NextLevelName);
            }
            else if (CustomTransform)
            {
                SaveGameManager.SaveGame(TargetTransform.position, new Vector2(TargetTransform.eulerAngles.y, LookUpDown), () =>
                {
                    if (LevelType == LevelTypeEnum.NextLevel)
                        GameManager.Instance.LoadNextLevel(NextLevelName);
                    else
                        GameManager.Instance.LoadNextWorld(NextLevelName);
                });
            }
            else
            {
                SaveGameManager.SaveGame(() =>
                {
                    if (LevelType == LevelTypeEnum.NextLevel)
                        GameManager.Instance.LoadNextLevel(NextLevelName);
                    else
                        GameManager.Instance.LoadNextWorld(NextLevelName);
                });
            }
        }

        private void OnDrawGizmos()
        {
            if(CustomTransform && TargetTransform != null)
            {
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.green.Alpha(0.01f);
                UnityEditor.Handles.DrawSolidDisc(TargetTransform.position, Vector3.up, 1f);
                UnityEditor.Handles.color = Color.green;
                UnityEditor.Handles.DrawWireDisc(TargetTransform.position, Vector3.up, 1f);
#endif
                Gizmos.DrawSphere(TargetTransform.position, 0.05f);
                GizmosE.DrawGizmosArrow(TargetTransform.position, TargetTransform.forward);
            }
        }
    }
}