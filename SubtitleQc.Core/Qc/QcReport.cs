using System.Collections.Generic;
using System.Linq;

namespace SubtitleQc.Core.Qc;

/// <summary>
/// Aggregated, immutable view of a QC run. Iterating the flat result list
/// keeps consumers (UI, CLI, JSON exporter) decoupled from rule internals.
/// </summary>
public sealed class QcReport
{
    public QcReport(IEnumerable<QcResult> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        Results = results.ToArray();
    }

    public IReadOnlyList<QcResult> Results { get; }

    public bool HasFailures => Results.Any(r => r.Status == QcStatus.Failed);
}
