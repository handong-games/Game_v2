# VContainer Migration Plan

## Current Status Note

This document is the initial broad migration baseline.

The active source of truth has moved to:

```text
docs/vcontainer-adoption-doc-index.md
docs/title-scene-vcontainer-implementation-phases.md
docs/title-scene-vcontainer-lifecycle-design.md
docs/scene-lifetimescope-adoption-decision.md
docs/view-manager-redesign-plan.md
docs/vcontainer-registration-map.md
```

Some original sections below describe the project before the VContainer package, scope skeletons, root adapters, TitleScene support classes, and TitleScene resource-owner application were added. Keep this document as background context, not as the current implementation sequence.

Superseded decision:

```text
Earlier sections prefer a SceneManagerEx code-first child scope.
That is no longer the target direction.

Current target:
Unity Scene + VContainer LifetimeScope.
SceneManagerEx remains only as a temporary bridge until BaseScene responsibilities are migrated.
```

## 문서 목적

이 문서는 현재 커스텀 DI 시스템을 VContainer로 전환하기 위한 설계 초안이다.

이번 문서는 코드나 에셋을 수정하지 않는다. 먼저 현재 DI의 책임, 수명, 호출 지점을 정리하고, VContainer 도입 시 어떤 단위로 나눠 옮길지 결정하기 위한 기준을 남긴다.

## 범위

포함한다:

- 현재 커스텀 DI 구조 요약
- VContainer 수명 매핑 초안
- Root, Scene, Adventure Session 스코프 설계
- 단계별 마이그레이션 계획
- 리스크와 확인 필요 사항

제외한다:

- 실제 VContainer 패키지 추가
- `LifetimeScope` 코드 작성
- 기존 `[Dependency]`, `[Inject]` 제거
- 매니저 싱글톤 구조 전면 개편
- Unity 씬 에셋 수정

## 현재 구조 요약

현재 DI 시스템은 `DependencyManager`가 담당한다.

```text
[Dependency] 타입 선언
→ DependencyRegistryGenerator가 DependencyRegistry.g.cs 생성
→ DependencyManager가 registry를 읽음
→ Resolve<T>() 또는 Instantiate<T>() 호출
→ 필드의 [Inject] 대상에 의존성 주입
```

현재 DI의 주요 특징:

- 생성은 `Activator.CreateInstance` 기반이다.
- 주입은 필드 `[Inject]` 기반이다.
- 생성자 주입은 사용하지 않는다.
- `[Dependency]`에 scene name이 없으면 global dependency다.
- `[Dependency(nameof(TitleScene))]` 같은 타입은 scene dependency다.
- scene dependency는 Unity scene unload 시 dispose된다.
- `DependencyManager.Instance.Instantiate<T>()`는 `new T()` 이후 field injection을 수행한다.

## 주요 파일

| 파일 | 역할 |
| --- | --- |
| `Assets/@Scripts/Core/Manager/Dependency/DependencyManager.cs` | 커스텀 DI 컨테이너 |
| `Assets/@Scripts/Core/Manager/Dependency/DependencyAttribute.cs` | DI 등록 대상 표시 |
| `Assets/@Scripts/Core/Manager/Dependency/InjectAttribute.cs` | 필드 주입 대상 표시 |
| `Assets/@Scripts/Core/Manager/Dependency/Generated/DependencyRegistry.g.cs` | 생성된 DI 타입 목록 |
| `Assets/@Scripts/Core/Manager/Dependency/Editor/DependencyRegistryGenerator.cs` | `[Dependency]` 스캔 및 registry 생성 |
| `Assets/@Scripts/Core/GameBootStrap.cs` | 매니저 생성과 전역 초기화 |
| `Assets/@Scripts/Core/Manager/Generated/ManagerRegistry.g.cs` | 매니저 생성 순서 |
| `Assets/@Scripts/Core/Manager/Scene/SceneManagerEx.cs` | 순수 C# scene 객체 생성과 Unity scene 전환 |
| `Assets/@Scripts/Core/Manager/View/ViewManager.cs` | view 생성 이후 UXML bind |

