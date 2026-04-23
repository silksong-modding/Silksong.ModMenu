using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        if (scrollRect.vertical && VerticalSlider)
            VerticalSlider.gameObject.SetActive(
                scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent
                    || scrollRect.content.sizeDelta.y > scrollRect.viewport.sizeDelta.y
            );

        if (scrollRect.horizontal && HorizontalSlider)
            HorizontalSlider.gameObject.SetActive(
                scrollRect.horizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent
                    || scrollRect.content.sizeDelta.x > scrollRect.viewport.sizeDelta.x
            );
    }

    void UpdateScrollRect(float _)
    {
        if (!scrollRect)
            return;

        if (scrollRect.vertical && VerticalSlider)
            scrollRect.verticalNormalizedPosition = VerticalSlider.normalizedValue;

        if (scrollRect.horizontal && HorizontalSlider)
            scrollRect.horizontalNormalizedPosition = HorizontalSlider.normalizedValue;
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
