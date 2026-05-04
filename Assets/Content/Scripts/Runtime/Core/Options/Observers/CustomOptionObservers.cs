using System.Collections.Generic;
using UnityEngine;

namespace UHFPS.Runtime
{
    public class CustomOptionObservers : MonoBehaviour
    {
        [SerializeReference]
        public List<OptionObserverType> OptionObservers = new();

        private void Start()
        {
            foreach (var option in OptionObservers)
            {
                option.OnStart();
                OptionsManager.ObserveOption(option.ObserveOptionName, option.OptionUpdate);
            }
        }
    }
}