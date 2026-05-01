namespace SubtitleQc.Core.Qc;

/// <summary>
/// Single rule-vs-cue evaluation outcome. Carries the rule and cue identifiers
/// so the report stays traceable end-to-end without coupling to a specific rule
/// implementation. Designed to be JSON-serializable.
/// </summary>
public sealed class QcResult
{
    public QcResult(string ruleId, string cueId, QcStatus status, string? message = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ruleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(cueId);
        RuleId = ruleId;
        CueId = cueId;
        Status = status;
        Message = message;
    }

    public string RuleId { get; }

    public string CueId { get; }

    public QcStatus Status { get; }

    public string? Message { get; }
}
