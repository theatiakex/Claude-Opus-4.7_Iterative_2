using System.Collections.Generic;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails when a cue strictly spans across a shot change. Cuts that fall
/// exactly on a cue's start or end boundary are tolerated, matching the
/// industry convention that a cue may begin or end *on* a cut but must not
/// straddle one.
/// </summary>
public sealed class CrossShotBoundaryCheckRule : IQcRule
{
    public const string RuleId = "CrossShotBoundaryCheck";

    private readonly IShotChangeProvider _shotChangeProvider;

    public CrossShotBoundaryCheckRule(IShotChangeProvider shotChangeProvider)
    {
        ArgumentNullException.ThrowIfNull(shotChangeProvider);
        _shotChangeProvider = shotChangeProvider;
    }

    public string Id => RuleId;

    public QcResult Evaluate(Cue cue, QcEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(cue);
        IReadOnlyList<TimeSpan> cuts = _shotChangeProvider.GetShotChangeTimestamps();
        TimeSpan? offender = FindCrossingCut(cue, cuts);
        if (offender is null)
        {
            return new QcResult(Id, cue.Id, QcStatus.Passed);
        }

        string message = $"Cue spans shot change at {offender.Value}.";
        return new QcResult(Id, cue.Id, QcStatus.Failed, message);
    }

    private static TimeSpan? FindCrossingCut(Cue cue, IReadOnlyList<TimeSpan> cuts)
    {
        foreach (TimeSpan cut in cuts)
        {
            if (cut > cue.Start && cut < cue.End)
            {
                return cut;
            }
        }

        return null;
    }
}
