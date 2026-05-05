using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Auto Player Parent")]
    [Summary("Tự động gán người chơi làm con (Parent) của vật thể này khi người chơi bước vào vùng Trigger.")]
    public class AutoPlayerParent : MonoBehaviour, ICharacterControllerHit
    {
        [Tooltip("Transform mục tiêu mà người chơi sẽ được gán làm con. Nếu để trống, sẽ dùng Transform của vật thể này.")]
        public Transform Parent;

        public void OnCharacterControllerEnter(CharacterController controller)
        {
            Transform parent = Parent != null ? Parent : transform;
            PlayerManager.Instance.ParentToObject(parent);
        }

        public void OnCharacterControllerExit()
        {
            PlayerManager.Instance.UnparentFromObject();
        }
    }
}