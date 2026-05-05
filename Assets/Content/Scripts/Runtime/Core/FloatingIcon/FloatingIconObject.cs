using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Component gắn vào vật thể để ghi đè hoặc chỉ định Icon nổi (Floating Icon) riêng cho vật thể đó.")]
    public class FloatingIconObject : MonoBehaviour
    {
        [Tooltip("Có ghi đè Icon mặc định không?")]
        public bool Override;

        [Tooltip("Icon tùy chỉnh thay cho Icon mặc định.")]
        public Sprite CustomIcon;

        [Tooltip("Kích thước Icon.")]
        public Vector2 IconSize;

        [Tooltip("Ghi đè cấu hình ẩn/hiện Icon (Culling) theo khoảng cách hoặc layer.")]
        public bool OverrideCulling;

        [Tooltip("Chỉ hiển thị khi không bị chắn bởi các Layer này.")]
        public LayerMask CullLayers;

        [Tooltip("Khoảng cách tối đa để Icon hiện lên.")]
        public float DistanceShow = 4;

        [Tooltip("Khoảng cách tối đa mà Icon bị ẩn đi.")]
        public float DistanceHide = 4;
    }
}