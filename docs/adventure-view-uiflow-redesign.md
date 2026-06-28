# AdventureView UIFlow Redesign

## Current Implementation-Ready Direction

이 문서의 최신 방향은 아래 흐름을 기준으로 한다.

```text
Controller 초기 설정
-> Screen 초기 설정
-> IntroAnimation 요청
```

현재 Intro 범위:

```text
지역 배너
초기 보드 카드 등장
```

아직 포함하지 않는다:

```text
턴 배너
파우치
EndTurnWidget
CoinStatusWidget
스킬 슬롯 등장
```

Intro 다음 설계 범위:

```text
PlayerTurn 시작 UI 흐름
```

공식 Unity UI Toolkit 기준:

```text
USS가 visual state, transition-duration, transition-delay, easing을 소유한다.
C#은 state class 변경과 실행 순서만 소유한다.
완료가 필요한 경우 TransitionEndEvent를 사용한다.
```

참고:

- https://docs.unity3d.com/6000.5/Documentation/Manual/UIE-Transitions.html
- https://docs.unity3d.com/6000.5/Documentation/Manual/UIE-Transition-Events.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-Panel-Events.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/UIElements.TransitionRunEvent.html
- https://docs.unity3d.com/6000.0/Documentation/ScriptReference/UIElements.TransitionStartEvent.html

### Startup Flow

```text
AdventureSceneEntryPoint.Start
-> AdventureStartFlow.StartAdventure()
   Runtime 상태 초기화
-> AdventureSceneNavigator.ShowAdventure()
   Screen 생성/표시/Event 구독 준비 완료
-> AdventureSceneController.StartAdventure()
   Stage 시작
   AdventureInitialPresentationViewModel 생성
   InitialPresentationPrepared 이벤트 발행
```

### Screen Flow

```text
AdventureView.OnInitialPresentationPrepared(viewModel)
-> InitializeScreen(viewModel)
   AdventureIntroUIFlow.Prepare(viewModel)
-> RequestIntroAnimation()
   AdventureIntroUIFlow.Play()
```

`AdventureView`는 초기 데이터가 도착해도 바로 세부 애니메이션을 실행하지 않는다.

```text
초기 설정
애니메이션 요청
```

을 분리한다.

### Screen Ready Gate

Intro 시작 조건은 하나가 아니다.

```text
Screen이 Panel에 attach됨
InitialPresentationPrepared 데이터가 도착함
```

두 조건이 모두 만족되어야 Intro를 시작한다.

이유:

```text
Controller가 데이터를 먼저 보낼 수 있다.
Screen이 먼저 attach될 수도 있다.
둘 중 하나의 순서에 의존하면 Intro가 누락되거나 너무 이르게 실행될 수 있다.
```

Unity UI Toolkit 기준:

```text
AttachToPanelEvent는 VisualElement가 Panel에 attach된 뒤 발생한다.
Panel에 attach되어야 runtime UI가 render/event 대상이 된다.
```

따라서 `AdventureView`는 다음 상태를 가진다.

```csharp
private bool _screenAttached;
private bool _introStarted;
private AdventureInitialPresentationViewModel _pendingInitialPresentation;
```

예상 형태:

```csharp
protected override void OnAttachedToPanel(AttachToPanelEvent evt)
{
    base.OnAttachedToPanel(evt);

    _screenAttached = true;
    TryStartIntro();
}

public void OnInitialPresentationPrepared(
    AdventureInitialPresentationViewModel viewModel)
{
    _pendingInitialPresentation = viewModel;
    TryStartIntro();
}

private void TryStartIntro()
{
    if (_introStarted)
        return;

    if (!_screenAttached)
        return;

    if (_pendingInitialPresentation == null)
        return;

    _introStarted = true;

    InitializeScreen(_pendingInitialPresentation);
    RequestIntroAnimation();
}
```

규칙:

```text
OnAttachedToPanel은 화면이 실제 Panel에 올라온 사실만 기록한다.
OnInitialPresentationPrepared는 초기 표시 데이터를 보관한다.
TryStartIntro만 Intro 시작 여부를 판단한다.
```

### AdventureIntroUIFlow

`AdventureIntroUIFlow`는 Adventure intro presentation sequence를 소유한다.

허용 책임:

- 지역 배너 표시 요청
- 보드 카드 intro 준비 요청
- 보드 카드 intro 완료 대기
- intro 완료 후 `AdventureView` 또는 Controller 흐름으로 반환

금지 책임:

- Runtime 상태 변경
- Controller에서 데이터 pull
- 카드 Widget 생성 세부 구현
- Layout Add 직접 수행

예상 형태:

```csharp
public sealed class AdventureIntroUIFlow
{
    public void Prepare(AdventureInitialPresentationViewModel viewModel);
    public Awaitable Play();
}
```

### AdventureBoardUIFlow Intro Contract

초기 카드 intro는 세 단계로 분리한다.

```text
PrepareInitialCardsIntro(cards)
WaitInitialCardsIntroEndOnly()
StartInitialCardsIntro()
```

호출 순서:

```text
Prepare
-> Wait registration
-> NextFrame
-> Start
-> Await
```

의미:

- `PrepareInitialCardsIntro`: 카드 생성/배치, delay class 적용, intro shown class 제거, 마지막 wait target 저장
- `WaitInitialCardsIntroEndOnly`: 마지막 카드의 `TransitionEndEvent` 대기
- `StartInitialCardsIntro`: 보드 intro shown class 추가

### Animation Rules

```text
Animation은 Add를 책임지지 않는다.
Layout/BoardUIFlow가 배치한다.
Animation은 배치된 요소의 visual state 전환만 책임진다.
```

```text
단순 stagger는 USS transition-delay를 사용한다.
C# WaitForSeconds 반복으로 단순 stagger를 만들지 않는다.
```

```text
기본 완료 방식은 End-only다.
TransitionCancelEvent와 fallback timeout은 기본형에 넣지 않는다.
TransitionEndEvent 누락은 우선 USS/design bug로 본다.
```

Intro 완료 후 Controller 메서드는 다음 이름을 사용한다.

```text
AdventureSceneController.OnIntroCompleted()
```

`OnInitialBoardShown`은 범위가 좁은 이름이므로 새 설계에서는 피한다.

## PlayerTurn Start UI Direction

Intro 이후 전투가 시작되면 다음 문제를 해결해야 한다.

```text
플레이어 턴이 시작되었음을 보여준다.
적 Intent를 보여준다.
현재 Coin 상태를 보여준다.
SkillSlot 상태를 준비한다.
Pouch를 클릭 가능한 상태로 만든다.
EndTurnWidget은 아직 숨긴다.
```

이 흐름은 여러 개의 작은 이벤트로 나누지 않는다.

피해야 할 형태:

```text
PlayerTurnBannerRequested
IntentRevealRequested
PouchShowRequested
SkillSlotShowRequested
CoinStatusShowRequested
```

이 방식은 이벤트 순서가 곧 UI 시퀀스가 된다. 이벤트가 많아질수록 “어떤 화면 흐름이 실행되는지”가 코드에서 보이지 않는다.

새 방향:

```text
AdventureScreenEvents.PlayerTurnStarted(viewModel)
```

`PlayerTurnStarted`는 플레이어 턴 시작을 표현하기 위한 하나의 화면 요청이다.

예상 데이터:

```csharp
public sealed class AdventurePlayerTurnStartViewModel
{
    public CombatTurnViewModel Turn { get; }
    public IReadOnlyList<MonsterIntentRevealViewModel> IntentReveals { get; }
    public IReadOnlyList<AdventureSkillSlotViewModel> SkillSlots { get; }
    public CoinStatusViewModel CoinStatus { get; }
}
```

### PlayerTurn Start Flow

```text
AdventureSceneController.StartPlayerTurnPresentation()
-> AdventureTurnGameFlow.StartPlayerTurn()
   combat/runtime/intent 준비
-> AdventurePresenter.CreatePlayerTurnStartViewModel()
-> AdventureScreenEvents.PlayerTurnStarted(viewModel)
-> AdventureView.OnPlayerTurnStarted(viewModel)
-> AdventurePlayerTurnUIFlow.PlayStart(viewModel)
```

### AdventurePlayerTurnUIFlow

`AdventurePlayerTurnUIFlow`는 플레이어 턴 시작 UI 시퀀스를 조율한다.

허용 책임:

- Turn banner 재생 순서 조율
- Intent reveal 순서 조율
- CoinStatus 표시 요청
- SkillSlot 표시 요청
- Pouch 표시 요청
- EndTurnWidget 숨김 유지

금지 책임:

- Runtime coin 값 변경
- Intent 계산
- Skill 사용 가능 여부 계산
- Controller에서 데이터 pull
- Widget event 직접 구독

예상 형태:

```csharp
public sealed class AdventurePlayerTurnUIFlow
{
    public async Awaitable PlayStart(
        AdventurePlayerTurnStartViewModel viewModel)
    {
        _coinUIFlow.Apply(viewModel.CoinStatus);
        _skillUIFlow.PrepareSlots(viewModel.SkillSlots);

        await _turnUIFlow.PlayPlayerTurnBanner(viewModel.Turn);
        await _intentUIFlow.Reveal(viewModel.IntentReveals);

        await _coinUIFlow.ShowStatus();
        await _skillUIFlow.ShowSlots();
        await _pouchUIFlow.ShowClickable();
    }
}
```

### Pouch Click Boundary

최종 방향에서 `PouchWidget`은 클릭을 알릴 뿐이다.

피해야 할 형태:

```text
PouchWidget clicked
-> PouchWidget hides itself
-> PouchWidget publishes gameplay-like event
```

권장 형태:

```text
PouchWidget clicked
-> WidgetEvents.Pouch.Clicked
-> AdventureView.OnWidgetPouchClicked()
-> AdventurePlayerTurnUIFlow.PlayPouchClicked()
-> AdventureSceneController.OnPouchClicked()
```

이유:

```text
PouchWidget은 재사용 가능한 UI 부품이다.
Pouch 클릭이 어떤 화면 흐름을 의미하는지는 PlayerTurn UIFlow가 안다.
CoinFlip을 실행하는 것은 Controller/GameFlow가 안다.
```

### Coin Flip Boundary

Coin flip 결과는 gameplay cue로 돌아온다.

```text
Gameplay coin flip
-> CoinFlipCueData
-> AdventureView.HandleCoinFlipCue(data)
-> AdventureCoinUIFlow.PlayCoinFlip(data)
-> AdventurePlayerTurnUIFlow.PlayAfterCoinFlip()
```

`AdventureCoinUIFlow`는 코인 연출과 CoinStatus 표시 갱신을 담당한다.

`AdventurePlayerTurnUIFlow.PlayAfterCoinFlip()`은 코인 이후 화면 상태 전환을 담당한다.

예:

```text
SkillSlotGroup 표시 유지
EndTurnWidget 표시
Pouch 숨김 유지
```

Coin 값 자체는 UI가 만들지 않는다.

```text
CoinStatusWidget의 0/0도 UI 기본값이 아니라 gameplay state를 표시한 결과여야 한다.
```

### AdventureCoinUIFlow

`AdventureCoinUIFlow`는 코인 표현만 담당한다.

허용 책임:

- `CoinStatusWidget` 초기 값 적용
- `CoinStatusWidget` 표시
- `CoinFlipCueData` 기반 코인 이동 연출
- 코인 도착 시 `CoinStatusWidget` 카운트 증가
- `CoinChangeCueData` 기반 코인 값 변경 표시

금지 책임:

- Coin flip 실행
- Coin 결과 계산
- EndTurnWidget 표시 여부 판단
- SkillSlotGroup 표시 여부 판단
- Controller 호출

예상 형태:

```csharp
public sealed class AdventureCoinUIFlow
{
    public void Apply(CoinStatusViewModel viewModel);
    public Awaitable ShowStatus();
    public Awaitable PlayCoinFlip(CoinFlipCueData data);
    public Awaitable PlayCoinChange(CoinChangeCueData data);
}
```

`PlayCoinFlip` 예상 흐름:

```text
CoinFlipCueData
-> CoinEffectPlayer.Play
-> coin arrives at Heads/Tails target
-> CoinStatusWidget.Add(face)
-> all coin effects completed
```

### After Coin Flip

Coin flip 연출이 끝나면 턴 UI phase가 바뀐다.

```text
Before coin flip
  Pouch clickable
  EndTurn hidden
  SkillSlots prepared

After coin flip
  Pouch hidden/non-clickable
  EndTurn shown
  SkillSlots shown/usable according to ability state
```

이 전환은 `AdventureCoinUIFlow`가 아니라 `AdventurePlayerTurnUIFlow`가 담당한다.

예상 형태:

```csharp
public sealed class AdventurePlayerTurnUIFlow
{
    public async Awaitable PlayAfterCoinFlip()
    {
        await _skillUIFlow.ShowSlots();
        await _turnUIFlow.ShowEndTurn();
    }
}
```

이유:

```text
CoinUIFlow는 coin presentation을 안다.
PlayerTurnUIFlow는 player turn phase를 안다.
EndTurnWidget 표시 여부는 coin animation 문제가 아니라 turn phase 문제다.
```

