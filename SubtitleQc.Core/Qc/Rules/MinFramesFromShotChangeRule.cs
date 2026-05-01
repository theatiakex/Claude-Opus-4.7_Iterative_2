using System.Collections.Generic;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc.Abstractions;

namespace SubtitleQc.Core.Qc.Rules;

/// <summary>
/// Fails when a cue's start frame is closer to any shot change than the
/// configured minimum-frame gap. Only the cue *start* is checked, per the
/// spec wording ("a cue starts too close to a cut"). Strict less-than on the
/// gap means a cue exactly at the boundary (gap == threshold) passes,
/// matching the industry interpretation of a "minimum frames" requirement.
/// </summary>
/// <remarks>
/// Cues that lack <see cref="Cue.StartFrame"/> data (i.e. were sourced from a
/// time-only format with no frame mapping) cannot be compared in the frame
/// domain and are reported as Passed rather than failing or throwing. This
/// keeps the rule composable with formats that don't carry frame indices.
/// </remarks>
public sealed class MinFramesFromShotChangeRule : IQcRule
{
    public const string RuleId = "MinFramesFromShotChange";

    private readonly IShotChangeProvider _shotChangeProvider;
    private readonly int _thresholdFrames;

    public MinFramesFromShotChangeRule(IShotChangeProvider shotChangeProvider, int thresholdFrames)
    {
        ArgumentNullException.ThrowIfNull(shotChangeProvider);
        if (thresholdFrames < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(thresholdFrames), "Threshold must be >= 1 frame.");
        }

        _shotChangeProvider = shotChangeProvider;
        _thresholdFrames = thresholdFrames;
    }

    public string Id => RuleId;

    public QcResult Evaluate(Cue cue, QcEvaluationContext context)
    {
        ArgumentNullException.ThrowIfNull(cue);
        if (cue.StartFrame is not int startFrame)
        {
            return new QcResult(Id, cue.Id, QcStatus.Passed);
        }

        IReadOnlyList<int> cuts = _shotChangeProvider.GetShotChangeFrames();
        int? offender = FindTooCloseCut(startFrame, cuts);
        if (offender is null)
        {
            return new QcResult(Id, cue.Id, QcStatus.Passed);
        }

        return BuildFailure(cue.Id, startFrame, offender.Value);
    }

    private QcResult BuildFailure(string cueId, int startFrame, int cutFrame)
    {
        int gap = Math.Abs(startFrame - cutFrame);
        string message = $"Cue starts {gap} frame(s) from cut at frame {cutFrame} (threshold {_thresholdFrames}).";
        return new QcResult(Id, cueId, QcStatus.Failed, message);
    }

    private int? FindTooCloseCut(int startFrame, IReadOnlyList<int> cuts)
    {
        foreach (int cut in cuts)
        {
            if (Math.Abs(startFrame - cut) < _thresholdFrames)
            {
                return cut;
            }
        }

        return null;
    }
}
