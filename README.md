# Unity Atmospheric Scattering SkyBox & Weather System

A GTA5-inspired atmospheric scattering sky system for Unity URP, featuring a full day/night cycle with stylized Genshin Impact-like twilight aesthetics.

## Features

### Sky Rendering
- **Procedural Sky Dome** — Runtime-generated sphere mesh with multi-layer gradient blending (azimuth, zenith, horizon band)
- **Mie Scattering** — Physically-based sun halo with configurable phase function
- **Sun & Moon** — Sharp disc rendering with adjustable size and glow
- **Starfield** — Texture-based stars with per-star twinkling, tri-planar projection (no stretching)
- **Procedural Clouds** — Multi-octave FBM noise with large + small cloud layers, wind animation, edge highlighting

### Day/Night Cycle
- **24-hour Solar Path** — Realistic sunrise (6:00) / sunset (20:00) timing
- **Daytime Variation** — Sky color shifts with sun height (morning warm → noon saturated blue → afternoon golden)
- **Twilight Colors** — Rose-pink, lavender-purple gradients during dawn/dusk (Genshin-inspired)
- **Smooth Transitions** — Hermite-interpolated sunrise/sunset fades, no discontinuities

### Weather System
- **6 Presets** — Clear, Overcast, Rainy, Thunder, Foggy, Smog
- **Smooth Transitions** — Configurable duration weather blending
- **Post-Processing** — Auto-configured URP Volume (ACES tonemapping, bloom, color adjustments)
- **Fog & Lighting** — Exponential squared fog + directional light synced with sky state

## Requirements

- Unity 2022.3+ (tested with 2022.3.62f3)
- Universal Render Pipeline (URP) 14.x

## Quick Start

1. Clone this repository
2. Open `shaderTest/` as a Unity project
3. Create a URP Asset: `Assets > Create > Rendering > URP Asset (with Universal Renderer)`
4. Set it in: `Edit > Project Settings > Graphics > Scriptable Render Pipeline Settings`
5. Ensure your scene has a **Camera** and a **Directional Light**
6. Press **Play** — everything auto-initializes

### Runtime Controls

| Key | Function |
|-----|----------|
| Top-left UI | Time slider, day speed, weather buttons |
| F3 | Toggle performance profiler overlay |
| F5 | Run full day/night benchmark (manual) |

## Project Structure

```
shaderTest/Assets/
├── Shaders/
│   └── GTA5Sky.shader              # Sky dome shader (scattering, clouds, stars, sun/moon)
├── Scripts/GTA5Sky/
│   ├── DayNightCycle.cs             # Time management & solar position
│   ├── GTA5TimecycleSky.cs          # Core: 88-parameter sky snapshot builder
│   ├── SkyDome.cs                   # Dome mesh generation & material binding
│   ├── WeatherController.cs         # Central orchestrator (sky, fog, light, post-fx)
│   ├── WeatherSettings.cs           # Weather profile ScriptableObject (6 presets)
│   ├── WeatherTransition.cs         # Smooth weather state interpolation
│   ├── WeatherType.cs               # Weather enum
│   ├── GTA5StarfieldTexture.cs      # Starfield texture loader
│   ├── NoiseTextureGenerator.cs     # Precomputed noise texture for GPU cloud FBM
│   ├── SkyProfiler.cs               # F3 runtime profiler overlay
│   ├── SkyBenchmark.cs              # F5 automated day/night benchmark
│   └── SkyDemoUI.cs                 # Runtime debug UI
├── Editor/
│   ├── AutoTestRunner.cs            # File-triggered logic tests + CPU benchmark
│   └── AutoBenchmarkRunner.cs       # File-triggered Play mode benchmark
├── Scripts/Tests/Editor/
│   └── TimecycleSkyTests.cs         # NUnit EditMode unit tests
└── Resources/
    └── StarfieldTex.png             # Star texture (1024x1024)
```

## Performance

Benchmarked at 1920×1080 on a full 24-hour cycle:

| Phase | Avg FPS | Avg Frame | P95 |
|-------|---------|-----------|-----|
| Noon (11-14h) | 196 | 5.09ms | 7.55ms |
| Afternoon (14-18h) | 222 | 4.50ms | 6.79ms |
| Dusk (18-21h) | 233 | 4.29ms | 6.26ms |
| Night (21-5h) | 245 | 4.07ms | 5.89ms |

**Night is 13.2% faster than day** — uniform-based `[branch]` skips stars/moon during daytime.

### GPU Optimizations
- Precomputed noise texture replaces procedural FBM (3 tex samples vs 24 ALU hash ops)
- Stars and moon skip entirely during daytime (`[branch]` on uniform `_MoonFade`)
- Early-out for clouds below horizon
- Branchless sky gradient blending (step+lerp)
- Mie scattering: `x*sqrt(x)` replaces `pow(x, 1.5)`
- Tri-planar star UV avoids dynamic branching

### CPU Optimizations
- All 47 shader property IDs cached via `Shader.PropertyToID`
- Solar parameter throttling (skip rebuild when change < 0.01°)
- Fog/post-processing throttled to every 4th frame
- Low-poly sky dome (768 triangles)
- Zero GC allocation in update loop
- CPU sky overhead: **0.028%** of 60fps frame budget

## Automated Testing

### Logic Tests (EditMode)
Create `run-tests.trigger` in project root, click Unity to compile:

```bash
echo "run" > run-tests.trigger
# Results: test-output/report.md
```

7 tests covering: determinism, solar position, value ranges, color validity, weather lerp, transition smoothness.

### GPU Benchmark (PlayMode)
Create `run-benchmark.trigger`, click Unity — auto enters Play, runs full cycle, exits:

```bash
echo "run" > run-benchmark.trigger
# Results: %LOCALAPPDATA%Low/DefaultCompany/shaderTest/SkyBenchmark/<timestamp>/
```

Outputs: `report.md` (per-phase stats), `frames.csv` (per-frame data), screenshots at 10 key times.

## API

```csharp
// Set time
FindFirstObjectByType<GTA5Sky.DayNightCycle>().SetTimeOfDay(18.5f);

// Change speed
FindFirstObjectByType<GTA5Sky.DayNightCycle>().DaySpeed = 0.5f;

// Weather (instant)
GTA5Sky.WeatherController.Instance.SetWeatherImmediate(GTA5Sky.WeatherType.Rainy);

// Weather (smooth 30s transition)
GTA5Sky.WeatherController.Instance.SetWeather(GTA5Sky.WeatherType.Foggy);
```

## License

MIT