### CoinChange Boundary

`CoinChangeCueData`는 coin flip 완료 흐름이 아니다.

예:

```text
Skill cost
Effect cost
Reward effect
```

따라서:

```csharp
public async void HandleCoinChangeCue(CoinChangeCueData data)
{
    await _coinUIFlow.PlayCoinChange(data);
}
```

`HandleCoinChangeCue`는 `AdventurePlayerTurnUIFlow.PlayAfterCoinFlip()`을 호출하지 않는다.

### Coin Animation Rule

Intro 카드처럼 단순히 숨김 상태에서 표시 상태로 바뀌는 애니메이션은 USS transition을 우선한다.

하지만 coin flip은 다르다.

```text
각 코인의 시작점은 Pouch 위치다.
각 코인의 도착점은 Heads/Tails target 위치다.
각 코인은 face와 순서에 따라 spread 위치가 달라진다.
```

따라서 coin flip은 C# procedural animation을 허용한다.

```text
Intro card stagger: USS transition-delay
Coin path motion: C# procedural animation
```

## Skill Targeting UI Direction

현재 문제:

```text
AdventureView가 active skill 상태를 가진다.
AdventureView가 ArrowWidget을 직접 조작한다.
AdventureView가 card hover class를 직접 바꾼다.
AdventureView가 card element -> card id mapping을 가진다.
AdventureView가 Controller.UseSkillOnTarget을 직접 호출한다.
```

이 구조는 `AdventureView`를 다시 UI god object로 만든다.

새 방향:

```text
AdventureView
  raw UI interaction을 처음 받는다.
  UIFlow 결과를 Controller 호출로 변환한다.

AdventureSkillUIFlow
  active skill 상태를 가진다.
  targeting 상태를 가진다.
  ArrowWidget 표시/갱신/숨김을 담당한다.
  card click이 선택인지 skill target 확정인지 판단한다.

AdventureBoardUIFlow
  card element -> card id lookup을 가진다.
  card hover USS class를 적용/해제한다.
  active skill 상태는 모른다.
```

### Skill State

`AdventureSkillUIFlow`는 다음 UI 상태를 가진다.

```csharp
public sealed class AdventureSkillUIState
{
    public IReadOnlyList<AdventureSkillSlotViewModel> SkillSlots { get; private set; }
    public int ActiveSkillIndex { get; private set; } = -1;
    public GameplayAbilitySpecHandle ActiveSkillHandle { get; private set; } =
        GameplayAbilitySpecHandle.Invalid;
    public ESkillTargetType ActiveTargetType { get; private set; } =
        ESkillTargetType.None;
    public uint? HoveredTargetCardId { get; private set; }
    public bool IsTargeting { get; private set; }
}
```

이 상태는 gameplay runtime이 아니다.

```text
어떤 스킬 슬롯이 UI에서 선택되었는가
현재 타겟팅 중인가
화살표를 표시해야 하는가
어떤 카드가 hover 중인가
```

를 표현하는 screen-local UI state다.

### Skill Slot Selection

```text
SkillSlotWidget selection changed
-> AdventureView.OnWidgetSkillSlotSelectionChanged
-> AdventureSkillUIFlow.OnSkillSlotSelectionChanged
-> AdventureSkillInputResult
-> AdventureView
-> Controller
```

예상 형태:

```csharp
internal void OnWidgetSkillSlotSelectionChanged(
    int selectedIndex,
    SkillSlotWidget selectedWidget)
{
    AdventureSkillInputResult result =
        _skillUIFlow.OnSkillSlotSelectionChanged(
            selectedIndex,
            selectedWidget);

    ApplySkillInputResult(result);
}
```

규칙:

```text
TargetType.None -> 즉시 UseSkill result 반환
TargetType이 있는 스킬 -> targeting mode 진입
```

`TargetType.None`을 preview-only로 해석하지 않는다.

preview-only가 필요해지면 `RequiresConfirm` 또는 `PreviewOnly` 같은 명시 데이터가 필요하다.

### Board Card Click

```text
Card pointer down
-> AdventureView
-> AdventureBoardUIFlow.TryGetCardId(element)
-> AdventureSkillUIFlow.OnBoardCardClicked(cardId)
-> AdventureCardInputResult
-> AdventureView
-> Controller
```

예상 형태:

```csharp
public AdventureCardInputResult OnBoardCardClicked(uint cardId)
{
    if (!_state.IsTargeting)
        return AdventureCardInputResult.SelectCard(cardId);

    GameplayAbilitySpecHandle handle = _state.ActiveSkillHandle;
    ClearTargeting();
    return AdventureCardInputResult.UseSkillOnTarget(handle, cardId);
}
```

이유:

```text
Board는 어떤 카드인지 안다.
SkillUIFlow는 지금 카드 클릭이 선택인지 타겟 확정인지 안다.
Controller는 실제 gameplay command를 실행한다.
```

### Board Card Hover

Hover visual은 Board가 가진다.

```text
AdventureSkillUIFlow
  hover가 의미 있는지 판단

AdventureBoardUIFlow
  hover USS class 적용/해제
```

예상 형태:

```csharp
if (_skillUIFlow.OnBoardCardPointerEnter(cardId))
    _boardUIFlow.SetTargetHover(cardId);

if (_skillUIFlow.OnBoardCardPointerLeave(cardId))
    _boardUIFlow.ClearTargetHover(cardId);
```

### Background Pointer

Targeting 중인 경우:

```text
Right click -> targeting cancel
Left click outside card and outside skill slot -> targeting cancel
Pointer move -> ArrowWidget update
```

`AdventureView`는 pointer event를 받지만, cancel rule은 `AdventureSkillUIFlow`가 가진다.

### ArrowWidget

`ArrowWidget`은 `AdventureSkillUIFlow`의 소유다.

이유:

```text
ArrowWidget은 skill targeting 중에만 의미가 있다.
origin은 선택된 skill slot에서 나온다.
target은 pointer movement를 따른다.
```

따라서:

```text
AdventureSkillUIFlow.ShowArrow(origin)
AdventureSkillUIFlow.UpdateArrow(pointerPosition)
AdventureSkillUIFlow.HideArrow()
```

### Unity UI Toolkit 기준

참고:

- https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Pointer-Events.html
- https://docs.unity3d.com/6000.0/Documentation/Manual/UIE-Events-Handling.html

규칙:

```text
Pointer event는 raw interaction이다.
VisualElement callback 안에 gameplay 판단을 숨기지 않는다.
화면 입력이 소비된 경우에만 screen/input boundary에서 propagation을 멈춘다.
```

## EnemyTurn UI Direction

현재 문제:

```text
EndTurnWidget clicked
-> AdventureView가 coin/end-turn UI를 직접 정리
-> AdventureView가 Controller.OnEndTurnClicked 호출
-> AdventureView가 EnemyTurn banner 재생
-> AdventureView가 Controller.OnEnemyTurnBannerCompleted 호출
-> Controller가 enemy action 실행
-> EnemyTurnCompleted 이벤트
-> AdventureView가 Controller.OnEnemyTurnCompleted 호출
-> Controller가 player turn으로 복귀
```

이 흐름은 View와 Controller가 서로 완료 콜백을 주고받으며 turn phase를 진행한다.

새 방향:

```text
EndTurn input
-> EnemyTurn start presentation
-> Enemy action execution
-> EnemyTurn action completed presentation
-> PlayerTurnStarted 재사용
```

### EndTurn Input

```text
EndTurnWidget clicked
-> AdventureView.OnWidgetEndTurnClicked()
-> AdventurePlayerTurnUIFlow.PlayEndTurnClicked()
-> AdventureSceneController.OnEndTurnClicked()
```

예상 형태:

```csharp
internal async void OnWidgetEndTurnClicked()
{
    await _playerTurnUIFlow.PlayEndTurnClicked();
    _controller.OnEndTurnClicked();
}
```

`PlayEndTurnClicked` 책임:

```text
EndTurnWidget 숨김
Skill targeting 정리
Pouch hidden/non-clickable 유지
CoinStatus reset/hide 정책 적용
```

`AdventureView`가 각 Widget을 직접 조작하지 않는다.

### EnemyTurn Start

Controller는 EndTurn을 받은 뒤 enemy action을 바로 실행하지 않는다.

```csharp
public void OnEndTurnClicked()
{
    _turnGameFlow.EndPlayerTurn();

    AdventureEnemyTurnStartViewModel viewModel =
        _presenter.CreateEnemyTurnStartViewModel();

    _events.Screen.EnemyTurnStarted?.Invoke(viewModel);
}
```

이유:

```text
Enemy action은 EnemyTurn start presentation이 끝난 뒤 실행되어야 한다.
```

### EnemyTurnStarted Event

```csharp
public sealed class AdventureScreenEvents
{
    public Action<AdventureEnemyTurnStartViewModel> EnemyTurnStarted;
}

public sealed class AdventureEnemyTurnStartViewModel
{
    public CombatTurnViewModel Turn { get; }
}
```

### AdventureEnemyTurnUIFlow

```csharp
public sealed class AdventureEnemyTurnUIFlow
{
    public async Awaitable PlayStart(
        AdventureEnemyTurnStartViewModel viewModel)
    {
        await _turnUIFlow.PlayEnemyTurnBanner(viewModel.Turn);
    }

    public async Awaitable PlayCompleted()
    {
        await _turnUIFlow.HideEnemyTurnState();
    }
}
```

View 흐름:

```csharp
internal async void OnEnemyTurnStarted(
    AdventureEnemyTurnStartViewModel viewModel)
{
    await _enemyTurnUIFlow.PlayStart(viewModel);
    _controller.OnEnemyTurnStartPresentationCompleted();
}
```

Controller 흐름:

```csharp
public void OnEnemyTurnStartPresentationCompleted()
{
    _turnGameFlow.StartEnemyTurn();
}
```

### Enemy Intent Trigger

Enemy action 실행 중 의도 발동 표시가 필요하다.

피해야 할 이름:

```text
IntentTriggeredRequested
```

권장 이름:

```text
EnemyIntentTriggered
```

예상 형태:

```csharp
public sealed class EnemyIntentTriggeredViewModel
{
    public uint EnemyCardId { get; }
}

internal async void OnEnemyIntentTriggered(
    EnemyIntentTriggeredViewModel viewModel)
{
    await _intentUIFlow.PlayEnemyIntentTriggered(viewModel);
}
```

### EnemyTurn Action Completed

Enemy action 실행이 모두 끝난 뒤:

```text
EnemyTurnActionCompleted
-> AdventureView.OnEnemyTurnActionCompleted()
-> AdventureEnemyTurnUIFlow.PlayCompleted()
-> AdventureSceneController.OnEnemyTurnActionPresentationCompleted()
-> AdventureTurnGameFlow.CompleteEnemyTurn()
-> AdventureScreenEvents.PlayerTurnStarted(viewModel)
```

예상 형태:

```csharp
internal async void OnEnemyTurnActionCompleted()
{
    await _enemyTurnUIFlow.PlayCompleted();
    _controller.OnEnemyTurnActionPresentationCompleted();
}
```

Controller:

```csharp
public void OnEnemyTurnActionPresentationCompleted()
{
    _turnGameFlow.CompleteEnemyTurn();

    AdventurePlayerTurnStartViewModel viewModel =
        _presenter.CreatePlayerTurnStartViewModel();

    _events.Screen.PlayerTurnStarted?.Invoke(viewModel);
}
```

규칙:

```text
PlayerTurn 복귀는 반드시 PlayerTurnStarted 경로를 재사용한다.
EnemyTurn 완료 후 View가 직접 PlayerTurn banner를 재생하지 않는다.
```

### EnemyTurn Naming

피한다:

```text
OnEnemyTurnBannerCompleted
EnemyTurnCompleted
IntentTriggeredRequested
```

사용한다:

```text
OnEnemyTurnStartPresentationCompleted
EnemyTurnActionCompleted
EnemyIntentTriggered
```

이유:

```text
Banner 완료는 enemy turn start presentation 완료의 한 구현 세부다.
EnemyTurnCompleted는 gameplay action 완료인지 UI presentation 완료인지 모호하다.
IntentTriggeredRequested는 player/enemy 문맥이 드러나지 않는다.
```

## CombatResult UI Direction

현재 흐름:

```text
GameplayDeathMessage
-> AdventureCombatDeathObserver
-> AdventureCombatResultFlow.CompleteCombat(result)
-> AdventureProgress.EnterReward or EnterDefeat
-> ResultRequested(result)
-> AdventureView.OnCombatEnded(result)
```

문제:

```text
ResultRequested는 화면에서 무엇을 시작해야 하는지 드러나지 않는다.
ECombatEndResult enum만 전달하면 View가 다시 문맥을 추측해야 한다.
Victory 표시, Reward 처리, Choice refresh가 한 흐름에 섞일 위험이 있다.
```

새 방향:

```text
CombatResultStarted
RewardStarted
ChoiceRefreshStarted
```

이 섹션은 `CombatResultStarted`만 정의한다.

Reward와 Choice refresh는 별도 후속 흐름이다.

### CombatResult Event

피한다:

```text
ResultRequested(ECombatEndResult result)
OnCombatEnded(ECombatEndResult result)
```

