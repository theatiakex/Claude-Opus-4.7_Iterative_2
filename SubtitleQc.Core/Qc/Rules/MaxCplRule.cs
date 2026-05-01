using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails when any line in a cue exceeds the configured character-per-line
/// limit. The first offending line is reported to keep messages actionable.
/// </summary>
public sealed class MaxCplRule : IQcRule
{
    public const string RuleId = "MaxCpl";

    private readonly int _threshold;

    public MaxCplRule(int threshold)
    {
        if (threshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "MaxCpl threshold must be >= 1.");
        }

        _threshold = threshold;
    }

    public string Id => RuleId;

    public QcResult Evaluate(Cue cue, QcEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(cue);
        for (int i = 0; i < cue.Lines.Count; i++)
        {
            string line = cue.Lines[i] ?? string.Empty;
            if (line.Length > _threshold)
            {
                string message = $"Line {i + 1} length {line.Length} exceeds threshold {_threshold}.";
                return new QcResult(Id, cue.Id, QcStatus.Failed, message);
            }
        }

        return new QcResult(Id, cue.Id, QcStatus.Passed);
    }
}
