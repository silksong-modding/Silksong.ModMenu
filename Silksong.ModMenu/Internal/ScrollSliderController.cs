using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Visibility = UnityEngine.UI.ScrollRect.ScrollbarVisibility;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// A component which enables <see cref="Slider"/>s to act as a scrollbars
/// for the attached <see cref="ScrollRect"/>.
/// </summary>
/// <remarks>
/// This is used because Slider handles can have a fixed size and
/// <see cref="Scrollbar"/> handles cannot.
/// </remarks>
[RequireComponent(typeof(ScrollRect))]
internal class ScrollSliderController : UIBehaviour
{
    /// <summary>
    /// A slider that acts as a vertical scrollbar for the attached ScrollRect.
    /// </summary>
    public Slider VerticalSlider
    {
        get => m_verticalSlider;
        set
        {
            if (m_verticalSlider)
                m_verticalSlider.onValueChanged.RemoveListener(UpdateScrollRectVertical);
            m_verticalSlider = value;
            if (m_verticalSlider)
                m_verticalSlider.onValueChanged.AddListener(UpdateScrollRectVertical);
        }
    }

    /// <summary>
    /// A slider that acts as a horizontal scrollbar for the attached ScrollRect.
    /// </summary>
    public Slider HorizontalSlider
    {
        get => m_horizontalSlider;
        set
        {
            if (m_horizontalSlider)
                m_horizontalSlider.onValueChanged.RemoveListener(UpdateScrollRectHorizontal);
            m_horizontalSlider = value;
            if (m_horizontalSlider)
                m_horizontalSlider.onValueChanged.AddListener(UpdateScrollRectHorizontal);
        }
    }

    [SerializeField]
    Slider m_verticalSlider;

    [SerializeField]
    Slider m_horizontalSlider;

    ScrollRect scrollRect;

    #region Unity Messages

    protected override void Awake() => scrollRect = GetComponent<ScrollRect>();

    protected override void OnEnable()
    {
        if (VerticalSlider)
            VerticalSlider.onValueChanged.AddListener(UpdateScrollRectVertical);
        if (HorizontalSlider)
            HorizontalSlider.onValueChanged.AddListener(UpdateScrollRectHorizontal);

        scrollRect.onValueChanged.AddListener(UpdateVerticalSlider);
        scrollRect.onValueChanged.AddListener(UpdateHorizontalSlider);
    }

    protected override void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(UpdateVerticalSlider);
        scrollRect.onValueChanged.RemoveListener(UpdateHorizontalSlider);

        if (VerticalSlider)
            VerticalSlider.onValueChanged.RemoveListener(UpdateScrollRectVertical);
        if (HorizontalSlider)
            HorizontalSlider.onValueChanged.RemoveListener(UpdateScrollRectHorizontal);
    }

    protected void LateUpdate()
    {
        if (!scrollRect || !scrollRect.content || !scrollRect.viewport)
            return;

        Rect viewRect = scrollRect.viewport.rect,
            contentRect = scrollRect.content.rect;

        if (VerticalSlider)
            VerticalSlider.gameObject.SetActive(
                scrollRect.vertical
                    && (
                        scrollRect.verticalScrollbarVisibility == Visibility.Permanent
                        || contentRect.height > viewRect.height + 0.01f
                    )
            );

        if (HorizontalSlider)
            HorizontalSlider.gameObject.SetActive(
                scrollRect.horizontal
                    && (
                        scrollRect.horizontalScrollbarVisibility == Visibility.Permanent
                        || contentRect.width > viewRect.width + 0.01f
                    )
            );
    }

    #endregion
    #region Event Handlers

    void UpdateVerticalSlider(Vector2 v)
    {
        if (VerticalUpdateNeeded())
            VerticalSlider.normalizedValue = scrollRect.normalizedPosition.y;
    }

    void UpdateHorizontalSlider(Vector2 v)
    {
        if (HorizontalUpdateNeeded())
            HorizontalSlider.normalizedValue = scrollRect.normalizedPosition.x;
    }

    void UpdateScrollRectVertical(float v)
    {
        if (VerticalUpdateNeeded())
            scrollRect.verticalNormalizedPosition = VerticalSlider.normalizedValue;
    }

    void UpdateScrollRectHorizontal(float v)
    {
        if (HorizontalUpdateNeeded())
            scrollRect.horizontalNormalizedPosition = HorizontalSlider.normalizedValue;
    }

    #endregion
    #region Utils

    bool VerticalUpdateNeeded() =>
        scrollRect.vertical
        && VerticalSlider
        && !Mathf.Approximately(scrollRect.normalizedPosition.y, VerticalSlider.normalizedValue);

    bool HorizontalUpdateNeeded() =>
        scrollRect.horizontal
        && HorizontalSlider
        && !Mathf.Approximately(scrollRect.normalizedPosition.x, HorizontalSlider.normalizedValue);

    #endregion
}
