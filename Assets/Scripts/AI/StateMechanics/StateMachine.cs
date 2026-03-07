namespace StateMechanics
{
	public class StateMachine<T>
	{
		public T Owner;

		public State<T> currentState { get; private set; }

		public StateMachine(T _o)
		{
			Owner = _o;
			currentState = null;
		}

		public void ChangeState(State<T> _newstate)
		{
			if (currentState != null)
			{
				currentState.ExitState(Owner);
			}
			currentState = _newstate;
			currentState.EnterState(Owner);
		}

		public void Update()
		{
			if (currentState != null)
			{
				currentState.UpdateState(Owner);
			}
		}
	}
}
