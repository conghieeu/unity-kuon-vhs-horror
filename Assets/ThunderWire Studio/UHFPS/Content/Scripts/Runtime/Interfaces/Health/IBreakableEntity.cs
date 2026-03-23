namespace UHFPS.Runtime
{
    public interface IBreakableEntity : IDamagable
    {
        /// <summary>
        /// Represents the current entity health.
        /// </summary>
        public int EntityHealth { get; set; }
    }
}