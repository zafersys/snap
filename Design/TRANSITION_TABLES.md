# GP-OYUN: Master State Logic (A1 Level)

This document serves as the technical authority for how state changes across the A1 game systems. Instead of complex, dedicated Finite State Machine classes, we rely on basic `switch` statements and `if/else` condition checking inside Unity's `Update()` loop.

---

## 1. Local Time & Day Cycle Logic
**Manager**: `TimeManager`

State changes happen linearly based on a single float variable (`currentHour`).

| Current State | Transition Condition | Next Action |
|---|---|---|
| `Morning` | `currentHour >= 8f` | Directly calls `NewspaperManager.Instance.PublishEdition()` |
| `Midday` | `currentHour >= 12f` | - |
| `Evening` | `currentHour >= 18f` | Enables streetlights. |
| `Night` | `currentHour >= 22f` | `EditorialUI` auto-opens. |
| `End of Day` | `currentHour >= 24f` | Reset `currentHour = 6f`, clear photos. |

---

## 2. NPC Master Behaviour Logic
**Manager**: `NPCController.cs` (Switch statement in `Update()`)

| Current `NPCState` | `if` Condition / Trigger | Next `NPCState` |
|---|---|---|
| `Idle` | `NewspaperManager` calls `ReceiveNews()` | `WalkingToBoard` |
| `WalkingToBoard`| `Vector3.Distance(pos, boardPos) < 0.5f` | `Reading` (starts `readTimer`) |
| `Reading` | `readTimer <= 0` | `Reacting` (Evaluates emotion, starts `reactTimer`) |
| `Reacting` | `reactTimer <= 0` | `WalkingHome` |
| `WalkingHome` | `Vector3.Distance(pos, homePos) < 0.5f` | `Idle` |

---

## 3. UI Master System Logic
**Manager**: Individual UI Scripts (`HUDManager`, `EditorialUI`, `SettingsController`)

| UI Element | Input / Condition | Action |
|---|---|---|
| `HUDManager` | `Input.GetKey(C)` | Enables viewfinder `GameObject`, pulses scale. |
| `EditorialUI` | `Input.GetKeyDown(N)` OR `Hour >= 22f` | Sets CanvasGroup alpha to 1, blocks raycasts. |
| `EditorialUI` | Click "Publish" button | Calls `NewspaperManager.PublishEdition()`, Hides UI. |
| `SettingsMenu`| `Input.GetKeyDown(Escape)` | Toggles CanvasGroup visibility. |

---

## 4. Camera & Newspaper Logic
**Manager**: `CameraController` & `NewspaperManager`

| Script | Input / Condition | Action |
|---|---|---|
| `CameraController` | `Input.GetKeyDown(Space)` + `cooldown <= 0` | Raycasts for `PhotoSubject`, creates `PhotoData`, sends to `NewspaperManager`. |
| `NewspaperManager` | Called by `PublishEdition()` | Iterates through `FindObjectsOfType<NPCController>()` and calls `npc.ReceiveNews()`. |

---

## 5. Settings Menu Logic
**Manager**: `SettingsController`

| Input | Action |
|---|---|
| Slider Value Changed (Sensitivity) | `FindObjectOfType<PlayerController>().moveSpeed = newValue;` |
| Slider Value Changed (Volume) | `AudioListener.volume = newValue;` |

---

## Summary of A1 Transitions
If you ever want to add a new behaviour to an NPC:
1. Add a new name to the `NPCState` enum (e.g., `Sitting`).
2. Add a `case NPCState.Sitting:` to the `Update()` switch statement.
3. Write an `if` condition to transition into that state (e.g., `if (nearBench) currentState = NPCState.Sitting;`).
