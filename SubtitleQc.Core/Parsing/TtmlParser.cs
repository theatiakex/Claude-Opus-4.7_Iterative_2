using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SubtitleQc.Core.Models;
using SubtitleQc.Core.Parsing.Abstractions;

namespace SubtitleQc.Core.Parsing;

/// <summary>
/// TTML (Timed Text Markup Language) parser. Walks every &lt;p&gt; element
/// regardless of XML namespace, splits its content on &lt;br/&gt; (including
/// nested ones inside &lt;span&gt; etc.), and emits the same internal
/// <see cref="Cue"/> model the QC engine already operates on. Because the
/// parser depends solely on <see cref="ISubtitleParser"/>, the existing engine
/// and rule set continue to work unchanged (DIP/OCP).
/// </summary>
public sealed class TtmlParser : ISubtitleParser
{
    private const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";
    private const string ParagraphLocalName = "p";
    private const string BreakLocalName = "br";

    public string FormatId => "TTML";

    public IReadOnlyList<Cue> Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);
        XDocument document = LoadDocument(content);
        List<Cue> cues = new();
        int autoId = 1;
        foreach (XElement paragraph in document.Descendants().Where(IsParagraph))
        {
            cues.Add(ParseParagraph(paragraph, ref autoId));
        }

        return cues;
    }

    private static XDocument LoadDocument(string content)
    {
        try
        {
            return XDocument.Parse(content, LoadOptions.PreserveWhitespace);
        }
        catch (XmlException ex)
        {
            throw new SubtitleParseException("TTML content is not well-formed XML.", ex);
        }
    }

    private static bool IsParagraph(XElement element) =>
        string.Equals(element.Name.LocalName, ParagraphLocalName, StringComparison.Ordinal);

    private static Cue ParseParagraph(XElement paragraph, ref int autoId)
    {
        TimeSpan start = ParseTimeAttribute(paragraph, "begin");
        TimeSpan end = ParseTimeAttribute(paragraph, "end");
        IReadOnlyList<string> lines = ExtractLines(paragraph);
        string cueId = ResolveCueId(paragraph) ?? autoId.ToString(CultureInfo.InvariantCulture);
        autoId++;
        return new Cue(cueId, start, end, lines);
    }

    private static TimeSpan ParseTimeAttribute(XElement paragraph, string attributeName)
    {
        XAttribute? attribute = paragraph.Attribute(attributeName);
        if (attribute is null)
        {
            throw new SubtitleParseException($"TTML <p> element is missing required '{attributeName}' attribute.");
        }

        return TtmlTimeParser.Parse(attribute.Value);
    }

    private static string? ResolveCueId(XElement paragraph)
    {
        XAttribute? idAttr = paragraph.Attribute(XName.Get("id", XmlNamespace))
                              ?? paragraph.Attribute("id");
        string? raw = idAttr?.Value.Trim();
        return string.IsNullOrEmpty(raw) ? null : raw;
    }

    private static IReadOnlyList<string> ExtractLines(XElement paragraph)
    {
        List<string> lines = new();
        StringBuilder buffer = new();
        AppendNodeContent(paragraph, lines, buffer);
        lines.Add(buffer.ToString());
        return NormalizeLines(lines);
    }

    private static void AppendNodeContent(XElement element, List<string> lines, StringBuilder buffer)
    {
        foreach (XNode node in element.Nodes())
        {
            HandleNode(node, lines, buffer);
        }
    }

    private static void HandleNode(XNode node, List<string> lines, StringBuilder buffer)
    {
        switch (node)
        {
            case XText text:
                buffer.Append(text.Value);
                break;
            case XElement element when IsBreak(element):
                lines.Add(buffer.ToString());
                buffer.Clear();
                break;
            case XElement element:
                AppendNodeContent(element, lines, buffer);
                break;
        }
    }

    private static bool IsBreak(XElement element) =>
        string.Equals(element.Name.LocalName, BreakLocalName, StringComparison.Ordinal);

    private static IReadOnlyList<string> NormalizeLines(IEnumerable<string> rawLines) =>
        rawLines.Select(line => line.Trim()).ToArray();
}
