# AdventureView UIFlow Redesign

## Current Implementation Update: Intro Deal and Turn Start Presentation

이 절은 2026-06-29 기준 구현 상태를 문서 기준으로 고정한다.
아래의 기존 "Current Implementation-Ready Direction"에 남아 있는 예전 범위 설명보다 이 절의 내용이 우선한다.

### Intro 카드 딜링 순서

현재 Intro는 다음 순서를 따른다.

```text
AdventureSceneEntryPoint.Start
-> AdventureScreenController.StartAdventure
-> AdventureScreenEvents.InitialPresentationPrepared(viewModel)
-> AdventureView.OnGameInitialPresentationPrepared
-> AdventureIntroUIFlow.Play
```

`AdventureIntroUIFlow.Play`의 현재 책임:

```text
1. intro shown class 제거
2. background / emblem 적용
3. ResourceStatusBar 초기 표시 데이터 준비
4. 1 frame 대기
5. CardDeck enter transition 완료 대기 등록
6. region banner presentation 시작
7. adventure-view--intro-shown class 추가
8. CardDeck enter transition 완료 대기
9. AdventureBoardUIFlow.ReplaceBoardWithEnter(viewModel.BoardCards, cardDeck, canContinue)
10. region banner 완료 대기
11. InitialPresentationPrepared Awaitable 완료 반환
```

카드 딜링은 `AdventureBoardUIFlow`와 `AdventureBoardLayout`이 담당한다.
deck deal motion 세부 구현은 `AdventureCardDealAnimator`가 담당한다.
`AdventureIntroUIFlow`는 카드 widget 생성, board slot 계산, card event binding을 알지 않는다.

### CardDeck Top-Card Deal Motion

현재 딜링 연출은 "덱 전체가 카드를 뿜는 것"이 아니라 "덱의 맨 위 카드가 딜러처럼 전달되는 것"으로 정의한다.

```text
CardDeck
-> card-deck-top-card worldBound center를 출발 좌표로 사용

AdventureBoardLayout
-> destination anchor는 보드 최종 좌표에 고정
-> placement.Card 내부의 실제 카드 본체만 이동
-> Player/Monster/Display: 내부 CardWidget 이동
-> Choice: 내부 .card-widget 이동

AdventureCardDealAnimator
-> top-card 기준 시작 transform 적용
-> card body transition 실행
-> deck nudge / settle motion 실행
```

연출 규칙:

```text
1. card anchor는 destination slot에 고정한다.
2. 실제 card body만 top-card 위치에서 시작한다.
3. 시작 scale은 0.28이다.
4. 이동 경로는 목적지 방향 직선이다.
5. 회전은 사용하지 않는다.
6. 여러 장은 0.12초 stagger로 겹쳐 출발한다.
7. 덱은 카드가 빠져나가는 방향으로 짧게 nudge한다.
8. 덱은 줄어들거나 사라지지 않는다.
9. 도착 시 0.98 scale + 2px down으로 짧게 눌렸다가 복귀한다.
```

이 구조를 선택한 이유:

```text
anchor를 움직이면 Health/Intent 예약 공간까지 움직인다.
그러면 "카드 본체를 딜링한다"가 아니라 "보드 슬롯 전체를 이동한다"가 된다.
따라서 anchor는 layout 좌표를 고정하고, 실제 card body만 animation target으로 삼는다.
```

주의할 점:

```text
딜링은 실제 보드 카드 widget을 이동시키는 연출이다.
연출용 임시 카드 clone을 만들지 않는다.
CardDeck의 top-card는 출발 좌표와 시각 기준만 제공한다.
덱은 그대로 남고, 보드에 붙은 실제 카드 본체가 top-card 위치에서 목적지로 들어온다.
```

이유:

```text
임시 clone을 사용하면 "clone 연출 완료 후 실제 card widget 표시"라는 두 번째 동기화 문제가 생긴다.
Health/Intent/Avatar/event binding이 붙는 실제 카드를 처음부터 움직이면,
연출 완료 후 별도 교체 없이 그대로 게임 UI가 된다.
```

### PlayerTurn 시작 표시 순서

Intro가 끝난 뒤 전투가 시작되면 `AdventureScreenEvents.PlayerTurnStarted(viewModel)`이 화면에 전달된다.
현재 PlayerTurn 시작 UI 순서는 다음과 같다.

```text
1. Intent reveal 시작
2. Turn banner 표시 시작
3. HealthBar 표시 시작
4. Turn banner와 HealthBar 완료 대기
5. Intent reveal 완료는 기다리지 않음
6. CoinStatus 표시
7. Pouch 표시
```

HealthBar는 카드 배치 중 표시하지 않는다.
`AdventureCardWidgetFactory`는 card placement 시 `ShowHealthAsync`를 호출하지 않는다.
HealthBar 표시는 `AdventureBoardUIFlow.ShowHealthBars(viewTransitionManager)`를 통해 PlayerTurn 시작 presentation에서 호출한다.

Intent는 PlayerTurnStarted 처리 초반에 표시를 시작한다.
다만 Intent reveal은 현재 PlayerTurn UI gate를 막지 않는다.
이 결정은 Pouch/CoinStatus 표시가 Intent animation 완료에 묶여 전체 템포가 느려지는 것을 피하기 위한 임시 기준이다.
구현은 `async void`가 아니라 `Awaitable` 기반 fire-and-forget 래퍼를 사용한다.
Screen lifetime이 끊긴 경우 intent reveal은 시작하지 않거나 중간에 중단된다.
추후 UX 확인 결과 Pouch가 너무 빨리 보이면 Intent reveal을 다시 await 대상으로 바꿀 수 있다.

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
-> AdventureStartFlow.InitializeRuntime()
   Runtime 상태 초기화
-> AdventureScreenController.InitializeAdventure()
   Stage 시작
   초기 board/runtime presentation state 준비
-> AdventureSceneLocalization.Preload()
   화면 표시 전에 필요한 localization 준비
-> AdventureSceneNavigator.ShowAdventure()
   Screen 생성/표시
-> AdventureScreenController.StartAdventure()
   AdventureInitialPresentationViewModel 생성
   InitialPresentationPrepared 이벤트 await
   Intro 완료 시 AdventureProgress.MarkIntroCompleted
   Intro 완료 후 즉시 encounter 확인
```

`StartAdventure()` 내부 책임은 두 단계로 나눈다.

```text
RequestInitialPresentation()
  InitialPresentationPrepared를 발행하고 Screen intro 완료를 기다린다.

ContinueAfterInitialPresentation()
  Intro 완료 이후 AdventureProgress, choice input, immediate encounter 흐름을 진행한다.
```

### Screen Flow

```text
AdventureView.OnGameInitialPresentationPrepared(viewModel)
-> attach/widget 준비 대기
-> PrepareInitialPresentation(viewModel)
-> AdventureIntroUIFlow.Play(viewModel)
-> Intro 완료를 StartAdventure 흐름에 반환
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
```

예상 형태:

```csharp
protected override void OnAttachedToPanel(AttachToPanelEvent evt)
{
    base.OnAttachedToPanel(evt);

    if (!_screenWidgets.IsInitialized)
    {
        _screenWidgets.Initialize(LogicalRoot);
        _screenUIFlow.Bind(_screenWidgets.EffectLayer);
    }

    _screenAttached = true;
}