사용한다:

```text
CombatResultStarted(AdventureCombatResultViewModel viewModel)
OnCombatResultStarted(AdventureCombatResultViewModel viewModel)
```

예상 형태:

```csharp
public sealed class AdventureCombatResultViewModel
{
    public ECombatEndResult Result { get; }
    public bool IsVictory => Result == ECombatEndResult.Victory;
    public bool IsDefeat => Result == ECombatEndResult.Defeat;
}
```

### AdventureCombatResultUIFlow

`AdventureCombatResultUIFlow`는 전투 결과 표시만 담당한다.

허용 책임:

- combat turn UI 정리 요청
- skill targeting 정리 요청
- Victory 표시
- Defeat 표시
- 결과 표시 완료 대기

금지 책임:

- Reward 지급
- Reward 카드 생성
- 다음 선택지 카드 생성
- Stage 진행
- Scene 이동

예상 형태:

```csharp
public sealed class AdventureCombatResultUIFlow
{
    public async Awaitable Play(
        AdventureCombatResultViewModel viewModel)
    {
        await _playerTurnUIFlow.ClearForCombatResult();
        await _enemyTurnUIFlow.ClearForCombatResult();
        await _skillUIFlow.ClearForCombatResult();

        if (viewModel.IsVictory)
        {
            await PlayVictory(viewModel);
            return;
        }

        await PlayDefeat(viewModel);
    }
}
```

View:

```csharp
internal async void OnCombatResultStarted(
    AdventureCombatResultViewModel viewModel)
{
    await _combatResultUIFlow.Play(viewModel);
    _controller.OnCombatResultPresentationCompleted(viewModel.Result);
}
```

Controller:

```csharp
public void OnCombatResultPresentationCompleted(
    ECombatEndResult result)
{
    if (result == ECombatEndResult.Victory)
    {
        StartRewardFlow();
        return;
    }

    StartDefeatFlow();
}
```

### Victory vs Reward

Victory 표시와 Reward 처리는 다른 흐름이다.

```text
Victory presentation
  "전투에서 이겼다"

Reward handling
  "보상을 선택하거나 지급받는다"
```

따라서:

```text
AdventureCombatResultUIFlow.PlayVictory()
  Reward 지급 금지
  Reward 카드 생성 금지
  Choice refresh 금지
```

### Defeat Boundary

Defeat 표시 이후에는 별도 Defeat flow가 진행된다.

예:

```text
CombatResultStarted(Defeat)
-> AdventureCombatResultUIFlow.PlayDefeat()
-> Controller.OnCombatResultPresentationCompleted(Defeat)
-> Defeat flow
```

Defeat flow는 나중에 다음 중 무엇을 할지 결정한다.

```text
run summary 표시
TitleScene 복귀
restart
```

### Choice Refresh Boundary

Choice refresh는 combat result에서 바로 실행하지 않는다.

이유:

```text
Victory 이후 Reward가 먼저 올 수 있다.
Reward는 player state를 바꿀 수 있다.
Reward 완료 후에야 다음 choice/next stage 표시를 결정할 수 있다.
```

따라서:

```text
CombatResult
-> Reward
-> ChoiceRefresh / NextStage
```

순서로 본다.

## ChoiceRefresh / NextStage UI Direction

### 문제

전투 승리와 Reward 완료 후에는 다음 선택지 카드가 오른쪽 보드에 다시 표시되어야 한다.

현재 역할을 기준으로 보면:

```text
AdventureStageFlow.StartCurrentStage()
  다음 stage의 gameplay state를 만든다.
  오른쪽 board runtime state를 비우고 새 offer card를 배치한다.

AdventureBoardUIFlow.PlaceCards()
  전달받은 CardViewModel을 기준으로 board side를 즉시 교체한다.

AdventureBoardLayout.ReplaceCards()
  VisualElement area를 즉시 Clear하고 새 card를 Add한다.
```

문제는 상태 교체와 화면 교체 사이에 연출 완료 지점이 없다는 것이다.

필요한 UI 경계:

```text
기존 오른쪽 카드 exit 완료
-> 기존 오른쪽 카드 제거
-> 새 오른쪽 카드 생성 및 hidden 배치
-> 새 오른쪽 카드 enter 완료
```

### 용어 분리

```text
NextStage
  게임 진행 상태 변경.
  AdventureRun/stage를 다음으로 진행한다.
  다음 오른쪽 offer card를 만든다.

ChoiceRefresh
  화면 표현 흐름.
  기존 오른쪽 카드를 제거한다.
  새 오른쪽 선택지 카드를 표시한다.
```

이유:

```text
NextStage는 "다음 게임 상태가 무엇인가?"에 답한다.
ChoiceRefresh는 "그 상태 변화를 화면에 어떻게 보여줄 것인가?"에 답한다.
```

### Event

Screen event는 높은 수준의 요청 하나로 둔다.

```csharp
public sealed class AdventureScreenEvents
{
    public Action<AdventureChoiceRefreshViewModel> ChoiceRefreshStarted;
}
```

아래처럼 쪼개지 않는다.

```text
RightCardsRemoveRequested
RightCardsCreateRequested
ChoiceCardsEnterRequested
```

이 이름들은 animation 구현 세부사항을 Controller/GameFlow 밖으로 노출한다.

### ViewModel

```csharp
public sealed class AdventureChoiceRefreshViewModel
{
    public IReadOnlyList<AdventureBoardCardViewModel> BoardCards { get; }
    public bool IsAdventureCompleted { get; }
}
```

규칙:

```text
BoardCards에 오른쪽 카드만 들어오면 오른쪽만 갱신한다.
왼쪽 player card는 유지한다.
```

이유:

Player card는 stage refresh마다 바뀌는 대상이 아니다.
매번 왼쪽까지 교체하면 불필요한 dispose/bind와 시각적 흔들림이 생긴다.

### Controller Flow

Reward 완료 후에는 gameplay state를 먼저 갱신하고, 그 결과를 UI에 요청한다.

```csharp
public void OnRewardCompleted(
    IReadOnlyList<uint> claimedRewardIds)
{
    _rewardGameFlow.CompleteReward(claimedRewardIds);

    AdventureStageStartResult result =
        _stageGameFlow.StartNextStage();

    if (result.IsAdventureCompleted)
    {
        AdventureCompleteViewModel completeViewModel =
            _presenter.CreateAdventureCompleteViewModel();

        _events.Screen.AdventureCompleteStarted?.Invoke(completeViewModel);
        return;
    }

    AdventureChoiceRefreshViewModel viewModel =
        _presenter.CreateChoiceRefreshViewModel();

    _events.Screen.ChoiceRefreshStarted?.Invoke(viewModel);
}
```

핵심 규칙:

```text
GameFlow는 runtime state를 바꾼다.
Presenter는 state를 UI data로 변환한다.
UIFlow는 UI data를 화면에 표현한다.
```

### UIFlow

`AdventureChoiceRefreshUIFlow`는 refresh sequence를 조율한다.

```csharp
public sealed class AdventureChoiceRefreshUIFlow
{
    private readonly AdventureBoardUIFlow _boardUIFlow;

    public async Awaitable Play(
        AdventureChoiceRefreshViewModel viewModel)
    {
        await _boardUIFlow.PlayRightSideExit();
        _boardUIFlow.PrepareRightSide(viewModel.BoardCards);
        await _boardUIFlow.PlayRightSideEnter();
    }
}
```

`AdventureBoardUIFlow`는 board에 특화된 작업만 제공한다.

```csharp
public sealed class AdventureBoardUIFlow
{
    public Awaitable PlayRightSideExit();
    public void PrepareRightSide(
        IReadOnlyList<AdventureBoardCardViewModel> cards);
    public Awaitable PlayRightSideEnter();
}
```

### 완료 기준

기본 규칙은 End-only completion이다.

```text
Exit completion
  기존 오른쪽 카드 중 마지막 카드의 TransitionEndEvent.
  기존 오른쪽 카드가 없으면 즉시 완료.

Enter completion
  새 오른쪽 카드 중 마지막 카드의 TransitionEndEvent.
  새 오른쪽 카드가 없으면 즉시 완료.
```

Fallback timeout은 기본 규칙에 넣지 않는다.
먼저 정상 End-only 경로를 가볍게 확립하고, 이후 animation reliability 정책으로 추가한다.

### BoardLayout 경계

`AdventureBoardLayout`은 공간 배치만 담당한다.

허용:

```text
slot/anchor 생성
area clear
card VisualElement add
placement 반환
```

금지:

```text
stage advance
choice/monster 판정
refresh animation 순서 결정
Controller 호출
```

이유:

Layout은 공간 문제이고 ChoiceRefresh는 시간 순서 문제이다.
둘을 섞으면 combat card, choice card, reward preview 등 다른 카드 표시 흐름에 재사용하기 어렵다.

### Screen Handler

```csharp
private async void OnScreenChoiceRefreshStarted(
    AdventureChoiceRefreshViewModel viewModel)
{
    await _choiceRefreshUIFlow.Play(viewModel);
    _controller.OnChoiceRefreshPresentationCompleted();
}
```

흐름:

```text
Monster defeated
-> CombatResultStarted(Victory)
-> CombatResult UI completed
-> RewardStarted
-> Reward UI completed
-> RewardGameFlow applies reward
-> StageGameFlow advances stage
-> Presenter creates AdventureChoiceRefreshViewModel
-> ChoiceRefreshStarted
-> right cards exit
-> right cards prepared
-> right cards enter
-> Controller.OnChoiceRefreshPresentationCompleted
```

## ResourceStatusBar UI Direction

### 문제

Adventure 화면은 지역 이름을 두 곳에서 보여줄 수 있다.

```text
Banner
  짧게 등장하는 연출용 표시.
  지역 진입, 플레이어 턴, 적 턴 같은 순간을 알린다.

ResourceStatusBar
  계속 남아 있는 HUD 표시.
  지역 이름이나 리소스 상태를 화면 상단에 유지한다.
```

둘 다 지역 이름을 보여줄 수 있지만 역할은 다르다.
같은 텍스트를 다룬다는 이유로 같은 흐름에 넣으면 HUD와 연출의 책임이 섞인다.

### 결정

`ResourceStatusBar`는 C# Widget으로 승격한다.

```text
ResourceStatusBarWidget
```

담당:

```text
label 조회
mode class 전환
region text 적용
resource value text 적용
HUD enter/exit class 적용
```

담당하지 않음:

```text
player HP 계산
coin 계산
soul/currency 계산
region 선택
stage 진행
banner 연출
```

이유:

현재 UXML/USS에는 이미 mode 개념이 있다.

```text
resource-status-bar--region
resource-status-bar--resources
resource-status-bar__region
resource-status-bar__resources
```

이는 단순 장식이 아니라 상태를 가진 UI이다.

### ViewModel

```csharp
public sealed class AdventureResourceStatusViewModel
{
    public LocalizedString RegionName { get; }
    public AdventureResourceValuesViewModel Resources { get; }
    public AdventureResourceStatusMode Mode { get; }
}

public enum AdventureResourceStatusMode
{
    Region,
    Resources,
}

public sealed class AdventureResourceValuesViewModel
{
    public string HpText { get; }
    public string CoinText { get; }
    public string SoulText { get; }
}
```

규칙:

```text
ViewModel은 표시 가능한 값을 가진다.
Widget은 값을 계산하지 않고 적용만 한다.
```

### Presenter

초기 화면 데이터에 ResourceStatus를 포함한다.

```csharp
public AdventureResourceStatusViewModel CreateInitialResourceStatus()
{
    return new AdventureResourceStatusViewModel(
        _region.Adventure.LocalizedRegionName,
        AdventureResourceValuesViewModel.Empty(),
        AdventureResourceStatusMode.Region);
}
```

Screen은 `AdventureRegionData`를 직접 읽지 않는다.

### UIFlow

`AdventureResourceStatusUIFlow`는 지속 HUD 표시를 담당한다.

```csharp
public sealed class AdventureResourceStatusUIFlow
{
    private readonly ResourceStatusBarWidget _statusBar;

    public void Prepare(
        AdventureResourceStatusViewModel viewModel)
    {
        _statusBar.SetRegionName(viewModel.RegionName);
        _statusBar.SetResources(viewModel.Resources);
        _statusBar.SetMode(viewModel.Mode);
        _statusBar.HideImmediate();
    }

    public Awaitable Show()
    {
        return _statusBar.PlayEnter();
    }

    public void Apply(
        AdventureResourceStatusViewModel viewModel)
    {
        _statusBar.SetRegionName(viewModel.RegionName);
        _statusBar.SetResources(viewModel.Resources);
        _statusBar.SetMode(viewModel.Mode);
    }
}
```

### Intro와의 관계

Intro는 ResourceStatusBar를 조율할 수 있지만 내부 상태를 직접 만지지 않는다.

```text
AdventureIntroUIFlow
  AdventureResourceStatusUIFlow.Show() 호출
  Banner.PresentRegion(...) 호출
  AdventureBoardUIFlow initial card intro 호출
```

### Coin 주의점

현재 전투 코인 UI는 별도이다.

```text
CoinStatusWidget
```

