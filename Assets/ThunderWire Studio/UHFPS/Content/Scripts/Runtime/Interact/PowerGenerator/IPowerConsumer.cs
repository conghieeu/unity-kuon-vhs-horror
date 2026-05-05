using System.Reactive.Subjects;

namespace UHFPS.Runtime
{
    public interface IPowerConsumer
    {
        /// <summary>
        /// Defines how much power will be drawn from the generator. A higher value means higher power consumption.
        /// </summary>
        public float ConsumeWattage { get; set; }

        /// <summary>
        /// Defines whether the power consumer is turned on/off. To change the value, use the OnNext function on the property: <code>IsTurnedOn.OnNext(state)</code>
        /// </summary>
        public BehaviorSubject<bool> IsTurnedOn { get; set; }

        /// <summary>
        /// Defines whether the generator is turned on/off.
        /// </summary>
        public void OnPowerState(bool state);
    }
}