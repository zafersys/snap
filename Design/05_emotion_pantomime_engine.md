# GP-OYUN — Game Design Document
## File 05: Emotion System & Pantomime Engine (A1 Level)

---

## 1. The Pantomime Principle

> **There are no words in this world. Every feeling is architecture.**

The entire game's communication layer is built on the **Pantomime Engine** — a system that translates internal emotional state data into physically visible, universally legible character performances.

### ⚠️ CRITICAL DESIGN PRIORITY: BODY > FACE

```
 PRIORITY ORDER (highest to lowest):
 ─────────────────────────────────────
 1. Body posture & movement     ← THIS IS THE GAME
 2. Arm/hand gestures           ← Core readability
 3. Color & Visual Indicators   ← A1 Support layer
```

**Why?** At isometric camera distance (45°, ortho size 9.5), facial micro-expressions are nearly invisible. Players read CHARACTER through HOW THEY MOVE, not how their eyebrows twitch. Detailed facial rigs are expensive and invisible to the player.

---

## 2. The Emotion Model

We use a simple discrete emotion model, derived from basic emotions, adapted for A1 gameplay.

```csharp
public enum EmotionType 
{ 
    Neutral, 
    Joy, 
    Sadness, 
    Anger, 
    Fear, 
    Surprise, 
    Disgust 
}
```

At any moment, each NPC has exactly **one active emotion**. 

---

## 3. Body Language System (A1 Level)

The body layer visually represents the current `EmotionType` through basic Unity Animator triggers and Visual Helpers (color coding).

### 3.1 Basic Visual Helper

Instead of complex blend shapes, the A1 level uses color and basic shape scaling to broadcast emotion.

```csharp
// Example Logic in NPCVisualHelper
switch (currentEmotion)
{
    case EmotionType.Anger:
        meshRenderer.material.color = Color.red;
        transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        break;
    case EmotionType.Sadness:
        meshRenderer.material.color = Color.blue;
        transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
        break;
    // ...
}
```

### 3.2 Animator Parameters (per NPC)

```
TRIGGER parameters:
  Emotion_Shock          — one-shot flinch (Surprise)
  Emotion_Stomp          — anger expression (Anger)
  Emotion_Jump           — joy burst (Joy)

BOOL parameters:
  IsWalking
```

---

## 4. Gesture Library

The core gestural vocabulary of the game — readable at 2.5D distance.

| Gesture Name | Trigger | Body Description |
|---|---|---|
| `Nod_Slow` | Approval / Agreement | One slow forward head dip |
| `HeadShake` | Disagreement | Side-to-side, 2–3 times |
| `Stomp` | Rage | Foot down, arms tense |
| `Spin_Small` | Delight burst | Quarter-turn on spot |
| `Clap` | Celebration | Visible, bouncy handclap |

---

## 5. Emotion State Tracking

All emotional states are stored directly on the `NPCController`.

```csharp
void Update()
{
    // A1 basic timer to reset emotion back to neutral after a few seconds
    if (currentEmotion != EmotionType.Neutral)
    {
        emotionTimer -= Time.deltaTime;
        if (emotionTimer <= 0)
        {
            currentEmotion = EmotionType.Neutral;
            // Update Visuals
        }
    }
}
```

---

## 6. The "Peak Moment" System

A **Peak Moment** is when an NPC is actively in a non-Neutral emotion state. This is the ideal photo moment.

```
Peak Moment conditions:
  → NPC's currentEmotion != EmotionType.Neutral
  → A soft visual indicator appears (subtle rim light or particle)
```

The camera UI frame glows faintly when pointing at a NPC in Peak Moment state (detected via the `PhotoSubject` component). The player must decide: capture this vulnerable moment, or respect it.