따라서 ResourceStatusBar의 coin text는 전투 coin face 상태를 의미하지 않는다.
향후 persistent currency가 정의될 때만 ResourceStatusBar에서 coin 값을 표시한다.

### Initial Screen ViewModel

초기 화면 데이터는 하나로 묶는다.

```csharp
public sealed class AdventureInitialScreenViewModel
{
    public AdventureEntryPresentationViewModel Entry { get; }
    public AdventureResourceStatusViewModel ResourceStatus { get; }
    public IReadOnlyList<AdventureBoardCardViewModel> BoardCards { get; }
}
```

Screen 흐름:

```csharp
private async void OnScreenInitialPresentationStarted(
    AdventureInitialScreenViewModel viewModel)
{
    _resourceStatusUIFlow.Prepare(viewModel.ResourceStatus);
    _introUIFlow.Prepare(viewModel.Entry, viewModel.BoardCards);

    await _introUIFlow.Play();

    _controller.OnInitialPresentationCompleted();
}
```

초기 ResourceStatus는 별도 이벤트로 보내지 않는다.
초기 화면 구성 데이터의 일부로 본다.

## EventBinder Direction

### 문제

현재 구조에는 다음 형태가 있다.

```text
AdventureViewEventBinder : IStartable, IDisposable
  AdventureGameEvents -> AdventureView
  AdventureWidgetEvents -> AdventureView
```

좋은 점:

```text
scene-scoped event 구독이 scene-scoped disposable 객체에 묶인다.
```

문제:

```text
Game -> Screen
Widget -> Screen
```

두 방향이 한 클래스에 섞인다.
나중에 이벤트가 늘어나면 어떤 방향의 연결인지 빠르게 읽기 어렵다.

### 결정

방향별 Binder로 나눈다.

```text
AdventureGameToScreenEventBinder
AdventureWidgetToScreenEventBinder
AdventureCardWidgetEventBinder
```

이름은 구현 시 조정 가능하지만 생명주기 분리는 유지한다.

```text
GameToScreenEventBinder
  VContainer scoped
  IStartable + IDisposable
  Game/Controller event -> Screen method

WidgetToScreenEventBinder
  VContainer scoped
  IStartable + IDisposable
  static Widget event -> Screen method

CardWidgetEventBinder
  dynamic card binding lifetime
  card 생성 시 bind
  card 제거 시 dispose
```

### Game Events

`AdventureGameEvents`는 game-to-screen event hub로 둔다.

```csharp
public sealed class AdventureGameEvents
{
    public AdventureScreenEvents Screen { get; }
}

public sealed class AdventureScreenEvents
{
    public Action<AdventureInitialScreenViewModel> InitialPresentationStarted;
    public Action<AdventurePlayerTurnStartedViewModel> PlayerTurnStarted;
    public Action<CoinFlipCueData> CoinFlipRequested;
    public Action<AdventureEnemyTurnStartedViewModel> EnemyTurnStarted;
    public Action<AdventureCombatResultViewModel> CombatResultStarted;
    public Action<AdventureRewardViewModel> RewardStarted;
    public Action<AdventureChoiceRefreshViewModel> ChoiceRefreshStarted;
    public Action<AdventureResourceStatusViewModel> ResourceStatusChanged;
}
```

이유:

```text
AdventureGameEvents는 출처를 말한다.
AdventureScreenEvents는 도착 표면을 말한다.
```

### Widget Events

정적 위젯 입력은 `AdventureWidgetEvents`에 둔다.

```csharp
public sealed class AdventureWidgetEvents
{
    public TurnWidgetEvents Turn { get; }
    public PouchWidgetEvents Pouch { get; }
    public SkillSlotWidgetEvents SkillSlot { get; }
}
```

Widget event 이름은 가능하면 widget 역할명으로 둔다.

```text
PouchWidgetEvents
TurnWidgetEvents
SkillSlotWidgetEvents
```

### Binder Example

```csharp
// Role:
// Connects game-to-screen events to AdventureScreen handlers for the AdventureScene scope.
public sealed class AdventureGameToScreenEventBinder : IStartable, IDisposable
{
    private readonly AdventureGameEvents _events;
    private readonly AdventureScreen _screen;

    public void Start()
    {
        _events.Screen.InitialPresentationStarted += _screen.OnGameInitialPresentationStarted;
        _events.Screen.PlayerTurnStarted += _screen.OnGamePlayerTurnStarted;
        _events.Screen.CoinFlipRequested += _screen.OnGameCoinFlipRequested;
        _events.Screen.EnemyTurnStarted += _screen.OnGameEnemyTurnStarted;
        _events.Screen.CombatResultStarted += _screen.OnGameCombatResultStarted;
        _events.Screen.RewardStarted += _screen.OnGameRewardStarted;
        _events.Screen.ChoiceRefreshStarted += _screen.OnGameChoiceRefreshStarted;
        _events.Screen.ResourceStatusChanged += _screen.OnGameResourceStatusChanged;
    }

    public void Dispose()
    {
        _events.Screen.InitialPresentationStarted -= _screen.OnGameInitialPresentationStarted;
        _events.Screen.PlayerTurnStarted -= _screen.OnGamePlayerTurnStarted;
        _events.Screen.CoinFlipRequested -= _screen.OnGameCoinFlipRequested;
        _events.Screen.EnemyTurnStarted -= _screen.OnGameEnemyTurnStarted;
        _events.Screen.CombatResultStarted -= _screen.OnGameCombatResultStarted;
        _events.Screen.RewardStarted -= _screen.OnGameRewardStarted;
        _events.Screen.ChoiceRefreshStarted -= _screen.OnGameChoiceRefreshStarted;
        _events.Screen.ResourceStatusChanged -= _screen.OnGameResourceStatusChanged;
    }
}
```

```csharp
// Role:
// Connects static widget input events to AdventureScreen handlers for the AdventureScene scope.
public sealed class AdventureWidgetToScreenEventBinder : IStartable, IDisposable
{
    private readonly AdventureWidgetEvents _events;
    private readonly AdventureScreen _screen;

    public void Start()
    {
        _events.Turn.EndTurnClicked += _screen.OnWidgetEndTurnClicked;
        _events.Pouch.Clicked += _screen.OnWidgetPouchClicked;
        _events.SkillSlot.SelectionChanged += _screen.OnWidgetSkillSlotSelectionChanged;
    }

    public void Dispose()
    {
        _events.Turn.EndTurnClicked -= _screen.OnWidgetEndTurnClicked;
        _events.Pouch.Clicked -= _screen.OnWidgetPouchClicked;
        _events.SkillSlot.SelectionChanged -= _screen.OnWidgetSkillSlotSelectionChanged;
    }
}
```

### Handler Naming

Screen handler는 출처를 포함한다.

```text
OnGamePlayerTurnStarted
OnGameChoiceRefreshStarted
OnWidgetPouchClicked
OnWidgetEndTurnClicked
OnCardChoiceSelected
```

이유:

`OnPlayerTurnStarted`만 보면 GameFlow 이벤트인지, Widget 입력인지, Animation 완료인지 알 수 없다.

### UIFlow Binding Rule

기본 규칙:

```text
Event -> Screen -> UIFlow
Event -> Screen -> Controller
```

피한다:

```text
Event -> UIFlow
```

이유:

Screen은 UI와 Controller 사이의 단일 Bridge이다.
Event가 UIFlow를 직접 호출하면 입력 허용 여부, 중복 실행 방지, 순서 보장이 여러 곳으로 흩어진다.

### Dynamic Card Events

동적 카드 이벤트는 scene-scoped binder를 쓰지 않는다.

```csharp
public sealed class AdventureCardWidgetEventBinder
{
    private readonly AdventureWidgetEvents _widgetEvents;

    public IDisposable BindChoiceCard(
        AdventureCardWidget card,
        EChoiceCardType choiceType)
    {
        void OnClicked()
        {
            _widgetEvents.ChoiceCard.Selected?.Invoke(choiceType);
        }

        card.Clicked += OnClicked;
        return Disposable.Create(() => card.Clicked -= OnClicked);
    }
}
```

카드 위젯은 board refresh 중 생성/제거된다.
따라서 카드 이벤트 구독은 card binding과 함께 사라져야 한다.

## Asset/Data Implementation Gate

### 문제

아래 C# 타입이 존재해도 구현 준비가 끝난 것은 아니다.

```text
AdventureChoiceCardUIModel
AdventureChoiceCardUITable
AdventureChoiceCardWidget
```

실제 런타임은 다음이 모두 필요하다.

```text
row assets
table asset
Addressables references
Localization entries
UXML/USS address registration
validation
```

### Required Assets

`EChoiceCardType`별로 `AdventureChoiceCardUIModel` row asset을 만든다.

```text
Monster
Elite
Boss
Event
Shop
```

각 row는 다음을 가진다.

```text
DisplayName: LocalizedString
Icon: Sprite
UssClassName: string
```

런타임 fallback은 두지 않는다.
누락은 content setup failure로 본다.

### Table Asset

```text
AdventureChoiceCardUITable.asset
```

규칙:

```text
_rows.Count == EChoiceCardType count
_rows[index] matches enum numeric index
each row is AssetReferenceT<AdventureChoiceCardUIModel>
table asset has ModelTable Addressables label
```

이유:

`AbstractTable<TModel, TKey>`는 enum index로 row를 찾는다.
순서가 틀리면 `Event`가 `Shop` row를 로드할 수 있다.

### Addressables

필수:

```text
AdventureChoiceCardWidget.uxml
  address: AdventureChoiceCardWidget

AdventureChoiceCardUIModel rows
  referenced by AdventureChoiceCardUITable rows

AdventureChoiceCardUITable.asset
  label: ModelTable
```

### Localization

ChoiceCard 표시명은 Localization entry로 만든다.

예시:

```text
choice_card_monster
choice_card_elite
choice_card_boss
choice_card_event
choice_card_shop
```

규칙:

```text
DisplayName은 LocalizedString.
Runtime은 localization entry가 있다고 가정.
Fallback string은 runtime에 넣지 않는다.
```

### Validation

구현 완료 전 editor validation을 둔다.

후보:

```text
AdventureChoiceCardUIAssetValidator
```

검사:

```text
AdventureChoiceCardUITable exists
table is Addressable with ModelTable label
row count equals EChoiceCardType count
each enum value has a valid row reference
each row has DisplayName
each DisplayName resolves to localization entry
each row has Icon
each row has UssClassName
AdventureChoiceCardWidget UXML address exists
```

대부분의 실패는 C# 로직 실패가 아니라 content wiring 실패이다.
따라서 play mode 진입 전에 잡는 것이 맞다.

### Loading Policy

최소 단계:

```text
AdventureBoardUIFlow may use DBManager.ChoiceCardUI.Get(choiceType)
```

선택지 종류가 작고 제한적이므로 과도기적으로 허용한다.

향후 단계:

```text
AdventureSceneLoader preloads required AdventureChoiceCardUIModel rows.
AdventureSceneAssetHandles owns and releases handles.
AdventureBoardUIFlow consumes already-loaded UI models.
```

이유:

`Get`은 내부에서 `WaitForCompletion` 경로로 이어질 수 있다.
작은 과도기에는 허용하지만 일반 런타임 데이터 로딩 규칙으로 확장하면 안 된다.

### Completion Gate

Choice-card UI 구현 완료 조건:

```text
C# type exists
UXML/USS exists
Addressables address exists
row assets exist
table asset exists
ModelTable label includes the table
Localization entries exist
validation passes
```

컴파일 성공만으로 완료로 보지 않는다.

## Reward UI Direction

현재 상태:

```text
AdventureProgress.EnterReward()
```

만 있고 실제 Reward UI 흐름은 없다.

따라서 Reward는 새로 정의해야 하는 흐름이다.

목표:

```text
CombatResult Victory
-> RewardStarted
-> Reward UI
-> RewardCompleted
-> ChoiceRefresh / NextStage
```

### Reward Event

```csharp
public sealed class AdventureScreenEvents
{
    public Action<AdventureRewardViewModel> RewardStarted;
}
```

예상 모델:

```csharp
public sealed class AdventureRewardViewModel
{
    public IReadOnlyList<AdventureRewardItemViewModel> Items { get; }
    public bool CanSkip { get; }
    public bool AutoCompleteIfEmpty => Items == null || Items.Count == 0;
}

public sealed class AdventureRewardItemViewModel
{
    public uint RewardId { get; }
    public AdventureRewardType Type { get; }
}
```

초기에는 실제 reward item이 없어도 된다.

하지만 그 경우도 fallback이 아니라 명시적인 empty reward로 본다.

### AdventureRewardGameFlow

`AdventureRewardGameFlow`는 gameplay reward 처리를 담당한다.

허용 책임:

- reward option 생성
- reward claim 검증
- reward 적용
- AdventureRun stage advance

금지 책임:

- reward panel 표시
- reward card animation
- board refresh animation

예상 형태:

```csharp
public sealed class AdventureRewardGameFlow
{
    public AdventureRewardViewModel StartReward()
    {
        _progress.EnterReward();
        return _presenter.CreateRewardViewModel();
    }

    public AdventureRewardCompletionResult CompleteReward(
        IReadOnlyList<uint> claimedRewardIds)
    {
        ApplyRewards(claimedRewardIds);
        _runState.CurrentRun.AdvanceStage();

        return AdventureRewardCompletionResult.Completed;
    }
}
```

