# Adventure Card UI Redesign

## Purpose

이 문서는 Intent UI 구현 전에 Adventure 카드 UI 구조를 재정리하기 위한 설계 기록이다.

현재 `CombatCardWidget` 하나로 선택지 카드, 플레이어 카드, 몬스터 카드를 모두 감당하려 하면 IntentBadge, Health, CardFace의 존재 여부가 mode 분기로 섞인다. 큰 변경이 허용되므로 역할별 UXML/Widget으로 분리하는 방향을 우선 설계한다.

## Current Implementation Update: Board Card Deal Contract

이 절은 2026-06-29 기준 구현 상태를 문서 기준으로 고정한다.
기존 절에 남아 있는 `ReplaceBoardWithEnter(cards, canContinue)` 또는 placement 시 HealthBar 표시 설명보다 이 절의 내용이 우선한다.

### CardDeck UXML Contract

`CardDeck.uxml`은 단일 박스가 아니라 top-card를 가진 덱 구조를 제공한다.

```text
card-deck
├─ card-deck-shadow-card
├─ card-deck-middle-card
└─ card-deck-top-card
```

`card-deck-top-card`는 실제 덱 카드가 제거되는 객체가 아니다.
딜링 연출의 출발 좌표를 제공하는 시각/측정용 요소다.
덱은 카드가 날아간 뒤에도 그대로 남는다.

### ReplaceBoardWithEnter Signature

현재 `AdventureBoardUIFlow.ReplaceBoardWithEnter`는 card deck 위치를 알아야 한다.

```csharp
ReplaceBoardWithEnter(
    IReadOnlyList<AdventureBoardCardViewModel> cards,
    VisualElement cardDeck,
    Func<bool> canContinue)
```

이 API는 초기 Intro뿐 아니라 "enter 연출이 필요한 board replace"의 기본 경로다.
단, 현재 연출은 CardDeck top-card 기준이므로 CardDeck이 없는 화면에서는 별도 API를 사용하거나 cardDeck을 제공해야 한다.

### Deal Animation Target

보드 배치의 layout target과 animation target은 다르다.

```text
AdventureBoardCardPlacement.Anchor
-> 보드 최종 좌표
-> Health/Intent 예약 공간까지 포함한 slot container
-> 움직이지 않는다

AdventureBoardCardPlacement.Card 내부의 실제 card body
-> 딜링 animation target
-> top-card 위치에서 목적지까지 이동한다
```

현재 deal target lookup:

```text
Player/Monster/Display card
-> 내부 CardWidget

Choice card
-> 내부 .card-widget

Fallback
-> placement.Card root
```

이 규칙은 "HealthBar/Intent 예약 공간은 목적지에 고정하고 카드 본체만 딜링한다"는 설계 결정을 반영한다.

### HealthBar Timing

`AdventureCardWidgetFactory`는 더 이상 placement 시 `IAdventureHealthCardWidget.ShowHealthAsync`를 호출하지 않는다.

현재 순서:

```text
Board card 생성/배치
-> 카드 본체 deal animation
-> HealthBar는 숨김 유지
-> PlayerTurn start presentation
-> Turn banner와 HealthBar 동시 표시
```

HealthBar 표시 책임은 `AdventureBoardUIFlow.ShowHealthBars(viewTransitionManager)`에 있다.
이 API는 현재 보드 binding 중 `IAdventureHealthCardWidget`을 구현한 카드만 대상으로 한다.

## Current Decision

역할별 카드 위젯을 만든다.

```text
AdventureChoiceCardWidget
AdventurePlayerCardWidget
AdventureMonsterCardWidget
AdventureDisplayCardWidget
```

공통 레이아웃은 공통 USS class로 공유한다.

```text
AdventureCardFrame.uss
```

역할별 UXML은 별도로 작성한다.

```text
AdventureChoiceCardWidget.uxml
AdventurePlayerCardWidget.uxml
AdventureMonsterCardWidget.uxml
AdventureDisplayCardWidget.uxml
```

`AdventureDisplayCardWidget`은 별도 UXML을 가진 표시 전용 wrapper다.

따라서 `AdventureDisplayCardWidget`도 Player/Monster card widget과 동일하게 Addressables preload 대상이다.
`AdventureDisplayCardWidget.uss`에 정의된 display wrapper USS class가 validator 검증 대상이다.

현재 구현 규칙:

```text
AdventureChoiceCardViewModel
-> AdventureChoiceCardWidget

AdventureBoardCardViewModel + AdventureBoardSide.Left
-> AdventurePlayerCardWidget

AdventureBoardCardViewModel + AdventureBoardSide.Right
-> MonsterModel이면 AdventureMonsterCardWidget
-> 그 외 모델이면 AdventureDisplayCardWidget
```

근거:

```text
AdventurePresenter가 게임 상태를 BoardSide로 변환한다.
AdventureCardWidgetFactory는 BoardSide와 runtime model을 보고 구체 widget만 생성한다.
BoardLayout은 생성된 widget을 side area에 배치한다.
```

따라서 Factory는 보드 배치 규칙이나 게임 진행을 판단하지 않는다.
단, 현재 Adventure 규칙상 Left side는 플레이어 팀 카드, Right side는 선택지/상대 카드라는 전제를 따른다.

Right side의 모든 카드를 전투 카드로 취급하지 않는다.
`MonsterModel`만 health/intent/avatar를 가진 `AdventureMonsterCardWidget`으로 표현한다.
Event/Shop처럼 전투 상태가 없는 모델은 `AdventureDisplayCardWidget`으로 표현한다.

`AdventureDisplayCardWidget`은 입력 대상이 아니다.
Board binding에는 `IsInteractive` 경계를 두고, display-only binding은 card click/hover event binding에서 제외한다.
이렇게 해야 Event/Shop 표시 카드가 선택지 카드처럼 다시 클릭되거나 skill target hover 대상으로 들어오지 않는다.

카드가 입력 이벤트를 받는 것과 그 입력이 현재 게임 상태에서 유효한 것은 별개다.
`AdventureView.OnWidgetCardClicked`는 non-targeting 상태에서 `AdventureScreenController.CanClickCard(cardId)`를 먼저 확인한다.
현재는 ChoiceSelection 상태이고 해당 cardId가 stage offer로 등록된 경우에만 일반 카드 클릭을 실행한다.
skill targeting 중에는 `CanUseSkillOnTarget`이 target 유효성을 판단한다.

Intent 표시 규칙:

```text
AdventureMonsterCardWidget
-> IAdventureIntentCardWidget 구현

AdventurePlayerCardWidget
-> IAdventureIntentCardWidget 미구현

AdventureChoiceCardWidget
-> IAdventureIntentCardWidget 미구현
```

