using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class LeversPuzzle : MonoBehaviour, ISaveable
    {
        public enum PuzzleType { LeversOrder, LeversState, LeversChain }

        public PuzzleType LeversPuzzleType;
        public List<LeversPuzzleLever> Levers = new();

        public LeversPuzzleOrder LeversOrder = new();
        public LeversPuzzleState LeversState = new();
        public LeversPuzzleChain LeversChain = new();

        public float LeverSwitchSpeed = 2.5f;

        public UnityEvent OnLeversCorrect;
        public UnityEvent OnLeversWrong;
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