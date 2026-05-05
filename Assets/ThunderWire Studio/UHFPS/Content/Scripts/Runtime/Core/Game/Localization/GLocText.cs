using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class GLocText : MonoBehaviour
    {
        public GString GlocKey;
        public bool ObserveMany;
        public UnityEvent<string> OnUpdateText;

        private void Start()
        {
            if (!GameLocalization.HasReference)
                return;

            if (!ObserveMany) GlocKey.SubscribeGloc(text => OnUpdateText?.Invoke(text));
            else GlocKey.SubscribeGlocMany(text => OnUpdateText?.Invoke(text));
        }
    }
}