using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Giao diện cho các đối tượng muốn lưu và tải trạng thái thông qua hệ thống SaveGame.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Gọi khi thực hiện lưu game. Trả về tập hợp dữ liệu cần lưu.
        /// </summary>
        StorableCollection OnSave();

        /// <summary>
        /// Gọi khi thực hiện tải game. Parse dữ liệu JToken để khôi phục trạng thái.
        /// </summary>
        void OnLoad(JToken data);
    }

    /// <summary>
    /// Giao diện cho các đối tượng được sinh ra trong lúc chơi (Runtime) và cần được lưu trạng thái.
    /// </summary>
    public interface IRuntimeSaveable : ISaveable
    {
        UniqueID UniqueID { get; set; }
    }
}