using System.Collections.Generic;

namespace UHFPS.Runtime
{
    public class ObservableValue<T>
    {
        private T _value;
        private bool _isChanged;

        public T Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_value, value))
                {
                    _value = value;
                    _isChanged = true;
                }
            }
        }

        public T SilentValue
        {
            get => _value;
            set => _value = value;
        }

        public bool IsChanged => _isChanged;

        public ObservableValue(T initialValue)
        {
            _value = initialValue;
            _isChanged = false;
        }

        public ObservableValue()
        {
            _value = default;
            _isChanged = false;
        }

        public void ResetFlag()
        {
            _isChanged = false;
        }

        public override string ToString() => $"[{_isChanged}] {_value}";
    }
}