`AdventureIntentUIFlow`는 concrete monster widget을 직접 찾지 않고 `IAdventureIntentCardWidget`만 찾는다.
이렇게 해야 IntentBadge가 없는 카드가 실수로 intent presentation 경로에 들어오는 것을 구조적으로 막을 수 있다.

## Role Split

### AdventureChoiceCardWidget

선택지 카드 전용이다.

구성:

```text
- CardFace
```

없어야 하는 것:

```text
- IntentBadge
- Health
```

### AdventurePlayerCardWidget

플레이어 카드 전용이다.

구성:

```text
- HealthWidget
- CardFace
```

없어야 하는 것:

```text
- IntentBadge
```

### AdventureMonsterCardWidget

몬스터 카드 전용이다.

구성:

```text
- IntentBadgeWidget
- HealthWidget
- CardFace
```

IntentBadge는 몬스터 카드에만 존재한다.

## Common Layout

역할별 UXML은 같은 class 구조를 사용한다.

```text
adventure-card-frame
adventure-card-frame__top-slot
adventure-card-frame__status-slot
adventure-card-frame__face
adventure-card-frame__bottom-slot
```

예상 배치:

```text
TopSlot
StatusSlot
Face
BottomSlot
```

몬스터 카드:

```text
TopSlot    -> IntentBadgeWidget
StatusSlot -> HealthWidget
Face       -> CardFace
```

플레이어 카드:

```text
TopSlot    -> empty
StatusSlot -> HealthWidget
Face       -> CardFace
```

선택지 카드:

```text
TopSlot    -> empty
StatusSlot -> empty
Face       -> CardFace
```

## Card Face

기존 `CardWidget`은 장기적으로 `CardFaceWidget` 역할로 정리하는 것이 적절하다.

역할:

```text
- CardViewModel을 받아 카드 face를 렌더링
- front/back slot 관리
- portrait/choice/locked face 표시
- card flip 상태 관리
```

주의:

```text
기존 CardWidget rename은 변경량이 크다.
Intent 구현과 동시에 할지 별도 phase로 나눌지 구현 직전에 다시 결정한다.
```

## Intent Badge

`IntentBadgeWidget`은 `AdventureMonsterCardWidget`에만 포함한다.

V1 형태:

```text
- 작은 badge
- icon 중심
- number overlay
- HP 위
```

데이터:

```text
IntentItemViewModel
- Sprite Icon
- int NumberValue
- int CountValue
- bool HasNumber
- string NumberText
```

`IntentBadgeWidget`은 Addressables를 모른다. Sprite 로딩/변환은 Presentation 계층에서 끝낸다.

### Intent Icon Consistency

Intent icon은 의미 단위로 재사용한다.

```text
Intent_Attack
Intent_Defense
Intent_Buff
Intent_Debuff
Intent_Heal
```

몬스터별 공격 icon을 기본으로 만들지 않는다.

```text
권장:
- TestSlime attack -> Intent_Attack
- Boss attack -> Intent_Attack

비권장:
- Intent_TestSlimeAttack
- Intent_BossAttack
```

스타일 규칙:

```text
- 원본 캔버스 크기 통일: 128x128 또는 256x256
- 투명 배경
- 같은 시점과 같은 선 두께
- 같은 손그림/sticker 톤
- 작은 badge에서 읽히는 단순한 실루엣
- 숫자는 이미지에 포함하지 않음
- 숫자 overlay와 충돌하지 않도록 하단 또는 좌하단 여백 확보
```

색 역할:

```text
Attack  -> warm red / orange
Defense -> steel blue / gray
Heal    -> green
Buff    -> golden yellow
Debuff  -> purple
```

현재 `IntentDisplay_Attack`의 Sprite는 필수다. `Tools/Codex/Validate Intent Assets`와 런타임 `IntentItemViewModel` 모두 null icon을 허용하지 않는다.

## CardDealer Direction

> Current status:
> `CardDealer` is removed from the runtime path.
> Card widget creation is handled by `AdventureCardWidgetFactory`.
> Board placement and presentation orchestration are handled by `AdventureBoardUIFlow`.

카드 UI 생성 책임은 다음처럼 분리한다.

```text
AdventureCardWidgetFactory
-> 역할별 완성형 card widget 생성

AdventureBoardUIFlow
-> 생성된 card widget을 board layout에 배치
-> 입장/퇴장/교체 presentation 조율

AdventureCardWidgetEventBinder
-> 생성된 card widget의 클릭 이벤트를 scoped widget events에 연결
```

동적 카드 이벤트 연결 규칙:

```text
AdventureBoardUIFlow가 card widget의 생성/교체/제거 생명주기를 소유한다.
따라서 보드 binding list가 변경된 직후 AdventureCardWidgetEventBinder를 갱신한다.
AdventureView는 card event callback을 직접 등록/해제하지 않는다.
```

이유:

```text
카드 위젯은 퇴장 transition 이후 제거된다.
이때 pointer/click callback을 별도 위치에서 관리하면,
제거된 카드의 callback 참조가 남거나 새 카드에 callback이 붙지 않는 상태가 생길 수 있다.
보드 상태 변경 직후 전체 card event binding을 다시 구성하면,
현재 보드에 남아 있는 카드만 입력 이벤트를 가진다.
```

Board side 즉시 교체 순서:

```text
1. target area 조회
2. 새 카드 수량 검증
3. 새 card widget/binding 생성
4. 기존 side binding dispose
5. area replace
6. placement binding
```

이 순서를 지키는 이유:

```text
새 카드 생성이나 검증이 실패했는데 기존 side를 먼저 dispose하면,
데이터 오류 하나가 기존 화면 상태까지 깨뜨린다.
따라서 실패 가능성이 있는 준비 작업은 기존 보드 제거 전에 끝낸다.
```

Board side 퇴장 후 교체 순서:

```text
1. target area 조회
2. 새 카드 수량 검증
3. 새 card widget/binding 생성
4. 현재 board card input event unbind
5. 기존 side card anchor에 exit class 적용
6. TransitionEndEvent 대기
7. screen lifetime이 아직 유효한지 확인
8. 기존 side binding dispose
9. area replace with enter-pending card anchors
10. placement binding
11. 현재 board 기준으로 card input event bind
12. card anchor enter class 적용
13. TransitionEndEvent 대기
```

이 순서를 지키는 이유:

```text
사용자가 보던 카드가 아무 연출 없이 사라지면 ChoiceRefresh, 전투 종료, 카드 제거 흐름이 모두 갑작스럽게 보인다.
따라서 presentation이 필요한 제거는 exit transition 완료 후 실제 hierarchy 제거를 수행한다.
단, scene dispose나 hard reset은 즉시 제거 경로를 사용할 수 있다.
```

