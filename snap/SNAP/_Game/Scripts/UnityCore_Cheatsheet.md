## Lifecycle & Execution

### Script Lifecycle
Unity scripts follow a strict execution order. Understanding when `Awake` (initialization), `Start` (first frame), and `Update` (every frame) run is crucial for preventing null references.

```csharp
void Awake() { /* Setup internal references */ }
```

```csharp
void Start() { /* Setup external references */ }
```

### Coroutines
A way to pause execution and return control to Unity, then resume on subsequent frames. Perfect for timed sequences or spreading heavy calculations over multiple frames.

```csharp
IEnumerator SequenceRoutine() {
    yield return new WaitForSeconds(1f);
    PerformAction();
    yield return null; // Wait one frame
}
```

### ScriptableObjects
Data containers that exist independently of scene objects. They are used to store configuration, settings, or shared variables, reducing memory usage by avoiding duplicate data on GameObjects.

```csharp
[CreateAssetMenu]
public class ConfigurationAsset : ScriptableObject {
    public float GlobalSpeed;
}
```

## Component Manipulation

### Object Composition
Getting references to other scripts attached to the same or different objects. This is the foundation of modularity in Unity.

```csharp
private OtherComponent _cachedReference;
```

```csharp
void Awake() {
    _cachedReference = GetComponent<OtherComponent>();
}
```

### Instantiate & Destroy
The primary way to create and remove objects at runtime. Objects are usually created from "Prefabs" to maintain consistent settings.

```csharp
GameObject newObj = Instantiate(templatePrefab, spawnPos, Quaternion.identity);
```

```csharp
Destroy(newObj, 5f); // Destroy after 5 seconds
```

### The Transform Component
Every GameObject has a Transform. It handles the hierarchy (parents/children) and spatial data. Local coordinates are relative to the parent, while World coordinates are absolute.

```csharp
transform.SetParent(newParent, true);
```

```csharp
Vector3 globalPos = transform.position;
Vector3 relativePos = transform.localPosition;
```
