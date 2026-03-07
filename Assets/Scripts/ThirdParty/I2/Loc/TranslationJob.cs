using System;

namespace I2.Loc
{
	public class TranslationJob : IDisposable
	{
		public enum eJobState
		{
			Running = 0,
			Succeeded = 1,
			Failed = 2
		}

		public eJobState mJobState;

		public virtual eJobState GetState()
		{
			return mJobState;
		}

		public virtual void Dispose()
		{
		}
	}
}
