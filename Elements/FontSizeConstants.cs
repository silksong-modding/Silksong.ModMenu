namespace Silksong.ModMenu.Elements;

/// <summary>
/// Standard font sizes.
/// </summary>
public static class FontSizeConstants
{
    public const int CHOICE_SMALL = 24;
    public const int CHOICE_MEDIUM = 38;
    public const int CHOICE_LARGE = 56;

    public static int ChoiceSize(this FontSizes self) =>
        self switch
        {
            FontSizes.Small => CHOICE_SMALL,
            FontSizes.Medium => CHOICE_MEDIUM,
            FontSizes.Large => CHOICE_LARGE,
            _ => CHOICE_MEDIUM,
        };

    public const int DESCRIPTION_SMALL = 4;
    public const int DESCRIPTION_MEDIUM = 6;
    public const int DESCRIPTION_LARGE = 9;

    public static int DescriptionSize(this FontSizes self) =>
        self switch
        {
            FontSizes.Small => DESCRIPTION_SMALL,
            FontSizes.Medium => DESCRIPTION_MEDIUM,
            FontSizes.Large => DESCRIPTION_LARGE,
            _ => DESCRIPTION_MEDIUM,
        };

    public const int LABEL_SMALL = 30;
    public const int LABEL_MEDIUM = 45;
    public const int LABEL_LARGE = 68;

    public static int LabelSize(this FontSizes self) =>
        self switch
        {
            FontSizes.Small => LABEL_SMALL,
            FontSizes.Medium => LABEL_MEDIUM,
            FontSizes.Large => LABEL_LARGE,
            _ => LABEL_MEDIUM,
        };

    public const int SLIDER_SMALL = 22;
    public const int SLIDER_MEDIUM = 32;
    public const int SLIDER_LARGE = 48;

    public static int SliderSize(this FontSizes self) =>
        self switch
        {
            FontSizes.Small => SLIDER_SMALL,
            FontSizes.Medium => SLIDER_MEDIUM,
            FontSizes.Large => SLIDER_LARGE,
            _ => SLIDER_MEDIUM,
        };
}
