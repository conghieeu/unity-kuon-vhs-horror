using Newtonsoft.Json.Linq;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Giao diện cho việc lưu/tải dữ liệu tùy chỉnh không thuộc logic save mặc định.
    /// </summary>
    public interface ISaveableCustom
    {
        /// <summary>
        /// Gọi để lưu dữ liệu tùy chỉnh.
        /// </summary>
        StorableCollection OnCustomSave();

        /// <summary>
        /// Gọi để tải dữ liệu tùy chỉnh.
        /// </summary>
        void OnCustomLoad(JToken data);
    }
}