# System Specification - Iteration 1 (Base Implementation)

## 1. Task Definition
Your task is to implement the foundational backend module for technical quality control (QC) of subtitles. 
In this iteration, the system must handle **SRT** and **WebVTT** formats and apply basic text and timing rules.

## 2. Architectural Requirements

### 2.1 Internal Data Model
All solutions must use a unified, internal representation of subtitle data that is completely agnostic and independent of the input format.
* Must represent individual `Cues` containing a start time, end time, and text content.
* Must support multiple lines per cue.

### 2.2 Parsers
The module must contain separate, isolated parsers for the following formats, converting raw data into the internal data model:
* **SRT**
* **WebVTT**

### 2.3 Rule Engine (QC Engine)
All QC rules must be applied exclusively to the internal data model.
* The results of the evaluation must be structured, traceable, and JSON-serializable.

---

## 3. Acceptance Criteria (Gherkin)
### Feature 1: Basic Text and Timing Rules
Feature: Validation of text amount and display duration

  Scenario: MaxLines - A subtitle with too many lines should be rejected
    Given an internal cue contains 3 lines of text
    When the QC check "MaxLines" is run with a threshold of 2
    Then the result should be "Failed"

  Scenario: MaxCpl - A line exceeds the allowed character length
    Given an internal cue contains a line with 45 characters
    When the QC check "MaxCpl" is run with a threshold of 42
    Then the result should be "Failed"
  
  Scenario: MaxCps - A subtitle with a high reading speed should be rejected
    Given an internal "cue" contains 60 characters
    And the display duration for the cue is 2.0 seconds
    When the QC check "MaxCps" is run with a threshold of 20 characters per second
    Then the result should be "Failed"

  Scenario: MinDuration - A subtitle is displayed for too short a time
    Given an internal cue has a display time of 0.5 seconds
    When the QC check "MinDuration" is run with a threshold of 1.0 seconds
    Then the result should be "Failed"

  Scenario: OverlapCheck - Two subtitles overlap in time
    Given cue A ends at 00:01:10.000
    And cue B starts at 00:01:09.500
    When the QC check "OverlapCheck" is run
    Then the result for cue B should be "Failed"

  Scenario: EmptyCueCheck - A subtitle lacks text content
    Given an internal cue contains only spaces and line breaks
    When the QC check "EmptyCueCheck" is run
    Then the result should be "Failed"