using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    public interface ISaveableCustom
    {
        StorableCollection OnCustomSave();
        void OnCustomLoad(JToken data);
    }
}