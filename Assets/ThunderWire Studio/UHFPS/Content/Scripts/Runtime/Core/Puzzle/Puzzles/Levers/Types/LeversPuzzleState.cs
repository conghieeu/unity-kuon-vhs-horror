using System;

namespace UHFPS.Runtime
{
    [Serializable]
    public class LeversPuzzleState : LeversPuzzleType
    {
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