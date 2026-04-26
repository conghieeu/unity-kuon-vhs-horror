using UnityEngine;
using NodeCanvas.Framework;
using ParadoxNotion.Design;

namespace NodeCanvas.Tasks.Test
{
    [Category("Extensions/Test/Utility")]
    [Name("Log Message")]
    [Description("Ghi nhật ký một thông điệp vào bảng điều khiển Unity")]
    public class LogMessage : ActionTask
    {
        public BBParameter<string> message = "Hello World";
        public BBParameter<LogType> logType = LogType.Log;

        protected override void OnExecute()
        {
            switch (logType.value)
            {
                case LogType.Log:
                    Debug.Log(message.value);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message.value);
                    break;
                case LogType.Error:
                    Debug.LogError(message.value);
                    break;
            }
            EndAction();
        }
    }
}