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


### Iteration 3

Iteration 3 complete — 19/19 tests pass (13 iteration-1 + 6 iteration-3), 0 regressions, on attempt 2.

What was added
New files:

Qc/Abstractions/IShotChangeProvider.cs — DIP seam for shot-change data sources.
Qc/Rules/CrossShotBoundaryCheckRule.cs — time-domain cut crossing.
Qc/Rules/MinFramesFromShotChangeRule.cs — frame-domain proximity.
Modified (purely additive):

Models/Cue.cs — added optional trailing parameter int? startFrame = null plus the StartFrame getter. All existing positional and named-argument callers (iteration-1 test, all three parsers) remain valid because the parameter is at the end and has a default.
Untouched: RuleEngine, QcEvaluationContext, IQcRule, all six iteration-1 rules, ISubtitleParser, SrtParser, WebVttParser, TtmlParser, all timing/block helpers.

What attempt 1 got wrong, and why
I initially designed external data to flow through QcEvaluationContext and a new Evaluate(SubtitleProgram) engine overload — touching the engine and context. The actual contract in Iteration3ShotChangeRulesTests.cs chooses a different (and stricter) DIP/OCP split:

Each new rule receives its own IShotChangeProvider via constructor — the same pattern existing rules already use for their thresholds.
The engine and context need zero changes because rule dependencies are private to the rule.
Frame-domain comparison uses Cue.StartFrame directly instead of computing frames from Time × FrameRate — no global frame rate needed, and time-only formats simply leave StartFrame null.
This is genuinely cleaner; I should have inferred it from the existing rules' constructor-injection pattern before speculating. Logging the failed attempt 1 build per the agent profile (Section 8) was important — test_metrics.jsonl now shows the corrective trajectory.

Design notes (the "why")
Why a provider interface, not raw lists. A List<TimeSpan> couples rules to a concrete data shape. IShotChangeProvider lets a future iteration plug a streaming source, a database-backed source, or a cached-file source without touching rule code.
Why both Timestamps and Frames projections on the provider. The two rules operate in genuinely different domains — CrossShotBoundary cares only about timeline overlap; MinFramesFromShotChange cares about exact frame counts (sub-frame timing rounding errors would cause spurious failures). Forcing one domain to convert to the other introduces error; keeping both projections is honest about what each rule needs.
Why Cue.StartFrame is int? not long. Tests pass int literals; a typical broadcast is well under 2 billion frames; nullable correctly models "unknown / not applicable" without a sentinel value. 0 would be a valid frame, so a sentinel like -1 would be a worse choice.
Why null StartFrame → Passed in MinFramesFromShotChange. This rule cannot meaningfully evaluate cues without frame data; throwing would make the rule incompatible with SRT/WebVTT/TTML cues (none of which carry frame data today). Passing means "no evidence of failure" which preserves clean composability.
JSON serializability preserved. int? serializes natively in System.Text.Json (omitted when null with default options, or as null). Cue remains fully JSON-roundtrippable.
Metrics

test_metrics.jsonl
Lines 1-4
{"project_iteration": 1, "attempt": 1, "timestamp": "2026-05-01T12:26:50+02:00", "build_successful": true, "total_tests": 13, "passed": 13, "failed": 0, "pass_rate_percentage": 100.0}
{"project_iteration": 2, "attempt": 1, "timestamp": "2026-05-01T12:38:00+02:00", "build_successful": true, "total_tests": 13, "passed": 13, "failed": 0, "pass_rate_percentage": 100.0}
{"project_iteration": 3, "attempt": 1, "timestamp": "2026-05-01T12:55:00+02:00", "build_successful": false, "total_tests": 0, "passed": 0, "failed": 0, "pass_rate_percentage": 0.0}
{"project_iteration": 3, "attempt": 2, "timestamp": "2026-05-01T12:58:00+02:00", "build_successful": true, "total_tests": 19, "passed": 1