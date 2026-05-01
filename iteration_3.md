# System Specification - Iteration 3 (External Data Integration)

## 1. Task Definition
Your task is to upgrade the existing subtitle QC module to synchronize with external "shot change" (visual cut) data.

## 2. Architectural Requirements

### 2.1 Internal Data Model Upgrade
Extend the existing unified data model.
* Must now be able to handle external attributes (e.g., arrays of timestamps for shot changes).

### 2.2 Rule Engine Expansion
Add new rules to the existing Rule Engine that utilize the new external data parameters.

---

## 3. Acceptance Criteria (Gherkin)
### Feature 2: Integration of External Data (Shot Changes)
Feature: Validation against visual cuts

  Scenario: CrossShotBoundaryCheck - A cue spans across a cut
    Given an internal cue has a start time of 00:05:00.000 and an end time of 00:05:04.000
    And external shot change data indicates a cut at 00:05:02.000
    When the QC check "CrossShotBoundaryCheck" is run
    Then the result should be "Failed"

  Scenario: MinFramesFromShotChange - A cue starts too close to a cut
    Given a cut occurs at frame 1000
    And an internal cue starts at frame 1001
    When the QC check "MinFramesFromShotChange" is run with a threshold of 2 frames
    Then the result should be "Failed"