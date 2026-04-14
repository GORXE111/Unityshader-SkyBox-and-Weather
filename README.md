# Unity Atmospheric Scattering SkyBox & Weather System

A GTA5-inspired atmospheric scattering sky system for Unity URP, featuring a full day/night cycle with stylized Genshin Impact-like twilight aesthetics.

## Features

- **Procedural Sky Dome** — Runtime-generated sphere mesh with multi-layer gradient blending (azimuth, zenith, horizon band)
- **Mie Scattering** — Physically-based sun halo with configurable phase function
- **Sun & Moon** — Sharp disc rendering with adjustable size and glow
- **Starfield** — Texture-based stars with per-star twinkling animation, tri-planar projection (no stretching)
- **Procedural Clouds** — Multi-octave FBM noise with large + small cloud layers, wind animation, edge highlighting
- **Day/Night Cycle** — Smooth 24-hour solar path with realistic sunrise/sunset timing
- **Daytime Variation** — Sky color shifts with sun height (morning warm → noon saturated blue → afternoon golden)
- **Twilight Colors** — Rose-pink, lavender-purple gradients during dawn/dusk
- **Weather System** — 6 presets (Clear, Overcast, Rainy, Thunder, Foggy, Smog) with smooth transitions
- **Post-Processing** — Auto-configured URP Volume with ACES tonemapping, bloom, color adjustments
- **Directional Light** — Automatic sun/moon light direction, color, shadow control
- **Fog** — Exponential squared fog synced with sky and weather state

## Requirements

- Unity 2022.3+ (tested with 2022.3 LTS)
- Universal Render Pipeline (URP) 14.x

## Quick Start

1. Clone this repository
2. Open `shaderTest/` as a Unity project
3. Create a URP Asset: `Assets > Create > Rendering > URP Asset (with Universal Renderer)`
4. Set it in: `Edit > Project Settings > Graphics > Scriptable Render Pipeline Settings`
5. Ensure your scene has a **Camera** and a **Directional Light**
6. Press **Play** — everything auto-initializes

A runtime UI panel appears in the top-left corner with:
- Time of day slider (0-24h)
- Day speed control
- Weather preset buttons

## Project Structure

```
shaderTest/Assets/
├── Shaders/
│   └── GTA5Sky.shader          # Sky dome shader (scattering, clouds, stars, sun/moon)
├── Scripts/GTA5Sky/
│   ├── DayNightCycle.cs         # Time management & solar position
│   ├── GTA5TimecycleSky.cs      # Core: 88-parameter sky snapshot from time + weather
│   ├── SkyDome.cs               # Dome mesh generation & material parameter binding
│   ├── WeatherController.cs     # Central orchestrator (sky, fog, light, post-processing)
│   ├── WeatherSettings.cs       # Weather profile ScriptableObject with 6 presets
│   ├── WeatherTransition.cs     # Smooth weather state interpolation
│   ├── WeatherType.cs           # Weather enum
│   ├── GTA5StarfieldTexture.cs  # Starfield texture loader
│   └── SkyDemoUI.cs             # Runtime debug UI
└── Resources/
    └── StarfieldTex.png         # Star texture (1024x1024)
```

## Performance

The system is optimized for real-time rendering:

**GPU:**
- 3-octave FBM noise (optimized from 4)
- Early-out for clouds below horizon and in clear areas
- Branchless sky gradient blending
- Mie scattering uses `x * sqrt(x)` instead of `pow(x, 1.5)`
- Tri-planar star projection avoids dynamic branching

**CPU:**
- All shader property IDs cached (`Shader.PropertyToID`)
- Low-poly sky dome (768 triangles)
- Zero GC allocation in update loop
- Directional light cached after first lookup
- Sky update skipped when time hasn't changed

## Customization

### Time Control (Runtime)
```csharp
// Set time directly
FindFirstObjectByType<GTA5Sky.DayNightCycle>().SetTimeOfDay(18.5f); // 6:30 PM

// Change day speed
FindFirstObjectByType<GTA5Sky.DayNightCycle>().DaySpeed = 0.5f;
```

### Weather Control (Runtime)
```csharp
// Instant weather change
GTA5Sky.WeatherController.Instance.SetWeatherImmediate(GTA5Sky.WeatherType.Rainy);

// Smooth transition (30 seconds default)
GTA5Sky.WeatherController.Instance.SetWeather(GTA5Sky.WeatherType.Foggy);
```

## License

MIT
