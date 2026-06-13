## URP Rendering

### Material Property Blocks
A performant way to change material properties on individual objects without creating unique material instances. This prevents "Draw Call" batching breaking and saves memory.

```csharp
MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
propBlock.SetColor("_Color", Color.red);
_renderer.SetPropertyBlock(propBlock);
```

### Camera Stacking
URP allows "Stacking" multiple cameras. A Base camera renders the world, and an Overlay camera renders the UI or specific objects (like a weapon) on top to prevent clipping.

```csharp
var cameraData = mainCamera.GetUniversalAdditionalCameraData();
cameraData.cameraStack.Add(overlayCamera);
```

## Visual Effects

### Particle System Control
Controlling visual effects via code. You can trigger "Bursts" or change emission rates based on game events like explosions or movement speed.

```csharp
[SerializeField] private ParticleSystem vfxEffect;
```

```csharp
public void PlayEffect() => vfxEffect.Play();
```

### Screen Space Effects (Post Processing)
Global visual adjustments (Bloom, Color Grading, Vignette). These are handled via "Volumes" that can be blended or modified at runtime to change the mood.

```csharp
[SerializeField] private Volume postProcessVolume;
```

```csharp
void Update() {
    if (postProcessVolume.profile.TryGet<Bloom>(out var bloom))
        bloom.intensity.value = 5f;
}
```
