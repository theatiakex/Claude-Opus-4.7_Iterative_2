using SubtitleQc.Core.Models;

namespace SubtitleQc.Core.Qc.Abstractions;

/// <summary>
/// QC rule contract. The engine depends on this abstraction (DIP), enabling
/// new rules (and future rule families such as shot-change validation) to be
/// added without modifying the engine itself (OCP).
/// </summary>
public interface IQcRule
{
    /// <summary>
    /// Stable identifier used in reports. Must be unique among configured rules.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Evaluates a single cue. Returning a <see cref="QcResult"/> per call keeps
    /// the engine's traversal logic generic and predictable.
    /// </summary>
    QcResult Evaluate(Cue cue, QcEvaluationContext context);
}
