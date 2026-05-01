using System.Collections.Generic;
using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Qc;

/// <summary>
/// Per-cue evaluation context exposed to rules. Inter-cue rules (e.g.
/// overlap detection) need access to neighbours; rules that only inspect a
/// single cue can ignore the surrounding state. Centralising this avoids
/// making every rule re-implement collection traversal.
/// </summary>
public sealed class QcEvaluationContext
{
    public QcEvaluationContext(IReadOnlyList<Cue> allCues, int currentIndex)
    {
        ArgumentNullException.ThrowIfNull(allCues);
        if (currentIndex < 0 || currentIndex >= allCues.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(currentIndex));
        }

        AllCues = allCues;
        CurrentIndex = currentIndex;
    }

    public IReadOnlyList<Cue> AllCues { get; }

    public int CurrentIndex { get; }

    public Cue Current => AllCues[CurrentIndex];
}