public async Awaitable<bool> OnGameInitialPresentationPrepared(
    AdventureInitialPresentationViewModel viewModel)
{
    int screenLifetimeVersion = _screenLifetimeVersion;
    if (!await WaitUntilScreenReady(screenLifetimeVersion))
        return false;

    PrepareInitialPresentation(viewModel);
    return await PlayIntroOnce(screenLifetimeVersion, viewModel);
}
```

규칙:

```text
OnVisualTreeCloned는 UXML clone 완료 시점만 의미한다.
OnAttachedToPanel은 화면이 실제 Panel에 올라온 뒤 LogicalRoot 기준으로 Screen 하위 widget 참조를 확정한다.
OnGameInitialPresentationPrepared는 초기 표시 데이터를 장기 field로 보관하지 않는다.
OnGameInitialPresentationPrepared는 attach/widget 준비를 기다린 뒤 Intro 완료까지 반환한다.
AdventureInitialPresentationViewModel은 요청 단위 데이터이며, screen lifetime이 끊기면 폐기된다.
```

### AdventureIntroUIFlow

`AdventureIntroUIFlow`는 Adventure intro presentation sequence를 소유한다.

허용 책임:

- 지역 배너 표시 요청
- 지역 배너 표시 완료 대기
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
    public Awaitable Play(
        VisualElement adventureRoot,
        VisualElement background,
        VisualElement emblem,
        VisualElement cardDeck,
        ResourceStatusBar resourceStatusBar,
        Banner banner,
        AdventureInitialPresentationViewModel viewModel,
        Func<bool> canContinue);
}
```

### AdventureBoardUIFlow Intro Contract

초기 카드 intro에서 Board 쪽은 별도 intro 전용 API를 만들지 않는다.

```text
AdventureIntroUIFlow
-> AdventureBoardUIFlow.ReplaceBoardWithEnter(initial display cards)
-> 지역 배너와 card anchor의 USS transition 대기
```

의미:

- `AdventureIntroUIFlow`: 첫 화면 연출 순서 조율
- `AdventureBoardUIFlow`: 카드 생성/배치/교체/제거 경계 제공
- `AdventureBoardLayout`: 실제 slot/anchor 구조와 transition target 제공
- `AdventureCardDealAnimator`: 덱에서 실제 카드 본체가 들어오는 animation 실행

현재 intro 딜링은 일반 board enter와 다르다.

```text
일반 enter:
-> anchor 자체의 enter-pending / enter class로 처리

intro deck deal:
-> anchor는 최종 위치에 고정
-> card body에 deck 기준 translate/scale/opacity 시작값 적용
-> 다음 frame에 card body class를 추가해 목적지로 transition
```

이유:

```text
일반 enter는 "보드 슬롯이 나타남"을 표현한다.
intro deck deal은 "덱에서 카드가 배달됨"을 표현한다.
같은 board enter 계열 API라도 animation target은 다르다.
```

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

예외:

```text
Intro deck deal처럼 실제 card widget 생성 순서와 출발 순서를 runtime placement가 결정하는 경우에는 C# stagger를 사용한다.
현재 기준은 0.12초 간격으로 다음 카드 deal을 시작한다.
이때 USS는 한 카드의 이동/settle transition만 정의하고, 여러 카드의 시작 간격은 UIFlow/Layout가 조율한다.
```

이유:

```text
USS delay만 사용하면 카드 수, 좌/우 side, 실제 생성 순서가 바뀔 때 delay class를 다시 설계해야 한다.
반면 deck deal은 "이번에 배치된 placement list를 순서대로 출발시킨다"가 핵심 규칙이다.
따라서 stagger는 placement를 알고 있는 Layout 쪽에서 조율한다.
```

```text
기본 완료 기준은 TransitionEndEvent다.
다만 화면 detach나 transition cancel은 screen lifetime 경계이므로 Awaitable 완료로 처리한다.
fallback timeout은 기본형에 넣지 않는다.
TransitionEndEvent가 정상 화면에서 누락되는 것은 우선 USS/design bug로 본다.
```

현재 구현 메모:

```text
ViewTransitionAwaiter.WaitForEnd는 DetachFromPanelEvent와 TransitionCancelEvent도 완료로 처리한다.
이는 fallback timeout이 아니라 screen lifetime 정리용 완료 처리다.
화면이 닫히거나 transition이 취소된 뒤 Awaitable이 영원히 남는 것을 막기 위한 경계이다.
AdventureCardDealAnimator는 deal transition 완료 후에도 card/anchor/deck이 panel에 붙어 있을 때만 settle motion을 이어간다.
AdventureCardDealAnimator가 적용한 card inline transform/opacity와 deck nudge inline translate는 정상 완료와 중단 경로 모두에서 animator가 정리한다.
```

Intro 완료 후 View callback 메서드는 만들지 않는다.

```text
AdventureScreenController.StartAdventure()
-> await InitialPresentationPrepared
-> AdventureEncounterStartFlow.TryStartImmediateEncounter()
```

`OnInitialBoardShown`, `OnIntroCompleted`처럼 View가 다시 Controller를 호출하는 완료 callback은 피한다.

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
AdventureScreenController.StartPlayerTurnPresentation()
-> AdventureTurnGameFlow.StartPlayerTurn()
   combat/runtime/intent 준비
-> AdventurePresenter.CreatePlayerTurnStartViewModel()
-> AdventureScreenEvents.PlayerTurnStarted(viewModel)
-> AdventureView.OnGamePlayerTurnStarted(viewModel)
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
-> AdventureScreenController.OnPouchClicked()
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
    try
    {
        int screenLifetimeVersion = _screenLifetimeVersion;
        if (!IsCurrentScreenLifetime(screenLifetimeVersion))
            return;

        await _screenUIFlow.PlayCoinChange(
            data,
            _screenWidgets.CoinStatus,
            () => IsCurrentScreenLifetime(screenLifetimeVersion));
    }
    catch (Exception exception)
    {
        Debug.LogException(exception);
    }
}
```

`HandleCoinChangeCue`는 `AdventurePlayerTurnUIFlow.PlayAfterCoinFlip()`을 호출하지 않는다.

Async boundary 규칙:

```text
async void는 Unity event, widget event, GameplayCue receiver 같은 외부 진입점에만 둔다.
내부 GameFlow/ScreenFlow request는 Awaitable 또는 Awaitable<T>로 반환한다.
async void 내부에서는 screen lifetime을 캡처하고, 예외는 Unity 콘솔에 남긴다.
```

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

기존 구조의 문제:

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
-> AdventureCardWidgetEventBinder
-> AdventureWidgetEvents.Card.Clicked(cardElement, cardId)
-> AdventureWidgetToScreenEventBinder
-> AdventureView
-> Controller
```

현재 형태:

```csharp
internal async void OnWidgetCardClicked(VisualElement card, uint cardId)
{
    if (!_skillUIFlow.IsTargetingActive)
    {
        await _controller.OnCardClicked(cardId);
        return;
    }

    if (!_controller.CanUseSkillOnTarget(_skillUIFlow.ActiveSkillHandle, cardId))
        return;

    _skillUIFlow.HoverCard(card);
    await ConfirmSkillTarget();
}
```

이유:

```text
Board는 어떤 카드인지 안다.
SkillUIFlow는 지금 카드 클릭이 선택인지 타겟 확정인지 안다.
Controller는 실제 gameplay command를 실행한다.
```

### Current Skill Target Rule

현재 구현에서 `ESkillTargetType.Unit`은 "아무 카드나 선택 가능"이 아니다.

```text
Unit skill target
-> runtime card가 존재해야 한다.
-> AbilitySystem이 있어야 한다.
-> Combat side가 Enemy여야 한다.
```

따라서 플레이어 카드, 선택지 카드, 비전투 카드는 현재 스킬 타겟이 될 수 없다.

이 규칙은 UI가 아니라 gameplay 쪽에서 최종 보장한다.

```text
AdventureView hover/click
-> AdventureScreenController.CanUseSkillOnTarget
-> AdventureSkillFlow.CanUseSkillOnTarget
-> AdventureCombatRuntime side check
```

이유:

```text
UI event binder는 left/right board widget에 모두 연결될 수 있다.
따라서 UI만 믿으면 플레이어 카드나 선택지 카드도 hover/click 대상이 될 수 있다.
최종 타겟 가능 여부는 game rule을 아는 AdventureSkillFlow가 판단해야 한다.
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

기존 구조의 문제:

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
-> AdventureTurnFlow.EndPlayerTurnAndStartEnemyTurn()
-> AdventureScreenEvents.EnemyTurnStarted gate
-> Enemy action execution
-> AdventureTurnFlow.CompleteEnemyTurn()
-> AdventureScreenEvents.PlayerTurnStarted gate 재사용
```

