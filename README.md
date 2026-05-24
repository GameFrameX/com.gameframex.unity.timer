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
  All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">Documentation</a> ·
  <a href="#quick-start">Quick Start</a> ·
  <a href="https://qm.qq.com/q/3dIpogITg">QQ Group</a> ·
  Language: **English** ·
  <a href="README.zh-CN.md">简体中文</a> ·
  <a href="README.zh-TW.md">繁體中文</a> ·
  <a href="README.ja.md">日本語</a> ·
  <a href="README.ko.md">한국어</a>
</p>

---

## Project Overview

GameFrameX.Timer is the Timer component for the GameFrameX framework. It provides timer functionality for managing and handling timed tasks in Unity projects, making timer usage simpler and more efficient.

## Quick Start

### Installation (choose one)

1. Add the following to the `dependencies` section of your `manifest.json`:
   ```json
   {"com.gameframex.unity.timer": "https://github.com/GameFrameX/com.gameframex.unity.timer.git"}
   ```

2. In Unity's Package Manager, use `Git URL` to add the package: https://github.com/GameFrameX/com.gameframex.unity.timer.git

3. Download the repository and place it in your Unity project's `Packages` directory. It will be loaded automatically.

### Usage

```csharp
// Get the Timer component
var timerComponent = GameEntry.GetComponent<TimerComponent>();

// Add a recurring task: execute every 1000ms, repeat 5 times
timerComponent.Add(1000, 5, MyMethod);

// Add a one-time task: execute after 5000ms
timerComponent.AddOnce(5000, MyMethod);

// Add a per-frame update task
timerComponent.AddUpdate(MyMethod);

// Check if a task exists
bool exists = timerComponent.Exists(MyMethod);

// Remove a task
timerComponent.Remove(MyMethod);
```

## Documentation & Resources

- [Official Documentation](https://gameframex.doc.alianblank.com)

## Community & Support

- QQ Group: [Join](https://qm.qq.com/q/3dIpogITg)

## Changelog

See [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases) for changelog.

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/GameFrameX/com.gameframex.unity.timer/blob/main/LICENSE) file for details.
