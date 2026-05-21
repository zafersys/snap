# SNAP: Next-Generation AI & Photography Use Cases

## ONE-SENTENCE VISION

> **NPCs are not game characters; they are socially dynamic, unpredictable humans powered by memory and emotion who react to the player as a tangible, disruptive photographer in their world.**

---

## USE CASE MAP (Overview)

```mermaid
%%{init: {'theme': 'base', 'themeVariables': { 'fontFamily': 'Inter, sans-serif'}}}%%
graph TD
    subgraph PLAYER (The Photographer)
        A[Camera System] --> B(Capture Decisive Moment)
        A --> C(Manage Stealth & Visibility)
        D[Gallery System] --> E(Review & Rate Photos)
        F[Publishing System] --> G(Submit Front-Page Story)
    end

    subgraph NPC WORLD (Neural & Social AI)
        H[Individual NPC AI] --> I(OCEAN Personality Core)
        H --> J(Memory Stream & Reflection)
        H --> K(OCC Emotion Appraisal)
        L[Social Dynamics] --> M(Group Gossip & Reputations)
        L --> N(Friend/Rival Confrontations)
    end

    subgraph SYSTEM DIRECTOR
        O[Simulation Loops] --> P(Day/Night Phase Shifts)
        O --> Q(Database Persistence)
    end

    A <-->|Player raises camera, breaking stealth| H
    G -->|Published photo impacts world| L
    I -->|Dictates reaction to camera| B
```

---

## SECTION 1 — THE NEURAL-NETWORK AI BEHAVIOR SYSTEM

The core simulation pivots from simple behavior trees to a **Neural & Memory-Driven Architecture** integrated with our existing `NPCController` and `RelationshipMatrix`.

### 1.1 The OCEAN Personality Core (Predictable Unpredictability)
Every NPC is generated with a fixed 5-dimensional personality vector (Openness, Conscientiousness, Extraversion, Agreeableness, Neuroticism). 
*   **High-Extraversion / Low-Neuroticism**: Will pose for the camera and greet the player.
*   **Low-Agreeableness / High-Neuroticism**: Will become angry if photographed without consent, confront the player, and hold a grudge.

### 1.2 Memory Stream & Generative Reflections
NPCs are no longer amnesiacs resetting every day. 
*   **Memory Logging**: Every interaction (being photographed, reading a scandal, being hugged) is saved into the NPC's `MemoryStream` with a *recency* and *emotional weight* score.
*   **Neural Synthesizing (AI)**: During the `Night` phase, the simulation uses an AI model (Neural Network/LLM) to synthesize these memories into permanent **Reflections**.
*   **Example**: "That photographer keeps taking pictures of me when I'm eating. I feel watched and annoyed."

### 1.3 The OCC Emotion Appraisal Model
Events are evaluated dynamically to shift the `currentEmotion` (Happy, Sad, Angry, Fearful).
*   **Trigger**: Player photographed an NPC in an embarrassing moment.
*   **Evaluation**: NPC appraises this as a violation of privacy.
*   **Result**: Instant shift to `EmotionType.Angry`, causing the NPC to flee or confront the player, instantly shifting the visual capsule color to Crimson Red.

---

## SECTION 2 — CAMERA SYSTEM & PLAYER VISIBILITY

The player is **not** an invisible floating camera. You are a physical entity holding a piece of glass and metal in a town square.

### 2.1 The "Unseen Photographer" Dynamic
*   **Camera Raises = Awareness Spike**: Holding the `C` key (Viewfinder) drastically increases the player's visibility radius.
*   **NPC Reactions**: Depending on the NPC's OCEAN profile, aiming the camera at them will interrupt their current state (`Wandering`, `Sitting`). They might turn away, cover their face, or walk up and yell at you.
*   **Stealth vs. Composition**: The player must balance getting close for a high `CompositionScore` versus staying back so they don't destroy the natural candid moment.

### 2.2 The "Decisive Moment"
*   **Peak Emotion Scoring**: `TakePictureUseCase` now evaluates the *exact emotional state* of the subject at the frame of capture. Catching an NPC at the exact moment their emotion changes (e.g., transitioning to `Angry` upon reading the board) yields a massive score multiplier.

---

## SECTION 3 — GALLERY & PHOTO PUBLISHING

### 3.1 Advanced Lightbox Gallery
*   **Technical & Emotional Rating**: The gallery doesn't just show a composition score. It breaks the photo down by:
    *   **Framing Quality**: Distance and rule-of-thirds.
    *   **Emotional Weight**: Was the subject experiencing a strong emotion?
    *   **Rarity**: Did you capture an interaction between two rival NPCs?

### 3.2 Bulletproof Publishing Consequences
When the player hits **Publish Edition** via the Editorial UI:
*   **The Gossip Network**: The `RelationshipMatrix` propagates the photo to the subject's immediate social circle.
*   **World Alteration**: If you publish a Scandal about an NPC, their friends will be hostile to you the next day. If you publish a flattering portrait, their social circle will welcome you, allowing you to get closer for photos without triggering avoidance behaviors.

---

## SECTION 4 — ONE-SENTENCE USE CASES (Bulletproof Definitions)

Below is the definitive, bulletproof list of system use cases mapping directly to the new Neural AI and Camera architecture:

### 4.1 Player & Camera Actions
*   **UC-P01 (Navigate)**: The player maneuvers their physical avatar through the 2.5D space while maintaining stealth to avoid disrupting NPC routines.
*   **UC-P02 (Raise Camera)**: The player raises the viewfinder overlay, which instantly increases their physical visibility radius and alerts nearby observant NPCs.
*   **UC-P03 (Decisive Capture)**: The player snaps a photograph, calculating a score based on framing geometry and the exact micro-emotional state of the subject in that specific frame.
*   **UC-P04 (Gallery Review)**: The player opens the lightbox gallery to review daily captures, analyzing them for technical composition and emergent narrative value.
*   **UC-P05 (Publish Edition)**: The player selects a daily photo and category to publish on the public board, permanently altering their own reputation and the social standing of the subjects.

### 4.2 Neural NPC & Social Actions
*   **UC-N01 (Autonomous AI Wandering)**: NPCs execute their daily schedules (wandering, sitting) driven by their OCEAN personality traits and current emotional valence.
*   **UC-N02 (Camera Reaction)**: An NPC detects the player aiming a camera and dynamically chooses to pose, ignore, hide, or confront based on their personality and past memory of the player.
*   **UC-N03 (Memory Reflection)**: During the night phase, an NPC synthesizes their daily interactions into permanent memories, updating their long-term bias toward the player and other citizens.
*   **UC-N04 (Board Reading)**: NPCs pathfind to the morning newspaper board, parse the player's published photo, and calculate massive emotional and relationship shifts.
*   **UC-N05 (Social Contagion)**: An NPC shares their emotional reaction to a news story or a photographer encounter with nearby friends, spreading the sentiment through the social network.
