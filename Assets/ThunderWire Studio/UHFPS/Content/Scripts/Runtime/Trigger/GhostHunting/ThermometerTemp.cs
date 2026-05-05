using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public class ThermometerTemp : MonoBehaviour, ISaveable
    {
        public enum TempType { Base, Trigger, Raycast, Event }
        public enum TempChangeType { SetBase, ResetBase }
        public enum TriggerTypeEnum { Once, SetReset }

        public TempType TemperatureType = TempType.Base;
        public TempChangeType ChangeType = TempChangeType.SetBase;
        public TriggerTypeEnum TriggerType = TriggerTypeEnum.Once;
        public string ThermometerItem = "Thermometer";
        public float Temperature = 23.6f;

        public MinMax RandomTempScale = new(10f, 25f);

        public UnityEvent<float> OnSetTemp;
        public UnityEvent OnResetTemp;

        public bool IsBaseTrigger => TemperatureType == TempType.Trigger || TemperatureType == TempType.Event;

        private ThermometerItem thermometer;
        private bool isTriggered;

        private void Start()
        {
            PlayerManager player = PlayerPresenceManager.Instance.PlayerManager;
            PlayerItemsManager playerItems = player.PlayerItems;
            thermometer = playerItems.GetItemByName<ThermometerItem>(ThermometerItem);

            if (thermometer != null && TemperatureType == TempType.Base && !SaveGameManager.GameWillLoad) 
                thermometer.SetResetTemp(Temperature);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || TemperatureType != TempType.Trigger || isTriggered)
                return;

            if(ChangeType == TempChangeType.SetBase) 
                thermometer.SetBaseTemperature(Temperature);
            else thermometer.ResetTemperature();

            if(TriggerType == TriggerTypeEnum.Once)
                isTriggered = true;

            OnSetTemp?.Invoke(Temperature);
        }

        public void SetTemperature()
        {
            if (TemperatureType != TempType.Event)
                return;

            if (ChangeType == TempChangeType.SetBase)
                thermometer.SetBaseTemperature(Temperature);
            else thermometer.ResetTemperature();

            OnSetTemp?.Invoke(Temperature);
        }

        public void SetTemperatureValue(float temperature)
        {
            if (TemperatureType != TempType.Event)
                return;

            if (ChangeType == TempChangeType.SetBase)
                thermometer.SetTemperature(temperature);
            else thermometer.ResetTemperature();

            OnSetTemp?.Invoke(temperature);
        }

        public void ResetTemperature()
        {
            if (TemperatureType != TempType.Event)
                return;

            thermometer.ResetTemperature();
            OnResetTemp?.Invoke();
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(isTriggered), isTriggered },
            };
        }

        public void OnLoad(JToken data)
        {
            isTriggered = (bool)data[nameof(isTriggered)];
        }
    }
}