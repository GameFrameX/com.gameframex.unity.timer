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
  独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">文档</a> ·
  <a href="#快速开始">快速开始</a> ·
  <a href="https://qm.qq.com/q/3dIpogITg">QQ群</a> ·
  语言: <a href="README.md">English</a> ·
  **简体中文** ·
  <a href="README.zh-TW.md">繁體中文</a> ·
  <a href="README.ja.md">日本語</a> ·
  <a href="README.ko.md">한국어</a>
</p>

---

## 项目简介

GameFrameX.Timer 是 GameFrameX 框架的计时器组件。提供计时器功能，用于在 Unity 项目中管理和处理定时任务，使计时器功能的使用更加简单高效。

## 快速开始

### 安装方式（任选其一）

1. 直接在 `manifest.json` 的 `dependencies` 节点下添加以下内容：
   ```json
   {"com.gameframex.unity.timer": "https://github.com/GameFrameX/com.gameframex.unity.timer.git"}
   ```

2. 在 Unity 的 `Packages Manager` 中使用 `Git URL` 的方式添加库，地址为：https://github.com/GameFrameX/com.gameframex.unity.timer.git

3. 直接下载仓库放置到 Unity 项目的 `Packages` 目录下，会自动加载识别。

### 使用示例

```csharp
// 获取计时器组件
var timerComponent = GameEntry.GetComponent<TimerComponent>();

// 添加重复任务：每隔 1000 毫秒执行，共执行 5 次
timerComponent.Add(1000, 5, MyMethod);

// 添加一次性任务：5000 毫秒后执行一次
timerComponent.AddOnce(5000, MyMethod);

// 添加每帧更新任务
timerComponent.AddUpdate(MyMethod);

// 检查任务是否存在
bool exists = timerComponent.Exists(MyMethod);

// 移除任务
timerComponent.Remove(MyMethod);
```

## 文档与资源

- [官方文档](https://gameframex.doc.alianblank.com)

## 社区与支持

- QQ群: [加入](https://qm.qq.com/q/3dIpogITg)

## 更新日志

查看 [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases) 了解更新日志。

## 开源协议

本项目基于 MIT 协议开源 - 详见 [LICENSE](https://github.com/GameFrameX/com.gameframex.unity.timer/blob/main/LICENSE) 文件。