### AdventureRewardUIFlow

`AdventureRewardUIFlow`는 reward 표시와 선택만 담당한다.

```csharp
public sealed class AdventureRewardUIFlow
{
    public async Awaitable<AdventureRewardUIResult> Play(
        AdventureRewardViewModel viewModel)
    {
        if (viewModel.AutoCompleteIfEmpty)
            return AdventureRewardUIResult.Empty();

        await ShowRewardPanel(viewModel);
        return await WaitRewardSelection();
    }
}
```

View:

```csharp
internal async void OnRewardStarted(
    AdventureRewardViewModel viewModel)
{
    AdventureRewardUIResult result =
        await _rewardUIFlow.Play(viewModel);

    _controller.OnRewardCompleted(result.ClaimedRewardIds);
}
```

Controller:

```csharp
public void OnRewardCompleted(
    IReadOnlyList<uint> claimedRewardIds)
{
    _rewardGameFlow.CompleteReward(claimedRewardIds);

    AdventureChoiceRefreshViewModel viewModel =
        _stageFlow.StartNextStageAndCreateChoiceRefresh();

    _events.Screen.ChoiceRefreshStarted?.Invoke(viewModel);
}
```

`StartNextStageAndCreateChoiceRefresh`는 계획상의 이름이다.

구현 시에는 다음처럼 나눌 수 있다.

```text
AdventureStageFlow.StartCurrentStage()
AdventurePresenter.CreateChoiceRefreshViewModel()
```

### Empty Reward Rule

초기 구현에서 reward item이 없다면:

```text
RewardStarted(empty)
-> AdventureRewardUIFlow auto-complete
-> Controller.OnRewardCompleted(empty)
-> ChoiceRefreshStarted
```

규칙:

```text
empty reward는 명시 상태다.
누락 데이터 fallback으로 취급하지 않는다.
```

### Reward vs ChoiceRefresh

Reward UI는 다음 선택지 카드를 만들지 않는다.

```text
Reward UI result
-> Controller
-> RewardGameFlow applies reward and advances stage
-> Stage/Presenter creates ChoiceRefreshViewModel
-> ChoiceRefreshStarted
```

이유:

```text
Reward는 player state를 바꿀 수 있다.
Choice refresh는 reward 적용 이후 상태를 기준으로 해야 한다.
```

### Defeat Boundary

Defeat는 reward로 들어가지 않는다.

```text
CombatResultStarted(Defeat)
-> CombatResult presentation completed
-> Defeat flow
```

## Purpose

`AdventureView`를 얇은 View Bridge로 재설계한다.

현재 `AdventureView`는 Widget 조회, Widget bind, 게임 stage 시작, intro animation, card input, skill targeting, turn banner, coin animation, intent reveal, controller 호출을 함께 들고 있다. 이 설계는 장기적으로 `AdventureView`가 UI god object가 되는 것을 막기 위한 기준이다.

## Core Direction

### AdventureView

`AdventureView`는 Thin View Bridge로 둔다.

허용 책임:

- UXML clone 이후 Widget 조회
- `AdventureViewWidgets.Initialize(...)` 호출
- Game event 수신
- UIFlow 호출
- Controller 호출
- UIFlow 결과를 Controller 명령으로 변환
- Dispose 시 Widget/UIFlow 관련 정리 호출

금지 책임:

- Widget 직접 조작
- USS class 직접 조작
- Animation 직접 실행
- Game runtime 직접 변경
- Card hover 상태 직접 관리
- Skill targeting 상태 직접 관리
- Board refresh 직접 처리
- Coin/Intent/Turn UI 세부 처리 직접 수행

Controller를 아는 UI 계층 객체는 `AdventureView` 하나로 제한한다.

## Naming Rules

### GameFlow

게임 상태를 변경하는 흐름은 `AdventureXXXGameFlow`로 명명한다.

예:

- `AdventureStartGameFlow`
- `AdventureStageGameFlow`
- `AdventureChoiceGameFlow`
- `AdventureEncounterGameFlow`
- `AdventureCombatEncounterGameFlow`
- `AdventureTurnGameFlow`
- `AdventureCoinGameFlow`
- `AdventureSkillGameFlow`
- `AdventureEnemyActionGameFlow`
- `AdventureCombatResultGameFlow`

규칙:

- Runtime 상태 변경 가능
- Progress 상태 변경 가능
- Game event 발행 가능
- VisualElement 접근 금지
- Widget 접근 금지
- UI animation 실행 금지

### UIFlow

UI 표현 흐름은 `AdventureXXXUIFlow`로 명명한다.

예:

- `AdventureIntroUIFlow`
- `AdventureBoardUIFlow`
- `AdventureTurnUIFlow`
- `AdventureCoinUIFlow`
- `AdventureIntentUIFlow`
- `AdventureSkillUIFlow`
- `AdventureCombatResultUIFlow`

규칙:

- Widget 상태 변경 가능
- Widget animation 실행 가능
- USS class 추가/제거 가능
- UI transition 대기 가능
- UIState 변경 가능
- Controller 접근 금지
- GameFlow 호출 금지
- Runtime 상태 변경 금지

## Flow Direction

### 일반 입력

```text
Widgets -> UIFlow -> AdventureView -> Controller
```

예: 카드 클릭, SkillSlot 선택.

### 입력 직후 UI 반응이 필요한 경우

```text
Widgets -> AdventureView -> UIFlow
                         -> Controller
```

예: Pouch 클릭 직후 Pouch 숨김 또는 CoinStatus 표시 후 Controller에 coin flip 요청.

### Game 결과 표시

```text
Controller / Game Event with ViewModel -> AdventureView -> UIFlow -> Widgets
```

View는 Game 결과 이벤트를 받을 때 가능한 한 ViewModel을 함께 받는다.

```csharp
internal async void OnPlayerTurnStarted(
    AdventurePlayerTurnStartViewModel viewModel)
{
    await _playerTurnUIFlow.PlayStart(viewModel);
}
```

View가 Controller에서 ViewModel을 다시 pull하는 방식은 기본 방향으로 두지 않는다.

이유:

```text
이벤트가 화면 요청을 의미한다면, 그 요청에 필요한 표시 데이터도 같이 들어와야 한다.
그렇지 않으면 View가 다시 Controller API를 알아야 하고, 화면 흐름이 숨겨진 pull 호출에 의존한다.
```

## Start vs Intro

`Start`는 Game side 용어로 사용한다.

```text
AdventureStartFlow
```

역할:

- Adventure runtime 시작
- Run/Region/Player/EncounterSequence 초기화

`Intro`는 UI side 용어로 사용한다.

```text
AdventureIntroUIFlow
```

역할:

- Adventure 첫 화면 intro presentation sequence
- 지역 배너 표시 요청
- 초기 보드 카드 등장 요청
- intro 완료 대기

`Entry`라는 이름은 화면 진입 전체를 넓게 포함하므로 현재 intro 범위에서는 사용하지 않는다.

## Initialization Flow

초기화는 세 단계로 분리한다.

### 1. Controller Initial Setup

`AdventureSceneEntryPoint.Start()`에서 수행한다.

```text
AdventureSceneEntryPoint.Start
-> AdventureStartFlow.StartAdventure()
-> AdventureSceneNavigator.ShowAdventure()
-> AdventureSceneController.StartAdventure()
```

규칙:

- `AdventureStartFlow.StartAdventure()`는 Runtime 상태만 초기화한다.
- `AdventureSceneNavigator.ShowAdventure()`는 Screen 표시와 event subscription 준비 완료를 보장한다.
- `AdventureSceneController.StartAdventure()`는 Stage 시작, InitialPresentation 생성, `InitialPresentationPrepared` 이벤트 발행을 담당한다.
- Controller는 VisualElement/Widget/Animation을 모른다.

### 2. Screen Initial Setup

`AdventureView.OnInitialPresentationPrepared(viewModel)`에서 수행한다.

```text
AdventureView.OnInitialPresentationPrepared
-> InitializeScreen(viewModel)
-> AdventureIntroUIFlow.Prepare(viewModel)
```

규칙:

- Screen 초기 설정은 intro animation을 시작하지 않는다.
- `Prepare`는 보드 카드 생성/배치, delay class 적용, shown class 제거 같은 시작 상태 준비만 한다.
- Controller에서 추가 데이터를 pull하지 않는다.

### 3. Intro Play

Screen initial setup 이후 명시적으로 요청한다.

```text
AdventureView.RequestIntroAnimation
-> AdventureIntroUIFlow.Play()
-> AdventureSceneController.OnIntroCompleted()
```

규칙:

- `OnShown()`은 Controller 시작이나 Intro 세부 실행을 직접 담당하지 않는다.
- `AdventureIntroUIFlow.Play()`는 이미 준비된 UI 상태를 전환한다.
- Intro 완료 후 `OnIntroCompleted()`를 호출한다.

## Widgets

`WidgetRefs`라는 이름은 사용하지 않는다. `Widgets`로 통일한다.

전체 묶음:

```text
AdventureViewWidgets
```

기능별 묶음:

- `AdventureEntryWidgets`
- `AdventureBoardWidgets`
- `AdventureTurnWidgets`
- `AdventureCoinWidgets`
- `AdventureSkillWidgets`
- `AdventureIntentWidgets`

구조:

```text
AdventureViewWidgets
├─ Entry
├─ Board
├─ Turn
├─ Coin
├─ Skill
└─ Intent
```

권장 주입 방식:

```csharp
public sealed class AdventureIntroUIFlow
{
    public void Prepare(AdventureInitialPresentationViewModel viewModel);
    public Awaitable Play();
}
```

UIFlow는 가능한 한 필요한 Widget group만 주입받는다. `AdventureViewWidgets` 전체를 주입받는 것은 각 UIFlow가 모든 Widget에 접근할 수 있게 하므로 피한다.

## UIState

UIState는 기능별로 분리한다.

- `AdventureIntroUIState`
- `AdventureBoardUIState`
- `AdventureTurnUIState`
- `AdventureCoinUIState`
- `AdventureIntentUIState`
- `AdventureSkillUIState`

`AdventureUIState` 하나에 모든 상태를 모으지 않는다.

이유:

```text
AdventureView god object
-> AdventureUIState god object
```

로 책임이 이동할 수 있기 때문이다.

### AdventureIntroUIState

현재는 별도 `AdventureIntroUIState`를 만들지 않는다.

`AdventureIntroUIFlow` 내부의 준비 데이터가 커질 때만 분리한다.

보유 상태:

- EntryPresentation
- Started 여부

### AdventureBoardUIState

보유 상태:

- 현재 board card view models

### AdventureSkillUIState

보유 상태:

- SkillSlots
- ActiveSkillIndex
- ActiveSkillHandle
- ActiveTargetType
- IsTargeting
- HoveredTargetCardId

보유하지 않는 상태:

- HoveredCard `VisualElement`
- Card hover USS class 상태

이유:

```text
SkillUIState는 타겟팅 의미를 저장한다.
Board card VisualElement와 hover class는 AdventureBoardUIFlow가 소유한다.
```

`AdventureSkillTargetingUIFlow`라는 이름은 사용하지 않는다. Skill 쪽 책임은 targeting보다 넓으므로 `AdventureSkillUIFlow`를 사용한다.

## Intro and Board Relationship

`AdventureIntroUIFlow`는 첫 intro 흐름의 전체 순서를 담당한다.

카드 생성/배치와 보드 intro state 관리는 `AdventureBoardUIFlow`에 위임한다.

```text
AdventureIntroUIFlow
-> Banner 표시
-> AdventureBoardUIFlow.PrepareInitialCardsIntro()
-> AdventureBoardUIFlow.WaitInitialCardsIntroEndOnly()
-> NextFrame
-> AdventureBoardUIFlow.StartInitialCardsIntro()
-> Await
```

규칙:

- `AdventureIntroUIFlow`는 `AdventureBoardUIFlow`를 호출할 수 있다.
- `AdventureBoardUIFlow`는 `AdventureIntroUIFlow`를 몰라야 한다.
- 순환 의존 금지.

## Board / Skill Input Split

카드 클릭은 두 단계로 나눈다.

```text
1. 어떤 카드를 클릭했는가?
2. 그 클릭이 무슨 의미인가?
```

역할:

```text
AdventureBoardUIFlow
- VisualElement -> cardId 해석
- 카드 생성/갱신/제거
- 카드 registry 관리
- cardId, avatar, part 조회 지원

AdventureSkillUIFlow
- 현재 skill targeting 상태 확인
- cardId 클릭 의미 판단
- AdventureCardInputResult 반환

AdventureView
- Widget/user interaction 최초 수신
- BoardUIFlow를 통해 cardId 해석
- Result를 Controller 호출로 변환
```

확정 흐름:

```text
Card pointer down
-> AdventureView
-> AdventureBoardUIFlow.TryGetCardId(element)
-> AdventureSkillUIFlow.HandleCardClicked(cardId)
-> AdventureCardInputResult
-> AdventureView
-> Controller
```

