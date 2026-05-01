using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails when a cue's start time falls strictly before the end time of any
/// earlier cue in the sequence. Adjacency (B.start == A.end) is allowed,
/// matching the industry convention that consecutive subtitles may touch.
/// </summary>
public sealed class OverlapCheckRule : IQcRule
{
    public const string RuleId = "OverlapCheck";

    public string Id => RuleId;

    public QcResult Evaluate(Cue cue, QcEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(cue);
        ArgumentNullException.ThrowIfNull(context);
        Cue? offender = FindEarlierOverlap(cue, context);
        if (offender is null)
        {
            return new QcResult(Id, cue.Id, QcStatus.Passed);
        }

        string message = $"Cue overlaps earlier cue '{offender.Id}' ending at {offender.End}.";
        return new QcResult(Id, cue.Id, QcStatus.Failed, message);
    }

    private static Cue? FindEarlierOverlap(Cue cue, QcEvaluationContext context)
    {
        for (int i = 0; i < context.CurrentIndex; i++)
        {
            Cue prior = context.AllCues[i];
            if (cue.Start < prior.End)
            {
                return prior;
            }
        }

        return null;
    }
}
