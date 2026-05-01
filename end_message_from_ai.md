### Iteration 1

Prototype is in place and green on first attempt — build succeeded, 13/13 tests pass.

Architecture (Iteration 1)
SubtitleQc.Core/
├── Models/
│   └── Cue.cs                              // Format-agnostic internal model
├── Parsing/
│   ├── Abstractions/ISubtitleParser.cs     // DIP seam for future formats
│   ├── SubtitleParseException.cs
│   ├── SubtitleBlockReader.cs              // Shared blank-line block split
│   ├── TimingLineParser.cs                 // Shared "HH:MM:SS,mmm --> ..." parsing
│   ├── SrtParser.cs                        // FormatId = "SRT"
│   └── WebVttParser.cs                     // FormatId = "WebVTT"
└── Qc/
    ├── Abstractions/IQcRule.cs             // Engine depends on this only
    ├── QcStatus.cs / QcResult.cs / QcReport.cs / QcEvaluationContext.cs
    ├── RuleEngine.cs                       // Closed for modification
    └── Rules/
        ├── MaxLinesRule.cs
        ├── MaxCplRule.cs
        ├── MaxCpsRule.cs
        ├── MinDurationRule.cs
        ├── OverlapCheckRule.cs
        └── EmptyCueCheckRule.cs
Design notes (the "why")
IQcRule.Evaluate(Cue, QcEvaluationContext) — single-cue contract keeps each rule trivially testable (SRP); inter-cue rules (overlap) get sequence access through the context object instead of re-implementing traversal.
RuleEngine is pure orchestration over IQcRule[] and Cue[]. Adding TTML, shot-change rules, or external QC rules in later iterations only adds new types — no engine edits (OCP).
Parsers share SubtitleBlockReader + TimingLineParser but each owns its own format grammar. The fractionalSeparator parameter (, for SRT, . for WebVTT) is the only differentiator at the timestamp layer.
Boundary semantics were chosen to match the Gherkin's bias toward inclusive thresholds: MaxLines/MaxCpl/MaxCps use strict >, MinDuration uses strict <, and OverlapCheck uses strict < so adjacent cues (B.start == A.end) pass — matching the "adjacent intervals without overlap" test.
No external packages in SubtitleQc.Core; only System.*. Central package management (Directory.Packages.props) versions the test SDK packages the existing test csproj already lists without versions.
Test csproj got a <ProjectReference> to SubtitleQc.Core — this is build wiring, not test content; the .cs test file is untouched.
Metrics
test_metrics.jsonl recorded:


test_metrics.jsonl
Lines 1-1
{"project_iteration": 1, "attempt": 1, "timestamp": "2026-05-01T12:26:50+02:00", "build_successful": true, "total_tests": 13, "passed": 13, "failed": 0, "pass_rate_percentage": 100.0}


### Iteration 2

Iteration 2 complete — build succeeded, 13/13 tests still pass on first attempt, 0 regressions.

What was added (purely additive)
SubtitleQc.Core/Parsing/
├── TtmlTimeParser.cs   [NEW] Clock-time + offset-time grammar
└── TtmlParser.cs       [NEW] ISubtitleParser implementation, FormatId="TTML"
Untouched — every iteration-1 file is byte-identical: Cue, RuleEngine, IQcRule, all 6 rules, ISubtitleParser, SrtParser, WebVttParser, SubtitleBlockReader, TimingLineParser, Qc* types, exception, csproj/slnx/Directory.Packages.props. This satisfies the iteration 2 directive that "the core QC engine and existing rules must remain untouched."

Design notes
OCP demonstrated end-to-end. Adding TTML required zero edits to existing logic — only two new files implementing the existing ISubtitleParser abstraction. The RuleEngine discovers TTML cues through the unified internal Cue model with no awareness of XML.
Why a separate TtmlTimeParser instead of extending TimingLineParser. TTML's grammar is fundamentally different: it accepts both clock-time and offset-time (Ns, Nms, Nm, Nh), and uses . exclusively for fractional seconds. Forcing it into the SRT/WebVTT timing helper would require modifying that helper (an OCP violation) and would couple the SRT/WebVTT parsers to TTML-specific code paths.
Recursive line extraction. <br/> is allowed inside <span> and other inline wrappers in TTML. AppendNodeContent recurses through any non-<br/> element, so styling spans don't break line splitting.
Namespace-agnostic element matching. TTML files come with various default namespaces (http://www.w3.org/ns/ttml, sometimes none, sometimes prefixed). Matching by LocalName keeps the parser robust to all of them without coupling to a specific namespace string.
Documented limitations. Frame-based (Nf) and tick-based (Nt) time metrics are explicitly rejected with a clear SubtitleParseException message — they require document-level frameRate/tickRate metadata, which would couple TtmlTimeParser to document context. A context-aware variant can be introduced in a future iteration without modifying this code.
Standard library only. System.Xml.Linq is part of base .NET 10; no external packages were added.
Metrics

test_metrics.jsonl
Lines 1-2
{"project_iteration": 1, "attempt": 1, "timestamp": "2026-05-01T12:26:50+02:00", "build_successful": true, "total_tests": 13, "passed": 13, "failed": 0, "pass_rate_percentage": 100.0}
{"project_iteration": 2, "attempt": 1, "timestamp": "2026-05-01T12:38:00+02:00", "build_successful": true, "total_tests": 13, "passed": 13, "failed": 0, "pass_rate_percentage": 100.0}
