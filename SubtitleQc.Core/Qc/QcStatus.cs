namespace SubtitleQc.Core.Qc;

/// <summary>
/// Outcome of evaluating a single QC rule against a single cue. Kept as a
/// minimal closed enum so reports remain serializable and machine-readable.
/// </summary>
public enum QcStatus
{
    Passed = 0,
    Failed = 1
}
