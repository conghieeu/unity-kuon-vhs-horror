using Newtonsoft.Json.Linq;
using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public class LeversPuzzleOrder : LeversPuzzleType
    {
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