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
                m_verticalSlider.onValueChanged.RemoveListener(UpdateScrollRect);
            m_verticalSlider = value;
            if (m_verticalSlider)
                m_verticalSlider.onValueChanged.AddListener(UpdateScrollRect);
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
                m_horizontalSlider.onValueChanged.RemoveListener(UpdateScrollRect);
            m_horizontalSlider = value;
            if (m_horizontalSlider)
                m_horizontalSlider.onValueChanged.AddListener(UpdateScrollRect);
        }
    }

    [SerializeField]
    Slider m_verticalSlider;

    [SerializeField]
    Slider m_horizontalSlider;

    ScrollRect scrollRect;

    protected override void Awake() => scrollRect = GetComponent<ScrollRect>();

    protected override void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(UpdateSliders);

        if (scrollRect.vertical && VerticalSlider)
            VerticalSlider.onValueChanged.AddListener(UpdateScrollRect);
        if (scrollRect.horizontal && HorizontalSlider)
            HorizontalSlider.onValueChanged.AddListener(UpdateScrollRect);

        UpdateSliders(default);
    }

    protected override void OnDisable()
    {
        scrollRect.onValueChanged.RemoveListener(UpdateSliders);
        if (VerticalSlider)
            VerticalSlider.onValueChanged.RemoveListener(UpdateScrollRect);
        if (HorizontalSlider)
            HorizontalSlider.onValueChanged.RemoveListener(UpdateScrollRect);
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
                        || contentRect.height > viewRect.height
                    )
            );

        if (HorizontalSlider)
            HorizontalSlider.gameObject.SetActive(
                scrollRect.horizontal
                    && (
                        scrollRect.horizontalScrollbarVisibility == Visibility.Permanent
                        || contentRect.width > viewRect.width
                    )
            );
    }

    void UpdateScrollRect(float _)
    {
        if (!scrollRect)
            return;

        Vector2 pos = Vector2.one * 0.5f;

        if (scrollRect.vertical && VerticalSlider)
            pos = pos with { y = VerticalSlider.normalizedValue };

        if (scrollRect.horizontal && HorizontalSlider)
            pos = pos with { x = HorizontalSlider.normalizedValue };

        scrollRect.normalizedPosition = pos;
    }

    void UpdateSliders(Vector2 _)
    {
        if (!scrollRect)
            return;

        if (scrollRect.vertical && VerticalSlider)
            VerticalSlider.normalizedValue = scrollRect.verticalNormalizedPosition;

        if (scrollRect.horizontal && HorizontalSlider)
            HorizontalSlider.normalizedValue = scrollRect.horizontalNormalizedPosition;
    }
}