규칙:

- 사용자 상호작용의 최초 처리 위치는 `AdventureView`다.
- `AdventureBoardUIFlow`는 pointer event를 Controller로 전달하지 않는다.
- `AdventureBoardUIFlow`는 cardId 해석, card UI 생성/갱신/제거, board 연출을 담당한다.
- `AdventureSkillUIFlow`는 클릭의 의미를 판단한다.
- `AdventureView`는 판단 결과를 Controller 호출로 변환한다.

## Result Types

UIFlow 입력 해석은 Result 타입으로 반환한다.

규칙:

```text
UI 연출 실행 -> Awaitable 반환
입력 해석 -> Result 타입 반환
단순 조회/검증 -> TryXXX 허용
UIFlow event -> 최소화
```

초기 필수 Result:

- `AdventureCardInputResult`
- `AdventureSkillSelectionResult`

예:

```csharp
AdventureCardInputResult result =
    _skillUIFlow.HandleCardClicked(cardId);

switch (result.Kind)
{
    case AdventureCardInputResultKind.SelectCard:
        _controller.OnCardClicked(result.CardId);
        break;

    case AdventureCardInputResultKind.UseSkillOnTarget:
        _controller.UseSkillOnTarget(result.SkillHandle, result.CardId);
        break;
}
```

## Game Events

Game 쪽에서 넘어오는 이벤트는 `AdventureView`가 받는다.

```text
Game Event with ViewModel -> AdventureView -> UIFlow -> Widgets
```

예:

```csharp
internal async void OnPlayerTurnStarted(
    AdventurePlayerTurnStartViewModel viewModel)
{
    await _playerTurnUIFlow.PlayStart(viewModel);
}
```

규칙:

```text
Game event는 가능한 한 해당 화면 요청에 필요한 ViewModel을 포함한다.
AdventureView가 이벤트를 받은 뒤 Controller에서 다시 데이터를 pull하지 않는다.
AdventureViewEventBinder는 계속 AdventureView에 연결할 수 있다.
AdventureView 내부 구현은 직접 처리하지 않고 UIFlow 호출로 제한한다.
```

## Current First Refactor Target

첫 구현 대상은 다음 순서가 적절하다.

1. `AdventureXXXFlow` -> `AdventureXXXGameFlow` 네이밍 정리 계획 수립
2. `AdventureViewWidgets` 및 기능별 `AdventureXXXWidgets` 추가
3. 기능별 UIState 추가
4. `AdventureInitialPresentationViewModel` 추가
5. `AdventureView.OnVisualTreeCloned()`에서 `_controller.StartInitialStage()` 제거
6. `AdventureScreenEvents.InitialPresentationPrepared` 추가
7. `AdventureSceneController.StartAdventure()`에서 initial presentation 이벤트 발행
8. `AdventureView.OnInitialPresentationPrepared()`에서 `InitializeScreen`과 `RequestIntroAnimation` 분리
9. `AdventureIntroUIFlow` 추가
10. `AdventureBoardUIFlow`에 `PrepareInitialCardsIntro`, `WaitInitialCardsIntroEndOnly`, `StartInitialCardsIntro` 추가
11. `AdventureBoardUIFlow`와 `AdventureSkillUIFlow`로 card/skill input 분리
12. `AdventureScreenEvents.PlayerTurnStarted(viewModel)` 추가
13. `AdventurePlayerTurnStartViewModel` 추가
14. `AdventurePlayerTurnUIFlow.PlayStart(viewModel)` 추가
15. Pouch click을 `PouchWidget` 자체 hide가 아닌 `AdventurePlayerTurnUIFlow.PlayPouchClicked()`로 이동
16. Coin flip cue 이후 `AdventureCoinUIFlow.PlayCoinFlip(data)`와 `AdventurePlayerTurnUIFlow.PlayAfterCoinFlip()` 분리

## Minimal Card Design Strategy

카드 시스템은 최종 구조를 한 번에 만들지 않는다.

진행 방식:

```text
최소 설계 제시
-> 문제점 확인
-> 문제를 해결하는 최소 확장
-> 다시 문제점 확인
-> 다음 확장
```

첫 대상은 `AdventureChoiceCardWidget`이다.

이유:

- Health가 없다.
- Intent가 없다.
- Ability Avatar binding이 없다.
- Portrait가 없다.
- GameFlow에는 `EChoiceCardType`만 전달하면 된다.

아직 하지 않는 것:

- Player combat card
- Enemy combat card
- HealthBar
- IntentBar
- Ability Avatar binding
- 공통 `AdventureCardRegistry` 고도화
- 전체 카드 UI model resolver 일반화

## Choice Card Minimum Design

최소 단계의 목적은 다음 흐름을 검증하는 것이다.

```text
EChoiceCardType
-> AdventureChoiceCardViewModel
-> AdventureChoiceCardUITable
-> AdventureChoiceCardUIModel
-> AdventureCardWidgetFactory.CreateChoiceCard()
-> AdventureChoiceCardWidget
-> 클릭 시 EChoiceCardType 전달
```

### AdventureChoiceCardViewModel

`AdventureChoiceCardViewModel`은 UI에 놓일 선택지 카드 한 장을 표현한다.

```csharp
public sealed class AdventureChoiceCardViewModel
    : AdventureBoardCardViewModel
{
    public AdventureChoiceCardViewModel(
        uint offerCardId,
        EChoiceCardType choiceType)
        : base(AdventureBoardSide.Right, offerCardId, null)
    {
        OfferCardId = offerCardId;
        ChoiceType = choiceType;
    }

    public uint OfferCardId { get; }
    public EChoiceCardType ChoiceType { get; }
}
```

규칙:

- Board에 올라가는 CardViewModel은 `AdventureBoardSide`를 가진다.
- ChoiceCard는 기본적으로 `AdventureBoardSide.Right`에 배치된다.
- `OfferCardId`는 GameFlow가 선택된 offer를 식별하기 위한 runtime id다.
- `ChoiceType`은 UI가 어떤 선택지 카드 외형을 사용할지 결정하는 값이다.
- GameFlow에 전달하는 선택 값은 임시 UI index가 아니라 `OfferCardId` 또는 `EChoiceCardType` 중 의도에 맞는 값이어야 한다.

같은 `EChoiceCardType`이 여러 장 나올 수 있으므로 `ChoiceType`만으로는 runtime 선택을 구분할 수 없다.

예:

```text
Monster / Monster / Event
```

위 경우 두 Monster 카드는 UI 외형은 같지만 runtime offer는 다를 수 있다.

### AdventureChoiceCardUIModel

`AdventureChoiceCardUIModel`은 선택지 카드 하나가 화면에서 어떻게 표현되는지 정의하는 row asset이다.

기존 `AbstractTable<TModel, TKey>` 구조를 따르기 위해 `AbstractModel<EChoiceCardType>`를 상속한다.

포함 값:

- `EChoiceCardType Id`
- `LocalizedString DisplayName`
- `Sprite Icon`
- `string UssClassName`

예상 코드:

```csharp
[CreateAssetMenu(
    menuName = "Game/Adventure/Cards/Choice Card UI Model")]
public sealed class AdventureChoiceCardUIModel : AbstractModel<EChoiceCardType>
{
    [SerializeField] private LocalizedString _displayName;
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _ussClassName;

    public EChoiceCardType ChoiceType => Id;
    public LocalizedString DisplayName => _displayName;
    public Sprite Icon => _icon;
    public string UssClassName => _ussClassName;
}
```

이 경우 `ChoiceType`은 row 내부 필드로 중복 저장하지 않고, table의 `_rows` index를 통해 `Id`로 부여된다.

`Definition`, `Presentation`, `Style`, `Visual` 대신 `UIModel`을 사용한다.

이유:

- `Definition`은 Game definition인지 UI definition인지 불명확하다.
- `Presentation`은 표현 계층 의미는 맞지만 기존 프로젝트의 `AbstractModel`/`Table` 명명과 덜 맞는다.
- `Style`은 USS class 같은 스타일에 치우친다.
- `Visual`은 `LocalizedString`까지 포함하기에는 좁다.
- `UIModel`은 row asset이며 UI 표시 데이터라는 점을 가장 직접적으로 드러낸다.

### AdventureChoiceCardUITable

`AdventureChoiceCardUITable`은 `EChoiceCardType`을 받아 `AdventureChoiceCardUIModel`을 반환하는 table asset이다.

기존 프로젝트의 `AbstractTable<TModel, TKey>`를 따른다.

이유:

- 기존 테이블은 row asset을 `AssetReferenceT<TModel>`로 보관한다.
- enum index가 `_rows` 순서와 연결된다.
- `LoadAsync(TKey)`와 `Get(TKey)` 정책을 재사용할 수 있다.
- Addressables 기반 row 로딩 흐름과 일관된다.

예상 코드:

```csharp
[CreateAssetMenu(
    menuName = "Game/Adventure/Cards/Choice Card UI Table")]
public sealed class AdventureChoiceCardUITable
    : AbstractTable<AdventureChoiceCardUIModel, EChoiceCardType>
{
}
```

규칙:

- fallback은 두지 않는다.
- 누락된 `EChoiceCardType`은 런타임 에러다.
- `_rows` 순서는 `EChoiceCardType` enum index와 일치해야 한다.
- `_rows`에는 `AdventureChoiceCardUIModel` row asset의 Addressables reference를 넣는다.
- 이후 editor validation 또는 table 검증 도구를 추가한다.
- 최소 단계에서는 선택지 종류가 작고 제한적이므로 `Get(EChoiceCardType)` 사용을 허용한다.
- 전환 중 블로킹이 문제가 되면 `AdventureSceneLoader` preload에서 `LoadAsync(EChoiceCardType)`로 미리 로드한다.

### Existing ChoiceCardFaceModel Migration

기존 코드에는 다음 구조가 있었다.

```text
EChoiceCardType
ChoiceCardFaceModel
ChoiceCardFaceViewModel
ChoiceCardFaceWidget
```

문제:

- `ChoiceCardFaceModel`은 `AbstractModel<EChoiceCardType>` 기반 row가 아니다.
- `ChoiceCardFaceWidget` 안에 `EChoiceCardType -> USS class` 분기가 하드코딩되어 있다.
- fallback label이 Widget 안에 있다.
- 새 설계의 `UITable -> UIModel -> Widget` 흐름과 책임이 겹친다.

정리 방향:

- `EChoiceCardType`은 유지한다.
- `ChoiceCardFaceModel`은 `AdventureChoiceCardUIModel`으로 대체한다.
- 선택지 label/icon/USS class는 Widget이 아니라 `AdventureChoiceCardUIModel`이 가진다.
- Widget fallback label은 제거한다. 누락은 데이터 오류로 처리한다.
- 기존 `ChoiceCardFaceWidget` 활성 C# 경로는 제거한다.
- 기존 관련 asset은 향후 정리 대상으로 남긴다.

### AdventureCardWidgetFactory

Factory는 public 명시 Create 함수를 제공한다.

외부 Flow는 generic Create를 직접 호출하지 않는다.

```csharp
public sealed class AdventureCardWidgetFactory
{
    public AdventureChoiceCardWidget CreateChoiceCard(
        uint boardCardId,
        AdventureChoiceCardViewModel viewModel,
        AdventureChoiceCardUIModel uiModel)
    {
        AdventureChoiceCardWidget widget = new AdventureChoiceCardWidget();
        widget.Bind(boardCardId, viewModel, uiModel);
        return widget;
    }
}
```

규칙:

- Flow는 `CreateChoiceCard(...)`처럼 의도가 드러나는 함수를 호출한다.
- Factory는 어떤 카드를 만들지 판단하지 않는다.
- generic Create는 아직 도입하지 않는다.

### AdventureChoiceCardWidget

`AdventureChoiceCardWidget`은 선택지 카드 전용 Widget이다.

책임:

- 선택지 카드 UXML/USS 구조 보유
- `BoardCardId` 보관
- `EChoiceCardType` 보관
- Icon 표시
- DisplayName 표시
- 선택지별 USS class 적용

금지:

- Controller 호출
- GameFlow 호출
- `EChoiceCardType`에 따른 UI model lookup
- encounter 시작 판단

예상 코드:

```csharp
public sealed class AdventureChoiceCardWidget
    : BaseCardWidget
{
    public uint BoardCardId { get; private set; }
    public EChoiceCardType ChoiceType { get; private set; }

    public void Bind(
        uint boardCardId,
        AdventureChoiceCardViewModel viewModel,
        AdventureChoiceCardUIModel uiModel)
    {
        BoardCardId = boardCardId;
        ChoiceType = viewModel.ChoiceType;

        // Icon = uiModel.Icon
        // DisplayName = uiModel.DisplayName
        // AddToClassList(uiModel.UssClassName)
    }
}
```

### AdventureBoardUIFlow

`AdventureBoardUIFlow`는 ChoiceCard 생성 흐름을 조율한다.

