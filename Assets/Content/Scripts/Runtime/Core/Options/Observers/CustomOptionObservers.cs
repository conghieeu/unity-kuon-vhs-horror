using System.Collections.Generic;
using UnityEngine;

using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Thành phần quản lý các bộ lắng nghe (Observer) cho nhiều Custom Option cùng lúc. Gắn vào các Object cần thay đổi giá trị dựa trên Settings (Ví dụ: Player, Camera).")]
    public class CustomOptionObservers : MonoBehaviour
    {
        [SerializeReference]
        [Tooltip("Danh sách các hành động lắng nghe (Thay đổi Volume, FOV, Độ sáng...).")]
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