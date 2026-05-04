using UnityEngine;

namespace UHFPS.Runtime
{
    public interface ICharacterControllerHit
    {
        void OnCharacterControllerEnter(CharacterController controller);
        void OnCharacterControllerExit();
    }
}