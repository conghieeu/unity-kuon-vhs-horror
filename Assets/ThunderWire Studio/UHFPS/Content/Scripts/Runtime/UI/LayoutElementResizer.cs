using UnityEngine;
using UnityEngine.UI;

namespace UHFPS.Runtime
{
    public class LayoutElementResizer : LayoutElement
    {
        public bool AutoResizeWidth;
        public bool AutoResizeHeight;

        public bool CustomWidthResize;
        public bool CustomHeightResize;

        public RectTransform WidthTarget;
        public RectTransform HeightTarget;

        public float WidthPadding;
        public float HeightPadding;

        public override float preferredWidth
        {
            get
            {
                if (AutoResizeWidth)
                {
                    if (!CustomWidthResize)
                    {
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            Transform tr = transform.GetChild(i);
                            if (!tr.gameObject.activeSelf)
                                continue;

                            RectTransform rectTransform = tr as RectTransform;
                            return LayoutUtility.GetPreferredWidth(rectTransform) + WidthPadding;
                        }
                    }
                    else
                    {
                        return LayoutUtility.GetPreferredWidth(WidthTarget) + WidthPadding;
                    }
                }

                return base.preferredWidth;
            }
            set => base.preferredWidth = value;
        }

        public override float preferredHeight
        {
            get
            {
                if (AutoResizeHeight)
                {
                    if (!CustomWidthResize)
                    {
                        for (int i = 0; i < transform.childCount; i++)
                        {
                            Transform tr = transform.GetChild(i);
                            if (!tr.gameObject.activeSelf)
                                continue;

                            RectTransform rectTransform = tr as RectTransform;
                            return LayoutUtility.GetPreferredHeight(rectTransform) + HeightPadding;
                        }
                    }
                    else
                    {
                        return LayoutUtility.GetPreferredHeight(HeightTarget) + HeightPadding;
                    }
                }

                return base.preferredHeight;
            }
            set => base.preferredHeight = value;
        }
    }
}