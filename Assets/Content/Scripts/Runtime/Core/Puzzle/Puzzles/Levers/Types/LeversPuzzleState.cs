using System;
using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Định nghĩa trạng thái hiện tại hoặc trạng thái cần đạt được của các cần gạt (Levers) trong hệ thống giải đố.")]
    public class LeversPuzzleState : LeversPuzzleType
    {
        [Tooltip("Mảng lưu trữ trạng thái (Bật/Tắt) tương ứng cho từng cần gạt.")]
        public bool[] LeverStates;

        public override void OnLeverInteract(LeversPuzzleLever lever)
        {
            TryToValidate();
        }

        public override void TryToValidate()
        {
            ValidateLevers();
        }

        public override bool OnValidate()
        {
            int correctLeverStates = 0;
            for (int i = 0; i < Levers.Count; i++)
            {
                bool leverState = Levers[i].LeverState;
                bool expectedState = LeverStates[i];

                if (leverState == expectedState)
                    correctLeverStates++;
            }

            if(correctLeverStates == Levers.Count)
            {
                DisableLevers();
                return true;
            }

            return false;
        }
    }
}