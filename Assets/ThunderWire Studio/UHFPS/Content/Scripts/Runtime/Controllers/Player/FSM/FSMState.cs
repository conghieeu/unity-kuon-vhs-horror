namespace UHFPS.Runtime
{
    public abstract class FSMState
    {
        public virtual void OnStateUpdate() { }
        public virtual void OnStateFixedUpdate() { }
        public virtual void OnStateEnter() { }
        public virtual void OnStateExit() { }
        public virtual void OnDrawGizmos() { }
    }
}