namespace UHFPS.Runtime
{
    [System.Serializable]
    public struct Percentage
    {
        public ushort Value;

        public Percentage(ushort value)
        {
            Value = value;
        }

        public float Ratio() => (float)Value / 100;

        public float From(float value) => Ratio() * value;

        public static implicit operator ushort(Percentage percentage)
        {
            return percentage.Value;
        }

        public static implicit operator Percentage(ushort value)
        {
            return new Percentage(value);
        }
    }
}