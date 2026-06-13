## Spatial Queries

### Raycasting
Projecting an invisible line into the world to detect what it hits. Essential for shooting, line-of-sight, or detecting the floor.

```csharp
if (Physics.Raycast(origin, direction, out RaycastHit info, distance)) {
    GameObject hitTarget = info.collider.gameObject;
}
```

### Overlap Queries
Checking for all objects within a specific shape (Sphere, Box). This is used for area-of-effect logic or finding nearby interactables.

```csharp
Collider[] nearby = Physics.OverlapSphere(centerPoint, radius, targetLayer);
```

## Collisions & Triggers

### Collision Callbacks
Triggered when two objects with colliders and at least one Rigidbody physically impact each other. Used for bouncing, damage on impact, or sound effects.

```csharp
void OnCollisionEnter(Collision impact) {
    float force = impact.relativeVelocity.magnitude;
}
```

### Trigger Callbacks
Triggered when an object enters a volume marked as "Is Trigger." Unlike collisions, triggers don't push objects back; they are used for zones (e.g., checkpoints, proximity alerts).

```csharp
void OnTriggerEnter(Collider other) {
    if (other.CompareTag("Player")) GrantAccess();
}
```

### Layer Masks
A way to tell physics queries to ignore certain objects. This is critical for performance and ensuring that, for example, a ground check doesn't "hit" the player itself.

```csharp
[SerializeField] private LayerMask groundLayer;
```

```csharp
bool isGrounded = Physics.CheckSphere(pos, radius, groundLayer);
```
