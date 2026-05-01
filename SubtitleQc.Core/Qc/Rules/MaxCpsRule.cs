using System.Linq;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails when the cue's reading speed (characters per second of display time)
/// exceeds the configured threshold. Zero/negative durations are treated as
/// invalid input rather than infinite reading speed: callers should run
/// <see cref="MinDurationRule"/> alongside if they want to surface those.
/// </summary>
public sealed class MaxCpsRule : IQcRule
{
    public const string RuleId = "MaxCps";

    private readonly double _threshold;

    public MaxCpsRule(double threshold)
    {
        if (threshold <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "MaxCps threshold must be > 0.");
        }

        _threshold = threshold;
    }

    public string Id => RuleId;

    public QcResult Evaluate(Cue cue, QcEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(cue);
        double seconds = cue.Duration.TotalSeconds;
        if (seconds <= 0)
        {
            return new QcResult(Id, cue.Id, QcStatus.Passed);
        }

        int characters = cue.Lines.Sum(line => (line ?? string.Empty).Length);
        double cps = characters / seconds;
        if (cps > _threshold)
        {
            string message = $"Reading speed {cps:0.##} cps exceeds threshold {_threshold:0.##}.";
            return new QcResult(Id, cue.Id, QcStatus.Failed, message);
        }

        return new QcResult(Id, cue.Id, QcStatus.Passed);
    }
}
