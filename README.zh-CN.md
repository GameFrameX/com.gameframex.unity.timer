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
  轻量级、线程安全的 Unity 计时器系统 — 支持重复、一次性和逐帧回调，具备暂停/恢复、标签分组和 async/await 支持。
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">文档</a> ·
  <a href="#安装">安装</a> ·
  <a href="#使用">使用</a> ·
  <a href="https://qm.qq.com/q/3dIpogITg">QQ群</a> ·
  语言: <a href="README.md">English</a> ·
  <strong>简体中文</strong> ·
  <a href="README.zh-TW.md">繁體中文</a> ·
  <a href="README.ja.md">日本語</a> ·
  <a href="README.ko.md">한국어</a>
</p>

---

## 功能特性

- **三种计时器模式** — 重复（`Add`）、一次性（`AddOnce`）、逐帧（`AddUpdate`）
- **暂停 / 恢复** — 按计时器 ID 单独操作，或按标签批量操作
- **标签分组** — 为计时器分配字符串标签，支持批量暂停、恢复和移除
- **双时间尺度** — 每个计时器可使用真实时间（`Unscaled`）或受 `Time.timeScale` 影响（`Scaled`）
- **线程安全** — 基于 lock 的更新循环，回调在锁外执行避免死锁
- **对象池** — `TimerItem` 实例池化复用，减少 GC 压力
- **异步等待** — `WaitForSecondsAsync`、`WaitForNextFrameAsync`、`WaitForFramesAsync`，支持 `CancellationToken`
- **查询 API** — 检查剩余时间、已用时间和重复次数
- **完成回调** — 计时器自然结束或被移除时触发 `onComplete`
- **IL2CPP 安全** — 裁剪辅助类防止 AOT 构建中的类型剥离

## 安装

**1. Scoped Registry（推荐）**

编辑 Unity 项目的 `Packages/manifest.json`，添加 `scopedRegistries` 部分：

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

`scopes` 控制哪些包通过此注册表解析。只有以 `com.gameframex` 开头的包才会从这个注册表获取。

**2. Git URL**

在 Unity 中打开 **Window → Package Manager → Add package from git URL**，输入：

```
https://github.com/GameFrameX/com.gameframex.unity.timer.git
```

**3. 手动安装**

将此仓库克隆或下载到 Unity 项目的 `Packages/` 目录下。

## 使用

所有示例使用 `TimerComponent`（Unity `MonoBehaviour` 封装）。通过 GameFrameX 组件系统获取：

```csharp
using GameFrameX.Timer.Runtime;

var timer = GameEntry.GetComponent<TimerComponent>();
```

### 重复计时器

按指定间隔（毫秒）触发。`repeat` 控制触发次数；`0` 表示无限循环。

```csharp
// 每隔 1 秒触发一次，共触发 5 次
int id = timer.Add(1000f, 5, (param) =>
{
    Debug.Log("Tick!");
});
```

### 一次性计时器

间隔到达后触发一次，然后自动移除。

```csharp
timer.AddOnce(3000f, (param) =>
{
    Debug.Log("3 秒已到");
});
```

### 逐帧回调

每帧触发。

```csharp
timer.AddUpdate((param) =>
{
    // 每 Update 调用
});
```

### 暂停与恢复

```csharp
timer.Pause(id);

if (timer.IsPaused(id))
{
    timer.Resume(id);
}
```

### 标签批量操作

```csharp
// 创建时分配标签
timer.Add(1000f, 0, callback, tag: "enemy-spawn");

// 对该标签的所有计时器进行操作
timer.PauseByTag("enemy-spawn");
timer.ResumeByTag("enemy-spawn");
timer.RemoveByTag("enemy-spawn");

// 检查是否存在指定标签的计时器
bool hasTag = timer.HasTag("enemy-spawn");
```

### 查询计时器状态

```csharp
float remaining = timer.GetRemaining(id);    // 距下次触发的秒数，未找到返回 -1
float elapsed   = timer.GetElapsed(id);      // 距上次触发的秒数，未找到返回 -1
int   repeats   = timer.GetRepeatLeft(id);   // 剩余触发次数，未找到返回 -1，0 表示无限
```

### 完成回调

```csharp
timer.Add(1000f, 3, callback, onComplete: () =>
{
    Debug.Log("计时器完成");
});
```

### 时间尺度

```csharp
// 受 Time.timeScale 影响（适用于慢动作、暂停菜单等场景）
timer.Add(1000f, 0, callback, timeScale: TimerTimeScale.Scaled);
```

### 移除与检查

```csharp
// 通过回调引用
timer.Remove(callback);
bool exists = timer.Exists(callback);

// 通过计时器 ID
timer.Remove(id);
bool exists = timer.Exists(id);
```

### 异步等待

```csharp
// 等待 2 秒
await timer.WaitForSecondsAsync(2f);

// 带取消令牌的等待
var cts = new CancellationTokenSource();
cts.CancelAfter(5000);
await timer.WaitForSecondsAsync(10f, cts.Token);

// 等待一帧
await timer.WaitForNextFrameAsync();

// 等待 N 帧
await timer.WaitForFramesAsync(3);
```

### 异常处理

将 `TimerManager.CatchCallbackExceptions` 设为 `true`，可捕获计时器回调中的异常并以警告形式记录，而非向上抛出。

```csharp
TimerManager.CatchCallbackExceptions = true;
```

## 环境要求

- Unity 2019.4 或更高版本
- [com.gameframex.unity](https://github.com/GameFrameX/com.gameframex.unity) 1.1.1+

## 文档

- [官方文档](https://gameframex.doc.alianblank.com)

## 社区

- QQ群: [加入](https://qm.qq.com/q/3dIpogITg)

## 更新日志

查看 [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases) 了解版本历史。

## 开源协议

[开源协议](LICENSE.md)
