namespace StateMechanics
{
	public abstract class State<T>
	{
		public abstract void EnterState(T _owner);

		public abstract void ExitState(T _owner);

		public abstract void UpdateState(T _owner);
	}
}
