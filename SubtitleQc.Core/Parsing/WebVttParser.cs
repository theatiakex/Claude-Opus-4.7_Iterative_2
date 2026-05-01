using System.Collections.Generic;
using System.Globalization;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Parsing.Abstractions;

namespace SubtitleQc.Core.Parsing;

/// <summary>
/// WebVTT (.vtt) parser. WebVTT blocks share SRT's outer shape but use '.'
/// as the fractional separator, may include an optional cue identifier line,
/// and require a "WEBVTT" file header. STYLE/NOTE/REGION blocks are skipped
/// rather than parsed; they carry no QC-relevant content for iteration 1.
/// </summary>
public sealed class WebVttParser : ISubtitleParser
{
    private static readonly string[] NonCueBlockPrefixes = { "NOTE", "STYLE", "REGION" };

    public string FormatId => "WebVTT";

    public IReadOnlyList<Cue> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        List<Cue> cues = new();
        int autoId = 1;
        bool headerSeen = false;
        foreach (IReadOnlyList<string> block in SubtitleBlockReader.ReadBlocks(content))
        {
            if (!headerSeen)
            {
                EnsureHeader(block);
                headerSeen = true;
                continue;
            }

            if (IsNonCueBlock(block))
            {
                continue;
            }

            cues.Add(ParseBlock(block, ref autoId));
        }

        if (!headerSeen)
        {
            throw new SubtitleParseException("WebVTT content is missing the 'WEBVTT' header.");
        }

        return cues;
    }

    private static void EnsureHeader(IReadOnlyList<string> block)
    {
        if (!block[0].StartsWith("WEBVTT", StringComparison.Ordinal))
        {
            throw new SubtitleParseException("First block of a WebVTT file must start with 'WEBVTT'.");
        }
    }

    private static bool IsNonCueBlock(IReadOnlyList<string> block)
    {
        string first = block[0];
        foreach (string prefix in NonCueBlockPrefixes)
        {
            if (first.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static Cue ParseBlock(IReadOnlyList<string> block, ref int autoId)
    {
        int timingIndex = LocateTimingLine(block);
        string? explicitId = timingIndex == 1 ? block[0].Trim() : null;
        (TimeSpan start, TimeSpan end) = TimingLineParser.ParseTimingLine(block[timingIndex], fractionalSeparator: '.');
        List<string> textLines = new(block.Count - timingIndex - 1);
        for (int i = timingIndex + 1; i < block.Count; i++)
        {
            textLines.Add(block[i]);
        }

        string cueId = explicitId ?? autoId.ToString(CultureInfo.InvariantCulture);
        autoId++;
        return new Cue(cueId, start, end, textLines);
    }

    private static int LocateTimingLine(IReadOnlyList<string> block)
    {
        for (int i = 0; i < block.Count; i++)
        {
            if (block[i].Contains("-->", StringComparison.Ordinal))
            {
                return i;
            }
        }

        throw new SubtitleParseException("WebVTT cue block missing timing line.");
    }
}
