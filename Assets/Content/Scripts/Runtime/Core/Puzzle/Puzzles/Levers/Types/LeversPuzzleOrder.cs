using Newtonsoft.Json.Linq;
using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Serializable]
    [Summary("Loại câu đố cần gạt theo thứ tự (người chơi phải kéo các cần gạt theo đúng thứ tự quy định).")]
    public class LeversPuzzleOrder : LeversPuzzleType
    {
        [Tooltip("Thứ tự kéo cần gạt đúng (ví dụ: '0123' tương ứng với kéo cần 0, sau đó 1, 2, 3).")]
        public string LeversOrder = "";

        private string currentOrder = "";
        private bool validate = false;

        public override void OnLeverInteract(LeversPuzzleLever lever)
        {
            if (validate) 
                return;

            int leverIndex = Levers.IndexOf(lever);
            currentOrder += leverIndex;
            TryToValidate();
        }

        public override void TryToValidate()
        {
            if (currentOrder.Length >= Levers.Count)
            {
                validate = true;
                ValidateLevers();
            }
        }

        public override bool OnValidate()
        {
            bool result = LeversOrder.Equals(currentOrder);

            if (result) DisableLevers();
            else validate = false;

            currentOrder = "";
            return result;
        }

        public override StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(currentOrder), currentOrder },
                { nameof(validate), validate },
            };
        }

        public override void OnLoad(JToken token)
        {
            currentOrder = token[nameof(currentOrder)].ToString();
            validate = (bool)token[nameof(validate)];
        }
    }
}