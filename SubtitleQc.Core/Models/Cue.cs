using System;
using System.Collections.Generic;
using System.Linq;

namespace SubtitleQc.Core.Models;

/// <summary>
/// Format-agnostic representation of a single subtitle cue. This is the
/// unified internal model that the QC engine and rules operate on, keeping
/// parsing concerns isolated from validation concerns (SRP/DIP).
/// </summary>
public sealed class Cue
{
    public Cue(string id, TimeSpan start, TimeSpan end, IReadOnlyList<string> lines)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(lines);
        if (end < start)
        {
            throw new ArgumentException("End time must not precede start time.", nameof(end));
        }

        Id = id;
        Start = start;
        End = end;
        Lines = lines.ToArray();
    }

    public string Id { get; }

    public TimeSpan Start { get; }

    public TimeSpan End { get; }

    public IReadOnlyList<string> Lines { get; }

    public TimeSpan Duration => End - Start;
}
