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

