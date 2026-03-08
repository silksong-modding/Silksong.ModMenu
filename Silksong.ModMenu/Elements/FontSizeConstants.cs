namespace Silksong.ModMenu.Elements;

/// <summary>
/// Standard font sizes.
/// </summary>
public static class FontSizeConstants
{
    /// <summary>
    /// Small font size for a selected choice string.
    /// </summary>
    public const int CHOICE_SMALL = 24;

    /// <summary>
    /// Medium font size for a selected choice string.
    /// </summary>
    public const int CHOICE_MEDIUM = 38;

    /// <summary>
    /// Large font size for a selected choice string.
    /// </summary>
    public const int CHOICE_LARGE = 56;

    /// <summary>
    /// Corresponding font size for a selected choice string.
    /// </summary>
    public static int ChoiceSize(this FontSizes self) =>
        self switch
        {
            FontSizes.Small => CHOICE_SMALL,
            FontSizes.Medium => CHOICE_MEDIUM,
            FontSizes.Large => CHOICE_LARGE,
            _ => CHOICE_MEDIUM,
        };

    /// <summary>
    /// Small font size for a choice description string.
    /// </summary>
    public const int DESCRIPTION_SMALL = 4;

    /// <summary>
    /// Standard font size for a choice description string.
    /// </summary>
    public const int DESCRIPTION_MEDIUM = 6;

    /// <summary>
    /// Large font size for a choice description string.
    /// </summary>
    public const int DESCRIPTION_LARGE = 9;

    /// <summary>
    /// Corresponding font size for a choice description string.
    /// </summary>
    public static int DescriptionSize(this FontSizes self) =>
        self switch
        {
            FontSizes.Small => DESCRIPTION_SMALL,
            FontSizes.Medium => DESCRIPTION_MEDIUM,
            FontSizes.Large => DESCRIPTION_LARGE,
            _ => DESCRIPTION_MEDIUM,
        };

    /// <summary>
    /// Small font size for a basic label string.
    /// </summary>
    public const int LABEL_SMALL = 30;

    /// <summary>
    /// Standard font size for a basic label string.
    /// </summary>
    public const int LABEL_MEDIUM = 45;

    /// <summary>
    /// Large font size for a basic label string.
    /// </summary>
    public const int LABEL_LARGE = 68;

    /// <summary>
    /// Corresponding font size for a basic label string.
    /// </summary>
    public static int LabelSize(this FontSizes self) =>
        self switch
        {
            FontSizes.Small => LABEL_SMALL,
            FontSizes.Medium => LABEL_MEDIUM,
            FontSizes.Large => LABEL_LARGE,
            _ => LABEL_MEDIUM,
        };

    /// <summary>
    /// Small font size for a slider value string.
    /// </summary>
    public const int SLIDER_SMALL = 22;

    /// <summary>
    /// Standard font size for a slider value string.
    /// </summary>
    public const int SLIDER_MEDIUM = 32;

    /// <summary>
    /// Large font size for a slider value string.
    /// </summary>
    public const int SLIDER_LARGE = 48;

    /// <summary>
    /// Corresponding font size for a slider value string.
    /// </summary>
    public static int SliderSize(this FontSizes self) =>
        self switch
        {
            FontSizes.Small => SLIDER_SMALL,
            FontSizes.Medium => SLIDER_MEDIUM,
            FontSizes.Large => SLIDER_LARGE,
            _ => SLIDER_MEDIUM,
        };
}
