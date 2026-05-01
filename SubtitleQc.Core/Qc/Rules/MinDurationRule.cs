using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails when a cue is displayed for less than the configured minimum.
/// Equality with the threshold is allowed so that integer-second thresholds
/// (e.g. 1 second) accept exact-boundary cues.
/// </summary>
public sealed class MinDurationRule : IQcRule
{
    public const string RuleId = "MinDuration";

    private readonly TimeSpan _threshold;

    public MinDurationRule(TimeSpan threshold)
    {
        if (threshold <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "MinDuration threshold must be > 0.");
        }

        _threshold = threshold;
    }

    public string Id => RuleId;

    public QcResult Evaluate(Cue cue, QcEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(cue);
        if (cue.Duration < _threshold)
        {
            string message = $"Duration {cue.Duration.TotalSeconds:0.###}s below threshold {_threshold.TotalSeconds:0.###}s.";
            return new QcResult(Id, cue.Id, QcStatus.Failed, message);
        }

        return new QcResult(Id, cue.Id, QcStatus.Passed);
    }
}
