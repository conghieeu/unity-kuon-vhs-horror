using System;
using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/interactions#changing-interact-reticle")]
    public class CustomInteractReticle : MonoBehaviour, IReticleProvider
    {
        public Reticle OverrideReticle;
        public Reticle HoldReticle;

        public bool DynamicHoldReticle;
        public ReflectionField DynamicHold;

        public (Type, Reticle, bool) OnProvideReticle()
        {
            bool hold = DynamicHoldReticle && DynamicHold.Value;
            Reticle reticle = hold ? HoldReticle : OverrideReticle;
            return (null, reticle, hold);
        }
    }
}