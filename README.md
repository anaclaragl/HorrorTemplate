# Horror Template

A reusable Unity 3D foundation for building short, atmospheric first-person horror games with a PS2/PS3 early aesthetic. Each game is built on top of this shared template, keeping development fast and consistent across titles.

---

## Shared Systems

The following systems are built once and reused across all games in this template:

**Core**
- First-person movement
- First-person camera
- Contextual interaction system (raycast-based)
- Inventory system (items, keys, documents)
- Scene loading system with seamless transitions
- Video and audio settings (resolution, quality, volume)

**Gameplay**
- Enemy AI base (waypoint patrol, sound detection)
- Soft-capture system (no instant death — reposition + disorientation effect)
- Capture counter with configurable defeat threshold
- Flashlight / light source system with battery drain
- Quest / objective tracker (silent, no HUD clutter)
- Interactable audio sources (radios, recorders, ambience triggers)
- Door and lock system (key-based and puzzle-based)

**Narrative**
- Document collection system with readable UI
- Journal system with automatic entries and scripted extra entries
- Proximity narrative triggers (zone-based story events)
- Multi-ending tracker (silent boolean flags per collectible / action)
- Cutscene system (minimal, mostly environmental)

**Atmosphere**
- Dynamic fog system (per-zone density and color)
- Ambient audio zones with smooth crossfade
- Scripted environmental events (object repositioning, geometry shifts)
- Post-processing stack (film grain, vignette, chromatic aberration, color grading)

---

## Tech Stack

| | |
|---|---|
| Engine | Unity 3D |
| Render Pipeline | URP |
| Language | C# |
| 3D Art | Blender + Asset Store mix |
| Target Platform | PC (Windows) |

---

## Project Structure

```
HorrorTemplate/
├── _Template/          # Shared systems and base prefabs
│   ├── Player/
│   ├── AI/
│   ├── Inventory/
│   ├── Narrative/
│   ├── Audio/
│   └── Shaders/
```