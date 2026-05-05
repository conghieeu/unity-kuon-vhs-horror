using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [InspectorHeader("Option Separator")]
    [Summary("Thành phần UI đóng vai trò như một thanh phân cách hoặc tiêu đề phụ trong menu Options.")]
    public class OptionsSeparator : OptionBehaviour
    {
        public override void SetOptionValue(object value) { }
        public override object GetOptionValue() { return 0; }

        public override void SetOptionData(StorableCollection data)
        {
            if (data.TryGetValue("text", out GString text))
                if (Title.TryGetComponent(out GLocText gloc))
                    gloc.GlocKey = new(text);
        }
    }
}