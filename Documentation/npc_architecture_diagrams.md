# SNAP: Neural NPC System Diagrams

This document visualizes the exact technical architecture, classes, and activity flows for the decoupled Neural NPC System described in the 100+ Use Case design.

---

## 1. System Architecture Diagram

This diagram shows how the newly decoupled layers connect to process the world and drive the Execution Layer (`NPCController`).

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'fontFamily': 'Inter, sans-serif'}}}%%
graph TD
    subgraph SENSORY LAYER [1. Sensory Matrix]
        A[Vision Cone Raycasts]
        B[Audio OverlapSpheres]
        A -->|Player/NPC Detected| C(Stimulus Event)
        B -->|Shutter/Dialogue Heard| C
    end

    subgraph APPRAISAL LAYER [2. Appraisal Engine]
        C --> D{OCC Evaluator}
        P[(NPCPersonalityData\nOCEAN Vector)] --> D
        D -->|Determines Emotion Shift| E(Emotional State Delta)
    end

    subgraph MEMORY LAYER [3. Memory Stream]
        E --> F[Log MemoryEvent]
        F --> G[(Short-Term Memory List)]
        G -->|Night Phase| H[LLM / Neural Reflection Synthesis]
        H --> I[(Permanent Beliefs)]
        I -.->|Bias overrides| D
    end

    subgraph EXECUTION LAYER [4. Action Planner / Controller]
        E --> J[Update CurrentEmotion]
        J --> K{Action Selector}
        K -->|High Utility| L(Pose / Greet)
        K -->|High Utility| M(Flee / Confront)
        K -->|High Utility| N(Ignore / Wander)
    end
```

---

## 2. Class & Object Diagram

This diagram details the C# scripts, their properties, methods, and exact relationships in the Unity `Assets/_Game/Scripts/NPC/` folder.

```mermaid
classDiagram
    class NPCSensoryMatrix {
        +float visionRadius = 15f
        +float visionAngle = 120f
        +float hearingRadius = 10f
        +Action~Transform, StimulusType~ OnStimulusDetected
        -Update() void
        -ScanVision() void
        -ListenAudio() void
    }

    class NPCAppraisalEngine {
        -NPCPersonalityData _ocean
        +EvaluateStimulus(Transform source, StimulusType type) AppraisalResult
        -CalculateDesirability() float
        -CalculateArousal() float
    }

    class NPCMemoryStream {
        -List~MemoryEvent~ _shortTermMemories
        -List~Belief~ _permanentBeliefs
        +AddMemory(MemoryEvent evt) void
        +GetMemoriesRegarding(int npcId) List~MemoryEvent~
        +SynthesizeNightlyReflections() void
    }

    class NPCController {
        +NPCState currentState
        +EmotionType currentEmotion
        -NPCSensoryMatrix _sensory
        -NPCAppraisalEngine _appraisal
        -NPCMemoryStream _memory
        -Start() void
        -HandleStimulus(Transform source, StimulusType type) void
        -ExecuteAction(NPCAction action) IEnumerator
    }

    class MemoryEvent {
        <<struct>>
        +StimulusType Type
        +int SourceNetworkId
        +float EmotionalWeight
        +float Timestamp
    }

    class NPCPersonalityData {
        <<ScriptableObject>>
        +float Openness
        +float Conscientiousness
        +float Extraversion
        +float Agreeableness
        +float Neuroticism
    }

    NPCSensoryMatrix --> NPCController : Fires Events
    NPCController --> NPCAppraisalEngine : Requests Evaluation
    NPCController --> NPCMemoryStream : Logs Result
    NPCAppraisalEngine --> NPCPersonalityData : Reads Traits
    NPCMemoryStream *-- MemoryEvent : Stores
```

---

## 3. Activity Diagram: "The Player takes a photo"

This traces the exact execution path across the new decoupled components when the player takes a picture.

```mermaid
sequenceDiagram
    autonumber
    actor Player as Player (Camera)
    participant SM as NPCSensoryMatrix
    participant AE as NPCAppraisalEngine
    participant MS as NPCMemoryStream
    participant Ctrl as NPCController

    Player->>Player: Fires Shutter (Flash / Sound)
    
    rect rgb(240, 248, 255)
        Note over SM: 1. Sensory Layer
        Player->>SM: Casts ShutterAudioEvent
        SM->>SM: Check if distance < 10m
        SM->>Ctrl: Invoke OnStimulusDetected(Player, CameraShutter)
    end

    rect rgb(255, 240, 245)
        Note over Ctrl, AE: 2. Appraisal Layer
        Ctrl->>AE: EvaluateStimulus(Player, CameraShutter)
        AE->>AE: Read OCEAN Personality
        AE->>AE: Check MS for Permanent Beliefs about Player
        AE-->>Ctrl: Return: Threat Level HIGH, Emotion = Angry
    end

    rect rgb(245, 255, 240)
        Note over Ctrl, MS: 3. Memory & Execution Layer
        Ctrl->>MS: AddMemory(Photographed, Source=Player, Weight=-0.8)
        Ctrl->>Ctrl: Abort current Wander routine
        Ctrl->>Ctrl: Set Emotion = Angry (Capsule turns Red)
        Ctrl->>Player: Pathfind toward Player (Confront Action)
    end
```
