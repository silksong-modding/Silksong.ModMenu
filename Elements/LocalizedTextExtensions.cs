using UnityEngine.UI;

namespace Silksong.ModMenu.Elements;

/// <summary>
/// Helper methods for working with LocalizedText.
/// </summary>
public static class LocalizedTextExtensions
{
    extension(Text self)
    {
        /// <summary>
        /// Helper field to set localized text on a Text component.
        /// </summary>
        public LocalizedText LocalizedText
        {
            get
            {
                if (self.TryGetComponent<AutoLocalizeTextUI>(out var auto) && !auto.text.IsEmpty)
                    return auto.text;
                else
                    return self.text;
            }
            set
            {
                if (value.IsLocalized)
                {
                    if (!self.TryGetComponent<AutoLocalizeTextUI>(out var auto))
                    {
                        auto = self.gameObject.AddComponent<AutoLocalizeTextUI>();
                        auto.textField = self;
                    }

                    auto.text = value.Localized;
                    auto.RefreshTextFromLocalization();
                }
                else
                {
                    if (self.TryGetComponent<AutoLocalizeTextUI>(out var auto))
                        UnityEngine.Object.Destroy(auto);
                    self.text = value.Text;
                }
            }
        }
    }
}
