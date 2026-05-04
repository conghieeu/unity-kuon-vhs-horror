namespace UHFPS.Runtime
{
    public interface IHealable
    {
        /// <summary>
        /// Override this method to define custom behavior when an entity receives a health call.
        /// </summary>
        void OnApplyHeal(int healAmount);

        /// <summary>
        /// Override this method to define custom behavior when applying maximum heal.
        /// </summary>
        void ApplyHealMax();
    }
}