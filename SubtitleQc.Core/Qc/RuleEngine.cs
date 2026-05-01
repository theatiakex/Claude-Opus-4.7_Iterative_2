using System.Collections.Generic;
using System.Linq;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc;

/// <summary>
/// Orchestrates rule evaluation over a sequence of cues. The engine is closed
/// for modification: rule sets are injected, and the traversal logic itself is
/// stable across iterations.
/// </summary>
public sealed class RuleEngine
{
    private readonly IReadOnlyList<IQcRule> _rules;

    public RuleEngine(IEnumerable<IQcRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);
        _rules = rules.ToArray();
    }

    public QcReport Evaluate(IEnumerable<Cue> cues)
    {
        ArgumentNullException.ThrowIfNull(cues);
        IReadOnlyList<Cue> materialized = cues.ToArray();
        List<QcResult> results = new(capacity: materialized.Count * _rules.Count);
        for (int i = 0; i < materialized.Count; i++)
        {
            results.AddRange(EvaluateCue(materialized, i));
        }

        return new QcReport(results);
    }

    private IEnumerable<QcResult> EvaluateCue(IReadOnlyList<Cue> cues, int index)
    {
        QcEvaluationContext context = new(cues, index);
        Cue cue = context.Current;
        foreach (IQcRule rule in _rules)
        {
            yield return rule.Evaluate(cue, context);
        }
    }
}
