using UnityEngine;
using UHFPS.Runtime;
using PixelCrushers.DialogueSystem;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Bridge script to synchronize Pixel Crushers Dialogue System events with UHFPS Player Freeze system.
    /// Attach this script to the Dialogue Manager or the Player object.
    /// </summary>
    public class UHFPSDialogueBridge : MonoBehaviour
    {
        /// <summary>
        /// Called by Dialogue System when a conversation starts.
        /// </summary>
        public void OnConversationStart()
        {
            if (GameManager.Instance != null)
            {
                // Freeze player: (freeze: true, showCursor: true, lockInput: true)
                GameManager.Instance.FreezePlayer(true, true, true);
            }
        }

        /// <summary>
        /// Called by Dialogue System when a conversation ends.
        /// </summary>
        public void OnConversationEnd()
        {
            if (GameManager.Instance != null)
            {
                // Unfreeze player: (freeze: false, showCursor: false, lockInput: true)
                GameManager.Instance.FreezePlayer(false, false, true);
            }
        }
    }
}
