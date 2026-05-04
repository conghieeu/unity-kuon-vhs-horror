using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [InspectorHeader("Option Observer")]
    [HelpBox("Observes the change in the custom option value, which will be assigned to a specific reflection type using reflection.")]
    public class OptionObserver : MonoBehaviour
    {
        [Space] public string OptionName;
        [Space] public GenericReflectionField OptionAction;

        private void Start()
        {
            OptionsManager.ObserveOption(OptionName, (obj) => OptionAction.Value = obj);
        }
    }
}