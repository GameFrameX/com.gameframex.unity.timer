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
  獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">文檔</a> ·
  <a href="#快速開始">快速開始</a> ·
  <a href="https://qm.qq.com/q/3dIpogITg">QQ群</a> ·
  語言: <a href="README.md">English</a> ·
  <a href="README.zh-CN.md">简体中文</a> ·
  **繁體中文** ·
  <a href="README.ja.md">日本語</a> ·
  <a href="README.ko.md">한국어</a>
</p>

---

## 項目簡介

GameFrameX.Timer 是 GameFrameX 框架的計時器組件。提供計時器功能，用於在 Unity 專案中管理和處理定時任務，使計時器功能的使用更加簡單高效。

## 快速開始

### 安裝方式（任選其一）

1. 直接在 `manifest.json` 的 `dependencies` 節點下加入以下內容：
   ```json
   {"com.gameframex.unity.timer": "https://github.com/GameFrameX/com.gameframex.unity.timer.git"}
   ```

2. 在 Unity 的 `Packages Manager` 中使用 `Git URL` 的方式添加庫，地址為：https://github.com/GameFrameX/com.gameframex.unity.timer.git

3. 直接下載倉庫放置到 Unity 專案的 `Packages` 目錄下，會自動載入識別。

### 使用範例

```csharp
// 獲取計時器組件
var timerComponent = GameEntry.GetComponent<TimerComponent>();

// 添加重複任務：每隔 1000 毫秒執行，共執行 5 次
timerComponent.Add(1000, 5, MyMethod);

// 添加一次性任務：5000 毫秒後執行一次
timerComponent.AddOnce(5000, MyMethod);

// 添加每幀更新任務
timerComponent.AddUpdate(MyMethod);

// 檢查任務是否存在
bool exists = timerComponent.Exists(MyMethod);

// 移除任務
timerComponent.Remove(MyMethod);
```

## 文檔與資源

- [官方文檔](https://gameframex.doc.alianblank.com)

## 社區與支援

- QQ群: [加入](https://qm.qq.com/q/3dIpogITg)

## 更新日誌

查看 [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases) 了解更新日誌。

## 開源協議

本專案基於 MIT 協議開源 - 詳見 [LICENSE](https://github.com/GameFrameX/com.gameframex.unity.timer/blob/main/LICENSE) 檔案。
