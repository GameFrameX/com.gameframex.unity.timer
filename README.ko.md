<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX Timer

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.timer)](https://github.com/GameFrameX/com.gameframex.unity.timer/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.timer)](https://github.com/GameFrameX/com.gameframex.unity.timer/releases)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현

<br />

[문서](https://gameframex.doc.alianblank.com) · [빠른 시작](#quick-start) · QQ 그룹: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | **한국어**

</div>
## 기능

- **3가지 타이머 모드** — 반복(`Add`), 원샷(`AddOnce`), 프레임별(`AddUpdate`)
- **일시정지 / 재개** — 타이머 ID로 개별 조작 또는 태그로 일괄 조작
- **태그 그룹화** — 문자열 태그를 할당하여 일괄 일시정지, 재개, 제거
- **이중 타임 스케일** — 각 타이머에서 실제 시간(`Unscaled`) 또는 `Time.timeScale` 영향(`Scaled`) 선택
- **스레드 안전** — lock 기반 업데이트 루프, 콜백은 락 외부에서 실행하여 데드락 방지
- **오브젝트 풀** — `TimerItem` 인스턴스를 풀링하여 재사용, GC 부하 최소화
- **비동기 대기** — `WaitForSecondsAsync`, `WaitForNextFrameAsync`, `WaitForFramesAsync` (`CancellationToken` 지원)
- **조회 API** — 남은 시간, 경과 시간, 남은 반복 횟수 확인
- **완료 콜백** — 타이머가 자연 종료되거나 제거될 때 `onComplete` 발생
- **IL2CPP 안전** — 크로핑 헬퍼로 AOT 빌드에서 타입 스트리핑 방지

## 설치

**1. Scoped Registry (권장)**

Unity 프로젝트의 `Packages/manifest.json`을 편집하여 `scopedRegistries` 섹션을 추가:

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

`scopes`는 이 레지스트리를 통해 어떤 패키지를 해석할지 제어합니다. `com.gameframex`로 시작하는 패키지만 이 레지스트리에서 가져옵니다.

**2. Git URL**

Unity에서 **Window → Package Manager → Add package from git URL**을 열고 다음을 입력:

```
https://github.com/GameFrameX/com.gameframex.unity.timer.git
```

**3. 수동**

이 리포지토리를 클론하거나 다운로드하여 Unity 프로젝트의 `Packages/` 디렉토리에 배치.

## 사용법

모든 예제는 `TimerComponent`(Unity `MonoBehaviour` 래퍼)를 사용합니다. GameFrameX 컴포넌트 시스템에서 가져오기:

```csharp
using GameFrameX.Timer.Runtime;

var timer = GameEntry.GetComponent<TimerComponent>();
```

### 반복 타이머

지정된 간격(밀리초)으로 발생합니다. `repeat`로 발생 횟수를 제어, `0`은 무한 반복.

```csharp
// 1초마다 발생, 총 5회
int id = timer.Add(1000f, 5, (param) =>
{
    Debug.Log("Tick!");
});
```

### 원샷 타이머

간격 도달 후 1회 발생하고 자동 제거됩니다.

```csharp
timer.AddOnce(3000f, (param) =>
{
    Debug.Log("3초 경과");
});
```

### 프레임별 콜백

매 프레임 발생.

```csharp
timer.AddUpdate((param) =>
{
    // 매 Update마다 호출
});
```

### 일시정지와 재개

```csharp
timer.Pause(id);

if (timer.IsPaused(id))
{
    timer.Resume(id);
}
```

### 태그 일괄 작업

```csharp
// 생성 시 태그 할당
timer.Add(1000f, 0, callback, tag: "enemy-spawn");

// 같은 태그의 타이머를 일괄 작업
timer.PauseByTag("enemy-spawn");
timer.ResumeByTag("enemy-spawn");
timer.RemoveByTag("enemy-spawn");

// 지정 태그의 타이머가 존재하는지 확인
bool hasTag = timer.HasTag("enemy-spawn");
```

### 타이머 상태 조회

```csharp
float remaining = timer.GetRemaining(id);    // 다음 발생까지 남은 초, 미발견 시 -1
float elapsed   = timer.GetElapsed(id);      // 이전 발생 이후 경과 초, 미발견 시 -1
int   repeats   = timer.GetRepeatLeft(id);   // 남은 발생 횟수, 미발견 시 -1, 0 = 무한
```

### 완료 콜백

```csharp
timer.Add(1000f, 3, callback, onComplete: () =>
{
    Debug.Log("타이머 완료");
});
```

### 타임 스케일

```csharp
// Time.timeScale의 영향을 받음 (슬로우 모션, 일시정지 메뉴 등에 적용)
timer.Add(1000f, 0, callback, timeScale: TimerTimeScale.Scaled);
```

### 제거 및 존재 확인

```csharp
// 콜백 참조로 조작
timer.Remove(callback);
bool exists = timer.Exists(callback);

// 타이머 ID로 조작
timer.Remove(id);
bool exists = timer.Exists(id);
```

### 비동기 대기

```csharp
// 2초 대기
await timer.WaitForSecondsAsync(2f);

// 취소 토큰과 함께 대기
var cts = new CancellationTokenSource();
cts.CancelAfter(5000);
await timer.WaitForSecondsAsync(10f, cts.Token);

// 1프레임 대기
await timer.WaitForNextFrameAsync();

// N프레임 대기
await timer.WaitForFramesAsync(3);
```

### 예외 처리

`TimerManager.CatchCallbackExceptions`를 `true`로 설정하면 타이머 콜백 내의 예외를 포착하여 경고로 로그 출력합니다 (전파하지 않음).

```csharp
TimerManager.CatchCallbackExceptions = true;
```

## 요구 사항

- Unity 2019.4 이상
- [com.gameframex.unity](https://github.com/GameFrameX/com.gameframex.unity) 1.1.1+

## 문서

- [공식 문서](https://gameframex.doc.alianblank.com)

## 커뮤니티

- QQ 그룹: [가입](https://qm.qq.com/q/3dIpogITg)

## 변경 로그

버전 기록은 [Releases](https://github.com/GameFrameX/com.gameframex.unity.timer/releases)에서 확인하세요.

## 라이선스

[라이선스](LICENSE.md)