## 현재 등록 타입

현재 `DependencyRegistry.g.cs` 기준 등록 타입은 다음과 같다.

### Global dependency

```text
AdventureService
CardDeckService
CardBoardService
CardService
CharacterService
CombatService
MonsterService
PlayerService
AudioSettingsState
GraphicSettingsState
LocalizationSettingsState
ProgressState
```

### TitleScene dependency

```text
CharacterSelectController
TitleViewController
```

### AdventureScene dependency

```text
AdventureController
```

## VContainer 도입 목표

목표:

- 수명 관리를 코드 생성 registry가 아니라 명시적인 `LifetimeScope`로 옮긴다.
- 필드 주입 중심 구조를 점진적으로 생성자 주입 중심으로 바꾼다.
- `DependencyManager.Instance.Resolve<T>()` 호출을 점진적으로 제거한다.
- view 생성 지점을 `IViewFactory` 같은 명시적 팩토리로 모은다.
- scene, adventure session, global 수명 경계를 분명히 한다.
- 패키지 영역과 게임 컨텐츠 영역의 책임을 섞지 않는다.

비목표:

- 첫 단계에서 모든 `BaseManager<T>.Instance`를 제거하지 않는다.
- 첫 단계에서 모든 서비스를 interface로 분리하지 않는다.
- 첫 단계에서 Unity scene 구조를 `LifetimeScope` prefab 중심으로 전면 변경하지 않는다.

## 패키지 도입

VContainer는 Unity Package Manager git dependency로 추가할 수 있다.

```json
"jp.hadashikick.vcontainer": "https://github.com/hadashiA/VContainer.git?path=VContainer/Assets/VContainer#1.18.0"
```

버전은 실제 도입 시점에 다시 확인한다.

## 패키지 / 컨텐츠 영역 구분

현재 프로젝트에는 `Packages/com.gameplay.gas`, `Packages/com.uitoolkit.timeline` 같은 재사용 패키지와 `Assets/@Scripts` 중심의 게임 컨텐츠 코드가 함께 있다.

VContainer 도입 시 다음 경계를 지킨다.

### 패키지 영역

```text
Packages/
```

패키지 영역에는 재사용 가능하고 컨텐츠에 의존하지 않는 기반 코드만 둔다.

패키지 영역에 두면 안 되는 것:

- 특정 scene 이름에 의존하는 registration
- `AdventureService`, `CardService`, `CombatService` 같은 게임 도메인 concrete type registration
- Addressables label, UXML 이름, 프로젝트 리소스 asset 이름에 의존하는 composition root
- `Assets/@Scripts/Domains`에 의존하는 코드

### 컨텐츠 영역

```text
Assets/@Scripts/
Assets/@Resources/
Assets/AddressableAssetsData/
```

컨텐츠 영역에는 현재 게임 프로젝트에 종속된 VContainer composition root, scope skeleton, registration map, scene/session 수명 설계를 둔다.

1차 도입 원칙:

```text
VContainer 패키지 의존성은 Packages/manifest.json에 추가한다.
게임 전용 Root/Scene/AdventureSession scope 뼈대는 Assets/@Scripts 아래에 둔다.
com.gameplay.gas와 com.uitoolkit.timeline에는 게임 composition root를 넣지 않는다.
```

## 스코프 설계 초안

### RootLifetimeScope

앱 시작부터 종료까지 살아 있는 의존성을 등록한다.

초기 후보:

```text
ProgressState
AudioSettingsState
GraphicSettingsState
LocalizationSettingsState
CharacterService
MonsterService
```

전환 초기에는 아래 타입도 Root에 둘 수 있다.

```text
AdventureService
CardDeckService
CardService
CardBoardService
PlayerService
CombatService
```

하지만 이 타입들은 내부 상태를 가지므로 장기적으로는 `AdventureSessionScope`로 옮기는 것이 더 자연스럽다.

### SceneLifetimeScope

Unity scene 또는 현재 프로젝트의 `BaseScene` 단위와 대응되는 스코프다.

