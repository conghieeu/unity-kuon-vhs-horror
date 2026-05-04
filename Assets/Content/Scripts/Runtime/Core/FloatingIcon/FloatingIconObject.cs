using UnityEngine;

namespace UHFPS.Runtime
{
    public class FloatingIconObject : MonoBehaviour
    {
        public bool Override;
        public Sprite CustomIcon;
        public Vector2 IconSize;

        public bool OverrideCulling;
        public LayerMask CullLayers;
        public float DistanceShow = 4;
        public float DistanceHide = 4;
    }
}