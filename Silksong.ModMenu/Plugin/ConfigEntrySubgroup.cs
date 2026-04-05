using System.Collections.Generic;
using System.Linq;
using Silksong.ModMenu.Elements;

namespace Silksong.ModMenu.Plugin;

/// <summary>
/// A config entry attribute that designates how this element should be organized into subpages hierarchically.
/// If present, this overrides the default hierarchy names generated (or not generated) by ConfigEntryFactory.
/// </summary>
public record ConfigEntrySubgroup
{
    /// <summary>
    /// Construct subgroup names from a list.
    /// </summary>
    public ConfigEntrySubgroup(IEnumerable<LocalizedText> subgroups) => Subgroups = [.. subgroups];

    /// <summary>
    /// Construct subgroup names from explicit parameters.
    /// </summary>
    public ConfigEntrySubgroup(LocalizedText name1, params LocalizedText[] otherNames) =>
        Subgroups = [.. otherNames.Prepend(name1)];

    /// <summary>
    /// The subgroup names that designate this config element's place in the subpage hierarchy. An empty list designates the root page.
    /// </summary>
    public readonly IReadOnlyList<LocalizedText> Subgroups;
}
