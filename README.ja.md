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
  インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">ドキュメント</a> ·
  <a href="#クイックスタート">クイックスタート</a> ·
  <a href="https://qm.qq.com/q/3dIpogITg">QQグループ</a> ·
  言語: <a href="README.md">English</a> ·
  <a href="README.zh-CN.md">简体中文</a> ·
  <a href="README.zh-TW.md">繁體中文</a> ·
  **日本語** ·
  <a href="README.ko.md">한국어</a>
</p>

---

## プロジェクト概要

GameFrameX.Timer は GameFrameX フレームワークのタイマーコンポーネントです。Unity プロジェクトでタイマータスクの管理と処理を行う機能を提供し、タイマーの使用をよりシンプルで効率的にします。

## クイックスタート

### インストール（いずれかを選択）

1. `manifest.json` の `dependencies` セクションに以下を追加：
   ```json
   {"com.gameframex.unity.timer": "https://github.com/GameFrameX/com.gameframex.unity.timer.git"}
   ```

2. Unity の Package Manager で `Git URL` を使用してパッケージを追加：https://github.com/GameFrameX/com.gameframex.unity.timer.git

3. リポジトリをダウンロードして Unity プロジェクトの `Packages` ディレクトリに配置。自動的にロードされます。

### 使用例

```csharp
// タイマーコンポーネントを取得
var timerComponent = GameEntry.GetComponent<TimerComponent>();

// 繰り返しタスクを追加：1000ミリ秒ごとに実行、5回繰り返し
timerComponent.Add(1000, 5, MyMethod);

// 一度だけのタスクを追加：5000ミリ秒後に実行
timerComponent.AddOnce(5000, MyMethod);

// フレームごとの更新タスクを追加
timerComponent.AddUpdate(MyMethod);

// タスクが存在するか確認
bool exists = timerComponent.Exists(MyMethod);

// タスクを削除
timerComponent.Remove(MyMethod);
```

## ドキュメントとリソース

- [公式ドキュメント](https://gameframex.doc.alianblank.com)

## コミュニティとサポート

- QQグループ: [参加](https://qm.qq.com/q/3dIpogITg)

## 変更履歴

変更履歴は [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases) をご覧ください。

## ライセンス

このプロジェクトは MIT ライセンスの下で公開されています - 詳細は [LICENSE](https://github.com/GameFrameX/com.gameframex.unity.timer/blob/main/LICENSE) ファイルをご覧ください。
