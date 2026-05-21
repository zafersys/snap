# GP-OYUN — Game Design Document
## File 04: The Newspaper System (A1 Level)

---

## 1. Philosophy

> The newspaper is not just a mechanic. It is the town's **collective unconscious**.  
> What gets published shapes reality. What gets left out shapes secrets.

The player is a journalist with no words — only images. The single editorial decision of *which photos to publish* causes ripple effects that no NPC can anticipate, including the player.

---

## 2. Photo Capture System

### 2.1 The Camera
```
Resource:    3–5 shots per day (upgradeable)
Cooldown:    4 seconds between shots
Framing:     UI frame appears when player holds Capture button
Aim:         Any direction within player's radius
Result:      A PhotoData object stored directly in NewspaperManager
```

### 2.2 What Makes a Good Photo?

The photo-scoring system runs **silently** — the player never sees a score breakdown. 
We use a simple `PhotoSubject` component with predefined categories attached to objects in the world. When the camera raycast hits a `PhotoSubject`, it inherits that category.

---

## 3. The Publishing Interface

Triggered when player enters the **Newspaper Office** zone at evening.

```
┌───────────────────────────────────────────────┐
│           TOMORROW'S EDITION                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐    │
│  │  SLOT 1  │  │  SLOT 2  │  │  SLOT 3  │    │
│  │  (empty) │  │  (empty) │  │  (empty) │    │
│  └──────────┘  └──────────┘  └──────────┘    │
│                                               │
│  YOUR ROLL (today's captures):                │
│  [Photo1] [Photo2] [Photo3] [Photo4]          │
│                                               │
│  Category: [SCANDAL ▾]    [ PUBLISH ]         │
└───────────────────────────────────────────────┘
```

- When "Publish" is clicked, a `NewsPublishedData` object is created.
- `NewspaperManager` directly calls `PublishEdition()` which updates the game state.

---

## 4. The Newspaper Board (World Object)

```
Location:   Centre-left of town square
Visibility: Visible to all NPCs at all times
Reset:      Each morning, new paper is posted
State A:    Empty (before first publish)
State B:    Posted (photos pinned, text implied by category)
```

### Morning Sequence (The A1 Direct Call Flow)
```
1. TimeManager advances the hour to Morning.
2. TimeManager finds the NewspaperManager and calls CheckMorningPublish().
3. NewspaperManager finds all active NPCControllers using FindObjectsOfType.
4. For each NPC, NewspaperManager calls npc.ReceiveNews(newsData).
5. The NPC's internal state machine (switch statement in Update) changes to WalkingToBoard.
6. NPC uses transform.Translate to walk to the board.
7. Upon arrival (Vector3.Distance < 0.5f), state changes to Reading.
8. NPC uses NPCPersonalityData to evaluate the news category and picks an EmotionType.
9. State changes to Reacting, applying a visual color change based on EmotionType.
10. State changes to WalkingHome.
```

---

## 5. NPC Reaction Matrix

### Category × Personality Trait → Emotion Output

*(Evaluated via `NPCPersonalityData` ScriptableObject)*

```
Category: SCANDAL
─────────────────────────────────────────────────────────────
  High Neuroticism    → Fear
  High Agreeableness  → Sadness
  Low Openness        → Disgust
  High Openness       → Surprise

Category: CELEBRATION
─────────────────────────────────────────────────────────────
  High Extraversion   → Joy
  High Agreeableness  → Joy
  High Neuroticism    → Neutral

Category: DISASTER
─────────────────────────────────────────────────────────────
  High Neuroticism    → Fear
  High Agreeableness  → Sadness
  Hüseyin (Elder)     → Neutral
```

---

## 6. Player Moral Dimension

The player is **never told** how their editorial choices affect NPCs. There are no "good photo" / "bad photo" labels. The player must observe, infer, and decide.

> Publishing a false "celebration" photo during a real disaster may temporarily calm the town — but NPCs with high conscientiousness will eventually exhibit **distrust** animations whenever they pass the Newspaper Board.

These are moral consequences written in body language. No text. No score. Only behaviour.
