# GP-OYUN — Game Design Document
## File 09: NPC State Logic (A1 Level)

---

## 1. Design Philosophy

> **Keep it simple, keep it readable.**

The NPC architecture is now built using straightforward A1 Unity concepts. We do not use complex object-oriented Finite State Machines (FSMs), decoupled scriptable objects, or abstract interface classes. 

Instead, every NPC is governed by a single, readable `NPCController.cs` script attached to their GameObject.

---

## 2. The Master State Loop

Every NPC manages its behaviour using a simple `enum` and a `switch` statement inside Unity's built-in `Update()` loop.

### 2.1 State Definitions

```csharp
public enum NPCState 
{ 
    Idle, 
    WalkingToBoard, 
    Reading, 
    Reacting, 
    WalkingHome 
}
```

### 2.2 The Update Loop

```csharp
void Update()
{
    switch (currentState)
    {
        case NPCState.Idle:
            // Stand still, wait for NewspaperManager
            break;
            
        case NPCState.WalkingToBoard:
            // Use transform.Translate to move towards boardPosition
            MoveTowards(boardPosition.position);
            
            // Check arrival
            if (Vector3.Distance(transform.position, boardPosition.position) < 0.5f)
            {
                currentState = NPCState.Reading;
                readTimer = 3f;
            }
            break;
            
        case NPCState.Reading:
            // Stand at the board for a few seconds
            readTimer -= Time.deltaTime;
            if (readTimer <= 0)
            {
                ProcessReadNews();
                currentState = NPCState.Reacting;
                reactTimer = 2f;
            }
            break;
            
        case NPCState.Reacting:
            // Play a reaction (color change) based on currentEmotion
            reactTimer -= Time.deltaTime;
            if (reactTimer <= 0)
            {
                currentState = NPCState.WalkingHome;
            }
            break;
            
        case NPCState.WalkingHome:
            // Use transform.Translate to return home
            MoveTowards(homePosition);
            
            if (Vector3.Distance(transform.position, homePosition) < 0.5f)
            {
                currentState = NPCState.Idle;
            }
            break;
    }
}
```

---

## 3. World Object States (A1)

Just like the NPCs, world objects do not use complex state machines. They rely on simple booleans and standard Unity triggers.

### 3.1 The Newspaper Board
The board does not have an FSM. It just holds a reference to the `NewsPublishedData`. When the player clicks publish in the UI, `NewspaperManager` directly populates the data and tells the NPCs to walk to it.

### 3.2 The Bench
The bench simply has a boolean `IsOccupied`. 

```csharp
public bool TryOccupy(NPCController npc)
{
    if (IsOccupied) return false;
    _occupant = npc;
    return true;
}
```

---

## 4. Time and Day Cycle

The day cycle is managed by a simple float timer in the `TimeManager` which converts elapsed time into hours (6:00 to 24:00). 

It uses simple `if` conditions to trigger events:
```csharp
if (currentHour >= 8f && !morningTriggered)
{
    morningTriggered = true;
    NewspaperManager.Instance.PublishEdition();
}
```

---

## 5. Implementation Notes

### File Structure
```
/Scripts/
  ├── NPC/
  │    ├── NPCController.cs         — Holds the switch statement and enum
  │    └── NPCVisualHelper.cs       — Changes color based on emotion
  ├── Managers/
  │    ├── TimeManager.cs           — Simple float timer
  │    └── NewspaperManager.cs      — Calls NPC methods directly
```

### Best Practices for A1
- Do not create new classes for states. Keep logic inside the `switch` statement.
- Use `GetComponent()` or public variables assigned in the Inspector to talk to other scripts.
- Avoid abstract interfaces. Concrete classes are preferred for readability.
