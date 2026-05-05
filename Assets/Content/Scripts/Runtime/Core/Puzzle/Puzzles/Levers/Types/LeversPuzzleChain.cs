using System;
using System.Collections.Generic;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Loại câu đố cần gạt theo chuỗi (kéo một cần gạt sẽ tác động đến các cần gạt khác).")]
    public class LeversPuzzleChain : LeversPuzzleType
    {
        [Serializable]
        public sealed class LeversChain
        {
            [Tooltip("Danh sách chỉ số (index) của các cần gạt sẽ bị tác động khi cần gạt này được kéo.")]
            public List<int> ChainIndex = new();
        }

        [Tooltip("Cấu hình chuỗi phản ứng cho từng cần gạt. Số lượng phần tử phải tương ứng với tổng số cần gạt.")]
        public List<LeversChain> LeversChains;
        [Tooltip("Số lượng phản ứng tối đa của một cần gạt.")]
        public int MaxLeverReactions;
        [Tooltip("Số lượng cần gạt tối đa có thể tham gia phản ứng.")]
        public int MaxReactiveLevers;

        public override void OnLeverInteract(LeversPuzzleLever lever)
        {
            int leverIndex = Levers.IndexOf(lever);
            LeversChain leverChain = LeversChains[leverIndex];

            if(leverChain.ChainIndex.Count > 0)
            {
                foreach (var chain in leverChain.ChainIndex)
                {
                    LeversPuzzleLever chainLever = Levers[chain];
                    chainLever.ChangeLeverState();
                }
            }

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
                if (leverState == true)
                    correctLeverStates++;
            }

            if (correctLeverStates == Levers.Count)
            {
                DisableLevers();
                return true;
            }

            return false;
        }
    }
}