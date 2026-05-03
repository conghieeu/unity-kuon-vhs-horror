namespace UHFPS.Runtime
{
    public interface IHealthEntity : IDamagable, IHealable
    {
        /// <summary>
        /// Represents the current entity health.
        /// </summary>
        public int EntityHealth { get; set; }

        /// <summary>
        /// Represents the maximum entity health.
        /// </summary>
        public int MaxEntityHealth { get; set; }
    }
}