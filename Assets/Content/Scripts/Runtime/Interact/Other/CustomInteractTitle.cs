using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/interactions#changing-interact-title")]
    [Summary("Ghi đè tiêu đề (Title) và các nút tương tác (Use/Examine) hiển thị trên UI khi người chơi nhìn vào vật thể.")]
    public class CustomInteractTitle : MonoBehaviour, IInteractTitle
    {
        [Tooltip("Biến bool động dùng để thay đổi Tiêu đề chính (Title).")]
        public ReflectionField DynamicTitle;

        [Tooltip("Biến bool động dùng để thay đổi Tên nút Tương tác (Use).")]
        public ReflectionField DynamicUseTitle;

        [Tooltip("Biến bool động dùng để thay đổi Tên nút Xem xét (Examine).")]
        public ReflectionField DynamicExamineTitle;

        [Tooltip("Bật tính năng ghi đè Tiêu đề chính.")]
        public bool OverrideTitle;

        [Tooltip("Bật tính năng ghi đè Tên nút Tương tác (Use).")]
        public bool OverrideUseTitle;

        [Tooltip("Bật tính năng ghi đè Tên nút Xem xét (Examine).")]
        public bool OverrideExamineTitle;

        [Tooltip("Sử dụng Tiêu đề chính dạng động (Theo biến DynamicTitle).")]
        public bool UseTitleDynamic;

        [Tooltip("Sử dụng Tên nút Tương tác dạng động.")]
        public bool UseUseTitleDynamic;

        [Tooltip("Sử dụng Tên nút Xem xét dạng động.")]
        public bool UseExamineTitleDynamic;

        [Tooltip("Nội dung Tiêu đề chính.")]
        public GString Title;
        [Tooltip("Tiêu đề chính khi DynamicTitle = True.")]
        public GString TrueTitle;
        [Tooltip("Tiêu đề chính khi DynamicTitle = False.")]
        public GString FalseTitle;

        [Tooltip("Nội dung Tên nút Tương tác (Use).")]
        public GString UseTitle;
        [Tooltip("Tên nút Tương tác khi DynamicUseTitle = True.")]
        public GString TrueUseTitle;
        [Tooltip("Tên nút Tương tác khi DynamicUseTitle = False.")]
        public GString FalseUseTitle;

        [Tooltip("Nội dung Tên nút Xem xét (Examine).")]
        public GString ExamineTitle;
        [Tooltip("Tên nút Xem xét khi DynamicExamineTitle = True.")]
        public GString TrueExamineTitle;
        [Tooltip("Tên nút Xem xét khi DynamicExamineTitle = False.")]
        public GString FalseExamineTitle;

        private void Start()
        {
            if (OverrideTitle)
            {
                if (UseTitleDynamic)
                {
                    TrueTitle.SubscribeGloc();
                    FalseTitle.SubscribeGloc();
                    Title = DynamicTitle.Value ? TrueTitle : FalseTitle;
                }
                else
                {
                    Title.SubscribeGloc();
                }
            }

            if (OverrideUseTitle)
            {
                if (UseUseTitleDynamic)
                {
                    TrueUseTitle.SubscribeGlocMany();
                    FalseUseTitle.SubscribeGlocMany();
                    UseTitle = DynamicUseTitle.Value ? TrueUseTitle : FalseUseTitle;
                }
                else
                {
                    UseTitle.SubscribeGlocMany();
                }
            }

            if (OverrideExamineTitle)
            {
                if (UseExamineTitleDynamic)
                {
                    TrueExamineTitle.SubscribeGlocMany();
                    FalseExamineTitle.SubscribeGlocMany();
                    ExamineTitle = DynamicExamineTitle.Value ? TrueExamineTitle : FalseExamineTitle;
                }
                else
                {
                    ExamineTitle.SubscribeGlocMany();
                }
            }
        }

        public TitleParams InteractTitle()
        {
            string title = Title;
            string useTitle = UseTitle;
            string examineTitle = ExamineTitle;

            if (!OverrideTitle) title = null;
            else if (UseTitleDynamic) title = DynamicTitle.Value ? TrueTitle : FalseTitle;

            if (!OverrideUseTitle) useTitle = null;
            else if (UseUseTitleDynamic) useTitle = DynamicUseTitle.Value ? TrueUseTitle : FalseUseTitle;

            if (!OverrideExamineTitle) examineTitle = null;
            else if (UseExamineTitleDynamic) examineTitle = DynamicExamineTitle.Value ? TrueExamineTitle : FalseExamineTitle;

            return new TitleParams()
            {
                title = title,
                button1 = useTitle,
                button2 = examineTitle
            };
        }
    }
}