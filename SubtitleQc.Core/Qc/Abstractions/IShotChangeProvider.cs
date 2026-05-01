using System.Collections.Generic;

namespace SubtitleQc.Core.Qc.Abstractions;

/// <summary>
/// Source of external shot-change (visual cut) data. Exposed as an interface
/// so production sources (file readers, external services) and test stubs
/// alike can satisfy the same contract without QC rules taking a dependency
/// on any concrete implementation (DIP).
/// </summary>
/// <remarks>
/// Two projections are provided because the consuming rules operate in
/// distinct domains: cross-boundary detection compares against the timing
/// timeline, while minimum-frame-distance checks compare against integer
/// frame indices. A given provider may legitimately expose only one of the
/// two and return an empty list for the other.
/// </remarks>
public interface IShotChangeProvider
{
    IReadOnlyList<TimeSpan> GetShotChangeTimestamps();

    IReadOnlyList<int> GetShotChangeFrames();
}
