## Language Syntax & Types

### Generics
Generics allow you to write classes or methods that can work with any data type. This promotes code reuse and type safety without the performance cost of boxing/unboxing.

```csharp
public class Container<T> {
    private T _data;
    public void SetData(T value) { _data = value; }
}
```

### Delegates & Actions
Delegates are type-safe function pointers. They allow you to pass methods as arguments, enabling powerful callback systems and event-driven architectures.

```csharp
public delegate void EventTrigger(string message);
public Action<int> OnValueUpdate;
```

```csharp
public void InvokeCallback() {
    OnValueUpdate?.Invoke(42);
}
```

### Expression-Bodied Members
A concise syntax for writing methods or properties that consist of a single expression. This reduces boilerplate and improves readability for simple logic.

```csharp
public float CurrentRatio => _activeValue / _maxValue;
```

```csharp
public void LogEntry(string txt) => Debug.Log(txt);
```

## Advanced Logic Patterns

### Asynchronous Tasks (Async/Await)
Allows for non-blocking execution of code that takes time (like loading assets). It keeps the game responsive by offloading waiting periods without freezing the main thread.

```csharp
public async Task<DataResult> FetchRemoteData() {
    await Task.Delay(1000);
    return new DataResult();
}
```

### LINQ (Language Integrated Query)
A powerful syntax for querying and manipulating collections. It allows you to filter, sort, and transform lists with a declarative, readable style.

```csharp
var activeItems = allItems.Where(i => i.IsActive).OrderBy(i => i.Rank).ToList();
```

### Extension Methods
Allow you to "add" methods to existing types without modifying their source code. This is perfect for creating utility functions that feel like built-in features.

```csharp
public static class VectorExtensions {
    public static Vector3 WithY(this Vector3 vec, float y) => new Vector3(vec.x, y, vec.z);
}
```