Exit 시작 전에는 현재 card input event를 먼저 끊는다.

```text
AdventureBoardUIFlow
-> AdventureCardWidgetEventBinder.Unbind()
-> exit transition
-> binding list 변경
-> AdventureCardWidgetEventBinder.Bind(current board bindings)
```

이유:

```text
exit 중인 choice card가 다시 클릭되면 같은 선택지가 중복 commit될 수 있다.
exit 중인 combat card가 hover/click target으로 남으면 skill targeting 상태와 실제 board 상태가 어긋날 수 있다.
따라서 card가 시각적으로 사라지는 시점부터는 입력 대상에서도 제외한다.
```

Board side 입장 순서:

```text
1. 새 card widget/binding 생성
2. anchor에 enter-pending class를 붙인 상태로 board area에 배치
3. 다음 frame까지 대기
4. enter-pending class 제거
5. enter class 추가
6. TransitionEndEvent 대기
7. enter class 제거
```

입장 중단 규칙:

```text
ReplaceBoardWithEnter(cards, canContinue)
-> 새 widget/binding을 board area에 붙이고 enter-pending 상태로 배치
-> enter 시작 전 canContinue가 false이면 해당 side를 즉시 clear
-> enter 완료 후 canContinue가 false이면 해당 side를 즉시 clear
```

이유:

```text
enter-pending 상태의 카드는 아직 사용자에게 확정적으로 보인 카드가 아니다.
이 상태에서 화면 수명이 끊기면 숨김 카드와 pointer callback이 남을 수 있다.
따라서 enter 흐름도 exit 흐름과 같이 "완료 또는 정리" 중 하나로 끝나야 한다.
```

이 순서를 지키는 이유:

```text
UI Toolkit transition은 이전 style 값과 다음 style 값의 차이가 있어야 재생된다.
새 카드를 붙인 즉시 visible 상태로만 만들면 "이전 값"이 없어서 입장 연출이 생기지 않는다.
따라서 먼저 숨김/이동 상태를 style에 반영하고, 다음 frame에 visible 상태로 바꾼다.
```

현재 pooling은 사용하지 않는다.

```text
Remove는 widget을 pool에 반환하지 않고 hierarchy에서 제거한다.
Binding dispose는 카드 단위 event/timeline/avatar 연결 해제를 끝낸다.
Pooling/reuse는 현재 구현 목표에서 제외한다.
제거된 카드 widget은 숨겨서 재사용하지 않는다.
화면 detach/dispose 중에는 남은 card input callback을 다시 열지 않는다.
```

입장 대기 중 퇴장 요청 규칙:

```text
anchor가 enter-pending 상태이면 아직 사용자에게 표시되지 않은 카드다.
이 상태에서 exit transition을 다시 걸지 않는다.
enter-pending class를 제거하고 다음 frame에 완료된 것으로 본다.
```

이유:

```text
enter-pending과 exit는 둘 다 숨김/이동 상태다.
같은 style 상태에서 exit class를 추가하면 UI Toolkit이 transition으로 볼 값 차이가 없을 수 있다.
그러면 TransitionEndEvent를 기다리는 코드가 끝나지 않을 수 있다.
```

Exit 대기 중 화면이 닫히면 새 카드를 붙이지 않는다.

```text
ReplaceBoardAfterExit(cards, canContinue)
-> 새 widget/binding은 미리 생성할 수 있다.
-> exit transition 대기
-> canContinue가 false이면 새 widget/binding을 dispose
-> 이미 exit가 끝난 기존 side도 binding/hierarchy에서 제거
-> 새 카드는 화면 hierarchy에 붙이지 않음
```

이유:

```text
Scene 종료와 board refresh가 겹치면 TransitionEndEvent 이후에 await가 재개될 수 있다.
이때 screen lifetime 확인 없이 새 카드를 area에 붙이면 닫힌 화면에 UI 객체가 다시 추가된다.
따라서 "퇴장 후 제거"는 exit 완료뿐 아니라 screen lifetime gate까지 포함한다.
단, exit가 이미 끝난 side는 rollback하지 않는다. 제거까지 마친 뒤 false를 반환한다.
```

개별 카드 제거 순서:

```text
1. GameFlow가 board runtime state에서 제거 대상 card id를 제거
2. Presenter가 제거 후 board runtime cards를 포함한 AdventureBoardCardRemoveViewModel 생성
3. AdventureBoardEvents.CardRemoveRequested 발행
4. AdventureView가 기존 gameplay cue/avatar binding 해제
5. BoardUIFlow가 card id로 현재 board binding 조회
6. card anchor에 exit class 적용
7. TransitionEndEvent 대기
8. 같은 binding이 아직 board list에 남아 있는지 다시 확인
9. binding list에서 제거
10. binding dispose
11. slot hierarchy 제거
12. 같은 side에 남은 card slot을 현재 card count 기준으로 재배치
13. screen lifetime이 아직 유효하면 남은 card input event 재바인딩
14. AdventureView가 남은 board runtime cards로 gameplay cue/avatar binding 재구성
```

단일 카드 제거 중단 규칙:

```text
RemoveCardAfterExit(cardId, canContinue)
-> exit transition 완료 후 제거 대상 binding/hierarchy는 제거한다.
-> 제거 후 canContinue가 false이면 남은 card input event를 다시 bind하지 않는다.
-> 호출자에게 false를 반환한다.
```

이유:

```text
card exit가 끝났다면 사용자는 이미 해당 카드가 사라지는 것을 보았다.
따라서 화면 수명이 끊겼다는 이유로 제거를 rollback하지 않는다.
대신 닫힌 화면 또는 detach된 화면의 남은 카드에 pointer callback을 다시 붙이지 않는다.
```

단일 카드 제거 후 재배치 규칙:

```text
제거된 카드의 slot은 hierarchy에서 제거한다.
같은 side에 남은 card placement는 현재 card count 기준으로 다시 계산한다.
남은 card widget은 재생성하지 않고 기존 anchor/slot의 layout class와 index만 갱신한다.
```

이유:

```text
세 장 중 한 장만 제거했는데 남은 카드가 기존 3-card offset을 유지하면 화면상 빈자리가 남는다.
하지만 남은 카드를 재생성하면 avatar/timeline/event binding이 불필요하게 끊긴다.
따라서 Remove는 제거 대상만 exit/dispose하고, 남은 placement는 slot layout만 갱신한다.
```

Stage advance/choice refresh 수명 순서:

```text
1. AdventureStageAdvanceFlow가 기존 stage card id를 TakeStageBoardCardIds로 board state에서 먼저 분리한다.
2. 새 stage를 시작해 새 right-side card id를 board state에 배치한다.
3. Presenter가 현재 board state 기준으로 새 board presentation을 만든다.
4. UI는 기존 visual binding을 기준으로 exit를 재생한 뒤 새 presentation으로 교체한다.
5. exit/refresh 완료 후 AdventureCards에서 이전 card instance를 제거한다.
```

이유:

```text
사용자가 보는 기존 card widget은 UI binding list에 남아 있으므로 exit animation을 재생할 수 있다.
반면 Presenter는 board state만 읽기 때문에 이미 board에서 빠진 이전 stage card를 새 presentation에 포함하지 않는다.
따라서 "퇴장 후 제거" 정책을 유지하면서도 새 presentation의 runtime card binding은 현재 board 상태와 맞는다.

여기서 "퇴장 후 제거"는 card widget 제거와 AdventureCards registry 제거를 뜻한다.
Board state는 다음 presentation 계산을 위해 먼저 분리될 수 있다.
이 둘을 같은 제거로 취급하면 UI exit 대상과 새 board presentation 대상이 섞인다.
```

## Board Refresh Gate

ChoiceCard 선택 후 board refresh는 완료를 기다리는 요청이다.

```text
AdventureEncounterStartFlow.StartSelectedChoice
-> AdventureChoiceFlow.CommitSelection
-> AdventureEncounterFlow.StartEncounter
-> AdventureBoardEvents.RefreshRequested
-> AdventureView.OnGameBoardRefreshRequested
-> AdventureBoardUIFlow.ReplaceBoardAfterExit
-> refresh 완료 후 PlayerTurn 시작
```

`AdventureBoardEvents.RefreshRequested`는 `Func<AdventureBoardPresentationViewModel, Awaitable<bool>>`이다.

이유:

```text
보드 전환이 끝나기 전에 PlayerTurnStarted가 시작되면,
choice card 퇴장, monster card 표시, turn banner, intent reveal이 서로 겹칠 수 있다.
따라서 선택지 클릭 후 전투 시작은 board refresh presentation 완료를 기다린 뒤 다음 흐름으로 넘어간다.
```

이 이벤트는 multicast event가 아니라 단일 screen request로 사용한다.

입력 재개 규칙:

```text
BoardUIFlow는 exit/enter animation 중 card input callback을 열지 않는다.
새 binding list는 enter animation과 screen lifetime 확인이 끝난 뒤 AdventureCardWidgetEventBinder에 다시 연결한다.
```

```text
AdventureGameToScreenEventBinder
-> _events.Board.RefreshRequested = _view.OnGameBoardRefreshRequested
```

보드 refresh를 여러 구독자가 동시에 처리하면 완료 기준이 모호해지기 때문이다.

## Validator Coverage

`AdventureChoiceCardUIAssetValidator`는 Adventure UI 계약을 검증한다.

현재 포함:

```text
AdventureView.uxml 필수 screen/widget 요소
AdventureView.uxml 필수 dynamic card/card board/card deck/health/skill USS import
UXML Style reference의 GUID가 실제 USS meta GUID와 일치
CardBoard.uxml 필수 board area/pouch 요소
CardBoard.uss 필수 transition class
CardDeck.uxml 필수 deck/top-card 요소
CardDeck.uss 필수 deck/top-card class
CardDeck.Animation.uss 필수 hidden/enter class
CardWidget.uss 필수 card-v2 class
ChoiceCard widget UXML 필수 USS import
ChoiceCard UIModel UssClassName 실제 USS class 존재
Player/Monster/Display card widget UXML 필수 USS import
Player/Monster card widget USS 필수 health/intent 위치 class
HealthWidget.uss 필수 health bar class
Player/Monster card widget UXML health widget 초기 transition class
CardDeck.uss `.card-deck__card--top` translate transition
CardBoard.uss deal-enter/deal-settle/deal-settle-release transition contract
Monster card intent badge transition contract
Adventure card widget UXML Addressables address/label
ChoiceCard UI table row/localization/icon/uss class
```

CardBoard.uss 필수 class:

```text
.card-board__area--refresh-hidden
.card-board__card-anchor
.card-board__card-anchor--enter-pending
.card-board__card-anchor--enter
.card-board__card-anchor--exit
.card-board__card--deal-enter
.card-board__card--deal-settle
.card-board__card--deal-settle-release
```

CardBoard.uss 필수 deal transition:

```text
.card-board__card--deal-enter
  transition-property: opacity, scale, translate
  transition-duration: var(--motion-duration-long-700ms)

.card-board__card--deal-settle
  transition-property: scale, translate
  transition-duration: var(--motion-duration-instant-80ms)

.card-board__card--deal-settle-release
  transition-property: scale, translate
  transition-duration: var(--motion-duration-instant-80ms)
```

CardDeck.uxml 필수 요소:

```text
card-deck
card-deck-shadow-card
card-deck-middle-card
card-deck-top-card
```

CardDeck.uss 필수 class:

```text
.card-deck
.card-deck__card
.card-deck__card--shadow
.card-deck__card--middle
.card-deck__card--top
```

CardDeck.Animation.uss 필수 class:

```text
.card-deck--hidden
.card-deck--enter
```

AdventureDisplayCardWidget.uss display-only card 필수 class:

```text
.adventure-display-card
.adventure-display-card__card
```

Choice/Health/Intent 표시 검증:

```text
AdventureChoiceCardWidget.uxml
-> AdventureChoiceCardWidget.uss include

AdventureChoiceCardUIModel
-> UssClassName 값이 AdventureChoiceCardWidget.uss에 실제 class로 존재

AdventurePlayerCardWidget.uxml
-> CardWidget.uss include
-> AdventurePlayerCardWidget.uss include
-> HealthWidget.uss include

AdventureMonsterCardWidget.uxml
-> CardWidget.uss include
-> AdventureMonsterCardWidget.uss include
-> HealthWidget.uss include

AdventurePlayerCardWidget.uss
-> .adventure-player-card__health-widget
-> .adventure-player-card__card

AdventureMonsterCardWidget.uss
-> .adventure-monster-card__intent-badge
-> .adventure-monster-card__health-widget
-> .adventure-monster-card__card
-> .intent-badge--hidden / .intent-badge--revealed / .intent-badge--triggered

HealthWidget.uss
-> .health-widget
-> .health-widget__fill-clip
-> .health-widget__fill
-> .health-widget__text
```

이유:

```text
AdventureBoardLayout과 AdventureBoardUIFlow는 USS class 이름을 문자열 계약으로 사용한다.
USS class가 삭제되거나 이름이 바뀌면 C# 빌드는 통과하지만 enter/exit transition은 작동하지 않는다.
HealthBar/IntentBadge도 UXML element만 존재하면 C# 검증은 통과할 수 있다.
하지만 USS import나 위치 class가 빠지면 화면에서는 보이지 않거나 카드 본체와 겹친다.
따라서 코드 컴파일과 별도로 UI asset validator가 USS 계약을 검증해야 한다.
```

Generator scope:

```text
CodexAdventureChoiceCardUIAssetGenerator
-> AdventureChoiceCardUITable / row assets 생성
-> AdventureView root UXML Addressables 등록
-> dynamic card widget UXML Addressables 등록
-> SkillSlotWidget UXML Addressables 등록
```

이유:

```text
AdventureSceneLoader는 card widget뿐 아니라 SkillSlotWidget도 scene preload 단계에서 Addressables로 로드한다.
AdventureView root UXML도 ViewManager template cache에 preload되므로 Addressables wiring 계약에 포함된다.
validator가 AdventureView/SkillSlotWidget address를 요구한다면 generator도 같은 address/label을 구성해야 한다.
```

Addressables address/label 문자열은 `AdventureSceneAddressables`를 단일 출처로 둔다.
`AdventureSceneLoader`, `CodexAdventureChoiceCardUIAssetGenerator`, `AdventureChoiceCardUIAssetValidator`가 같은 상수를 참조해야 한다.

이유:

```text
preload, generation, validation이 서로 다른 문자열을 가지면 C# compile은 통과해도
실제 Addressables wiring은 한쪽만 맞고 다른 한쪽은 깨질 수 있다.
주소 문자열은 데이터 계약이므로 한 곳에서만 변경되어야 한다.
```

단, generator가 Adventure UI 전체 계약을 생성하는 것은 아니다.

```text
generator가 자동 보정하는 범위:
- ChoiceCard UI row assets
- ChoiceCard localization entries
- ChoiceCard table row references
- preload 대상 UXML Addressables address/label

validator가 별도로 검증하는 정적 자산 계약:
- AdventureView.uxml 내부 필수 element
- AdventureView.uxml의 USS include
- CardBoard.uxml / CardBoard.uss
- CardDeck.uxml / CardDeck.uss
- AdventureView.Animation.uss
- DefaultViewTheme / ViewTransition.uss
- AdventureTable / AdventureRegionModel
```

즉, ChoiceCard row나 UXML Addressables wiring이 깨졌다면 generator를 다시 실행할 수 있다.
하지만 CardDeck top-card가 없거나 CardBoard transition class가 사라진 문제는 generator가 고치지 않는다.
그 경우는 화면 구조 또는 USS 계약을 직접 수정해야 한다.

## Board Refresh Diff Policy

`ReplaceBoardAfterExit`는 보드 전체 refresh 요청을 처리하지만, 항상 양쪽 side를 전부 제거하고 재생성하지 않는다.

현재 규칙:

```text
1. incoming cards를 Left / Right로 분리한다.
2. 각 side의 현재 binding list와 incoming view model list를 순서대로 비교한다.
3. 같은 side, 같은 runtime type, 같은 card id이면 동일한 카드 presentation으로 본다.
4. ChoiceCard는 choice type까지 비교한다.
5. 동일한 side는 exit/recreate를 하지 않고 현재 placement를 유지한다.
6. 달라진 side만 exit transition 후 제거하고 새 widget으로 교체한다.
7. 동일한 side는 `Changed...Placements` 결과에 포함하지 않는다.
```

동일성 기준의 제약:

```text
같은 CardId를 유지한 채 카드의 종류나 큰 외형을 바꾸지 않는다.
같은 CardId의 health, intent, lock 상태 같은 값 변경은 card update 경로로 처리한다.
다른 카드로 보이게 해야 한다면 GameFlow/Presenter는 다른 CardId 또는 다른 ViewModel 타입을 전달해야 한다.
```

퇴장/교체 순서:

```text
1. 변경이 필요한 side만 미리 계산한다.
2. 변경 side의 새 widget/binding을 hierarchy에 붙이기 전에 생성한다.
3. 변경 side들의 기존 card exit transition을 먼저 시작한다.
4. 모든 변경 side의 exit가 끝난 뒤 기존 binding/hierarchy를 제거한다.
5. 새 widget을 board area에 붙이고 enter-pending 상태로 배치한다.
6. 새 widget들의 enter transition을 실행한다.
```

생성/교체 실패 정리 규칙:

```text
새 widget/binding은 board refresh가 끝나기 전까지 임시 소유 상태다.
카드 생성 중 예외가 발생하면 이미 생성된 임시 binding을 dispose한다.
좌/우 side replacement 준비 중 한쪽 생성이 실패하면 먼저 생성된 다른 side의 임시 binding도 dispose한다.
board binding list로 최종 소유권을 넘기는 시점은 배치와 enter transition이 성공한 뒤다.
전체 board refresh와 단일 side 교체 모두 같은 소유권 규칙을 따른다.
```

screen lifetime gate 규칙:

```text
canContinue가 false가 된 뒤에는 card input event를 다시 bind하지 않는다.
이미 exit가 끝난 side 또는 enter 중이던 side는 hierarchy/binding에서 제거한다.
남은 side가 있더라도 닫힌 화면에 pointer callback을 다시 붙이지 않는다.
```

이유:

```text
canContinue는 현재 AdventureView screen lifetime을 의미한다.
false 이후에 RefreshCardEventBindings를 호출하면 detach/dispose 중인 화면의 VisualElement에 callback을 다시 붙일 수 있다.
따라서 false 분기는 "정리만 수행"하고 정상 완료 분기만 event binding을 복구한다.
```

이유:

```text
ReplaceBoardAfterExit는 보드 전체 refresh API다.
양쪽 side가 모두 바뀌는 경우 한쪽의 exit/enter가 끝난 뒤 다른 쪽이 exit를 시작하면
같은 refresh 안에서도 side마다 전환 타이밍이 달라진다.
따라서 변경 side들은 먼저 함께 퇴장하고, 그 다음 제거/교체/입장한다.
```

이유:

```text
Adventure에서 player card는 scene 시작 후 왼쪽 board에 고정된다.
ChoiceCard 선택 후 monster card로 전환되거나 전투 종료 후 choice cards가 다시 표시될 때,
오른쪽 side만 바뀌어야 하는데 전체 board를 교체하면 왼쪽 player card도 불필요하게 퇴장/재생성된다.
```

