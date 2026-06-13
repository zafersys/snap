## Core Architecture & Infrastructure

### Singleton Pattern
A design pattern that ensures a class has only one instance and provides a global point of access to it. It is primarily used for managers that need to persist across scenes or provide centralized control.

```csharp
public static GlobalManager Instance { get; private set; }
```

```csharp
private void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
}
```

### EventBus / Decoupled Communication
Systems communicate via a central hub without direct references. This decouples logic by allowing any system to broadcast a message that others can listen for, ensuring changes in one module don't break others.

```csharp
public void EmitMessage<T>(T payload) {
    var type = typeof(T);
    foreach (var callback in _messageRegistry[type])
        (callback as Action<T>)?.Invoke(payload);
}
```

```csharp
public void RegisterListener<T>(Action<T> callback) {
    _messageRegistry[typeof(T)].Add(callback);
}
```

### The Bootstrap Process
A centralized initialization sequence that ensures systems are started in the correct order. This prevents "Race Conditions" where one system tries to access another before it has been properly set up.

```csharp
private void Start() {
    InitializeHardware();
    InitializeDataSystems();
    NotifySystemsReady();
}
```

## Gameplay Logic & Systems

### Newspaper Pipeline
A data-driven system where captured "content" (images/text) is processed into a published format. It acts as a bridge between player actions (photography) and world consequences (NPC reactions).

```csharp
public void DistributeContent(DataEntry primary, DataEntry secondary) {
    CurrentIssue.SlotA = primary;
    CurrentIssue.SlotB = secondary;
}
```

### Deterministic Emotion System
NPC reactions are calculated by mapping external events (news) against internal personality traits. This ensures behaviors are predictable and logically consistent rather than randomly generated.

```csharp
public Emotion CalculateResponse(ContextData eventData, PersonalityData traits) {
    float affinity = traits.GetAffinity(eventData.Category);
    return affinity > Threshold ? Emotion.Positive : Emotion.Negative;
}
```

### Time-Based State Machine
The game world transitions through distinct phases (e.g., Morning to Evening) based on a timer or event triggers. Each phase alters the available actions and environment lighting to simulate time progression.

```csharp
public void AdvancePhase() {
    CurrentPhase = (Phase)((int)(CurrentPhase + 1) % TotalPhases);
    SignalPhaseShift(CurrentPhase);
}
```

## Entity & AI Mechanics

### Component-Based Controller
Entities are built by attaching modular scripts (Components) to a base object. This allows for flexible behavior where an NPC can easily gain "Movement" or "Vision" by simply adding a script.

```csharp
public class EntityController : MonoBehaviour {
    private MovementModule _motor;
    private SensoryModule _vision;
}
```

### Finite State Machine (FSM)
Logic is divided into discrete "States." An entity can only be in one state at a time, and transitions between states are governed by specific rules, simplifying complex AI decision-making.

```csharp
public void ChangeBehavior(IBehavior newState) {
    _activeState?.Exit();
    _activeState = newState;
    _activeState?.Enter();
}
```

### Steering & Avoidance
Instead of simple linear movement, agents calculate a "Desired Velocity" and apply a "Steering Force" to reach it while avoiding obstacles. This produces fluid, organic-looking paths.

```csharp
Vector3 CalculatePathfinding(Vector3 currentPos, Vector3 targetPos) {
    Vector3 direction = (targetPos - currentPos).normalized;
    return direction * MaxVelocity;
}
```

### Procedural Animation (Bobbing)
A mathematical approach to animation where positions are offset using Sine or Cosine waves. This creates smooth, repetitive motion (like floating or breathing) without needing pre-made animation clips.

```csharp
private void ApplyFloatingEffect() {
    float offset = Mathf.Sin(Time.time * Frequency) * Amplitude;
    transform.position += new Vector3(0, offset, 0);
}
```

## Utilities & Interaction

### Input System Mapping
Player inputs are decoupled from game logic via an Input Action Asset. This allows the same "Action" (e.g., "Interact") to be triggered by different keys or controllers without changing the script.

```csharp
public void OnInteractionTriggered(InputAction.CallbackContext context) {
    if (context.performed) PerformInteraction();
}
```

### Material Manipulation
Changing visual properties (Color, Texture, Emission) at runtime by accessing the renderer's material properties. This is used for highlighting objects or signaling status changes.

```csharp
public void SetVisualStatus(Color targetColor) {
    _entityRenderer.material.SetColor("_BaseColor", targetColor);
}
```

### Performance Telemetry
Tracking and reporting metrics (Frame Rate, Memory, Position) to a central logger. This data is vital for debugging performance bottlenecks and identifying "bottleneck" code sections.

```csharp
public void LogPerformanceMetric(string label, float value) {
    _metricsContainer[label] = value;
}
```
