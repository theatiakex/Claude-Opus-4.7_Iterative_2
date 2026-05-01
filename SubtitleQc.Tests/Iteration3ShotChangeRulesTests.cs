using System;
using System.Collections.Generic;
using System.Linq;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc;
using SubtitleQc.Core.Qc.Abstractions;
using SubtitleQc.Core.Qc.Rules;
using Xunit;

namespace SubtitleQc.Tests;

public sealed class Iteration3ShotChangeRulesTests
{
    [Fact]
    public void CrossShotBoundaryCheck_CueSpansAcrossCut_ShouldFail()
    {
        Cue cue = CreateCue(
            start: TimeSpan.Parse("00:05:00.000"),
            end: TimeSpan.Parse("00:05:04.000"),
            lines: new[] { "Spans cut" });
        IShotChangeProvider shotChangeProvider = new StubShotChangeProvider(
            cutTimestamps: new[] { TimeSpan.Parse("00:05:02.000") },
            cutFrames: Array.Empty<int>());
        var engine = new RuleEngine(new IQcRule[] { new CrossShotBoundaryCheckRule(shotChangeProvider) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Failed, cueResult.Status);
    }

    [Fact]
    public void CrossShotBoundaryCheck_CueEndsBeforeCut_ShouldPass()
    {
        Cue cue = CreateCue(
            start: TimeSpan.Parse("00:05:00.000"),
            end: TimeSpan.Parse("00:05:01.500"),
            lines: new[] { "Ends before cut" });
        IShotChangeProvider shotChangeProvider = new StubShotChangeProvider(
            cutTimestamps: new[] { TimeSpan.Parse("00:05:02.000") },
            cutFrames: Array.Empty<int>());
        var engine = new RuleEngine(new IQcRule[] { new CrossShotBoundaryCheckRule(shotChangeProvider) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    [Fact]
    public void CrossShotBoundaryCheck_CueStartsStrictlyAfterCut_ShouldPass()
    {
        Cue cue = CreateCue(
            start: TimeSpan.Parse("00:05:02.500"),
            end: TimeSpan.Parse("00:05:04.000"),
            lines: new[] { "Starts after cut" });
        IShotChangeProvider shotChangeProvider = new StubShotChangeProvider(
            cutTimestamps: new[] { TimeSpan.Parse("00:05:02.000") },
            cutFrames: Array.Empty<int>());
        var engine = new RuleEngine(new IQcRule[] { new CrossShotBoundaryCheckRule(shotChangeProvider) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    [Fact]
    public void MinFramesFromShotChange_CueStartsTooCloseToCut_ShouldFail()
    {
        Cue cue = CreateCueWithStartFrame(
            startFrame: 1001,
            start: TimeSpan.FromSeconds(100),
            end: TimeSpan.FromSeconds(102),
            lines: new[] { "Too close" });
        IShotChangeProvider shotChangeProvider = new StubShotChangeProvider(
            cutTimestamps: Array.Empty<TimeSpan>(),
            cutFrames: new[] { 1000 });
        var engine = new RuleEngine(new IQcRule[] { new MinFramesFromShotChangeRule(shotChangeProvider, thresholdFrames: 2) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Failed, cueResult.Status);
    }

    [Fact]
    public void MinFramesFromShotChange_CueStartsExactlyAtRequiredLimit_ShouldPass()
    {
        Cue cue = CreateCueWithStartFrame(
            startFrame: 1002,
            start: TimeSpan.FromSeconds(100),
            end: TimeSpan.FromSeconds(102),
            lines: new[] { "At limit" });
        IShotChangeProvider shotChangeProvider = new StubShotChangeProvider(
            cutTimestamps: Array.Empty<TimeSpan>(),
            cutFrames: new[] { 1000 });
        var engine = new RuleEngine(new IQcRule[] { new MinFramesFromShotChangeRule(shotChangeProvider, thresholdFrames: 2) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    [Fact]
    public void MinFramesFromShotChange_CueStartsSafelyAwayFromCut_ShouldPass()
    {
        Cue cue = CreateCueWithStartFrame(
            startFrame: 1010,
            start: TimeSpan.FromSeconds(100),
            end: TimeSpan.FromSeconds(102),
            lines: new[] { "Safely away" });
        IShotChangeProvider shotChangeProvider = new StubShotChangeProvider(
            cutTimestamps: Array.Empty<TimeSpan>(),
            cutFrames: new[] { 1000 });
        var engine = new RuleEngine(new IQcRule[] { new MinFramesFromShotChangeRule(shotChangeProvider, thresholdFrames: 2) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    private static Cue CreateCue(TimeSpan start, TimeSpan end, IReadOnlyList<string> lines)
    {
        return new Cue(
            id: Guid.NewGuid().ToString("N"),
            start: start,
            end: end,
            lines: lines);
    }

    private static Cue CreateCueWithStartFrame(int startFrame, TimeSpan start, TimeSpan end, IReadOnlyList<string> lines)
    {
        return new Cue(
            id: Guid.NewGuid().ToString("N"),
            start: start,
            end: end,
            lines: lines,
            startFrame: startFrame);
    }

    private sealed class StubShotChangeProvider : IShotChangeProvider
    {
        private readonly IReadOnlyList<TimeSpan> _cutTimestamps;
        private readonly IReadOnlyList<int> _cutFrames;

        public StubShotChangeProvider(IReadOnlyList<TimeSpan> cutTimestamps, IReadOnlyList<int> cutFrames)
        {
            _cutTimestamps = cutTimestamps;
            _cutFrames = cutFrames;
        }

        public IReadOnlyList<TimeSpan> GetShotChangeTimestamps()
        {
            return _cutTimestamps;
        }

        public IReadOnlyList<int> GetShotChangeFrames()
        {
            return _cutFrames;
        }
    }
}
