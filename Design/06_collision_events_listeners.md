# GP-OYUN — Game Design Document
## File 06: Collision, Triggers & Interactions (A1 Level)

---

## 1. Architecture Overview

All game events flow through a **simple direct-reference system**:

```
Layer A: Physics/Spatial Events — Unity Collider triggers (OnTriggerEnter / Stay / Exit)
Layer B: Direct Method Calls    — Using GetComponent() to trigger logic directly
```

There is no complex EventBus. When a physical collision happens, the trigger script looks for the appropriate component and calls a method directly.

---

## 2. Trigger Zone Examples

Every trigger zone in the world uses basic Unity A1 physics logic.

### 2.1 `TRG_NewspaperBoard`

```
GameObject:   NewspaperBoard
Collider:     BoxCollider — 2 units wide, 1 unit deep, 1.5 units tall (IsTrigger: true)
Layer:        TriggerZones (Layer 5)

Logic (in NPCController.Update):
  → Instead of OnTriggerEnter, NPCs check Vector3.Distance(transform.position, boardPosition.position)
  → If distance < 0.5f, currentState changes to Reading.
```

### 2.2 `TRG_BenchZone`

```
Collider: SphereCollider, radius 1.2

OnTriggerEnter (Collider other):
  → if (other.CompareTag("NPC"))
       var bench = GetComponent<BenchObject>();
       bench.TryOccupy(other.GetComponent<NPCController>());
```

### 2.3 `TRG_PramZone` (Selin's Pram)

```
Collider: SphereCollider on Pram object, radius 0.8 (IsTrigger: true)

OnTriggerEnter (Collider other):
  → if (other.CompareTag("Player") || other.CompareTag("Prop"))
       var selin = selinGameObject.GetComponent<NPCController>();
       selin.currentEmotion = EmotionType.Fear; // Direct state change
```

### 2.4 `TRG_MihailAura` (Flower Vendor proximity effect)

```
Collider: SphereCollider on Mihail, radius 2.5

OnTriggerStay (Collider other):
  → if (other.CompareTag("NPC"))
       var npc = other.GetComponent<NPCController>();
       npc.currentEmotion = EmotionType.Joy; // Directly influence nearby NPCs
```

---

## 3. Communication Architecture

Since we use direct references, the communication flow looks like this:

```
TimeManager         → Finds NewspaperManager and calls PublishEdition()
NewspaperManager    → Finds all NPCControllers and calls ReceiveNews()
CameraSystem        → Finds NewspaperManager and calls StorePhoto()
Triggers            → Use other.GetComponent<T>() and call public methods
```

---

## 4. Collision Layer Matrix

| | Ground | Walls | Props | NPC Bodies | Player | Triggers |
|---|---|---|---|---|---|---|
| **Ground** | — | — | ✓ | ✓ | ✓ | — |
| **Walls** | — | — | ✓ | ✓ | ✓ | — |
| **Props** | ✓ | ✓ | ✓ | — | — | — |
| **NPC Bodies** | ✓ | ✓ | — | — | ✓ | ✓ |
| **Player** | ✓ | ✓ | — | ✓ | — | ✓ |
| **Triggers** | — | — | — | ✓ | ✓ | — |

> ✓ = layers collide / detect  
> Props don't collide with NPC bodies (NPCs pass through props unless scripted)  
> Props do collide with Ground and Walls (physics-based)
