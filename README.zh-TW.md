<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX Timer

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.timer)](https://github.com/GameFrameX/com.gameframex.unity.timer/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.timer)](https://github.com/GameFrameX/com.gameframex.unity.timer/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#quick-start) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>
## 功能特性

- **三種計時器模式** — 重複（`Add`）、一次性（`AddOnce`）、逐幀（`AddUpdate`）
- **暫停 / 恢復** — 按計時器 ID 單獨操作，或按標籤批量操作
- **標籤分組** — 為計時器分配字串標籤，支援批量暫停、恢復和移除
- **雙時間尺度** — 每個計時器可使用真實時間（`Unscaled`）或受 `Time.timeScale` 影響（`Scaled`）
- **執行緒安全** — 基於 lock 的更新循環，回呼在鎖外執行避免死結
- **物件池** — `TimerItem` 實例池化復用，減少 GC 壓力
- **非同步等待** — `WaitForSecondsAsync`、`WaitForNextFrameAsync`、`WaitForFramesAsync`，支援 `CancellationToken`
- **查詢 API** — 檢查剩餘時間、已用時間和重複次數
- **完成回呼** — 計時器自然結束或被移除時觸發 `onComplete`
- **IL2CPP 安全** — 裁剪輔助類防止 AOT 建置中的類型剥离

## 安裝

**1. Scoped Registry（推薦）**

編輯 Unity 專案的 `Packages/manifest.json`，添加 `scopedRegistries` 部分：

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

`scopes` 控制哪些套件透過此註冊表解析。只有以 `com.gameframex` 開頭的套件才會從這個註冊表取得。

**2. Git URL**

在 Unity 中開啟 **Window → Package Manager → Add package from git URL**，輸入：

```
https://github.com/GameFrameX/com.gameframex.unity.timer.git
```

**3. 手動安裝**

將此倉庫克隆或下載到 Unity 專案的 `Packages/` 目錄下。

## 使用

所有範例使用 `TimerComponent`（Unity `MonoBehaviour` 封裝）。透過 GameFrameX 元件系統取得：

```csharp
using GameFrameX.Timer.Runtime;

var timer = GameEntry.GetComponent<TimerComponent>();
```

### 重複計時器

按指定間隔（毫秒）觸發。`repeat` 控制觸發次數；`0` 表示無限循環。

```csharp
// 每隔 1 秒觸發一次，共觸發 5 次
int id = timer.Add(1000f, 5, (param) =>
{
    Debug.Log("Tick!");
});
```

### 一次性計時器

間隔到達後觸發一次，然後自動移除。

```csharp
timer.AddOnce(3000f, (param) =>
{
    Debug.Log("3 秒已到");
});
```

### 逐幀回呼

每幀觸發。

```csharp
timer.AddUpdate((param) =>
{
    // 每 Update 呼叫
});
```

### 暫停與恢復

```csharp
timer.Pause(id);

if (timer.IsPaused(id))
{
    timer.Resume(id);
}
```

### 標籤批量操作

```csharp
// 建立時分配標籤
timer.Add(1000f, 0, callback, tag: "enemy-spawn");

// 對該標籤的所有計時器進行操作
timer.PauseByTag("enemy-spawn");
timer.ResumeByTag("enemy-spawn");
timer.RemoveByTag("enemy-spawn");

// 檢查是否存在指定標籤的計時器
bool hasTag = timer.HasTag("enemy-spawn");
```

### 查詢計時器狀態

```csharp
float remaining = timer.GetRemaining(id);    // 距下次觸發的秒數，未找到回傳 -1
float elapsed   = timer.GetElapsed(id);      // 距上次觸發的秒數，未找到回傳 -1
int   repeats   = timer.GetRepeatLeft(id);   // 剩餘觸發次數，未找到回傳 -1，0 表示無限
```

### 完成回呼

```csharp
timer.Add(1000f, 3, callback, onComplete: () =>
{
    Debug.Log("計時器完成");
});
```

### 時間尺度

```csharp
// 受 Time.timeScale 影響（適用於慢動作、暫停選單等場景）
timer.Add(1000f, 0, callback, timeScale: TimerTimeScale.Scaled);
```

### 移除與檢查

```csharp
// 透過回呼參考
timer.Remove(callback);
bool exists = timer.Exists(callback);

// 透過計時器 ID
timer.Remove(id);
bool exists = timer.Exists(id);
```

### 非同步等待

```csharp
// 等待 2 秒
await timer.WaitForSecondsAsync(2f);

// 帶取消令牌的等待
var cts = new CancellationTokenSource();
cts.CancelAfter(5000);
await timer.WaitForSecondsAsync(10f, cts.Token);

// 等待一幀
await timer.WaitForNextFrameAsync();

// 等待 N 幀
await timer.WaitForFramesAsync(3);
```

### 異常處理

將 `TimerManager.CatchCallbackExceptions` 設為 `true`，可捕獲計時器回呼中的例外並以警告形式記錄，而非向上擲出。

```csharp
TimerManager.CatchCallbackExceptions = true;
```

## 環境需求

- Unity 2019.4 或更高版本
- [com.gameframex.unity](https://github.com/GameFrameX/com.gameframex.unity) 1.1.1+

## 文檔

- [官方文檔](https://gameframex.doc.alianblank.com)

## 社群

- QQ群: [加入](https://qm.qq.com/q/3dIpogITg)

## 更新日誌

查看 [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases) 了解版本歷史。

## 開源協議

[開源協議](LICENSE.md)