```csharp
public sealed class AdventureBoardUIFlow
{
    private readonly AdventureCardWidgetFactory _factory;
    private readonly DBManager _dbManager;

    public IReadOnlyList<AdventureChoiceCardWidget> CreateChoiceCards(
        IReadOnlyList<AdventureChoiceCardViewModel> viewModels)
    {
        List<AdventureChoiceCardWidget> widgets = new(viewModels.Count);
        uint nextBoardCardId = 0;

        foreach (AdventureChoiceCardViewModel viewModel in viewModels)
        {
            uint boardCardId = nextBoardCardId++;
            AdventureChoiceCardUIModel uiModel =
                _dbManager.ChoiceCardUI.Get(viewModel.ChoiceType);

            AdventureChoiceCardWidget widget =
                _factory.CreateChoiceCard(boardCardId, viewModel, uiModel);

            widgets.Add(widget);
        }

        return widgets;
    }
}
```

규칙:

- Widget은 UI model lookup을 하지 않는다.
- Factory는 생성만 한다.
- BoardUIFlow가 `DBManager`를 통해 `AdventureChoiceCardUITable`에 접근한다.
- BoardUIFlow가 UI-local `BoardCardId`를 지역 변수로 생성한다.
- 클릭 후 GameFlow에 전달하는 값은 선택 의도에 따라 `OfferCardId` 또는 `EChoiceCardType`이다.

### ChoiceCard 단계의 문제와 다음 확장

이 단계가 해결하는 것:

- 같은 `EChoiceCardType`이 여러 장 나와도 UI 인스턴스를 구분할 수 있다.
- 선택지 표시 정보가 Widget에 하드코딩되지 않는다.
- 선택지별 이름/아이콘/USS class가 ScriptableObject asset으로 분리된다.

이 단계 이후 실제 문제가 되면 추가할 것:

- `AdventureChoiceCardRegistry`
- VisualElement -> BoardCardId / OfferCardId 조회
- hover/selected 상태
- choice card 제거 animation
- player/enemy combat card
- health/intent
- ability avatar binding

## Dynamic Card CRUD Direction

### 문제

Adventure board에는 한 종류의 카드만 올라가지 않는다.

현재/예정 카드:

```text
Choice card
Player combat card
Enemy combat card
Reward preview card
Future event/shop preview card
```

이 카드는 CRUD 특성이 다르다.

```text
Choice card
  create: stage choice refresh
  update: 거의 없음
  remove: 선택 후 제거

Player combat card
  create: scene/startup 또는 combat start
  update: health, skill avatar, buffs
  remove: 일반 stage refresh에서는 유지

Enemy combat card
  create: encounter 선택 후 combat start
  update: health, intent, status
  remove: death/combat result

Reward preview card
  create: reward phase
  update: selected/claimed state
  remove: reward completion
```

따라서 하나의 `ReplaceCards()`만으로 모든 것을 처리하면 결국 다음 문제가 생긴다.

```text
health만 바꾸고 싶은데 card를 통째로 재생성한다.
intent만 reveal하고 싶은데 board side를 통째로 replace한다.
choice refresh 제거 animation과 combat damage update가 같은 API를 탄다.
```

### 결정

동적 카드 UI는 CRUD를 분리한다.

```text
Create
  ViewModel -> Widget 생성
  UIModel lookup
  event bind
  registry 등록

Place
  Widget을 BoardSide/Area에 배치
  slot/anchor 생성
  placement 저장

Update
  기존 Widget/Part에 상태 적용
  health/intent/selected/locked 같은 변경 처리

Remove
  exit animation
  event unbind
  registry 제거
  VisualElement 제거
```

### 책임 분리

```text
AdventureCardWidgetFactory
  Widget 생성만 담당.
  board 배치, event bind, Controller 호출 금지.

AdventureCardWidgetEventBinder
  동적 card input event 구독/해제 담당.

AdventureBoardLayout
  공간 배치 담당.
  slot/anchor/area만 다룸.

AdventureBoardUIFlow
  board-level CRUD 조율.
  Create/Place/Update/Remove 순서 결정.
  registry와 binding 생명주기 소유.

AdventureChoiceRefreshUIFlow
  stage choice refresh라는 화면 sequence 담당.
  board 내부 구현은 BoardUIFlow에 위임.
```

### Card Registry

동적 카드는 조회가 필요하다.

```text
VisualElement -> BoardCardId
BoardCardId -> AdventureBoardCardBinding
OfferCardId -> AdventureBoardCardBinding
Runtime CardId -> AdventureBoardCardBinding
```

단, 모든 id를 처음부터 다 넣지 않는다.

최소 규칙:

```text
Choice card
  BoardCardId
  OfferCardId

Combat card
  BoardCardId
  Runtime CardId
```

이유:

Choice card 선택은 runtime offer를 찾아야 한다.
Combat card 선택/targeting은 runtime card를 찾아야 한다.

### AdventureBoardCardBinding

카드 하나의 UI 생명주기를 묶는다.

```csharp
public sealed class AdventureBoardCardBinding : IDisposable
{
    public uint BoardCardId { get; }
    public uint? OfferCardId { get; }
    public uint? RuntimeCardId { get; }
    public AdventureBoardSide Side { get; }
    public AdventureCardWidget Widget { get; }
    public AdventureBoardCardPlacement Placement { get; private set; }

    private readonly IDisposable _eventSubscription;

    public void BindPlacement(
        AdventureBoardCardPlacement placement)
    {
        Placement = placement;
    }

    public void Dispose()
    {
        _eventSubscription?.Dispose();
        Widget?.Unbind();
        Widget?.RemoveFromHierarchy();
    }
}
```

규칙:

- Binding은 동적 카드의 event subscription을 소유한다.
- Binding은 Widget의 `Unbind`와 hierarchy removal을 책임진다.
- BoardUIFlow는 Binding collection을 소유한다.

### BoardUIFlow API

구현 전 목표 API:

```csharp
public sealed class AdventureBoardUIFlow
{
    public IReadOnlyList<AdventureBoardCardBinding> CreateCards(
        IReadOnlyList<AdventureBoardCardViewModel> viewModels);

    public void PlaceCards(
        IReadOnlyList<AdventureBoardCardBinding> bindings);

    public Awaitable PlayRemoveSide(
        AdventureBoardSide side);

    public void RemoveSideImmediate(
        AdventureBoardSide side);

    public bool TryGetBindingByBoardCardId(
        uint boardCardId,
        out AdventureBoardCardBinding binding);

    public bool TryGetBindingByRuntimeCardId(
        uint runtimeCardId,
        out AdventureBoardCardBinding binding);

    public bool TryGetBindingByOfferCardId(
        uint offerCardId,
        out AdventureBoardCardBinding binding);
}
```

`CreateCards`와 `PlaceCards`를 나누는 이유:

```text
Create는 UI 인스턴스 생성을 다룬다.
Place는 보드 공간 배치를 다룬다.
ChoiceRefresh는 새 카드를 미리 만들고 hidden 상태로 둔 뒤 enter animation을 시작할 수 있어야 한다.
```

### Update Rule

카드의 일부 상태 변경은 Replace가 아니라 Update다.

예:

```text
Enemy health changed
-> BoardUIFlow.TryGetBindingByRuntimeCardId(enemyCardId)
-> EnemyCombatCardWidget.SetHealth(...)

Enemy intent revealed
-> BoardUIFlow.TryGetBindingByRuntimeCardId(enemyCardId)
-> EnemyCombatCardWidget.ShowIntent(...)

Choice card locked
-> BoardUIFlow.TryGetBindingByOfferCardId(offerCardId)
-> ChoiceCardWidget.SetLocked(...)
```

이유:

카드를 통째로 교체하면 기존 animation state, event binding, avatar binding이 끊긴다.

### Remove Rule

제거는 두 경로를 가진다.

```text
RemoveImmediate
  scene dispose
  hard reset
  no presentation needed

PlayRemove
  choice refresh
  combat death
  reward close
```

기본 완료 기준:

```text
PlayRemove는 제거 대상 중 마지막 카드의 TransitionEndEvent를 기다린다.
대상이 없으면 즉시 완료한다.
```

### First Implementation Slice

첫 구현 slice는 ChoiceCard만 대상으로 한다.

포함:

```text
AdventureChoiceCardWidget
AdventureChoiceCardUIModel
AdventureChoiceCardUITable
AdventureCardWidgetFactory.CreateChoiceCard
AdventureCardWidgetEventBinder.BindChoiceCard
AdventureBoardCardBinding
AdventureBoardUIFlow.CreateCards / PlaceCards
right-side ChoiceRefresh enter/remove path
AdventureChoiceCardUIAssetValidator
```

제외:

```text
Player combat card update
Enemy combat card health update
Enemy intent reveal
Reward preview card
Pooling
Full Part composition
```

이유:

ChoiceCard는 health/intent/avatar가 없어서 동적 카드 CRUD 뼈대를 검증하기 가장 작다.

## Long-term Card UI Composition Candidate

이 섹션은 장기 확장 후보로 둔다.

현재 최소 구현은 `AdventureChoiceCardWidget`, `AdventurePlayerCombatCardWidget`, `AdventureEnemyCombatCardWidget`처럼 고정 Widget 타입을 먼저 사용한다.

Shell + Part 조립 방식은 카드 기능 조합이 실제로 복잡해진 뒤 다시 검토한다.

후보 구조:

```text
AdventureCardWidget
-> AdventureCardView
-> AdventureCardWidgetPart[]
```

### AdventureCardWidget

`AdventureCardWidget`은 카드의 공통 Shell 후보이다.

책임:

- 카드 root VisualElement 제공
- face/status/badge/overlay 같은 slot 제공
- 공통 USS class 제공
- Part가 붙을 위치 제공

금지 책임:

- Health 수치 변경
- Intent 표시
- Skill targeting 판단
- Controller 호출
- AbilitySystem 직접 접근

`AdventureBoardCardWidget`이라는 이름은 사용하지 않는다. 보드 전용 카드가 아니라 캐릭터 선택, 보드 플레이어 카드, 보드 선택지 카드, 보드 몬스터 카드 등 여러 문맥에서 사용할 수 있는 공통 카드 Shell이기 때문이다.

### AdventureCardView

`AdventureCardView`는 런타임에 생성된 카드 UI 한 장을 표현한다.

책임:

- `CardId` 보관
- `AdventureCardWidget` 보관
- Part 목록 보관
- `Bind`, `Unbind` 처리
- `TryGetPart<T>()` 제공
- AbilitySystem에 전달할 avatar object 제공

예:

```csharp
public sealed class AdventureCardView
{
    public uint CardId { get; }
    public AdventureCardWidget Widget { get; }
    public object Avatar => Widget;

    public bool TryGetPart<T>(out T part)
        where T : class, IAdventureCardWidgetPart;
}
```

### AdventureCardWidgetPart

기능별 표현을 Part로 분리하는 것은 장기 확장안이다.

예:

- `HealthCardWidgetPart`
- `IntentCardWidgetPart`
- `SelectionCardWidgetPart`

규칙:

- 기능이 추가될 때 `AdventureCardWidget`의 `if/is` 분기를 늘리지 않는다.
- 기능이 추가되면 Part와 PartDefinition을 추가한다.
- Part는 자기 UI 조각만 알고, Controller/GameFlow를 모른다.

예:

```csharp
public interface IAdventureCardWidgetPart
{
    void Attach(AdventureCardWidget shell);
    void Bind(AdventureCardWidgetViewModel viewModel);
    void Unbind();
}

public interface IHealthCardWidgetPart : IAdventureCardWidgetPart
{
    void SetHealth(int current, int max);
}

public interface IIntentCardWidgetPart : IAdventureCardWidgetPart
{
    Awaitable ShowIntentAsync(IntentViewModel viewModel);
}
```

### AdventureCardWidgetPartFactory

`AdventureCardWidgetPartFactory`는 정적 presentation definition을 보고 필요한 Part를 만드는 장기 확장안이다.

초기 구현은 단순 `new` 기반으로 둔다. Part 하나하나를 VContainer로 만들지 않는다.

이유:

- VisualElement/Part는 수명이 짧고 많이 생성될 수 있다.
- VContainer는 Scene, Flow, Factory, Registry 같은 큰 단위 생성 책임에 집중한다.
- Part 의존성이 복잡해질 때만 `RegisterFactory` 도입을 검토한다.

## General Card Presentation Candidate

이 섹션은 ChoiceCard 이후 Player/Enemy/CharacterSelect 카드까지 확장할 때 검토할 일반화 후보이다.

현재 최소 단계에서는 `AdventureChoiceCardUIModel`과 `AdventureChoiceCardUITable`만 사용한다.

장기적으로 카드 UI 조합은 런타임 조건문이 아니라 정적 presentation asset으로 결정할 수 있다.

```text
Model + PresentationContext
-> AdventureCardPresentationResolver
-> AdventureCardPresentationDefinition
-> AdventureCardWidgetPartDefinition[]
-> AdventureCardView
```

### PresentationContext

`CharacterModel`, `MonsterModel`, `EventModel`, `ShopModel`은 특정 Card UI에 묶이지 않는다.

같은 모델이라도 어디에 표시되는지에 따라 다른 카드 UI가 될 수 있다.

확정 context:

- `CharacterSelect`
- `AdventureBoardPlayer`
- `AdventureBoardChoice`
- `AdventureBoardEnemy`

