using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Hệ thống giải đố Đòn bẩy (Levers). Quản lý 3 loại giải đố chính: Theo thứ tự, Theo trạng thái Bật/Tắt, và Kéo dây chuyền (Chain).")]
    public class LeversPuzzle : MonoBehaviour, ISaveable
    {
        public enum PuzzleType { LeversOrder, LeversState, LeversChain }

        [Tooltip("Loại giải đố đòn bẩy: Order (Theo thứ tự), State (Bật/Tắt đúng trạng thái), Chain (Dây chuyền ảnh hưởng lẫn nhau).")]
        public PuzzleType LeversPuzzleType;

        [Tooltip("Danh sách các đòn bẩy con nằm trong hệ thống này.")]
        public List<LeversPuzzleLever> Levers = new();

        [Tooltip("Cấu hình khi chọn chế độ giải đố: Levers Order.")]
        public LeversPuzzleOrder LeversOrder = new();

        [Tooltip("Cấu hình khi chọn chế độ giải đố: Levers State.")]
        public LeversPuzzleState LeversState = new();

        [Tooltip("Cấu hình khi chọn chế độ giải đố: Levers Chain.")]
        public LeversPuzzleChain LeversChain = new();

        [Tooltip("Tốc độ animation gạt đòn bẩy.")]
        public float LeverSwitchSpeed = 2.5f;

        [Tooltip("Sự kiện gọi ra khi giải đố thành công.")]
        public UnityEvent OnLeversCorrect;

        [Tooltip("Sự kiện gọi ra khi giải đố thất bại (Sai thứ tự/Trạng thái).")]
        public UnityEvent OnLeversWrong;

        [Tooltip("Sự kiện gọi ra mỗi khi gạt bất kỳ đòn bẩy nào (Truyền ID và trạng thái bật/tắt).")]
        public UnityEvent<int, bool> OnLeverChanged;

        public LeversPuzzleType CurrentLeverPuzzle
        {
            get => LeversPuzzleType switch
            {
                PuzzleType.LeversOrder => LeversOrder,
                PuzzleType.LeversState => LeversState,
                PuzzleType.LeversChain => LeversChain,
                _ => null,
            };
        }

        private void OnValidate()
        {
            LeversOrder.LeversPuzzle = this;
            LeversState.LeversPuzzle = this;
            LeversChain.LeversPuzzle = this;
        }

        private void Update()
        {
            CurrentLeverPuzzle?.OnLeverUpdate();
        }

        public void OnLeverInteract(LeversPuzzleLever lever)
        {
            if (CurrentLeverPuzzle == null)
                return;

            int leverIndex = Levers.IndexOf(lever);
            OnLeverChanged?.Invoke(leverIndex, LeversPuzzleType == PuzzleType.LeversOrder || lever.LeverState);
            CurrentLeverPuzzle.OnLeverInteract(lever);
        }

        public void ValidateLevers()
        {
            if (CurrentLeverPuzzle == null)
                return;

            if (CurrentLeverPuzzle.OnValidate())
                OnLeversCorrect.Invoke();
            else
                OnLeversWrong.Invoke();
        }

        public void ResetLevers()
        {
            foreach (var lever in Levers)
            {
                lever.ResetLever();
            }
        }

        public void DisableLevers()
        {
            foreach (var lever in Levers)
            {
                lever.SetInteractState(false);
            }
        }

        public StorableCollection OnSave()
        {
            StorableCollection leverStates = new StorableCollection();
            StorableCollection storableCollection = new StorableCollection();

            for (int i = 0; i < Levers.Count; i++)
            {
                string leverName = "lever_" + i;
                bool leverState = Levers[i].LeverState;
                leverStates.Add(leverName, leverState);
            }

            storableCollection.Add("leverStates", leverStates);
            storableCollection.Add("leverData", CurrentLeverPuzzle.OnSave());

            return storableCollection;
        }

        public void OnLoad(JToken data)
        {
            JToken leverStates = data["leverStates"];
            for (int i = 0; i < Levers.Count; i++)
            {
                string leverName = "lever_" + i;
                bool leverState = (bool)leverStates[leverName];
                Levers[i].SetLeverState(leverState);
            }

            JToken leverData = data["leverData"];
            CurrentLeverPuzzle.OnLoad(leverData);
            CurrentLeverPuzzle.TryToValidate();
        }
    }
}