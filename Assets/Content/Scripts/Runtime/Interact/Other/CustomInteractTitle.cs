using ThunderWire.Attributes;
using UnityEngine;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/interactions#changing-interact-title")]
    public class CustomInteractTitle : MonoBehaviour, IInteractTitle
    {
        public ReflectionField DynamicTitle;
        public ReflectionField DynamicUseTitle;
        public ReflectionField DynamicExamineTitle;

        public bool OverrideTitle;
        public bool OverrideUseTitle;
        public bool OverrideExamineTitle;

        public bool UseTitleDynamic;
        public bool UseUseTitleDynamic;
        public bool UseExamineTitleDynamic;

        public GString Title;
        public GString TrueTitle;
        public GString FalseTitle;

        public GString UseTitle;
        public GString TrueUseTitle;
        public GString FalseUseTitle;

        public GString ExamineTitle;
        public GString TrueExamineTitle;
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