`ShopPanel`, `EventPanel`은 카드 context가 아니다. Shop/Event 모델이 보드에 카드로 놓이면 `AdventureBoardChoice` context를 사용하고, 클릭 이후 열리는 패널은 별도 UIFlow가 담당한다.

### PresentationDefinition

`AdventureCardPresentationDefinition`은 카드가 어떤 Part를 가질지 정의하는 ScriptableObject 후보이다.

예:

```text
BoardPlayerCardPresentation
-> HealthPart

BoardChoiceCardPresentation
-> SelectionPart

BoardEnemyCardPresentation
-> SelectionPart
-> HealthPart
-> IntentPart

CharacterSelectCardPresentation
-> CharacterSelect 전용 Part 구성
```

규칙:

- fallback 없이 완전한 table을 전제로 한다.
- 누락된 context는 런타임 에러로 본다.
- PresentationDefinition은 생성 이후 변경하지 않는다.
- 표현 구성이 크게 바뀌면 기존 card view를 갱신하기보다 재생성한다.

### Resource Ownership

리소스 소유 위치:

```text
Card shell UXML/USS
-> AdventureCardWidget / AdventureCardFactory

Part UXML/USS/icon/slot 설정
-> AdventureCardWidgetPartDefinition

캐릭터 이미지, 몬스터 이미지, 이름, 설명
-> CharacterModel / MonsterModel / ViewModel

어떤 Part를 붙일지
-> AdventureCardPresentationDefinition
```

중요한 규칙:

- PartDefinition에는 캐릭터/몬스터 이미지가 들어가지 않는다.
- 모델에는 UI Part 구성이 들어가지 않는다.
- Presenter가 context를 정하고, Resolver가 presentation을 찾는다.

## Board UIFlow and Card Creation

`CardDealer`는 연결을 제거하고 파일은 임시 유지한다.

`CardDealer`가 담당하던 카드 배치/딜링/애니메이션 조율은 단계적으로 `AdventureBoardUIFlow`로 이동한다.

책임:

- 초기 보드 카드 생성
- 선택지 카드 생성
- 카드 배치/딜링 연출
- 카드 갱신
- 카드 제거
- card registry 관리
- Ability avatar로 넘길 object 반환

`AdventureBoardUIFlow`는 Controller를 호출하지 않는다.

예:

```csharp
AdventureBoardRenderResult result =
    await _boardUIFlow.CreateInitialCardsAsync(viewModel.Board);

foreach (AdventureCardAvatarBinding binding in result.CreatedAvatars)
{
    _controller.BindCardAvatar(binding.CardId, binding.Avatar);
}
```

## Ability Avatar Binding

AbilitySystem의 Avatar 설정은 UIFlow가 직접 하지 않는다.

확정 흐름:

```text
AdventureBoardUIFlow
-> AdventureBoardRenderResult(cardId, avatar object)
-> AdventureView
-> AdventureSceneController
-> AdventureCardAvatarBindingGameFlow
-> AbilitySystem.SetAvatar(object)
```

이유:

- UIFlow는 UI 객체 생성 책임만 가진다.
- AbilitySystem 상태 변경은 GameFlow 책임이다.
- View는 UI와 Controller 사이의 유일한 연결 지점이다.

## Implementation Roadmap

이 로드맵은 설계 순서가 아니라 구현 진입 순서다.
설계 원칙을 가장 작은 검증 가능한 vertical slice부터 코드에 적용한다.

### Phase 0. Content/Data Gate

목표:

```text
ChoiceCard UI에 필요한 content wiring 실패를 먼저 잡는다.
```

작업:

```text
AdventureChoiceCardUIModel row assets 생성
AdventureChoiceCardUITable asset 생성
ModelTable Addressables label 등록
AdventureChoiceCardWidget UXML address 등록
Localization entries 수동 생성
AdventureChoiceCardUIAssetValidator 추가
```

결정:

```text
Localization entry는 우선 수동 생성한다.
Editor generation tool은 후속 개선으로 둔다.
```

### Phase 1. Event/Screen Boundary

목표:

```text
Event -> Screen -> UIFlow / Controller
```

흐름을 코드 구조로 고정한다.

작업:

```text
AdventureGameToScreenEventBinder
AdventureWidgetToScreenEventBinder
AdventureScreenEvents
Screen handler source-prefix naming
기존 AdventureViewEventBinder 분해
```

완료 조건:

```text
Game event가 UIFlow를 직접 호출하지 않는다.
Widget event가 Controller를 직접 호출하지 않는다.
Screen이 단일 bridge다.
```

### Phase 2. AdventureScreen Static Structure

목표:

```text
Screen > Layer > Layout > Area > Widget
```

구조를 코드와 UXML 조회 방식에 반영한다.

작업:

```text
AdventureScreen
AdventureScreenLayers
AdventureContentLayer
AdventureHudLayer
AdventureBoardLayout
AdventureBoardWidgets 또는 AdventureBoardAreas
OnAttachedToPanel에서 필요한 Q 조회
```

완료 조건:

```text
Screen은 layer를 알고, layer는 layout을 알고, layout은 area를 안다.
BoardLayout은 공간 배치만 담당한다.
```

### Phase 3. Initial Presentation / Intro

목표:

```text
Controller initial setup
-> Screen initial setup
-> Intro animation
```

흐름을 한 번에 끊는다.

작업:

```text
AdventureInitialScreenViewModel
AdventureResourceStatusViewModel
AdventureIntroUIFlow
AdventureResourceStatusUIFlow
AdventureBoardUIFlow initial card prepare/start/wait
```

완료 조건:

```text
Screen이 attach되기 전에 animation을 시작하지 않는다.
Intro는 Controller에서 데이터를 다시 pull하지 않는다.
ResourceStatusBar와 Banner의 책임이 분리된다.
```

### Phase 4. ChoiceCard Dynamic CRUD Slice

목표:

```text
동적 카드 CRUD의 최소 vertical slice를 ChoiceCard로 검증한다.
```

작업:

```text
AdventureBoardCardBinding
AdventureCardWidgetFactory.CreateChoiceCard
AdventureCardWidgetEventBinder.BindChoiceCard
AdventureBoardUIFlow.CreateCards
AdventureBoardUIFlow.PlaceCards
AdventureBoardUIFlow.PlayRemoveSide
AdventureChoiceRefreshUIFlow
```

완료 조건:

```text
ChoiceCard 생성/배치/이벤트/제거가 BoardUIFlow 중심으로 동작한다.
Factory는 생성만 한다.
Card event subscription은 Binding dispose와 함께 해제된다.
Right side ChoiceRefresh가 exit -> prepare -> enter 순서로 표현된다.
```

과도기 허용:

```text
DBManager.ChoiceCardUI.Get(choiceType)
```

이유:

`EChoiceCardType`은 작고 제한적이다.
Preload 전환은 Phase 8에서 처리한다.

### Phase 5. Player Turn / Coin / Skill UIFlow

목표:

```text
PlayerTurnStarted 하나로 turn start UI를 시작한다.
Coin flip과 post-coin turn UI를 분리한다.
Skill targeting은 SkillUIFlow가 판단한다.
```

작업:

```text
AdventurePlayerTurnUIFlow
AdventureCoinUIFlow
AdventureSkillUIFlow
AdventureCardInputResult
AdventureSkillSelectionResult
```

완료 조건:

```text
Pouch click은 Widget 내부 hide가 아니다.
CoinStatusWidget과 ResourceStatusBar coin text는 섞이지 않는다.
Card click은 View -> BoardUIFlow -> SkillUIFlow -> View -> Controller 흐름을 탄다.
```

### Phase 6. Enemy Turn / Combat Result / Reward

목표:

```text
전투 후속 흐름을 presentation 단위로 분리한다.
```

작업:

```text
AdventureEnemyTurnUIFlow
AdventureCombatResultUIFlow
AdventureRewardUIFlow
AdventureRewardGameFlow
CombatResultStarted
RewardStarted
```

완료 조건:

```text
Enemy action은 EnemyTurn start presentation 완료 후 실행된다.
CombatResult는 Reward를 직접 건너뛰지 않는다.
Reward는 ChoiceRefresh를 직접 만들지 않는다.
```

### Phase 7. ChoiceRefresh / NextStage Completion

목표:

```text
Reward 완료 후 gameplay state 갱신과 UI refresh를 분리한다.
```

작업:

```text
AdventureStageGameFlow.StartNextStage
AdventureChoiceRefreshViewModel
ChoiceRefreshStarted
AdventureChoiceRefreshUIFlow.Play
Controller.OnChoiceRefreshPresentationCompleted
```

완료 조건:

```text
NextStage는 game state 변경이다.
ChoiceRefresh는 right-side board presentation이다.
왼쪽 player card는 필요할 때만 교체된다.
```

### Phase 8. Loading / Validation Hardening

목표:

```text
첫 slice에서 허용한 동기 로딩 예외를 줄인다.
```

작업:

```text
AdventureSceneLoader에서 ChoiceCard UI rows preload
AdventureSceneAssetHandles가 handle 소유
BoardUIFlow는 preload된 UIModel 사용
Localization/Addressables validator 확장
```

완료 조건:

```text
runtime interaction 중 ChoiceCard UIModel을 WaitForCompletion으로 로드하지 않는다.
content wiring 오류는 editor validation에서 먼저 잡힌다.
```

### Phase 9. Combat Card CRUD Expansion

목표:

```text
ChoiceCard에서 검증한 dynamic card CRUD를 combat card로 확장한다.
```

작업:

```text
PlayerCombatCardWidget
EnemyCombatCardWidget
Health update
Intent reveal
Ability avatar binding
RuntimeCardId registry
```

완료 조건:

```text
health/intent 변경은 card replace가 아니라 update다.
Ability avatar binding은 card binding lifetime과 함께 정리된다.
```

### Phase Policy

규칙:

```text
각 Phase는 compile 가능 상태를 목표로 하지만, 큰 구조 변경 중 일시 compile break는 허용한다.
각 Phase 완료 시 문서의 decision과 실제 코드가 어긋나는지 확인한다.
테스트는 필요한 시점에만 실행한다.
```

## Implementation Assumptions

현재 패스에서 열린 설계 질문은 없다.

구현 전 확정 가정:

1. 첫 구현 slice는 `AdventureChoiceCardWidget + AdventureBoardUIFlow`로 시작한다.
2. `AdventureChoiceCardUIAssetValidator`는 첫 구현 slice에 포함한다.
3. LocalizationTable entry는 우선 수동으로 생성한다.
4. LocalizationTable editor generation tool은 후속 개선으로 둔다.
5. `DBManager.ChoiceCardUI.Get(...)` 과도기 사용은 허용한다.
6. ChoiceCard UIModel preload 전환은 Phase 8에서 처리한다.

## Pre-Implementation Completion Checklist

이 문서는 구현 전 설계 기준으로 다음을 만족해야 한다.

| Check | Status | Evidence |
| --- | --- | --- |
| Root UI ownership direction exists | Complete | `UIRoot`, screen layer, overlay layer, scene screen host/navigator 방향이 정의되어 있다. |
| Screen lifecycle direction exists | Complete | `OnVisualTreeCloned`와 `OnAttachedToPanel`의 역할이 분리되어 있다. |
| Adventure screen hierarchy exists | Complete | `Screen > Layer > Layout > Area > Widget` 구조가 정의되어 있다. |
| UIFlow responsibility exists | Complete | UIFlow는 presentation sequence와 dynamic UI operation을 담당하고 game runtime을 변경하지 않는다. |
| GameFlow responsibility exists | Complete | GameFlow는 runtime/progress 상태 변경을 담당하고 VisualElement를 모른다. |
| Event direction exists | Complete | `Event -> Screen -> UIFlow / Controller` 규칙과 direction-based binder가 정의되어 있다. |
| Initial presentation flow exists | Complete | Controller initial setup, Screen initial setup, Intro animation 경계가 정의되어 있다. |
| Animation base rule exists | Complete | USS transition, C# class order, End-only completion 기준이 정의되어 있다. |
| ResourceStatusBar boundary exists | Complete | Banner와 ResourceStatusBar의 책임이 분리되어 있다. |
| Dynamic card CRUD rule exists | Complete | Create, Place, Update, Remove가 분리되어 있다. |
| ChoiceCard first slice exists | Complete | ChoiceCard asset/data, widget, board flow, validator gate가 정의되어 있다. |
| PlayerTurn/Coin/Skill direction exists | Complete | PlayerTurnStarted, CoinUIFlow, PlayerTurnUIFlow, SkillUIFlow 경계가 정의되어 있다. |
| EnemyTurn/CombatResult/Reward direction exists | Complete | enemy action, combat result, reward, choice refresh가 분리되어 있다. |
| ChoiceRefresh/NextStage direction exists | Complete | NextStage는 gameplay state, ChoiceRefresh는 board presentation으로 분리되어 있다. |
| Implementation roadmap exists | Complete | Phase 0-9 구현 진입 순서가 정의되어 있다. |
| Open design questions remain | None | 현재 패스에서 열린 설계 질문은 없다. |

주의:

```text
Complete는 코드 구현 완료가 아니다.
Complete는 구현에 들어가기 전 설계 기준이 문서화되었다는 뜻이다.
```
