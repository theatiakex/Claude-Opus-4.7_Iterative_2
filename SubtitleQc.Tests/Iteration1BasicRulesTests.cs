using System;
using System.Collections.Generic;
using System.Linq;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Qc;
using SubtitleQc.Core.Qc.Abstractions;
using SubtitleQc.Core.Qc.Rules;
using Xunit;

namespace SubtitleQc.Tests;

public sealed class Iteration1BasicRulesTests
{
    [Fact]
    public void MaxLines_SubtitleExceedingLineLimit_ShouldFail()
    {
        // Arrange
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(10),
            end: TimeSpan.FromSeconds(12),
            lines: new[] { "Line 1", "Line 2", "Line 3" });
        var engine = new RuleEngine(new IQcRule[] { new MaxLinesRule(threshold: 2) });

        // Act
        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        // Assert
        Assert.Equal(QcStatus.Failed, cueResult.Status);
    }

    [Fact]
    public void MaxLines_SubtitleWithinLineLimit_ShouldPass()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(10),
            end: TimeSpan.FromSeconds(12),
            lines: new[] { "Line 1", "Line 2" });
        var engine = new RuleEngine(new IQcRule[] { new MaxLinesRule(threshold: 2) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    [Fact]
    public void MaxCpl_LineExceedsAllowedLength_ShouldFail()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(20),
            end: TimeSpan.FromSeconds(22),
            lines: new[] { new string('A', 45) });
        var engine = new RuleEngine(new IQcRule[] { new MaxCplRule(threshold: 42) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Failed, cueResult.Status);
    }

    [Fact]
    public void MaxCpl_LineExactlyAtAllowedLength_ShouldPass()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(20),
            end: TimeSpan.FromSeconds(22),
            lines: new[] { new string('A', 42) });
        var engine = new RuleEngine(new IQcRule[] { new MaxCplRule(threshold: 42) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    [Fact]
    public void MaxCpl_LineWellBelowAllowedLength_ShouldPass()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(20),
            end: TimeSpan.FromSeconds(22),
            lines: new[] { new string('A', 20) });
        var engine = new RuleEngine(new IQcRule[] { new MaxCplRule(threshold: 42) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    [Fact]
    public void MaxCps_HighReadingSpeed_ShouldFail()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(30),
            end: TimeSpan.FromSeconds(32),
            lines: new[] { new string('A', 60) });
        var engine = new RuleEngine(new IQcRule[] { new MaxCpsRule(threshold: 20) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Failed, cueResult.Status);
    }

    [Fact]
    public void MaxCps_AcceptableReadingSpeed_ShouldPass()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(30),
            end: TimeSpan.FromSeconds(32),
            lines: new[] { new string('A', 40) });
        var engine = new RuleEngine(new IQcRule[] { new MaxCpsRule(threshold: 20) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    [Fact]
    public void MinDuration_VeryShortDisplayDuration_ShouldFail()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(40),
            end: TimeSpan.FromSeconds(40.5),
            lines: new[] { "Short display" });
        var engine = new RuleEngine(new IQcRule[] { new MinDurationRule(threshold: TimeSpan.FromSeconds(1)) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Failed, cueResult.Status);
    }

    [Fact]
    public void MinDuration_ExactlyAtMinimumDuration_ShouldPass()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(40),
            end: TimeSpan.FromSeconds(41),
            lines: new[] { "Meets minimum" });
        var engine = new RuleEngine(new IQcRule[] { new MinDurationRule(threshold: TimeSpan.FromSeconds(1)) });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Passed, cueResult.Status);
    }

    [Fact]
    public void OverlapCheck_OverlappingIntervals_ShouldFailCueB()
    {
        Cue cueA = CreateCue(
            start: TimeSpan.Parse("00:01:08.000"),
            end: TimeSpan.Parse("00:01:10.000"),
            lines: new[] { "Cue A" });
        Cue cueB = CreateCue(
            start: TimeSpan.Parse("00:01:09.500"),
            end: TimeSpan.Parse("00:01:11.000"),
            lines: new[] { "Cue B" });
        var engine = new RuleEngine(new IQcRule[] { new OverlapCheckRule() });

        QcReport report = engine.Evaluate(new[] { cueA, cueB });
        QcResult cueBResult = report.Results.Single(r => r.CueId == cueB.Id);

        Assert.Equal(QcStatus.Failed, cueBResult.Status);
    }

    [Fact]
    public void OverlapCheck_AdjacentIntervalsWithoutOverlap_ShouldPassCueB()
    {
        Cue cueA = CreateCue(
            start: TimeSpan.Parse("00:01:08.000"),
            end: TimeSpan.Parse("00:01:10.000"),
            lines: new[] { "Cue A" });
        Cue cueB = CreateCue(
            start: TimeSpan.Parse("00:01:10.000"),
            end: TimeSpan.Parse("00:01:11.000"),
            lines: new[] { "Cue B" });
        var engine = new RuleEngine(new IQcRule[] { new OverlapCheckRule() });

        QcReport report = engine.Evaluate(new[] { cueA, cueB });
        QcResult cueBResult = report.Results.Single(r => r.CueId == cueB.Id);

        Assert.Equal(QcStatus.Passed, cueBResult.Status);
    }

    [Fact]
    public void EmptyCueCheck_WhitespaceOnlyContent_ShouldFail()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(50),
            end: TimeSpan.FromSeconds(52),
            lines: new[] { "   ", "\t", string.Empty });
        var engine = new RuleEngine(new IQcRule[] { new EmptyCueCheckRule() });

        QcReport report = engine.Evaluate(new[] { cue });
        QcResult cueResult = report.Results.Single();

        Assert.Equal(QcStatus.Failed, cueResult.Status);
    }

    [Fact]
    public void EmptyCueCheck_ValidTextContent_ShouldPass()
    {
        Cue cue = CreateCue(
            start: TimeSpan.FromSeconds(50),
            end: TimeSpan.FromSeconds(52),
            lines: new[] { "Hello world" });
        var engine = new RuleEngine(new IQcRule[] { new EmptyCueCheckRule() });

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
}
