using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Kế thừa lớp này nếu bạn muốn định nghĩa một đối tượng có thể lưu trạng thái và được khởi tạo (instantiate) trong quá trình chơi.
    /// </summary>
    [ThunderWire.Attributes.Summary("Lớp cơ sở cho các đối tượng có thể lưu trữ trạng thái (Saveable) và được khởi tạo trong lúc chơi.")]
    public abstract class SaveableBehaviour : MonoBehaviour, IRuntimeSaveable
    {
        /// <summary>
        /// Mã định danh duy nhất được sử dụng để xác định đối tượng nào đã được khởi tạo.
        /// </summary>
        /// <remarks>Đối tượng này phải được thêm vào tài sản (asset) ObjectReferences.</remarks>
        [field: SerializeField]
        public UniqueID UniqueID { get; set; }

        /// <summary>
        /// Giao diện bắt đầu khi người chơi bắt đầu nhìn (Hover) vào vật thể.
        /// </summary>
        public interface IHoverStart
        {
        }

        public abstract StorableCollection OnSave();

        public abstract void OnLoad(JToken data);
    }
}