따라서 `ReplaceBoardAfterExit`는 "전체 입력을 받는 API"일 뿐이고, 실제 presentation은 diff 결과에 따라 최소 side만 교체한다.

## Board Card Id Policy

보드에서 클릭되는 카드 id는 UI가 새로 만들지 않는다.

```text
AdventureBoardCardViewModel.CardId
-> 일반 플레이어/몬스터 카드 클릭 id

AdventureChoiceCardViewModel.OfferCardId
-> 선택지 카드 클릭 id
```

근거:

```text
GameFlow는 offer 선택, 전투 대상 선택, skill target 검증을 runtime id 기준으로 처리한다.
UIFlow가 별도 임시 id를 만들면 같은 카드에 대해 UI id와 runtime id가 갈라진다.
그러면 클릭 이벤트를 다시 매핑해야 하고, 선택지 카드/전투 카드 전환 시 실수할 가능성이 커진다.
```

따라서 `AdventureBoardUIFlow`와 `AdventureCardWidgetFactory`는 board card id를 생성하지 않는다.
id는 ViewModel에 이미 들어온 값을 그대로 사용한다.

## Board Card Lookup Policy

`AdventureBoardUIFlow`는 현재 화면에 붙어 있는 card binding을 조회할 수 있다.

```text
TryGetBinding(cardId)
TryGetBinding(side, cardId)
TryGetCardWidget<T>(side, cardId)
```

중요한 제한:

```text
이 API는 별도 card state 저장소가 아니다.
Dictionary나 registry를 새로 만들지 않는다.
현재 side binding list를 선형 탐색한다.
```

이유:

```text
GameFlow/Runtime이 카드 상태의 원본이다.
UIFlow가 별도 Dictionary를 가지면 runtime card state와 screen card state가 이중화된다.
대신 Intent 표시, Damage cue receiver, Avatar binding처럼 "현재 화면에 붙은 widget을 찾는" 요구만 BoardUIFlow 조회 API로 모은다.
```

현재 사용 위치:

```text
AdventureIntentUIFlow
-> TryGetCardWidget<IAdventureIntentCardWidget>(Right, cardId)

AdventureGameplayCueAvatarBinder
-> TryGetCardWidget<IAdventureDamageCueReceiver>(side, cardId)

AdventureView skill targeting
-> TryGetCardId(hoveredCard)
-> AdventureView does not access AdventureCardWidgetEventBinder directly
```

## Card Update Policy

카드 update는 카드 교체가 아니다.

```text
Health 변경
-> AdventureBoardUIFlow.TrySetCardHealth(cardId, current, max)
-> IAdventureHealthCardWidget.SetHealth(current, max)
-> HealthWidget 표시 갱신
```

체력 widget 표시 애니메이션은 card widget이 전역 manager를 직접 찾지 않는다.

```text
AdventureCardWidgetFactory
-> ViewTransitionManager 주입
-> IAdventureHealthCardWidget.ShowHealthAsync(viewTransitionManager)
-> HealthWidget.Show(viewTransitionManager)
```

이유:

```text
Player/Monster card widget은 UXML template에서 생성된다.
따라서 widget 생성자에 VContainer 주입을 강제하지 않고,
생성/배치 흐름을 알고 있는 Factory가 transition 실행 의존성을 명시적으로 넘긴다.
```

현재 `HealthWidget`은 `AbilitySystemComponent`의 `VitalAttributeSet`을 구독하므로 기본 체력 변경은 자동으로 반영된다.
`TrySetCardHealth`는 향후 GameFlow/Presenter가 명시적인 current/max health update를 보낼 때 사용하는 경계이다.

Damage cue와 Health update는 분리한다.

```text
DamageCueData.Amount
-> "몇 데미지를 연출할지"만 의미한다.

Health current/max
-> "최종 체력을 얼마로 표시할지"를 의미한다.
```

따라서 `DamageCueData`만으로 Health를 추정 갱신하지 않는다.
최종 health 값은 Runtime/AbilitySystem이 원본이다.

## Widget Creation Policy

역할별 카드 위젯은 `VisualTreeAsset.Instantiate()` 방식으로 생성한다.

카드 생성은 "완성형 카드 widget 하나를 만들어 board에 배치"하는 흐름이므로 `CloneTree(parent)`보다 `Instantiate()`가 더 적합하다.

확정된 생성 절차:

```text
1. Addressables에서 UXML VisualTreeAsset 로드
2. VisualTreeAsset.Instantiate() 호출
3. 반환된 TemplateContainer에서 root widget을 Q로 찾기
4. root widget을 RemoveFromHierarchy()로 TemplateContainer에서 분리
5. 분리된 root widget 반환
```

예상 코드:

```csharp
public static AdventureMonsterCardWidget Create()
{
    TemplateContainer container = LoadTemplate().Instantiate();

    AdventureMonsterCardWidget widget =
        container.Q<AdventureMonsterCardWidget>("adventure-monster-card");

    widget.RemoveFromHierarchy();
    return widget;
}
```

`CloneTree(parent)` 사용 경계:

## Adventure Card Template Preload Policy

Adventure board에서 동적으로 생성되는 카드 위젯은 런타임 생성 시점에 Addressables `WaitForCompletion()`을 호출하지 않는다.

현재 AdventureScene preload 대상:

```text
AdventureChoiceCardWidget
AdventurePlayerCardWidget
AdventureMonsterCardWidget
PortraitCardFaceWidget
LockedCardFaceWidget
SkillSlotWidget
AdventureChoiceCardUIModel rows
```

로드 흐름:

```text
AdventureSceneLoader.Preload
-> Addressables.LoadAssetAsync<VisualTreeAsset>(...)
-> AdventureChoiceCardUITable.LoadAsync(EChoiceCardType)
-> AdventureChoiceCardUIModels 생성
-> CardFaceWidgetTemplates 생성
-> AdventureCardWidgetTemplates 생성
-> AdventureScreenWidgetTemplates 생성
-> AdventureScenePayload로 전달
-> AdventureSceneScope에서 RegisterInstance
-> AdventureCardWidgetFactory 생성자 주입
-> AdventureScreenWidgetBinder 생성자 주입
-> Adventure card widget 생성 시 preloaded template 사용
-> Adventure skill slot 생성 시 preloaded template 사용
```

이유:

```text
ChoiceCard / PlayerCard / MonsterCard는 Adventure board refresh, choice selection, combat transition 중 생성된다.
이 시점에 WaitForCompletion()이 발생하면 UI transition 중 main thread stall이 발생할 수 있다.
SkillSlotWidget은 initial presentation 준비 중 생성되므로, intro 직전에 동기 로드가 걸릴 수 있다.
ChoiceCard UIModel은 choice card 생성 중 필요하므로 board refresh 중 동기 로드가 걸릴 수 있다.
따라서 scene activation 전에 필요한 UXML template과 ChoiceCard UI rows를 명시적으로 preload하고,
화면 표시 중에는 이미 로드된 VisualTreeAsset과 UIModel만 사용한다.
```

