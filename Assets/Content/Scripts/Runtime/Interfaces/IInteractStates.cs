using UnityEngine;

namespace UHFPS.Runtime 
{
    public struct TitleParams
    {
        public string title;
        public string button1;
        public string button2;
    }

    public class StateParams
    {
        public string stateKey;
        public StorableCollection stateData;
    }

    /// <summary>
    /// Giao diện bắt đầu khi người chơi bắt đầu nhìn (Hover) vào vật thể.
    /// </summary>
    public interface IHoverStart
    {
        void HoverStart();
    }

    /// <summary>
    /// Giao diện khi người chơi ngừng nhìn (Hover) vào vật thể.
    /// </summary>
    public interface IHoverEnd
    {
        void HoverEnd();
    }

    /// <summary>
    /// Giao diện bắt đầu tương tác khi người chơi nhấn nút tương tác.
    /// </summary>
    public interface IInteractStart
    {
        void InteractStart();
    }

    /// <summary>
    /// Giao diện tương tác liên tục khi người chơi giữ nút tương tác.
    /// </summary>
    public interface IInteractHold
    {
        void InteractHold(Vector3 point);
    }

    /// <summary>
    /// Giao diện tương tác theo thời gian (giữ nút một khoảng thời gian).
    /// </summary>
    public interface IInteractTimed
    {
        float InteractTime { get; set; }
        bool NoInteract { get; }
        void InteractTimed();
    }

    public interface IInteractStop
    {
        void InteractStop();
    }

    public interface IInteractStartPlayer
    {
        void InteractStartPlayer(GameObject player);
    }

    public interface IStateInteract
    {
        StateParams OnStateInteract();
    }

    public interface IInteractTitle
    {
        TitleParams InteractTitle();
    }
}