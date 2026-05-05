using UnityEngine;
using UnityEngine.UI;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Summary("Mở rộng của LayoutElement, cho phép tự động thay đổi kích thước dựa trên các RectTransform mục tiêu hoặc các phần tử con.")]
    public class LayoutElementResizer : LayoutElement
    {
        [Tooltip("Tự động thay đổi chiều rộng.")]
        public bool AutoResizeWidth;
        [Tooltip("Tự động thay đổi chiều cao.")]
        public bool AutoResizeHeight;

        [Tooltip("Sử dụng một RectTransform cụ thể làm mục tiêu tính toán chiều rộng thay vì các con.")]
        public bool CustomWidthResize;
        [Tooltip("Sử dụng một RectTransform cụ thể làm mục tiêu tính toán chiều cao thay vì các con.")]
        public bool CustomHeightResize;

        [Tooltip("RectTransform mục tiêu để tính chiều rộng.")]
        public RectTransform WidthTarget;
        [Tooltip("RectTransform mục tiêu để tính chiều cao.")]
        public RectTransform HeightTarget;

        [Tooltip("Khoảng đệm thêm vào chiều rộng.")]
        public float WidthPadding;
        [Tooltip("Khoảng đệm thêm vào chiều cao.")]
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