# Unity Atmospheric Scattering SkyBox & Weather System

A GTA5-inspired atmospheric scattering sky system for Unity URP, featuring a full day/night cycle with stylized Genshin Impact-like twilight aesthetics.

## Features

### Sky Rendering
- **Procedural Sky Dome** — Runtime-generated sphere mesh with multi-layer gradient blending (azimuth, zenith, horizon band)
- **Mie Scattering** — Physically-based sun halo with configurable phase function
- **Sun & Moon** — Sharp disc rendering with adjustable size and glow
- **Starfield** — Texture-based stars with per-star twinkling, tri-planar projection (no stretching)
- **Procedural Clouds** — Noise-texture FBM with large + small cloud layers, wind animation, edge highlighting

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
| F6 | Run GPU profiler (manual) |
| F7 | Capture 600 frames of Unity Profiler data (manual) |

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
│   ├── GPUProfiler.cs               # F6 GPU performance profiler
│   ├── ProfilerDataExporter.cs      # F7 Unity Profiler data exporter
│   └── SkyDemoUI.cs                 # Runtime debug UI
├── Editor/
│   ├── AutoTestRunner.cs            # File-triggered logic tests + CPU benchmark
│   ├── AutoBenchmarkRunner.cs       # File-triggered Play mode benchmark
│   ├── AutoGPUProfileRunner.cs      # File-triggered GPU profiler
│   └── AutoProfilerExportRunner.cs  # File-triggered Profiler data export
├── Scripts/Tests/Editor/
│   └── TimecycleSkyTests.cs         # NUnit EditMode unit tests
└── Resources/
    └── StarfieldTex.png             # Star texture (1024x1024)
```

## Performance

### Benchmark Results (RTX 3060 @ 1920x1080)

| Phase | Avg FPS | Avg Frame | P95 |
|-------|---------|-----------|-----|
| Noon (11-14h) | 265 | 3.77ms | — |
| Afternoon (14-18h) | 265 | 3.77ms | — |
| Dusk (18-21h) | 255 | 3.92ms | — |
| Night (21-5h) | 270 | 3.70ms | — |
| Dawn (5-7h) | 219 | 4.57ms | — |

### Unity Profiler Data (600 frames)

| Metric | Value |
|--------|-------|
| Main Thread avg | 4.06ms (246 FPS) |
| Main Thread P50 | 3.84ms (260 FPS) |
| Main Thread P99 | 8.34ms |
| Camera.Render | **0.003ms** (2.8us) |
| Draw Calls | 52 (1 sky dome + 51 editor) |
| Triangles | 1650 (768 sky dome) |
| Render Textures | 33 (URP + post-processing) |

### Real Overhead

The sky system's actual CPU cost:
- `GTA5TimecycleSky.Build()`: **3.7us** per call
- Material property updates: **~10us** per full update (every 15 frames)
- **Total: <0.02ms per frame = 0.1% of 60fps budget**

### GPU Optimizations Applied
- Precomputed noise texture replaces procedural FBM (3 tex samples vs 24 ALU hash ops)
- Stars and moon skip entirely during daytime (`[branch]` on uniform `_MoonFade`)
- Early-out for clouds below horizon
- Branchless sky gradient blending (step+lerp)
- Mie scattering: `x*sqrt(x)` replaces `pow(x, 1.5)`
- CPU pre-multiplied color×intensity (saves 5 per-pixel multiplies)
- Single dominant-face star UV projection

### CPU Optimizations Applied
- 3-tier update frequency: cloud offset every frame, sky params every 15 frames, fog/post-fx every 60 frames
- All shader property IDs cached via `Shader.PropertyToID`
- Solar parameter throttling (skip rebuild when change < 0.01°)
- Low-poly sky dome (768 triangles)
- Zero GC allocation in update loop

## Automated Testing

### Logic Tests
```bash
echo "run" > run-tests.trigger
# Click Unity → Results: test-output/report.md
```
7 tests: determinism, solar position, value ranges, color validity, weather lerp, transition smoothness, CPU perf.

### GPU Benchmark
```bash
echo "run" > run-benchmark.trigger
# Click Unity → auto Play → full day cycle → results in AppData
```

### GPU Profiler
```bash
echo "run" > run-gpu-profile.trigger
# Click Unity → auto Play → per-phase GPU analysis → test-output/gpu-report.md
```

### Unity Profiler Export
```bash
echo "run" > run-profiler-export.trigger
# Click Unity → auto Play → 600 frames → test-output/profiler-export.md + profiler-frames.csv
```

## API

```csharp
// Set time
FindFirstObjectByType<GTA5Sky.DayNightCycle>().SetTimeOfDay(18.5f);

// Change speed (24-minute day cycle = DaySpeed of 0.0167)
FindFirstObjectByType<GTA5Sky.DayNightCycle>().DaySpeed = 0.0167f;

// Weather (instant)
GTA5Sky.WeatherController.Instance.SetWeatherImmediate(GTA5Sky.WeatherType.Rainy);

// Weather (smooth 30s transition)
GTA5Sky.WeatherController.Instance.SetWeather(GTA5Sky.WeatherType.Foggy);
```

## License

MIT
