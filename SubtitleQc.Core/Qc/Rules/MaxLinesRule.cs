using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails when a cue exceeds the configured maximum number of text lines.
/// Equality with the threshold is intentionally allowed (industry convention:
/// "max" is inclusive).
/// </summary>
public sealed class MaxLinesRule : IQcRule
{
    public const string RuleId = "MaxLines";

    private readonly int _threshold;

    public MaxLinesRule(int threshold)
    {
        if (threshold < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(threshold), "MaxLines threshold must be >= 1.");
        }

        _threshold = threshold;
    }

    public string Id => RuleId;

    public QcResult Evaluate(Cue cue, QcEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(cue);
        int lineCount = cue.Lines.Count;
        if (lineCount > _threshold)
        {
            string message = $"Cue has {lineCount} lines (threshold {_threshold}).";
            return new QcResult(Id, cue.Id, QcStatus.Failed, message);
        }

        return new QcResult(Id, cue.Id, QcStatus.Passed);
    }
}