`TitleScene` 후보:

```text
TitleViewController
CharacterSelectController
TitleView
CharacterSelectView
SettingsView
```

`AdventureScene` 후보:

```text
AdventureController
AdventureView
```

현재 프로젝트의 scene 클래스는 MonoBehaviour가 아니라 순수 C# 객체다. 따라서 이 초기 문서 작성 시점에는 Unity scene에 `LifetimeScope` 컴포넌트를 배치하기보다, `SceneManagerEx`가 scene 전환 시 child scope를 만들고 dispose하는 code-first 방식이 적합하다고 판단했다.

이 판단은 현재 채택된 방향이 아니다. 현재 목표는 Unity Scene + VContainer `LifetimeScope`이며, 자세한 결정은 `docs/scene-lifetimescope-adoption-decision.md`를 따른다.

### AdventureSessionScope

어드벤처 1회 플레이 동안 유지되는 상태ful 서비스를 묶는 스코프다.

후보:

```text
AdventureService
CardDeckService
CardService
CardBoardService
PlayerService
CombatService
AdventureController
AdventureView
```

현재는 이 서비스들이 global dependency로 등록되어 있다. 하지만 실제 책임은 앱 전체보다 run/session에 가깝다.

예상 전환 방향:

```text
캐릭터 선택 완료
→ AdventureSessionScope 생성
→ 선택 캐릭터와 seed로 세션 초기화
→ AdventureScene 로드
→ AdventureScene unload 또는 run 종료 시 scope dispose
```

## 수명 매핑 초안

| 현재 타입 | 현재 수명 | VContainer 1차 | VContainer 최종 후보 |
| --- | --- | --- | --- |
| `ProgressState` | Global | Singleton | Singleton |
| `AudioSettingsState` | Global | Singleton | Singleton |
| `GraphicSettingsState` | Global | Singleton | Singleton |
| `LocalizationSettingsState` | Global | Singleton | Singleton |
| `CharacterService` | Global | Singleton | Singleton |
| `MonsterService` | Global | Singleton | Singleton |
| `AdventureService` | Global | Singleton | AdventureSession scoped |
| `CardDeckService` | Global | Singleton | AdventureSession scoped |
| `CardService` | Global | Singleton | AdventureSession scoped |
| `CardBoardService` | Global | Singleton | AdventureSession scoped |
| `PlayerService` | Global | Singleton | AdventureSession scoped |
| `CombatService` | Global | Singleton | AdventureSession scoped |
| `TitleViewController` | TitleScene | Scoped | TitleScene scoped |
| `CharacterSelectController` | TitleScene | Scoped | TitleScene scoped |
| `AdventureController` | AdventureScene | Scoped | AdventureSession or AdventureScene scoped |

## View 생성 전략

현재 view 생성은 다음 방식이다.

```csharp
TitleView titleView = DependencyManager.Instance.Instantiate<TitleView>();
ViewManager.Instance.Push(titleView);
```

VContainer 전환 시 직접 `Resolve<TView>()`를 퍼뜨리지 않고 view factory를 둔다.

```csharp
public interface IViewFactory
{
    T Create<T>() where T : BaseView;
}
```

초기 구현은 내부에서 기존 `DependencyManager`를 사용할 수 있다.

```text
IViewFactory
→ 기존 DependencyManager.Instantiate<T>()
```

이후 VContainer로 전환한다.

```text
IViewFactory
→ IObjectResolver.Resolve<T>()
```

이렇게 하면 `TitleViewController`, `TitleScene`, `AdventureScene`에 흩어진 `DependencyManager.Instance.Instantiate<T>()` 호출을 먼저 제거할 수 있다.

## Manager 연동 전략

현재 `SaveManager`, `AudioManager`, `GraphicManager`, `LocaleManager`는 저장 상태를 `DependencyManager.Instance.Resolve<T>()`로 가져온다.

첫 단계에서는 매니저 싱글톤 구조를 유지한다.

권장 순서:

