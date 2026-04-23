using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Silksong.ModMenu.Internal;

/// <summary>
/// A component which enables a <see cref="UnityEngine.UI.Slider"/> to act as a
/// vertical scrollbar for the attached <see cref="ScrollRect"/>.
/// </summary>
/// <remarks>
/// This is used because Slider handles can have a fixed size and
/// <see cref="Scrollbar"/> handles cannot.
/// </remarks>
[RequireComponent(typeof(ScrollRect))]
internal class VerticalScrollSlider : UIBehaviour
{
    public Slider Slider
    {
        get => m_Slider;
        set
        {
            if (m_Slider)
                m_Slider.onValueChanged.RemoveListener(SliderUpdatesScrollRect);
            m_Slider = value;
            if (m_Slider)
                m_Slider.onValueChanged.AddListener(SliderUpdatesScrollRect);
        }
    }

    [SerializeField]
    Slider m_Slider;

    ScrollRect scrollRect;

    protected override void Awake() => scrollRect = GetComponent<ScrollRect>();

    protected override void OnEnable()
    {
        scrollRect.onValueChanged.AddListener(ScrollRectUpdatesSlider);
        Slider = m_Slider;
    }

    protected override void OnDestroy()
    {
        scrollRect.onValueChanged.RemoveListener(ScrollRectUpdatesSlider);
        if (Slider)
            Slider.onValueChanged.RemoveListener(SliderUpdatesScrollRect);
    }

    protected void LateUpdate()
    {
        if (!scrollRect || !scrollRect.content || !scrollRect.viewport || !Slider)
            return;

        Slider.gameObject.SetActive(
            scrollRect.verticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent
                || scrollRect.content.sizeDelta.y > scrollRect.viewport.sizeDelta.y
        );
    }

    void SliderUpdatesScrollRect(float _)
    {
        if (scrollRect && Slider)
            scrollRect.normalizedPosition = new(
                scrollRect.normalizedPosition.x,
                Slider.normalizedValue
            );
    }

    void ScrollRectUpdatesSlider(Vector2 _)
    {
        if (scrollRect && Slider)
            Slider.normalizedValue = scrollRect.verticalNormalizedPosition;
    }
}
