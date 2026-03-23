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

    public interface IHoverStart
    {
        void HoverStart();
    }

    public interface IHoverEnd
    {
        void HoverEnd();
    }

    public interface IInteractStart
    {
        void InteractStart();
    }

    public interface IInteractHold
    {
        void InteractHold(Vector3 point);
    }

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