```text
1. 저장 상태 객체를 VContainer에도 등록
2. 기존 DependencyManager와 VContainer가 같은 state instance를 바라보도록 bridge 구성
3. SaveManager.Register<TState, TSave>() 내부 Resolve 경로를 교체
4. AudioManager, GraphicManager, LocaleManager의 state 접근을 교체
5. ManagerRegistry에서 DependencyManager 제거 가능 여부 검토
```

주의:

- `ManagerRegistry` 생성 순서는 현재 게임 부트스트랩의 핵심이다.
- VContainer 도입 첫 PR에서 `ManagerRegistry`를 제거하면 영향 범위가 커진다.
- `DBManager`, `ViewManager`, `SceneManagerEx`, `GameplayMessageManager`는 당분간 기존 싱글톤 접근을 유지한다.

## 단계별 전환 계획

### Phase 0. 설계 고정

목표:

- 현재 DI 타입 목록과 수명 매핑을 문서화한다.
- VContainer package version을 확인한다.
- 전환 중 공존할 bridge 범위를 정한다.

완료 기준:

- 이 문서가 최신 registry와 일치한다.
- 첫 구현 PR의 범위가 정해진다.

### Phase 1. 패키지와 RootLifetimeScope 추가

목표:

- VContainer 패키지를 추가한다.
- `RootLifetimeScope`를 추가한다.
- 저장 상태와 stateless service를 등록한다.
- 기존 커스텀 DI는 유지한다.

완료 기준:

- Unity compile이 통과한다.
- 기존 scene 흐름이 그대로 동작한다.
- 기존 `DependencyManager` 호출은 아직 제거하지 않아도 된다.

### Phase 2. ViewFactory 도입

목표:

- `DependencyManager.Instance.Instantiate<TView>()` 직접 호출을 `IViewFactory`로 감싼다.
- `TitleScene`, `AdventureScene`, `TitleViewController`의 view 생성 경로를 정리한다.

완료 기준:

- view 생성 호출 지점이 factory로 모인다.
- 내부 구현은 아직 기존 DI여도 된다.

### Phase 3. Scene scope 도입

목표:

- `SceneManagerEx`가 scene 전환 시 VContainer child scope를 만들고 dispose한다.
- `TitleViewController`, `CharacterSelectController`, `AdventureController`를 scene scope에 등록한다.
- scene scoped controller는 VContainer에서 resolve한다.

완료 기준:

- `DependencyManager.Resolve<AdventureController>()` 호출이 제거된다.
- scene unload 시 scoped disposable이 dispose된다.

### Phase 4. 필드 주입에서 생성자 주입으로 전환

목표:

- controller부터 생성자 주입으로 바꾼다.
- `AdventureController`, `CharacterSelectController`, `TitleViewController` 순서로 진행한다.
- view는 필요하면 당분간 method injection 또는 property injection을 유지한다.

완료 기준:

- controller의 `[Inject] private` 필드가 제거된다.
- controller 생성자가 의존성을 명시한다.

### Phase 5. AdventureSessionScope 분리

목표:

- run/session 상태를 가진 서비스를 global singleton에서 분리한다.
- 캐릭터 선택 시 session scope를 생성한다.
- run 종료 시 session scope를 dispose한다.

완료 기준:

- 새 어드벤처 시작 시 이전 카드, 전투, 덱 상태가 scope dispose로 정리된다.
- 명시적 `Clear()` 호출 의존이 줄어든다.

### Phase 6. 커스텀 DI 제거

목표:

- `[Dependency]`, `[Inject]`, `DependencyManager`, `DependencyRegistryGenerator`를 제거한다.
- generated registry 파일을 제거한다.
- 관련 using을 정리한다.

완료 기준:

- `Game.Core.Managers.Dependency` 네임스페이스 참조가 없다.
- Unity compile이 통과한다.
- 주요 scene smoke test가 통과한다.

## 첫 구현 PR 권장 범위

첫 PR은 작게 유지한다.

포함:

