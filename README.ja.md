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
  軽量でスレッドセーフな Unity タイマーシステム — 繰り返し・ワンショット・フレーム毎コールバックに対応し、一時停止/再開、タググループ、async/await をサポート。
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">ドキュメント</a> ·
  <a href="#インストール">インストール</a> ·
  <a href="#使用方法">使用方法</a> ·
  <a href="https://qm.qq.com/q/3dIpogITg">QQグループ</a> ·
  言語: <a href="README.md">English</a> ·
  <a href="README.zh-CN.md">简体中文</a> ·
  <a href="README.zh-TW.md">繁體中文</a> ·
  <strong>日本語</strong> ·
  <a href="README.ko.md">한국어</a>
</p>

---

## 機能

- **3つのタイマーモード** — 繰り返し（`Add`）、ワンショット（`AddOnce`）、フレーム毎（`AddUpdate`）
- **一時停止 / 再開** — タイマー ID で個別、またはタグで一括操作
- **タググループ** — 文字列タグを割り当てて一括一時停止・再開・削除
- **2つのタイムスケール** — 各タイマーでリアルタイム（`Unscaled`）または `Time.timeScale` 影響下（`Scaled`）を選択
- **スレッドセーフ** — lock ベースの更新ループ、コールバックはロック外で実行しデッドロックを防止
- **オブジェクトプール** — `TimerItem` インスタンスをプールして再利用し GC 負荷を軽減
- **非同期サポート** — `WaitForSecondsAsync`、`WaitForNextFrameAsync`、`WaitForFramesAsync`（`CancellationToken` 対応）
- **クエリ API** — 残り時間・経過時間・残り回数の確認
- **完了コールバック** — タイマーの自然終了または削除時に `onComplete` を発火
- **IL2CPP 対応** — コードストリッピング防止ヘルパーで AOT ビルドでも型を保持

## インストール

**1. Scoped Registry（推奨）**

Unity プロジェクトの `Packages/manifest.json` を編集し、`scopedRegistries` セクションを追加：

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

`scopes` は、どのパッケージをこのレジストリから解決するかを制御します。`com.gameframex` で始まるパッケージのみがこのレジストリから取得されます。

**2. Git URL**

Unity で **Window → Package Manager → Add package from git URL** を開き、以下を入力：

```
https://github.com/GameFrameX/com.gameframex.unity.timer.git
```

**3. 手動**

このリポジトリをクローンまたはダウンロードして、Unity プロジェクトの `Packages/` ディレクトリに配置。

## 使用方法

すべての例は `TimerComponent`（Unity `MonoBehaviour` ラッパー）を使用します。GameFrameX コンポーネントシステムから取得：

```csharp
using GameFrameX.Timer.Runtime;

var timer = GameEntry.GetComponent<TimerComponent>();
```

### 繰り返しタイマー

指定間隔（ミリ秒）で発火。`repeat` で発火回数を指定、`0` で無限繰り返し。

```csharp
// 1秒ごとに発火、5回繰り返し
int id = timer.Add(1000f, 5, (param) =>
{
    Debug.Log("Tick!");
});
```

### ワンショットタイマー

指定間隔後に1回だけ発火し、自動的に削除されます。

```csharp
timer.AddOnce(3000f, (param) =>
{
    Debug.Log("3秒経過");
});
```

### フレーム毎コールバック

毎フレーム発火。

```csharp
timer.AddUpdate((param) =>
{
    // Update 毎に呼び出し
});
```

### 一時停止と再開

```csharp
timer.Pause(id);

if (timer.IsPaused(id))
{
    timer.Resume(id);
}
```

### タグによる一括操作

```csharp
// 作成時にタグを割り当て
timer.Add(1000f, 0, callback, tag: "enemy-spawn");

// 同じタグのタイマーを一括操作
timer.PauseByTag("enemy-spawn");
timer.ResumeByTag("enemy-spawn");
timer.RemoveByTag("enemy-spawn");

// 指定タグのタイマーが存在するか確認
bool hasTag = timer.HasTag("enemy-spawn");
```

### タイマー状態の確認

```csharp
float remaining = timer.GetRemaining(id);    // 次回発火までの秒数、未検出時は -1
float elapsed   = timer.GetElapsed(id);      // 前回発火からの秒数、未検出時は -1
int   repeats   = timer.GetRepeatLeft(id);   // 残り発火回数、未検出時は -1、0 = 無限
```

### 完了コールバック

```csharp
timer.Add(1000f, 3, callback, onComplete: () =>
{
    Debug.Log("タイマー完了");
});
```

### タイムスケール

```csharp
// Time.timeScale の影響を受ける（スローモーションやポーズメニューなどに適用）
timer.Add(1000f, 0, callback, timeScale: TimerTimeScale.Scaled);
```

### 削除と存在確認

```csharp
// コールバック参照で操作
timer.Remove(callback);
bool exists = timer.Exists(callback);

// タイマー ID で操作
timer.Remove(id);
bool exists = timer.Exists(id);
```

### 非同期待機

```csharp
// 2秒待機
await timer.WaitForSecondsAsync(2f);

// キャンセルトークン付きで待機
var cts = new CancellationTokenSource();
cts.CancelAfter(5000);
await timer.WaitForSecondsAsync(10f, cts.Token);

// 1フレーム待機
await timer.WaitForNextFrameAsync();

// Nフレーム待機
await timer.WaitForFramesAsync(3);
```

### 例外処理

`TimerManager.CatchCallbackExceptions` を `true` に設定すると、タイマーコールバック内の例外をキャッチし警告としてログ出力します（伝播させません）。

```csharp
TimerManager.CatchCallbackExceptions = true;
```

## 動作環境

- Unity 2019.4 以上
- [com.gameframex.unity](https://github.com/GameFrameX/com.gameframex.unity) 1.1.1+

## ドキュメント

- [公式ドキュメント](https://gameframex.doc.alianblank.com)

## コミュニティ

- QQグループ: [参加](https://qm.qq.com/q/3dIpogITg)

## 変更履歴

バージョン履歴は [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases) をご覧ください。

## ライセンス

[ライセンス](LICENSE.md)