### EndTurn Input

```text
EndTurnWidget clicked
-> AdventureView.OnWidgetEndTurnClicked()
-> AdventurePlayerTurnUIFlow.PlayEndTurnClicked()
-> AdventureScreenController.OnEndTurnClicked()
```

예상 형태:

```csharp
internal async void OnWidgetEndTurnClicked()
{
    bool commandStarted = false;

    try
    {
        int screenLifetimeVersion = _screenLifetimeVersion;
        if (!IsCurrentScreenLifetime(screenLifetimeVersion))
            return;

        if (!_controller.CanClickEndTurn())
            return;

        if (!TryBeginWidgetCommand())
            return;

        commandStarted = true;

        await _screenUIFlow.PlayEndTurnClicked(
            _screenWidgets.CoinStatus,
            _screenWidgets.EndTurn,
            () => IsCurrentScreenLifetime(screenLifetimeVersion));

        if (!IsCurrentScreenLifetime(screenLifetimeVersion))
            return;

        await _controller.OnEndTurnClicked();
    }
    catch (Exception exception)
    {
        Debug.LogException(exception);
    }
    finally
    {
        if (commandStarted)
            EndWidgetCommand();
    }
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
EnemyTurn presentation 완료 여부를 먼저 확인한다.

```csharp
public async Awaitable OnEndTurnClicked()
{
    _turnGameFlow.EndPlayerTurn();

    AdventureEnemyTurnStartViewModel viewModel =
        _presenter.CreateEnemyTurnStartViewModel();

    bool enemyTurnPresentationCompleted =
        await _events.Screen.EnemyTurnStarted.Invoke(viewModel);

    if (!enemyTurnPresentationCompleted)
        return;

    await _enemyActionFlow.ExecuteEnemyTurn();
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
    public Func<AdventureEnemyTurnStartViewModel, Awaitable<bool>> EnemyTurnStarted;
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
internal async Awaitable<bool> OnEnemyTurnStarted(
    AdventureEnemyTurnStartViewModel viewModel)
{
    await _enemyTurnUIFlow.PlayStart(viewModel);
    return IsCurrentScreenLifetime(screenLifetimeVersion);
}
```

### Enemy Intent Trigger

Enemy action 실행 중 의도 발동 표시가 필요하다.

현재 구현 이름:

```text
IntentTriggeredRequested
```

규칙:

```text
IntentTriggeredRequested는 Awaitable<bool> gate이다.
true이면 trigger presentation 완료 후 enemy action을 실행한다.
false이면 scene/view lifetime이 끊겼으므로 enemy action을 실행하지 않는다.
```

예상 형태:

```csharp
public sealed class AdventureCombatEvents
{
    public Func<uint, Awaitable<bool>> IntentTriggeredRequested;
}

internal async Awaitable<bool> OnGameIntentTriggeredRequested(uint cardId)
{
    return await _intentUIFlow.PlayTriggered(cardId);
}
```

### EnemyTurn Completion

Enemy action 실행이 모두 끝난 뒤 View completion callback을 만들지 않는다.

```text
AdventureEnemyActionFlow.ExecuteEnemyTurn()
-> AdventureTurnFlow.CompleteEnemyTurn()
-> AdventureCombatRuntime.StartPlayerTurn()
-> IntentPrepareFlow.PrepareAll()
-> AdventureScreenEvents.PlayerTurnStarted(viewModel)
-> AdventureView.OnGamePlayerTurnStarted(viewModel)
-> Awaitable<bool> gate result
```

규칙:

```text
EnemyTurn 종료 후 PlayerTurn 복귀도 PlayerTurnStarted gate를 재사용한다.
View가 "enemy turn action completed"를 Controller로 다시 알려주는 흐름은 만들지 않는다.
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
EnemyTurnActionCompleted
```

사용한다:

```text
EnemyTurnStarted -> Awaitable<bool>
PlayerTurnStarted -> Awaitable<bool>
IntentTriggeredRequested -> Awaitable<bool>
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

현재 구현 방향:

```text
ResultRequested(ECombatEndResult result) -> Awaitable<bool>
RewardStarted
ChoiceRefreshStarted
```

이 섹션은 combat result presentation gate만 정의한다.

Reward와 Choice refresh는 별도 후속 흐름이다.

### CombatResult Event

현재 남아 있는 이름:

```text
ResultRequested(ECombatEndResult result)
OnCombatEnded(ECombatEndResult result)
```

규칙:

```text
ResultRequested는 Awaitable<bool> gate이다.
true이면 combat result presentation 완료.
false이면 scene/view lifetime이 끊겼으므로 reward/후속 흐름으로 진행하지 않는다.
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
    public Awaitable Play(
        ECombatEndResult result)
    {
        return result == ECombatEndResult.Victory
            ? _banner.PresentVictory()
            : _banner.PresentDefeat();
    }
}
```

View:

```csharp
internal async Awaitable<bool> OnGameCombatEnded(
    ECombatEndResult result)
{
    await _combatResultUIFlow.Play(result);

    return IsCurrentScreenLifetime(screenLifetimeVersion);
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
ResultRequested(Defeat)
-> AdventureCombatResultUIFlow.PlayDefeat()
-> returns true to AdventureCombatResultFlow
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

AdventureBoardUIFlow.ReplaceBoardAfterExit()
  전달받은 CardViewModel을 기준으로 변경된 board side만 퇴장 후 교체한다.

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
    public Func<AdventureChoiceRefreshViewModel, Awaitable<bool>> ChoiceRefreshStarted;
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

### Game Flow

Reward 완료 후에는 gameplay state를 먼저 갱신하고, 그 결과를 UI에 요청한다.

```csharp
AdventureRewardUIResult rewardResult =
    await _events.Screen.RewardStarted.Invoke(rewardViewModel);

await CompleteReward(rewardResult.ClaimedRewardIds);

AdventureChoiceRefreshViewModel viewModel =
    _presenter.CreateChoiceRefreshViewModel();

bool choiceRefreshCompleted =
    await _events.Screen.ChoiceRefreshStarted.Invoke(viewModel);

if (choiceRefreshCompleted)
    await _encounterStartFlow.TryStartImmediateEncounter();
```

핵심 규칙:

```text
GameFlow는 runtime state를 바꾼다.
Presenter는 state를 UI data로 변환한다.
UIFlow는 UI data를 화면에 표현한다.
ChoiceRefreshStarted는 request/response 이벤트이므로 GameFlow가 연출 완료 여부를 확인할 수 있다.
완료되지 않았으면 즉시 encounter 확인으로 진행하지 않는다.
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

기본 정상 완료 기준은 TransitionEndEvent다.

```text
Exit completion
  기존 오른쪽 카드 중 마지막 카드의 TransitionEndEvent.
  기존 오른쪽 카드가 없으면 즉시 완료.

Enter completion
  새 오른쪽 카드 중 마지막 카드의 TransitionEndEvent.
  새 오른쪽 카드가 없으면 즉시 완료.
```

화면 detach나 transition cancel은 screen lifetime 경계로 보고 완료 처리한다.
Fallback timeout은 기본 규칙에 넣지 않는다.
정상 화면에서 TransitionEndEvent가 오지 않는다면 USS/design bug로 본다.

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
```

이유:

Layout은 공간 문제이고 ChoiceRefresh는 시간 순서 문제이다.
둘을 섞으면 combat card, choice card, reward preview 등 다른 카드 표시 흐름에 재사용하기 어렵다.

### Screen Handler

```csharp
private async Awaitable<bool> OnGameChoiceRefreshStarted(
    AdventureChoiceRefreshViewModel viewModel)
{
    return await _choiceRefreshUIFlow.Play(
        viewModel,
        () => IsCurrentScreenLifetime(screenLifetimeVersion));
}
```

흐름:

```text
Monster defeated
-> ResultRequested(Victory)
-> CombatResult UI completed
-> RewardStarted
-> Reward UI completed
-> CombatResultFlow applies reward
-> CombatResultFlow advances stage
-> Presenter creates AdventureChoiceRefreshViewModel
-> ChoiceRefreshStarted
-> right cards exit
-> right cards prepared
-> right cards enter
-> refresh presentation ends
-> AdventureStageFlow.OpenChoiceSelection()
-> CombatResultFlow checks immediate encounter through AdventureEncounterStartFlow
```

규칙:

```text
AdventureStageFlow.StartCurrentStage는 board/runtime state를 준비하지만 choice input을 열지 않는다.
Choice card는 presentation을 위해 먼저 만들어질 수 있지만, 선택 입력은 intro 또는 choice-refresh presentation이 끝난 뒤 OpenChoiceSelection에서만 열린다.
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
public sealed class AdventureInitialPresentationViewModel
{
    public AdventureEntryPresentationViewModel Entry { get; }
    public AdventureResourceStatusViewModel ResourceStatus { get; }
    public IReadOnlyList<AdventureBoardCardViewModel> BoardCards { get; }
    public IReadOnlyList<AdventureSkillSlotViewModel> SkillSlots { get; }
    public IReadOnlyList<AdventureCardViewModel> RuntimeBoardCards { get; }
}
```

Screen 흐름:

```csharp
internal async Awaitable<bool> OnGameInitialPresentationPrepared(
    AdventureInitialPresentationViewModel viewModel)
{
    int screenLifetimeVersion = _screenLifetimeVersion;
    if (!await WaitUntilScreenReady(screenLifetimeVersion))
        return false;

    PrepareInitialPresentation(viewModel);
    return await PlayIntroOnce(screenLifetimeVersion, viewModel);
}
```

초기 ResourceStatus는 별도 이벤트로 보내지 않는다.
초기 화면 구성 데이터의 일부로 본다.

## EventBinder Direction

### 문제

현재 구조에는 다음 형태가 있다.

```text
AdventureSceneEntryPoint.Start
-> AdventureGameToScreenEventBinder.Bind()
-> AdventureWidgetToScreenEventBinder.Bind()

AdventureSceneEntryPoint.Dispose
-> AdventureWidgetToScreenEventBinder.Dispose()
-> AdventureGameToScreenEventBinder.Dispose()

AdventureGameToScreenEventBinder + AdventureWidgetToScreenEventBinder : IDisposable
  AdventureGameEvents -> AdventureView
  AdventureWidgetEvents -> AdventureView
```

좋은 점:

```text
scene-scoped event 구독이 scene-scoped disposable 객체에 묶인다.
첫 presentation event보다 먼저 명시적으로 Bind된다.
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
  explicit Bind + IDisposable
  Game/Controller event -> Screen method

WidgetToScreenEventBinder
  VContainer scoped
  explicit Bind + IDisposable
  static Widget event -> Screen method

CardWidgetEventBinder
  VContainer scoped
  dynamic board binding list 기준으로 pointer/click callback 갱신
```

AdventureView partial file rule:

```text
AdventureView.cs
-> screen lifecycle, initial presentation gate, Dispose

AdventureView.GameEvent.cs
-> GameFlow/Controller output event 수신
-> OnGame... methods

AdventureView.WidgetInput.cs
-> Widget-originated command 수신
-> 모든 OnWidget... entrypoint

AdventureView.Card.cs
-> dynamic board card CRUD, board refresh, board runtime connection

AdventureView.Skill.cs
-> skill targeting helper and target confirmation

AdventureView.GameplayCue.cs
-> GameplayCue receiver bridge and card avatar binding
```

이유:

```text
AdventureView는 Screen의 bridge 역할을 유지하지만,
Game event, Widget input, Card CRUD, Skill targeting, GameplayCue를 한 파일에 섞지 않는다.
파일 이름이 이벤트 방향과 UI 책임을 먼저 드러내야 다음 변경 지점을 빠르게 찾을 수 있다.
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
    public Func<AdventureInitialPresentationViewModel, Awaitable<bool>> InitialPresentationPrepared;
    public Func<AdventurePlayerTurnStartViewModel, Awaitable<bool>> PlayerTurnStarted;
    public Func<AdventureEnemyTurnStartViewModel, Awaitable<bool>> EnemyTurnStarted;
    public Func<AdventureRewardViewModel, Awaitable<AdventureRewardUIResult>> RewardStarted;
    public Func<AdventureChoiceRefreshViewModel, Awaitable<bool>> ChoiceRefreshStarted;
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
public sealed class AdventureGameToScreenEventBinder : IDisposable
{
    private readonly AdventureGameEvents _events;
    private readonly AdventureView _view;

    public void Bind()
    {
        _events.Screen.InitialPresentationPrepared = _view.OnGameInitialPresentationPrepared;
        _events.Screen.PlayerTurnStarted = _view.OnGamePlayerTurnStarted;
        _events.Screen.EnemyTurnStarted = _view.OnGameEnemyTurnStarted;
        _events.Screen.RewardStarted = _view.OnGameRewardStarted;
        _events.Screen.ChoiceRefreshStarted = _view.OnGameChoiceRefreshStarted;
    }

    public void Dispose()
    {
        if (_events.Screen.InitialPresentationPrepared == _view.OnGameInitialPresentationPrepared)
            _events.Screen.InitialPresentationPrepared = null;

        if (_events.Screen.PlayerTurnStarted == _view.OnGamePlayerTurnStarted)
            _events.Screen.PlayerTurnStarted = null;

        if (_events.Screen.EnemyTurnStarted == _view.OnGameEnemyTurnStarted)
            _events.Screen.EnemyTurnStarted = null;

        if (_events.Screen.RewardStarted == _view.OnGameRewardStarted)
            _events.Screen.RewardStarted = null;

        if (_events.Screen.ChoiceRefreshStarted == _view.OnGameChoiceRefreshStarted)
            _events.Screen.ChoiceRefreshStarted = null;
    }
}
```

```csharp
// Role:
// Connects static widget input events to AdventureScreen handlers for the AdventureScene scope.
public sealed class AdventureWidgetToScreenEventBinder : IDisposable
{
    private readonly AdventureWidgetEvents _events;
    private readonly AdventureView _view;

    public void Bind()
    {
        _events.Turn.EndTurnClicked += _view.OnWidgetEndTurnClicked;
        _events.Pouch.Clicked += _view.OnWidgetPouchClicked;
        _events.SkillSlot.SelectionChanged += _view.OnWidgetSkillSlotSelectionChanged;
    }

    public void Dispose()
    {
        _events.Turn.EndTurnClicked -= _view.OnWidgetEndTurnClicked;
        _events.Pouch.Clicked -= _view.OnWidgetPouchClicked;
        _events.SkillSlot.SelectionChanged -= _view.OnWidgetSkillSlotSelectionChanged;
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

동적 카드 이벤트는 scene-scoped `AdventureCardWidgetEventBinder`가 관리한다.

```csharp
public sealed class AdventureCardWidgetEventBinder
{
    private readonly AdventureCardWidgetEvents _events;
    private readonly List<CardRegistration> _registrations = new();

    public void Bind(
        IReadOnlyList<AdventureBoardCardWidgetBinding> bindings)
    {
        // 현재 보드에 배치된 동적 카드에 pointer/click callback을 연결한다.
    }

    public void Unbind()
    {
        // 이전 보드 카드 callback을 모두 해제한다.
    }
}
```

카드 위젯은 board refresh 중 생성/제거된다.
따라서 `AdventureBoardUIFlow`는 board binding list가 바뀐 직후 다음 순서로 이벤트를 갱신한다.

```text
1. 기존 card event callback 전부 해제
2. 현재 Left/Right binding list 기준으로 다시 callback 등록
```

이유:

```text
Binding이 개별 event subscription까지 소유하면 카드 생성/배치/이벤트 연결 책임이 한 객체에 섞인다.
현재 구조에서는 BoardUIFlow가 카드 생명주기를 알고,
AdventureCardWidgetEventBinder가 pointer/click callback 등록 세부사항만 안다.
```

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

현재 구현:

```text
AdventureSceneLoader.Preload
-> AdventureChoiceCardUITable.LoadAsync(EChoiceCardType)
-> AdventureChoiceCardUIModels
-> AdventureSceneScope.RegisterInstance
-> AdventureCardWidgetFactory
```

선택지 UI row는 scene activation 전에 preload된다.

이유:

`AbstractTable.Get`은 내부에서 `WaitForCompletion` 경로로 이어질 수 있다.
ChoiceCard는 board refresh, choice selection, combat transition 중 생성되므로,
interaction 중 UIModel 로딩이 발생하면 transition과 main thread stall이 겹칠 수 있다.

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

### Reward Completion Owner

Reward 완료 처리는 현재 `AdventureCombatResultFlow`가 소유한다.

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
public sealed class AdventureCombatResultFlow
{
    private async Awaitable CompleteReward(
        IReadOnlyList<uint> claimedRewardIds)
    {
        ApplyRewards(claimedRewardIds);
        _runState.CurrentRun.AdvanceStage();

        AdventureChoiceRefreshViewModel viewModel =
            _presenter.CreateChoiceRefreshViewModel();

        bool choiceRefreshCompleted =
            await _events.Screen.ChoiceRefreshStarted.Invoke(viewModel);

        if (choiceRefreshCompleted)
            await _encounterStartFlow.TryStartImmediateEncounter();
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
internal async Awaitable<AdventureRewardUIResult> OnRewardStarted(
    AdventureRewardViewModel viewModel)
{
    return await _rewardUIFlow.Play(viewModel);
}
```

Game Flow:

```csharp
AdventureRewardUIResult result =
    await _screenEvents.RewardStarted.Invoke(rewardViewModel);

await CompleteReward(result.ClaimedRewardIds);
```

Controller는 reward completion을 다시 받지 않는다.

```text
RewardStarted는 request/response 이벤트다.
View는 reward presentation 결과만 반환한다.
Gameplay state 전이는 CombatResultFlow가 계속 소유한다.
```

이유:

```text
View -> Controller.OnRewardCompleted 방식은 흐름이 되돌아온다.
CombatResultFlow가 reward를 시작했으므로, reward 결과도 같은 Flow가 받아 다음 상태로 진행하는 편이 단방향이다.
```

이전 형태:

```csharp
public void OnRewardCompleted(...)
{
    // removed
}
```

Choice refresh view model 생성과 refresh 요청도 reward를 시작한 game flow가 이어서 처리한다.

### Empty Reward Rule

초기 구현에서 reward item이 없다면:

```text
RewardStarted(empty)
-> AdventureRewardUIFlow auto-complete
-> CombatResultFlow receives AdventureRewardUIResult.Empty()
-> ChoiceRefreshStarted
```

규칙:

```text
empty reward는 명시 상태다.
누락 데이터 fallback으로 취급하지 않는다.
```

Reward 결과는 정상 완료와 중단을 구분한다.

```text
AdventureRewardUIResult.Empty()
-> reward item이 없는 정상 완료
-> Completed == true

AdventureRewardUIResult.Canceled()
-> screen detach/dispose 또는 RewardStarted handler 부재
-> Completed == false
```

`CombatResultFlow`는 `Completed == false`인 reward 결과를 받으면 다음 stage advance와 choice refresh를 진행하지 않는다.
이렇게 해야 scene/view lifetime이 끊긴 뒤에 gameplay flow가 닫힌 화면으로 UI 요청을 계속 보내지 않는다.

### Reward vs ChoiceRefresh

Reward UI는 다음 선택지 카드를 만들지 않는다.

```text
Reward UI result
-> CombatResultFlow
-> CombatResultFlow applies reward and advances stage
-> Presenter creates ChoiceRefreshViewModel
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
ResultRequested(Defeat)
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
internal async Awaitable<bool> OnGamePlayerTurnStarted(
    AdventurePlayerTurnStartViewModel viewModel)
{
    await _playerTurnUIFlow.PlayStart(viewModel);
    return true;
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
-> AdventureStartFlow.InitializeRuntime()
-> AdventureScreenController.InitializeAdventure()
-> AdventureSceneLocalization.Preload()
-> AdventureSceneNavigator.ShowAdventure()
-> AdventureScreenController.StartAdventure()
```

규칙:

- `AdventureStartFlow.InitializeRuntime()`은 Runtime 상태만 초기화한다.
- `AdventureScreenController.InitializeAdventure()`는 Stage 시작과 초기 board/runtime presentation state 준비를 담당한다.
- `AdventureSceneNavigator.ShowAdventure()`는 Screen 표시만 담당한다.
- `AdventureScreenController.StartAdventure()`는 InitialPresentation 생성, `InitialPresentationPrepared` await, intro 이후 즉시 encounter 확인을 담당한다.
- Controller는 VisualElement/Widget/Animation을 모른다.

### 2. Screen Initial Setup

`AdventureView.OnGameInitialPresentationPrepared(viewModel)`에서 수행한다.

```text
AdventureView.OnGameInitialPresentationPrepared
-> attach/widget 준비 대기
-> PrepareInitialPresentation(viewModel)
-> AdventureIntroUIFlow.Play(viewModel)
-> Intro 완료 반환
```

규칙:

- Screen 초기 설정과 intro animation은 같은 request/response 이벤트 안에서 순서대로 실행한다.
- View는 Intro 완료 후 Controller를 직접 호출하지 않고 Awaitable 완료로 반환한다.
- Controller에서 추가 데이터를 pull하지 않는다.

### 3. Intro Completion

Intro 완료 이후 흐름은 Controller/GameFlow가 이어간다.

```text
AdventureScreenController.StartAdventure()
-> await InitialPresentationPrepared
-> AdventureEncounterStartFlow.TryStartImmediateEncounter()
```

규칙:

- `OnShown()`은 Controller 시작이나 Intro 세부 실행을 직접 담당하지 않는다.
- `AdventureIntroUIFlow.Play()`는 이미 준비된 UI 상태를 전환한다.
- Intro 완료 후 View callback을 만들지 않는다.

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
Board card VisualElement와 click id 매핑은 AdventureCardWidgetEventBinder가 소유한다.
Target hover USS class 상태는 AdventureSkillUIFlow가 소유한다.
```

`AdventureSkillTargetingUIFlow`라는 이름은 사용하지 않는다. Skill 쪽 책임은 targeting보다 넓으므로 `AdventureSkillUIFlow`를 사용한다.

## Intro and Board Relationship

`AdventureIntroUIFlow`는 첫 intro 흐름의 전체 순서를 담당한다.

카드 생성/배치와 보드 intro state 관리는 `AdventureBoardUIFlow`에 위임한다.

```text
AdventureIntroUIFlow
-> Banner 표시
-> AdventureBoardUIFlow.ReplaceBoardWithEnter(...)
-> Banner와 card board transition end 대기
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
- 카드 생성/갱신/제거
- cardId, avatar, part 조회 지원

AdventureCardWidgetEventBinder
- 현재 board binding list 기준으로 VisualElement -> cardId 해석
- pointer/click callback 등록/해제

AdventureSkillUIFlow
- 현재 skill targeting 상태 확인
- selected skill/target hover/arrow 상태 관리

AdventureView
- Widget/user interaction 최초 수신
- cardId와 현재 skill targeting 상태를 보고 Controller 호출로 변환
```

확정 흐름:

```text
Card pointer down
-> AdventureCardWidgetEventBinder
-> AdventureWidgetEvents.Card.Clicked(cardElement, cardId)
-> AdventureWidgetToScreenEventBinder
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

UIFlow 입력 해석은 상태 전환이 복잡하거나 View가 판단 결과를 다시 써야 할 때만 Result 타입으로 반환한다.

규칙:

```text
UI 연출 실행 -> Awaitable 반환
복잡한 입력 해석 -> Result 타입 반환
단순 카드 클릭 명령 분기 -> AdventureView에서 Controller 호출
단순 조회/검증 -> TryXXX 허용
UIFlow event -> 최소화
```

현재 사용하는 Result:

- `AdventureSkillSelectionResult`

예:

```csharp
AdventureSkillSelectionResult result =
    _skillUIFlow.SelectSkill(selectedIndex, selectedButton);

if (result.ShouldActivateImmediately)
    await _controller.UseSkill(result.Handle);
```

## Game Events

Game 쪽에서 넘어오는 이벤트는 `AdventureView`가 받는다.

```text
Game Event with ViewModel -> AdventureView -> UIFlow -> Widgets
```

예:

```csharp
internal async Awaitable<bool> OnGamePlayerTurnStarted(
    AdventurePlayerTurnStartViewModel viewModel)
{
    await _playerTurnUIFlow.PlayStart(viewModel);
    return true;
}
```

규칙:

```text
Game event는 가능한 한 해당 화면 요청에 필요한 ViewModel을 포함한다.
AdventureView가 이벤트를 받은 뒤 Controller에서 다시 데이터를 pull하지 않는다.
AdventureGameToScreenEventBinder + AdventureWidgetToScreenEventBinder는 계속 AdventureView에 연결할 수 있다.
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
7. `AdventureScreenController.StartAdventure()`에서 initial presentation 이벤트 발행
8. `AdventureView.OnGameInitialPresentationPrepared()`가 초기 설정과 Intro 완료를 Awaitable로 반환
9. `AdventureIntroUIFlow` 추가
10. `AdventureBoardUIFlow`에 `ReplaceBoardWithEnter`, `ReplaceBoardAfterExit`, `RemoveCardAfterExit` 같은 board CRUD/transition 경계 추가
11. `AdventureCardWidgetEventBinder`와 `AdventureSkillUIFlow`로 card input mapping / skill targeting state 분리
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
- fallback label이 Widget 안에 있던 기존 구조는 제거 대상이다.
- 새 설계의 `UITable -> UIModel -> Widget` 흐름과 책임이 겹친다.

정리 방향:

- `EChoiceCardType`은 유지한다.
- `ChoiceCardFaceModel`은 `AdventureChoiceCardUIModel`으로 대체한다.
- 선택지 label/icon/USS class는 Widget이 아니라 `AdventureChoiceCardUIModel`이 가진다.
- Widget fallback label은 제거한다. 누락은 데이터 오류로 처리한다.
- 기존 `ChoiceCardFaceWidget` 활성 C# 경로는 제거한다.
- 기존 관련 asset은 향후 정리 대상으로 남긴다.

### AdventureCardWidgetFactory

Factory는 보드에 올라갈 구체 카드 Widget을 생성한다.

```csharp
public sealed class AdventureCardWidgetFactory
{
    public AdventureBoardCardWidgetBinding Create(
        AdventureBoardCardViewModel viewModel)
    {
        return viewModel switch
        {
            AdventureChoiceCardViewModel choice => CreateChoiceCard(choice),
            AdventureBoardCardViewModel card => CreateBoardCard(card),
        };
    }
}
```

규칙:

- Factory는 `AdventureBoardCardViewModel`의 타입과 `AdventureBoardSide`를 보고 구체 Widget만 선택한다.
- Factory는 board 배치를 하지 않는다.
- Factory는 Controller/GameFlow를 호출하지 않는다.
- Factory는 UI-local id를 만들지 않는다.
- 선택지 카드의 클릭 id는 `AdventureChoiceCardViewModel.OfferCardId`다.
- 일반 카드의 클릭 id는 `AdventureBoardCardViewModel.CardId`다.

### AdventureChoiceCardWidget

`AdventureChoiceCardWidget`은 선택지 카드 전용 Widget이다.

책임:

- 선택지 카드 UXML/USS 구조 보유
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
    public EChoiceCardType ChoiceType { get; private set; }

    public void Bind(
        AdventureChoiceCardViewModel viewModel,
        AdventureChoiceCardUIModel uiModel)
    {
        ChoiceType = viewModel.ChoiceType;

        // Icon = uiModel.Icon
        // DisplayName = uiModel.DisplayName
        // AddToClassList(uiModel.UssClassName)
    }
}
```

### AdventureBoardUIFlow

`AdventureBoardUIFlow`는 보드 단위의 카드 생성/배치/교체 흐름을 조율한다.

```csharp
public sealed class AdventureBoardUIFlow
{
    private readonly AdventureCardWidgetFactory _factory;

    public Awaitable<AdventureBoardPlacementResult> ReplaceBoardWithEnter(
        IReadOnlyList<AdventureBoardCardViewModel> cards,
        Func<bool> canContinue);

    public Awaitable<AdventureBoardPlacementResult> ReplaceBoardAfterExit(
        IReadOnlyList<AdventureBoardCardViewModel> cards,
        Func<bool> canContinue);
}
```

규칙:

- Widget은 UI model lookup을 하지 않는다.
- Factory는 생성만 한다. 선택지 UIModel lookup도 Factory 내부에서 끝낸다.
- BoardUIFlow는 어떤 side에 어떤 카드가 놓일지 조율한다.
- BoardUIFlow는 UI-local id를 만들지 않는다.
- 클릭 후 GameFlow에 전달하는 값은 `AdventureCardWidgetEventBinder`가 ViewModel에서 얻는다.
- 선택지 클릭은 `OfferCardId`, 일반 카드 클릭은 `CardId`를 사용한다.

### ChoiceCard 단계의 문제와 다음 확장

이 단계가 해결하는 것:

- 같은 `EChoiceCardType`이 여러 장 나와도 `OfferCardId`로 runtime offer를 구분할 수 있다.
- 선택지 표시 정보가 Widget에 하드코딩되지 않는다.
- 선택지별 이름/아이콘/USS class가 ScriptableObject asset으로 분리된다.

이 단계 이후 실제 문제가 되면 추가할 것:

- VisualElement -> OfferCardId / RuntimeCardId 조회 고도화
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
  현재 board binding list 기준으로 동적 card input event 구독/해제 담당.

AdventureBoardLayout
  공간 배치 담당.
  slot/anchor/area만 다룸.

AdventureBoardUIFlow
  board-level CRUD 조율.
  Create/Place/Update/Remove 순서 결정.
  binding 생명주기와 card event binder 갱신 시점 소유.
  VContainer scope 종료 시 IDisposable로 남은 card event binding과 board card binding을 정리.

AdventureChoiceRefreshUIFlow
  stage choice refresh라는 화면 sequence 담당.
  board 내부 구현은 BoardUIFlow에 위임.
```

### Card Registry

동적 카드는 조회가 필요하다.

```text
VisualElement -> runtime click id
OfferCardId -> AdventureBoardCardWidgetBinding
Runtime CardId -> AdventureBoardCardWidgetBinding
```

단, 모든 id를 처음부터 다 넣지 않는다.

최소 규칙:

```text
Choice card
  OfferCardId

Combat card
  Runtime CardId
```

이유:

Choice card 선택은 runtime offer를 찾아야 한다.
Combat card 선택/targeting은 runtime card를 찾아야 한다.
UI-only board id를 만들면 다시 runtime id로 매핑해야 하므로 현재 단계에서는 사용하지 않는다.

### AdventureBoardCardWidgetBinding

카드 하나의 UI 생명주기를 묶는다.

```csharp
public sealed class AdventureBoardCardWidgetBinding : IDisposable
{
    private readonly Action<AdventureBoardCardPlacement> _onPlaced;
    private Action _dispose;

    public AdventureBoardCardViewModel ViewModel { get; }
    public VisualElement Element { get; }
    public AdventureBoardCardPlacement Placement { get; private set; }

    public void BindPlacement(
        AdventureBoardCardPlacement placement)
    {
        Placement = placement;
    }

    public void Dispose()
    {
        _dispose?.Invoke();
        _dispose = null;
    }
}
```

규칙:

- Binding은 동적 카드의 ViewModel, Element, Placement를 묶는다.
- Binding dispose는 Widget 내부 bind/unbind, timeline/avatar release 같은 카드 단위 정리를 수행한다.
- 동적 card pointer/click callback은 `AdventureCardWidgetEventBinder`가 소유한다.
- BoardUIFlow는 Binding collection을 소유한다.
- BoardUIFlow는 Binding collection 변경 직후 `AdventureCardWidgetEventBinder`를 갱신한다.

### BoardUIFlow API

구현 전 목표 API:

```csharp
public sealed class AdventureBoardUIFlow
{
    public Awaitable<AdventureBoardPlacementResult> ReplaceBoardWithEnter(
        IReadOnlyList<AdventureBoardCardViewModel> cards,
        Func<bool> canContinue);

    public Awaitable<AdventureBoardPlacementResult> ReplaceBoardAfterExit(
        IReadOnlyList<AdventureBoardCardViewModel> cards);

    public Awaitable<bool> RemoveCardAfterExit(
        uint cardId);

    public void ClearAll();

    public bool TryGetCardId(
        VisualElement element,
        out uint cardId);

    public bool TryGetCardWidget<T>(
        AdventureBoardSide side,
        uint cardId,
        out T widget)
        where T : class;

    public bool TrySetCardHealth(
        uint cardId,
        int currentHealth,
        int maxHealth);
}
```

`CreateWidgets`와 `ReplaceBoard...`를 나누는 이유:

```text
Create는 UI 인스턴스 생성을 다룬다.
Replace는 보드 공간의 현재 표현 상태를 어떤 전환 정책으로 바꿀지 다룬다.
Intro는 기존 카드가 없어도 enter 연출이 필요하므로 ReplaceBoardWithEnter를 사용한다.
ChoiceRefresh는 기존 카드가 퇴장한 뒤 새 카드를 보여야 하므로 ReplaceBoardAfterExit를 사용한다.
전달받은 cards에 특정 side가 없으면 그 side는 유지한다.
즉시 Place 계열 API는 enter/exit 정책을 우회할 수 있어 공개하지 않는다.
side를 실제로 비워야 하면 Refresh가 아니라 SideClearRequested를 사용한다.
```

### Update Rule

카드의 일부 상태 변경은 Replace가 아니라 Update다.

예:

```text
Enemy health changed
-> BoardUIFlow 또는 IntentUIFlow가 RuntimeCardId로 기존 binding/widget 조회
-> AdventureMonsterCardWidget.SetHealth(...)

Enemy intent revealed
-> BoardUIFlow 또는 IntentUIFlow가 RuntimeCardId로 기존 binding/widget 조회
-> AdventureMonsterCardWidget.ShowIntent(...)

Choice card locked
-> BoardUIFlow가 OfferCardId로 기존 binding/widget 조회
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

Create 이후 Enter 기준:

```text
ReplaceBoardWithEnter
  intro처럼 기존 카드가 없어도 신규 카드 입장이 필요한 경우 사용한다.

ReplaceBoardAfterExit
  기존 카드 exit 완료 후 신규 카드를 enter-pending 상태로 배치하고 enter 완료를 기다린다.
```

Pooling은 현재 구현 범위에서 제외한다.

```text
카드는 퇴장 후 제거된다.
Widget 재사용, 재초기화 순서, callback 재사용 문제는 별도 설계가 필요하므로 이번 CRUD 기반에서는 다루지 않는다.
```

Runtime card lifetime rule:

```text
Board UI에서 퇴장한 encounter card는 AdventureCards registry에서도 제거한다.
선택 후 버려진 choice offer card는 board refresh 완료 후 Removed zone에서 제거한다.
전투 중 사망한 enemy card는 퇴장 애니메이션 완료 후 제거한다.
다음 stage로 넘어갈 때 이전 Right/Removed zone card id는 먼저 board state에서 분리하고,
ChoiceRefreshStarted 또는 SideClearRequested 완료 후 AdventureCards registry에서 제거한다.
Player card는 Adventure scene 동안 유지되므로 Left zone 정리 대상에 포함하지 않는다.
```

Non-combat encounter V1 rule:

```text
Event/Shop panel은 아직 구현하지 않는다.
Event/Shop choice가 선택되면 선택된 display card를 board refresh로 보여준 뒤,
해당 encounter를 즉시 완료 처리하고 다음 stage choice refresh로 넘어간다.
이 임시 흐름은 UI 패널이 생기면 EventUIFlow/ShopUIFlow 완료 응답으로 교체한다.
```

Stage advance rule:

```text
AdventureStageAdvanceFlow가 encounter 완료 이후의 진행/완료 분기를 담당한다.
결과는 NoCurrentRun, ScreenUnavailable, Advanced, Completed로 구분한다.
Advanced일 때만 다음 stage의 immediate encounter를 다시 확인한다.
Completed일 때는 Adventure complete 상태로 진입하고 오른쪽 board만 퇴장 후 제거한다.
Event/Shop V1의 선택된 display card id는 AdvanceOrComplete에 전달되어,
다음 board presentation await 이후 registry에서 제거된다.
ScreenUnavailable은 screen lifetime이 끊긴 경우를 뜻한다.
이 값은 gameplay rollback 신호가 아니며, 다음 immediate encounter를 이어가지 않기 위한 중단 신호다.
```

Reward V1 rule:

```text
Reward panel은 아직 구현하지 않는다.
AdventureRewardUIFlow는 empty reward 계약을 유지하되 screen lifetime canContinue를 받는다.
screen lifetime이 끊기면 Canceled를 반환하고 GameFlow는 다음 stage advance를 진행하지 않는다.
```

### First Implementation Slice

첫 구현 slice는 ChoiceCard만 대상으로 한다.

포함:

```text
AdventureChoiceCardWidget
AdventureChoiceCardUIModel
AdventureChoiceCardUITable
AdventureCardWidgetFactory.CreateChoiceCard
AdventureCardWidgetEventBinder.Bind
AdventureBoardCardWidgetBinding
AdventureBoardUIFlow.ReplaceBoardWithEnter / ReplaceBoardAfterExit
right-side ChoiceRefresh exit/remove/place path
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

현재 최소 구현은 `AdventureChoiceCardWidget`, `AdventurePlayerCardWidget`, `AdventureMonsterCardWidget`, `AdventureDisplayCardWidget`처럼 고정 Widget 타입을 먼저 사용한다.

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

`CardDealer`는 제거한다.

`CardDealer`가 담당하던 카드 배치/딜링/애니메이션 조율은 `AdventureBoardUIFlow`로 이동한다.

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
-> AdventureScreenController
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
AdventurePlayerCardWidget UXML address 등록
AdventureMonsterCardWidget UXML address 등록
AdventureDisplayCardWidget UXML address 등록
Portrait/Locked face widget UXML address 등록
SkillSlotWidget UXML address 등록
Localization entries 수동 생성
AdventureChoiceCardUIAssetValidator 추가
```

Addressables address/label:

```text
AdventureSceneAddressables
-> AdventureSceneLoader
-> CodexAdventureChoiceCardUIAssetGenerator
-> AdventureChoiceCardUIAssetValidator
```

주소 문자열은 preload/generator/validator가 공유하는 데이터 계약이다.
따라서 각 클래스가 같은 문자열을 별도로 선언하지 않고 `AdventureSceneAddressables`를 참조한다.

결정:

```text
CodexAdventureChoiceCardUIAssetGenerator는 ChoiceCard slice에 필요한
table/model/localization/widget address를 같은 기준으로 생성 또는 갱신한다.
Localization entry 자동 생성은 generator에 포함되어 있지만,
새 테이블/언어 추가 자동화는 후속 개선으로 둔다.
```

범위:

```text
generator가 맞추는 것:
- AdventureChoiceCardUIModel row assets
- AdventureChoiceCardUITable rows
- ChoiceCard localization entries
- AdventureView / dynamic card widget / SkillSlotWidget UXML Addressables address

validator가 별도로 깨뜨리는 것:
- AdventureView.uxml tree/style include
- CardBoard.uxml / CardBoard.uss
- CardDeck.uxml / CardDeck.uss
- AdventureView.Animation.uss
- DefaultViewTheme / ViewTransition.uss
- AdventureTable / AdventureRegionModel
```

이 분리는 중요하다.
generator는 반복 생성 가능한 데이터와 Addressables wiring만 보정한다.
화면 구조, USS transition class, theme import는 설계 계약이므로 validator 실패 시 직접 수정해야 한다.

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
기존 AdventureGameToScreenEventBinder + AdventureWidgetToScreenEventBinder 분해
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
AdventureInitialPresentationViewModel
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
AdventureView.Animation.uss는 intro completion target인 .adventure-view--intro-shown .card-deck selector를 유지한다.
DefaultViewTheme는 ViewTransition.uss를 import해야 한다.
ViewTransition.uss는 ui-transition--hidden/from-bottom/enter와 fade-in/out class를 유지한다.
```

### Phase 4. ChoiceCard Dynamic CRUD Slice

목표:

```text
동적 카드 CRUD의 최소 vertical slice를 ChoiceCard로 검증한다.
```

작업:

```text
AdventureBoardCardWidgetBinding
AdventureCardWidgetFactory.CreateChoiceCard
AdventureCardWidgetEventBinder.Bind
AdventureBoardUIFlow.ReplaceBoardWithEnter
AdventureBoardUIFlow.ReplaceBoardAfterExit
AdventureChoiceRefreshUIFlow
```

완료 조건:

```text
ChoiceCard 생성/배치/이벤트/제거가 BoardUIFlow 중심으로 동작한다.
Factory는 생성만 한다.
Card event callback은 BoardUIFlow가 보드 binding list 변경 직후 EventBinder를 갱신하면서 해제/재등록한다.
Right side ChoiceRefresh가 exit -> remove -> place 순서로 표현된다.
```

현재 로딩 규칙:

```text
AdventureChoiceCardUIModels.Get(choiceType)
```

이유:

ChoiceCard UI rows는 AdventureSceneLoader에서 preload되고,
AdventureCardWidgetFactory는 preload된 lookup만 사용한다.

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
AdventureSkillSelectionResult
```

완료 조건:

```text
Pouch click은 Widget 내부 hide가 아니다.
CoinStatusWidget과 ResourceStatusBar coin text는 섞이지 않는다.
Card click은 View -> BoardUIFlow -> SkillUIFlow -> View -> Controller 흐름을 탄다.
PlayerTurnStarted는 Awaitable<bool> gate로, Screen lifetime이 끊긴 경우 GameFlow가 완료로 오해하지 않는다.
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
AdventureCombatResultFlow reward completion
CombatResultStarted
RewardStarted
Board.CardRemoveRequested
```

완료 조건:

```text
Enemy action은 EnemyTurn start presentation 완료 후 실행된다.
Enemy death는 right-side board runtime에서 제거된 뒤 CardRemoveRequested를 통해 퇴장 후 제거된다.
CardRemoveRequested 완료 후 남은 board runtime cards로 avatar/cue binding을 다시 구성한다.
CardRemoveRequested가 false를 반환하면 CombatResult/Reward로 진행하지 않는다.
최종 stage 완료처럼 right side를 비워야 하는 경우는 Board.SideClearRequested를 사용한다.
CombatResult는 Reward를 직접 건너뛰지 않는다.
Reward UI는 ChoiceRefresh를 직접 만들지 않는다.
Reward 결과를 받은 CombatResultFlow가 ChoiceRefresh를 요청한다.
```

### Phase 7. ChoiceRefresh / NextStage Completion

목표:

```text
Reward 완료 후 gameplay state 갱신과 UI refresh를 분리한다.
```

작업:

```text
AdventureCombatResultFlow.CompleteReward
AdventureChoiceRefreshViewModel
ChoiceRefreshStarted
AdventureChoiceRefreshUIFlow.Play
```

완료 조건:

```text
NextStage는 game state 변경이다.
ChoiceRefresh는 right-side board presentation이다.
ChoiceSelection input은 ChoiceRefresh 완료 후 열린다.
왼쪽 player card는 필요할 때만 교체된다.
Controller completion callback을 만들지 않는다.
```

구현 규칙:

```text
AdventureBoardUIFlow는 board replace 중 AdventureCardWidgetEventBinder를 먼저 Unbind한다.
새 card input은 exit -> remove/place -> enter가 완료된 뒤에만 Bind한다.
```

Screen detach 규칙:

```text
AdventureView.OnDetachedFromPanel은 screen lifetime을 끊고 gameplay cue/avatar binding을 즉시 해제한다.
Widget event binding, dynamic board card binding, screen UIFlow binding도 즉시 해제한다.
Scene scope Dispose까지 기다리지 않는다.
AdventureSceneEntryPoint.Dispose는 Navigator.HideCurrent를 먼저 호출해 detach cleanup을 유도한 뒤 event route binder를 해제한다.
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
AdventureView root UXML은 ViewManager template cache에 preload
AdventureDisplayCardWidget UXML도 Player/Monster와 동일하게 preload
Localization/Addressables validator 확장
```

완료 조건:

```text
runtime interaction 중 ChoiceCard UIModel을 WaitForCompletion으로 로드하지 않는다.
AdventureScene 진입 직후 AdventureView root UXML을 WaitForCompletion으로 처음 로드하지 않는다.
content wiring 오류는 editor validation에서 먼저 잡힌다.
AdventureSceneLoader는 새 preload 전과 RootScope dispose 시점에 미소비 payload의 Addressables handle을 release한다.
SceneManagerEx.Load async void boundary는 예외를 throw하지 않고 Unity 콘솔에 기록한다.
parallel preload/fade/scene-load 중 실패해도 allowSceneActivation=false 상태로 Unity async queue를 stall시키지 않는다.
```

### Phase 9. Combat Card CRUD Expansion

목표:

```text
ChoiceCard에서 검증한 dynamic card CRUD를 combat card로 확장한다.
```

작업:

```text
AdventurePlayerCardWidget
AdventureMonsterCardWidget
AdventureDisplayCardWidget
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
5. `DBManager.ChoiceCardUI.Get(...)` 과도기 사용은 제거한다.
6. ChoiceCard UIModel preload 전환은 구현되어 있다.

## Current Implementation Notes

Adventure UI transition 실행은 `ViewTransitionManager.Instance` 직접 호출을 사용하지 않는다.
VContainer가 생성한 `ViewTransitionManager`를 UIFlow/Factory가 주입받아 호출 시점에 전달한다.

현재 Adventure 화면 경로에서 적용된 규칙:

```text
Adventure UIFlow
-> ViewTransitionManager 주입
-> UXML에서 생성된 Widget/Banner에 호출 시점 전달
```

이유:

```text
Banner, Pouch, CoinStatusWidget, EndTurnWidget, SkillSlotGroup, HealthWidget은 UXML 또는 순수 UI 객체로 생성될 수 있다.
따라서 Widget 생성자에 VContainer 주입을 강제하기보다,
해당 Widget을 조율하는 UIFlow/Factory가 transition 실행 책임을 넘기는 방식이 현재 구조에 더 맞다.
```

현재 구현 규칙:

```text
Show(ViewTransitionManager)
Hide(ViewTransitionManager)
Present...(ViewTransitionManager)
ShowHealthAsync(ViewTransitionManager)
```

전역 `ViewTransitionManager.Instance`는 제거한다.

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
| Animation base rule exists | Complete | USS transition, C# class order, TransitionEndEvent 중심 완료 기준과 detach/cancel 수명 경계가 정의되어 있다. |
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