- VContainer package 추가
- `RootLifetimeScope` 추가
- `IViewFactory` 인터페이스 추가
- 기존 DI 기반 `LegacyViewFactory` 추가
- view 생성 호출 일부를 `IViewFactory`로 이동

제외:

- `DependencyManager` 제거
- scene scope 전환
- 모든 `[Inject]` 제거
- manager singleton 제거
- adventure session scope 분리

이 범위가 좋은 이유:

- 동작 변화가 작다.
- compile 실패 지점을 좁게 유지할 수 있다.
- 이후 VContainer resolver로 factory 내부만 교체하기 쉽다.

## 확인 필요 사항

- VContainer package version을 어떤 버전으로 고정할지 결정해야 한다.
- Unity scene에 `LifetimeScope` MonoBehaviour를 둘지, `SceneManagerEx` code-first child scope로 갈지 결정해야 한다.
- `AdventureService`, `CardDeckService`, `CardService`, `CardBoardService`, `PlayerService`, `CombatService`를 언제 `AdventureSessionScope`로 옮길지 결정해야 한다.
- `SaveManager`가 저장 상태를 직접 resolve하는 방식을 유지할지, 저장 상태 registry 자체를 VContainer registration으로 대체할지 결정해야 한다.
- 테스트 환경에서 VContainer scope를 쉽게 만들 수 있도록 assembly definition 참조를 정리해야 할 수 있다.

## 리스크

### 이중 컨테이너 기간

전환 중에는 기존 `DependencyManager`와 VContainer가 동시에 존재한다. 같은 타입을 양쪽에서 각각 생성하면 상태 불일치가 생길 수 있다.

대응:

- 상태ful 타입은 한쪽 컨테이너만 소유하게 한다.
- bridge를 둘 경우 instance source를 명확히 정한다.

### Dispose 타이밍 변화

현재 scene dependency는 Unity scene unload 이벤트에 맞춰 dispose된다. VContainer child scope로 옮기면 dispose 타이밍이 `SceneManagerEx`의 scope dispose 호출에 따라 달라진다.

대응:

- scene load/unload smoke test를 둔다.
- `IDisposable` controller의 event unsubscribe가 호출되는지 확인한다.

### 생성자 주입 전환 비용

필드 주입에서 생성자 주입으로 바꾸면 생성자 파라미터가 늘어나고 테스트 생성 코드도 바뀐다.

대응:

- controller부터 전환한다.
- service는 실제 의존성이 생길 때만 생성자 주입으로 바꾼다.

### 수명 재설계에 따른 상태 초기화 변화

현재는 global service에 `Clear()`를 호출해 session 상태를 정리한다. session scope로 옮기면 dispose와 새 scope 생성이 초기화 책임을 가져간다.

대응:

- Phase 5 전까지는 기존 `Clear()` 호출을 유지한다.
- session scope 도입 PR에서 초기화 책임을 별도로 검증한다.

## 임시 공존 규칙

VContainer 전환 중에는 다음 규칙을 따른다.

```text
1. 새 코드에서는 DependencyManager.Instance.Resolve<T>()를 추가하지 않는다.
2. 새 view 생성 코드는 IViewFactory를 통한다.
3. 새 controller는 가능하면 생성자 주입으로 작성한다.
4. 상태를 가진 service의 lifetime 변경은 별도 PR로 분리한다.
5. 기존 manager singleton 제거는 VContainer 전환 마지막 단계로 미룬다.
```

## 최종 목표 구조

```text
GameBootstrap
→ RootLifetimeScope
  → app level service/state
  → SceneManagerEx
    → TitleScene child scope
      → TitleViewController
      → CharacterSelectController
      → title/character select views
    → AdventureSession child scope
      → run state services
      → AdventureScene child scope
        → AdventureController
        → AdventureView
```

최종적으로는 dependency ownership이 다음처럼 읽혀야 한다.

```text
앱 설정과 정적 데이터 조회는 Root
화면 흐름과 scene controller는 Scene
전투, 카드, 플레이어, 덱 상태는 AdventureSession
UI 인스턴스는 ViewFactory
```
