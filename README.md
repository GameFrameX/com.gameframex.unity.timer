<p align="center">
  <img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />
</p>

<h1 align="center">GameFrameX Timer</h1>

<p align="center">
  <a href="https://github.com/GameFrameX/com.gameframex.unity.timer/releases">
    <img src="https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.timer?style=flat-square" alt="Version" />
  </a>
  <a href="https://github.com/GameFrameX/com.gameframex.unity.timer/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.timer?style=flat-square" alt="License" />
  </a>
  <a href="https://gameframex.doc.alianblank.com">
    <img src="https://img.shields.io/badge/Documentation-online-blue?style=flat-square" alt="Documentation" />
  </a>
</p>

<p align="center">
  Lightweight, thread-safe timer system for Unity — repeated, one-shot, and per-frame callbacks with pause/resume, tags, and async/await support.
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">Documentation</a> ·
  <a href="#installation">Installation</a> ·
  <a href="#usage">Usage</a> ·
  <a href="https://qm.qq.com/q/3dIpogITg">QQ Group</a> ·
  Language: <strong>English</strong> ·
  <a href="README.zh-CN.md">简体中文</a> ·
  <a href="README.zh-TW.md">繁體中文</a> ·
  <a href="README.ja.md">日本語</a> ·
  <a href="README.ko.md">한국어</a>
</p>

---

## Features

- **Three timer modes** — repeated (`Add`), one-shot (`AddOnce`), per-frame (`AddUpdate`)
- **Pause / Resume** — individually by timer ID, or in bulk by tag
- **Tag-based grouping** — assign string tags for batch pause, resume, and removal
- **Dual time scale** — each timer uses real time (`Unscaled`) or `Time.timeScale`-affected (`Scaled`)
- **Thread-safe** — lock-based update loop with out-of-lock callback invocation
- **Object pooling** — `TimerItem` instances are pooled to minimize GC pressure
- **Async/await** — `WaitForSecondsAsync`, `WaitForNextFrameAsync`, `WaitForFramesAsync` with `CancellationToken`
- **Query API** — inspect remaining time, elapsed time, and repeat count
- **OnComplete callback** — fires when a timer finishes naturally or is removed
- **IL2CPP safe** — cropping helper prevents type stripping in AOT builds

## Installation

**1. Scoped Registry (recommended)**

Edit your Unity project's `Packages/manifest.json` and add the `scopedRegistries` section:

```json
{
  "scopedRegistries": [
    {
      "name": "GameFrameX",
      "url": "https://gameframex.upm.alianblank.uk",
      "scopes": [
        "com.gameframex"
      ]
    }
  ],
  "dependencies": {
    "com.gameframex.unity.timer": "1.1.1"
  }
}
```

`scopes` controls which packages are resolved through this registry. Only packages whose names start with `com.gameframex` will be fetched from it.

**2. Git URL**

In Unity, open **Window → Package Manager → Add package from git URL** and enter:

```
https://github.com/GameFrameX/com.gameframex.unity.timer.git
```

**3. Manual**

Clone or download this repository into your project's `Packages/` directory.

## Usage

All examples use `TimerComponent`, the Unity `MonoBehaviour` wrapper. Obtain it via the GameFrameX component system:

```csharp
using GameFrameX.Timer.Runtime;

var timer = GameEntry.GetComponent<TimerComponent>();
```

### Repeated Timer

Fires at the given interval (milliseconds). `repeat` controls how many times it fires; `0` means infinite.

```csharp
// Fire every 1 second, repeat 5 times
int id = timer.Add(1000f, 5, (param) =>
{
    Debug.Log("Tick!");
});
```

### One-Shot Timer

Fires once after the interval, then auto-removes.

```csharp
timer.AddOnce(3000f, (param) =>
{
    Debug.Log("3 seconds elapsed");
});
```

### Per-Frame Callback

Fires every frame.

```csharp
timer.AddUpdate((param) =>
{
    // called every Update
});
```

### Pause & Resume

```csharp
timer.Pause(id);

if (timer.IsPaused(id))
{
    timer.Resume(id);
}
```

### Tag-Based Batch Operations

```csharp
// Assign a tag when creating
timer.Add(1000f, 0, callback, tag: "enemy-spawn");

// Operate on all timers with that tag
timer.PauseByTag("enemy-spawn");
timer.ResumeByTag("enemy-spawn");
timer.RemoveByTag("enemy-spawn");

// Check if any timer has a given tag
bool hasTag = timer.HasTag("enemy-spawn");
```

### Query Timer State

```csharp
float remaining = timer.GetRemaining(id);    // seconds until next fire, -1 if not found
float elapsed   = timer.GetElapsed(id);      // time since last fire, -1 if not found
int   repeats   = timer.GetRepeatLeft(id);   // remaining fires, -1 if not found, 0 = infinite
```

### OnComplete Callback

```csharp
timer.Add(1000f, 3, callback, onComplete: () =>
{
    Debug.Log("Timer finished");
});
```

### Time Scale

```csharp
// Affected by Time.timeScale (useful during slow-motion, pause menus, etc.)
timer.Add(1000f, 0, callback, timeScale: TimerTimeScale.Scaled);
```

### Remove & Check Existence

```csharp
// By callback reference
timer.Remove(callback);
bool exists = timer.Exists(callback);

// By timer ID
timer.Remove(id);
bool exists = timer.Exists(id);
```

### Async / Await

```csharp
// Wait 2 seconds
await timer.WaitForSecondsAsync(2f);

// Wait with cancellation
var cts = new CancellationTokenSource();
cts.CancelAfter(5000);
await timer.WaitForSecondsAsync(10f, cts.Token);

// Wait one frame
await timer.WaitForNextFrameAsync();

// Wait N frames
await timer.WaitForFramesAsync(3);
```

### Exception Handling

Set `TimerManager.CatchCallbackExceptions` to `true` to catch exceptions in timer callbacks and log them as warnings instead of propagating.

```csharp
TimerManager.CatchCallbackExceptions = true;
```

## Requirements

- Unity 2019.4 or later
- [com.gameframex.unity](https://github.com/GameFrameX/com.gameframex.unity) 1.1.1+

## Documentation

- [Official Documentation](https://gameframex.doc.alianblank.com)

## Community

- QQ Group: [Join](https://qm.qq.com/q/3dIpogITg)

## Changelog

See [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases) for version history.

## License

[MIT](LICENSE.md)
