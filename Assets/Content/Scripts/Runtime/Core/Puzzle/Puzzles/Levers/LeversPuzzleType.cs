using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    [Serializable]
    public abstract class LeversPuzzleType
    {
        [field: SerializeField]
        public LeversPuzzle LeversPuzzle { get; internal set; }

        protected List<LeversPuzzleLever> Levers => LeversPuzzle.Levers;

        /// <summary>
        /// Override this method to set custom properties when you interact with the lever.
        /// </summary>
        public virtual void OnLeverInteract(LeversPuzzleLever lever) { }

        /// <summary>
        /// Override this method to define your own behavior at Update.
        /// </summary>
        public virtual void OnLeverUpdate() { }

        /// <summary>
        /// Override this method to define the completed state of the levers puzzle.
        /// </summary>
        public virtual bool OnValidate() => false;

        /// <summary>
        /// Try to validate levers state.
        /// </summary>
        public virtual void TryToValidate() { }

        /// <summary>
        /// Check whether the state of the levers is correct or incorrect. Requires <see cref="OnValidate"/> to implement.
        /// </summary>
        protected void ValidateLevers() => LeversPuzzle.ValidateLevers();

        /// <summary>
        /// Reset the levers to their default state.
        /// </summary>
        protected void ResetLevers() => LeversPuzzle.ResetLevers();

        /// <summary>
        /// Disable the levers to prevent further interaction.
        /// </summary>
        protected void DisableLevers() => LeversPuzzle.DisableLevers();

        /// <summary>
        /// This method collects the data that is to be saved.
        /// </summary>
        public virtual StorableCollection OnSave() { return new StorableCollection(); }

        /// <summary>
        /// This method is called when the loading process is executed.
        /// </summary>
        public virtual void OnLoad(JToken token) { }
    }
}