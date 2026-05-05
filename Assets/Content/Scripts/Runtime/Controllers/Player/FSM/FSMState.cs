using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Lớp cơ sở trừu tượng cho máy trạng thái hữu hạn (Finite State Machine).")]
    public abstract class FSMState
    {
        public virtual void OnStateUpdate() { }
        public virtual void OnStateFixedUpdate() { }
        public virtual void OnStateEnter() { }
        public virtual void OnStateExit() { }
        public virtual void OnDrawGizmos() { }
    }
}