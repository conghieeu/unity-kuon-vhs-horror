using UnityEngine;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Giao diện cho các vật thể muốn nhận phản hồi khi CharacterController va chạm (Enter/Exit).
    /// </summary>
    public interface ICharacterControllerHit
    {
        /// <summary>
        /// Gọi khi CharacterController bắt đầu chạm vào vật thể.
        /// </summary>
        void OnCharacterControllerEnter(CharacterController controller);

        /// <summary>
        /// Gọi khi CharacterController ngừng chạm vào vật thể.
        /// </summary>
        void OnCharacterControllerExit();
    }
}