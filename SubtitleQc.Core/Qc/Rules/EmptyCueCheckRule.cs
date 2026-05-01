using System.Linq;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails when a cue carries no visible text (every line is null, empty, or
/// whitespace). Whitespace-only cues commonly slip through manual editing and
/// are treated as a defect rather than an absence.
/// </summary>
public sealed class EmptyCueCheckRule : IQcRule
{
    public const string RuleId = "EmptyCueCheck";

    public string Id => RuleId;

    public QcResult Evaluate(Cue cue, QcEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(cue);
        bool hasContent = cue.Lines.Any(line => !string.IsNullOrWhiteSpace(line));
        if (!hasContent)
        {
            return new QcResult(Id, cue.Id, QcStatus.Failed, "Cue has no visible text content.");
        }

        return new QcResult(Id, cue.Id, QcStatus.Passed);
    }
}
