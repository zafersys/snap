## Input System Basics

### Input Actions
Input is handled via "Actions" rather than hardcoded keys. This layer of abstraction allows for easy controller support and player-defined remapping.

```csharp
public InputAction MoveAction;
```

```csharp
void OnEnable() => MoveAction.Enable();
```

### Event-Based Input
Instead of checking for a key press every frame (Polling), the Input System triggers events when an action's state changes (Started, Performed, Canceled).

```csharp
MoveAction.performed += ctx => {
    Vector2 moveDir = ctx.ReadValue<Vector2>();
};
```

### PlayerInput Component
A higher-level component that automatically maps BroadcastMessages or C# Events to your script methods based on the Input Action Asset.

```csharp
public void OnJump(InputValue value) {
    if (value.isPressed) ApplyJumpForce();
}
```

## Interactions & Values

### Reading Continuous Values
For sticks or WASD, you read "Continuous" values (Vector2). This allows for analog sensitivity and smoother movement logic.

```csharp
Vector2 currentInput = _inputSource.ReadValue<Vector2>();
```

### Interaction Thresholds
Defining "Deadzones" or "Tap" durations. This ensures that minor stick drift or accidental quick presses don't trigger unwanted actions.

```csharp
if (inputAction.WasPressedThisFrame()) {
    ExecuteAction();
}
```