스타일 적용 규칙:

```text
동적 card widget UXML은 TemplateContainer를 통해 만들어진 뒤 root widget만 분리될 수 있다.
따라서 AdventureView.uxml은 Adventure board에서 사용하는 동적 card widget USS를 명시적으로 포함한다.

AdventureChoiceCardWidget.uss
AdventurePlayerCardWidget.uss
AdventureMonsterCardWidget.uss
```

이유:

```text
UI Toolkit의 VisualTreeAsset.Instantiate/CloneTree는 TemplateContainer를 루트로 만든다.
TemplateContainer 내부 root widget만 떼어 쓰는 생성 방식에서는 UXML 내부 Style 의존이 불안정하다.
Screen이 사용하는 동적 widget USS를 Screen UXML에 포함하면 스타일 적용 경계가 명확해진다.
```

주의:

CardWidget.Bind(viewModel) legacy 경로는 제거한다.
Card face 생성은 반드시 CardWidget.Bind(viewModel, CardFaceWidgetTemplates)를 사용한다.
CharacterSelectView도 TitleSceneNavigator가 CharacterSelect를 표시하기 전에
TitleSceneCardFaceTemplateLoader.Load()로 CardFaceWidgetTemplates를 준비한 뒤 전달한다.

CardViewModel은 현재 표시할 face의 ViewModel을 반드시 가져야 한다.

```text
Face == Front -> Front face view model required
Face == Back  -> Back face view model required
```

반대쪽 face는 없는 카드가 있을 수 있으므로 null을 허용한다.
하지만 현재 표시할 face가 null이면 데이터 오류로 보고 즉시 예외를 낸다.
이렇게 해야 카드가 조용히 빈 슬롯으로 표시되는 fallback 경로가 생기지 않는다.

AdventureCardWidgetTemplates는 Choice/Player/Monster 카드 root UXML만 직접 소유한다.
Portrait/Locked face UXML은 CardFaceWidgetTemplates로 분리한다.

이유:

PortraitCardFaceWidget / LockedCardFaceWidget은 Adventure 전용이 아니다.
CharacterSelectView도 같은 face widget을 사용한다.
따라서 face template을 AdventureCardWidgetTemplates에 직접 넣으면
"공용 카드 face"가 "Adventure 카드 껍데기"에 종속된 것처럼 보인다.
또한 no-template fallback을 허용하면 어떤 화면은 preload template을 쓰고
어떤 화면은 WaitForCompletion으로 직접 로드하는 이중 경로가 생긴다.
따라서 no-template 생성 경로를 제거한다.

SkillSlotWidget의 no-template 생성 경로는 제거한다.
Adventure 경로는 AdventureSkillSlotGroup.Bind(..., AdventureScreenWidgetTemplates)를 통해 preloaded template을 전달한다.
CharacterSelect 경로는 TitleSceneSkillSlotTemplateLoader.Load()로 준비한 template을
CharacterSelectSkillSlotGroup.Bind(..., template)에 전달한다.
SkillSlotWidget의 fallback label 경로는 제거한다. icon 누락은 데이터 오류로 처리한다.

AdventureChoiceCardWidget도 같은 규칙을 따른다.
Choice card UI model의 icon, USS class, display name은 필수 데이터다.
Validator가 먼저 잡아야 하지만, 런타임 Bind에서도 null icon이나 빈 USS class를 fallback으로 넘기지 않고 즉시 예외를 낸다.

검증 규칙:

```text
AdventureChoiceCardUIAssetValidator는 AdventureScene preload 대상 widget UXML이
Addressables에 등록되어 있고 AdventureScene label을 가지는지 확인한다.

또한 같은 Addressables address가 여러 group에 중복 등록되어 있으면 실패한다.
중복 address는 Addressables.LoadAssetAsync(address)의 대상이 모호해질 수 있기 때문이다.

검증기는 각 UXML을 instantiate해서 필수 root/child element도 확인한다.
예를 들어 AdventureMonsterCardWidget.uxml에는 다음 요소가 있어야 한다.

- AdventureMonsterCardWidget `adventure-monster-card`
- IntentBadgeWidget `adventure-monster-card-intent-badge`
- HealthWidget `adventure-monster-card-health-widget`
- CardWidget `adventure-monster-card-widget`

이 검증은 C# compile만으로 잡히지 않는 UXML name/type drift를 Play 전에 발견하기 위한 것이다.

검증기는 AdventureView.uxml이 동적 card widget USS를 포함하는지도 확인한다.
각 USS가 Unity `StyleSheet` asset으로 import 가능한지도 함께 확인한다.

- AdventureChoiceCardWidget.uss
- AdventurePlayerCardWidget.uss
- AdventureMonsterCardWidget.uss
```

검증기는 screen-level UXML 계약도 확인한다.

```text
AdventureView.uxml
-> AdventureScreenWidgets가 찾는 name/type

CardBoard.uxml
-> AdventureBoardWidgets가 찾는 name/type
```

예:

```text
AdventureView.uxml
- VisualElement `adventure-root`
- Banner `banner`
- ResourceStatusBar `resource-status-bar`
- CoinStatusWidget `coin-status-widget`
- EndTurnWidget `end-turn-widget`
- AdventureSkillSlotGroup `skill-slot-group`

CardBoard.uxml
- VisualElement `card-board`
- VisualElement `card-board-left-area`
- VisualElement `card-board-right-area`
- Pouch `pouch`
```

이유:

```text
AdventureScreenWidgets와 AdventureBoardWidgets는 런타임에 Q<T>(name)으로 요소를 찾는다.
C# compile은 UXML name 변경이나 template 내부 타입 변경을 잡지 못한다.
따라서 Play 전에 validator가 화면 구조 계약을 먼저 깨뜨린다.
```

검증기는 `SkillSlotWidget.uxml`도 preload 대상 widget으로 확인한다.

```text
SkillSlotWidget
-> Addressables address `SkillSlotWidget`
-> AdventureScene label
-> AdventureView.uxml includes SkillSlot.uss
-> AdventureView.uxml includes SkillSlotWidget.uss
-> `.skill-slot`
-> `.skill-slot-button`
-> `skill-slot-frame`
-> `skill-slot-icon`
```

이유:

```text
AdventureSkillSlotGroup은 초기 presentation 준비 중 SkillSlotWidget을 동적으로 생성한다.
이 UXML이 Addressables에서 빠지거나 내부 element name이 바뀌면 intro 직전 UI 준비가 실패한다.
USS class가 누락되면 C# compile은 통과하지만 icon 표시, disabled 상태, slot frame 표시가 깨진다.
```

```text
사용하지 않는 곳:
- AdventureCardWidgetFactory가 카드 widget 하나를 생성해 반환하는 흐름

사용 가능한 곳:
- 이미 존재하는 parent slot에 UXML 내용을 직접 붙이는 흐름
```

주의:

```text
Instantiate()는 TemplateContainer를 생성한다.
따라서 factory가 실제로 반환할 root widget은 TemplateContainer 안에서 Q로 찾아 분리한다.
```

장점:

```text
- 선택지 카드에 IntentBadge가 생길 여지가 없다.
- 플레이어 카드에 IntentBadge가 생길 여지가 없다.
- 몬스터 카드에만 IntentBadge가 존재한다.
- UI 구조가 UXML에서 직접 보인다.
- 런타임 조립 코드가 줄어든다.
```

리스크:

```text
- 파일과 타입이 늘어난다.
- 기존 CombatCardWidget / CardWidget 참조 정리가 필요하다.
- AbilitySystem avatar binding 위치를 다시 정해야 한다.
- 나중에 Pooling/reuse를 도입하면 역할별 unbind 규칙을 다시 설계해야 한다.
```

## Implementation Phases

추천 단계:

```text
Phase A: Card UI 재설계
- 공통 AdventureCardFrame.uss 작성
- 역할별 Adventure*CardWidget UXML/CS 작성
- 기존 CardWidget/CardFace 역할 정리
- AdventureCardWidgetFactory가 역할별 widget을 생성하도록 변경

Phase B: Intent Badge 추가
- IntentBadgeWidget 작성
- AdventureMonsterCardWidget에만 포함
- IntentItemViewModel 연결

Phase C: Intent 계약 연결
- IntentRuntime / ResolvedIntentActionData
- Intent reveal sequence
- EnemyTurn execution contract
```

## Confirmed Creation Method

역할별 카드 위젯 생성은 다음 규칙을 따른다.

```text
AdventureChoiceCardWidget(preloaded choice template)
AdventurePlayerCardWidget.Create(preloaded player template)
AdventureMonsterCardWidget.Create(preloaded monster template)
AdventureDisplayCardWidget.Create(preloaded display template)
```

Player/Monster card widget의 `Create(template)`는 `VisualTreeAsset.Instantiate()`를 사용한다.
Display card widget도 같은 `Create(template)` 경로를 따른다.
Choice card widget은 `BaseCardWidget` 생성자 경로에서 preloaded template을 받는다.

```text
1. AdventureSceneLoader가 VisualTreeAsset을 preload한다.
2. AdventureCardWidgetFactory가 AdventureCardWidgetTemplates를 주입받는다.
3. AdventureCardWidgetTemplates.FaceTemplates를 CardWidget에 전달한다.
4. Factory가 역할별 widget 생성자 또는 Create(template)에 template을 전달한다.
5. TemplateContainer에서 root widget Q
6. root widget RemoveFromHierarchy()
7. root widget 반환
```

`CloneTree(parent)`는 카드 위젯 하나를 생성해서 반환하는 흐름에서는 사용하지 않는다.

Card deal intro rule:

```text
CardDeck root는 이동하지 않는다.
보드에 실제 배치될 card widget이 CardDeck top-card 위치에서 출발한다.
CardDeck top-card는 짧은 translate 피드백만 가진다.
```

이유:

```text
덱 전체가 움직이면 "카드를 딜링한다"가 아니라 "덱이 이동한다"로 보인다.
연출의 주체는 새로 배치되는 실제 카드이고, 덱은 출발 anchor로 남아야 한다.
```

## Current Implementation Decisions

현재 구현 기준:

```text
1. 역할별 카드 위젯을 명시적인 Create* 메서드로 생성한다.
2. AdventureCardWidgetFactory는 board placement를 모른다.
3. AdventureBoardUIFlow가 board side 교체, enter, exit, remove 흐름을 소유한다.
4. AdventureCardWidgetEventBinder는 interactive card에만 pointer/click callback을 등록한다.
5. AbilitySystem avatar/timeline binding은 board card binding lifetime과 함께 정리한다.
6. Pooling/reuse는 이번 구현 범위에 포함하지 않는다.
7. 동적 카드는 퇴장 transition 이후 hierarchy에서 제거한다.
8. Player/Monster/Display card widget의 필수 하위 UXML 요소 누락은 fallback 없이 예외로 처리한다.
9. CardWidget face slots, HealthWidget internals, IntentBadgeWidget internals, Portrait face internals, SkillSlot internals도 validator가 확인한다.
10. Player/Monster card widget은 AbilitySystem 누락을 fallback 없이 예외로 처리한다.
11. Interactive board card가 placement anchor 또는 click id 없이 event binding에 들어오면 예외로 처리한다.
12. 단일 카드 제거 후 같은 side에 남은 card slot은 현재 card count 기준으로 다시 배치한다.
13. Runtime card Timeline이 이미 bound된 상태로 새 board placement에 들어오면 예외로 처리한다.
```

보류 항목:

```text
1. Unity Play에서 Title -> CharacterSelect -> Adventure runtime flow 확인
```

코드 레거시 정리 상태:

```text
1. CardDealer runtime path는 제거되었고 AdventureBoardUIFlow / AdventureCardWidgetFactory가 대체한다.
2. AdventureSceneController는 제거되었고 AdventureScreenController가 대체한다.
3. AdventureViewEventBinder는 제거되었고 AdventureGameToScreenEventBinder / AdventureWidgetToScreenEventBinder가 대체한다.
4. IntentBadgeWidgetEvents는 사용하지 않는다. Intent 표시 흐름은 AdventureIntentUIFlow와 IAdventureIntentCardWidget 경계로 처리한다.
```

명시적 제외:

```text
1. Pooling/reuse는 현재 구현 범위에서 제외한다.
2. 제거된 동적 카드는 퇴장 후 hierarchy에서 제거하고 재사용하지 않는다.
```

## Cold Assessment

이 방향은 단기 변경량이 크다.

하지만 `CombatCardWidget` 하나에 선택지/플레이어/몬스터 분기를 계속 넣는 것보다 장기 유지보수성이 좋다.

구현 중 가장 위험한 부분은 UI 재설계 문제와 Intent 계약 문제를 동시에 디버깅하게 되는 것이다. 따라서 실제 구현에서는 Card UI 재설계와 Intent 계약 연결을 분리해서 진행하는 편이 좋다.
