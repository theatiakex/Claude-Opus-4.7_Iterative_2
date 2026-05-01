using System;
using System.Collections.Generic;
using System.Linq;

namespace SubtitleQc.Core.Models;

/// <summary>
/// Format-agnostic representation of a single subtitle cue. This is the
/// unified internal model that the QC engine and rules operate on, keeping
/// parsing concerns isolated from validation concerns (SRP/DIP).
/// </summary>
/// <remarks>
/// <see cref="StartFrame"/> is an optional external attribute that lets
/// frame-domain QC rules (e.g. <c>MinFramesFromShotChange</c>) compare cues
/// against frame-indexed shot-change data without depending on a global
/// frame-rate conversion. Cues sourced from time-only formats simply leave it
/// as <see langword="null"/>; consuming rules treat that as "no comparison
/// possible" rather than failing.
/// </remarks>
public sealed class Cue
{
    public Cue(
        string id,
        TimeSpan start,
        TimeSpan end,
        IReadOnlyList<string> lines,
        int? startFrame = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(lines);
        ValidateInterval(start, end);
        ValidateStartFrame(startFrame);

        Id = id;
        Start = start;
        End = end;
        Lines = lines.ToArray();
        StartFrame = startFrame;
    }

    public string Id { get; }

    public TimeSpan Start { get; }

    public TimeSpan End { get; }

    public IReadOnlyList<string> Lines { get; }

    public TimeSpan Duration => End - Start;

    public int? StartFrame { get; }

    private static void ValidateInterval(TimeSpan start, TimeSpan end)
    {
        if (end < start)
        {
            throw new ArgumentException("End time must not precede start time.", nameof(end));
        }
    }

    private static void ValidateStartFrame(int? startFrame)
    {
        if (startFrame is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startFrame), "Start frame must be non-negative when supplied.");
        }
    }
}
