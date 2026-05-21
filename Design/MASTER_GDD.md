# MASTER GAME DESIGN DOCUMENT — GP-OYUN (A1 Level)

## 1. Project Identity
- **Title**: GP-OYUN
- **Tagline**: "A town full of people who never say a word — but never stop communicating."
- **Genre**: 2.5D Narrative Social Simulation
- **Core Loop**: Observe → Capture → Publish → Watch Reactions
- **Styling**: Apple/UberEats aesthetic (Invisible Interface, Visible Content)

---

## 2. Technical Pillars (A1 Level)
| Pillar | Specification |
|---|---|
| **Architecture** | Direct Component Communication (Direct references and `FindObjectsOfType`) |
| **NPC Brain** | Simple State Enum in `Update()` (e.g., `Idle, WalkingToBoard, Reading`) |
| **Movement** | `transform.Translate` with basic obstacle avoidance raycasts |
| **Communication** | 100% Non-Verbal (Direct UI updates and basic animations) |
| **Camera** | Fixed Orthographic Isometric Follow Camera (`Vector3.Lerp`) |

---

## 3. Social Map & NPC Roster
There are **8 core residents** each with a unique personality profile (ScriptableObject).
- **Agop (Baker)**: Generous Patriarch, stability node.
- **Fatma (Gossip)**: Information node, carries and spreads emotions.
- **Leila (Artist)**: Sensitive observer, emotional "canary".
- **Hüseyin (Elder)**: Living memory, slow to react but deep impact.
- **Mustafa (Partner)**: Contrarian, emotional mirror to Hüseyin.
- **Selin (Mother)**: Protector, physically reactive (pram collision).
- **Mihail (Vendor)**: Town anchor, passive mood field provider.
- **Ayşe (Arrival)**: Blank slate, personality written by player's choices.

---

## 4. NPC Logic Spec (The A1 Brain)
### 4.1 Emotional States
- **Joy / Sadness / Anger / Fear / Surprise / Disgust**
- Represented as a simple `public enum EmotionType` in the `NPCController`.
- Evaluated when news is received using `NPCPersonalityData`.

### 4.2 Behavioural States
- Managed by a `switch(currentState)` inside the `Update()` loop.
- **IDLE**: Default state.
- **WALKING_TO_BOARD**: Moving towards the newspaper board using simple math (`Vector3.MoveTowards`).
- **READING_NEWS**: Waiting on a simple timer at the board.
- **REACTING**: Triggering a visual/color change based on the emotion enum.
- **WALKING_HOME**: Returning to origin.

---

## 5. Gameplay & Survival
- **The Browser/Camera**: Players press Space to capture a photo of a `PhotoSubject` using simple raycasts.
- **The Newspaper**: Editorial choices (Categories: Scandal, Celebration, Disaster, Local, Global) determine how the town reacts tomorrow.
- **Consequences**: If the town is angry, NPCs might turn red and walk faster.

---

## 6. Implementation Status
### [x] Foundation
- NPC Manager (Simple List Registry)
- Game Manager (Start/Stop coordination)
- Time Manager (Basic float timer converting to hours)

### [x] Player Systems
- WASD Movement (Input.GetAxis + Transform)
- Capture Trigger (Raycast to detect PhotoSubjects)

### [x] NPC Systems
- **State Loop**: Simple `switch` in `Update()` working cleanly.
- **Reactions**: Personality logic mapped to basic enums.
- **Movement**: `transform.Translate` working properly.

---

## 7. Reference Documents
1. [Game Overview](file:///Users/emre/dev/ai/gp-oyun/Design/01_game_overview.md)
2. [World Objects](file:///Users/emre/dev/ai/gp-oyun/Design/02_world_objects.md)
3. [NPC Profiles](file:///Users/emre/dev/ai/gp-oyun/Design/03_npc_profiles.md)
4. [Newspaper System](file:///Users/emre/dev/ai/gp-oyun/Design/04_newspaper_system.md)
5. [Emotion Engine](file:///Users/emre/dev/ai/gp-oyun/Design/05_emotion_pantomime_engine.md)
6. [Collision & Interactions](file:///Users/emre/dev/ai/gp-oyun/Design/06_collision_events_listeners.md)
7. [NPC State Logic](file:///Users/emre/dev/ai/gp-oyun/Design/09_fsm_specifications.md)
8. [Project Report](file:///Users/emre/dev/ai/gp-oyun/Design/10_project_report.md)
