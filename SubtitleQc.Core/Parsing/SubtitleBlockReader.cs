using System.Collections.Generic;

namespace SubtitleQc.Core.Parsing;

/// <summary>
/// Splits raw subtitle text into blank-line-separated blocks. Both SRT and
/// WebVTT use the same outer block layout, so this helper centralises the
/// line-handling logic and lets each parser focus on its own block grammar.
/// </summary>
internal static class SubtitleBlockReader
{
    public static IEnumerable<IReadOnlyList<string>> ReadBlocks(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        string[] rawLines = content.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        List<string> current = new();
        foreach (string line in rawLines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                if (current.Count > 0)
                {
                    yield return current.ToArray();
                    current = new List<string>();
                }
                continue;
            }

            current.Add(line);
        }

        if (current.Count > 0)
        {
            yield return current.ToArray();
        }
    }
}
