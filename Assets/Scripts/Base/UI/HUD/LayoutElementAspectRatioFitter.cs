using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayoutElementAspectRatioFitter : UIBehaviour, ILayoutElement, ILayoutIgnorer
{
    [SerializeField][SerializeProperty("ignoreLayout")] private bool m_IgnoreLayout;
    [SerializeField][SerializeProperty("minWidth")] private float m_MinWidth;
    [SerializeField][SerializeProperty("minHeight")] private float m_MinHeight;
    [SerializeField][SerializeProperty("preferredWidth")] private float m_PreferredWidth;
    [SerializeField][SerializeProperty("preferredHeight")] private float m_PreferredHeight;
    [SerializeField][SerializeProperty("flexibleWidth")] private float m_FlexibleWidth;
    [SerializeField][SerializeProperty("flexibleHeight")] private float m_FlexibleHeight;
    [SerializeField][SerializeProperty("layoutPriority")] private int m_LayoutPriority = 1;

    public enum Mode
    {
        WidthControlsHeight,
        HeightControlsWidth,
    }

    public Mode mode;

    public float AspectRatio
    {
        get => aspectRatio;
        set
        {
            aspectRatio = Mathf.Max(0.01f, value);
            SetDirty();
        }
    }
    [SerializeProperty("AspectRatio")]
    [SerializeField]
    private float aspectRatio = 1;

    public bool ignoreLayout { get => m_IgnoreLayout; set { m_IgnoreLayout = value; SetDirty(); } }
    public float minWidth { get => m_MinWidth; set { m_MinWidth = value; SetDirty(); } }
    public float minHeight { get => m_MinHeight; set { m_MinHeight = value; SetDirty(); } }
    public float preferredWidth { get => mode == Mode.HeightControlsWidth ? RectT.rect.height / aspectRatio : m_PreferredWidth; set { m_PreferredWidth = value; SetDirty(); } }
    public float preferredHeight { get => mode == Mode.WidthControlsHeight ? RectT.rect.width / aspectRatio : m_PreferredHeight; set { m_PreferredHeight = value; SetDirty(); } }
    public float flexibleWidth { get => m_FlexibleWidth; set { m_FlexibleWidth = value; SetDirty(); } }
    public float flexibleHeight { get => m_FlexibleHeight; set { m_FlexibleHeight = value; SetDirty(); } }
    public int layoutPriority { get => m_LayoutPriority; set { m_LayoutPriority = value; SetDirty(); } }

    private RectTransform RectT => rectT == null ? rectT = transform as RectTransform : rectT;
    private RectTransform rectT;

    public void CalculateLayoutInputHorizontal()
    {
    }

    public void CalculateLayoutInputVertical()
    {
    }

    protected void SetDirty()
    {
        if (!IsActive())
            return;
        LayoutRebuilder.MarkLayoutForRebuild(RectT);
    }
}
