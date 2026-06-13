## UI Structure

### The Canvas System
The root component for all UI. It handles the rendering order and scaling. "Screen Space - Overlay" stays fixed on the screen, while "World Space" places UI elements physically in the 3D world.

```csharp
[SerializeField] private Canvas mainInterface;
```

```csharp
public void ToggleUI(bool state) => mainInterface.enabled = state;
```

### Layout Groups
Automatic positioning systems (Vertical, Horizontal, Grid). They manage the size and position of child elements, ensuring UI adapts to different screen resolutions.

```csharp
[SerializeField] private VerticalLayoutGroup listContainer;
```

## Data to UI

### Text & Image Updates
Updating the visual state of UI components. Modern projects use TextMeshPro (TMP) for high-quality, resolution-independent text.

```csharp
using TMPro;
[SerializeField] private TMP_Text labelField;
```

```csharp
public void UpdateLabel(string content) => labelField.text = content;
```

### Event System Callbacks
Connecting UI buttons and sliders to code. Using UnityEvents allows you to link methods in the Inspector or dynamically in scripts.

```csharp
[SerializeField] private Button actionButton;
```

```csharp
void Start() {
    actionButton.onClick.AddListener(OnButtonPress);
}
```

### UI Navigation
Ensuring that menus can be navigated with a keyboard or controller. The EventSystem tracks the "Selected" object and handles transitions.

```csharp
EventSystem.current.SetSelectedGameObject(firstButton);
```
