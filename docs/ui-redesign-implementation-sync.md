# UI Redesign Implementation Sync

## Purpose

이 문서는 UI 재설계 문서와 현재 구현 상태를 연결하기 위한 진행 기록이다.
세부 설계는 각 문서에 남기고, 여기서는 "현재 무엇이 구현되었고 무엇을 확인해야 하는가"만 추적한다.

기준일: 2026-06-29

## Current Implemented Path

### Adventure Intro

```text
AdventureSceneEntryPoint.Start
-> AdventureScreenController.StartAdventure
-> AdventureScreenEvents.InitialPresentationPrepared(viewModel)
-> AdventureView.OnGameInitialPresentationPrepared
-> AdventureIntroUIFlow.Play
```

현재 Intro는 region banner와 CardDeck enter를 시작한 뒤, CardDeck transition 완료를 기다리고 실제 board card를 딜링한다.

```text
CardDeck enter 완료
-> AdventureBoardUIFlow.ReplaceBoardWithEnter(viewModel.BoardCards, cardDeck, canContinue)
-> left side cards enter
-> right side cards enter
```

`CardDeck.uxml`에는 딜링 출발점으로 사용할 `card-deck-top-card`가 존재한다.
이 요소는 실제로 제거되는 카드가 아니라 출발 좌표와 시각 기준을 제공한다.

### Board Card Deal

현재 board card enter는 임시 clone을 만들지 않는다.
보드에 배치될 실제 card widget을 목적지 anchor에 붙인 뒤, 내부 card body만 CardDeck top-card 위치에서 출발시킨다.

```text
Anchor
-> 보드 최종 위치 고정
-> Health/Intent 예약 공간 포함

Card body
-> top-card 위치에서 시작
-> 목적지까지 직선 이동
-> 도착 후 짧은 settle
```

현재 motion 규칙:

```text
시작 scale: 0.28
회전: 없음
카드 간격: 0.12초 stagger
덱 반응: CardDeck root는 고정, card-deck-top-card만 카드가 나가는 방향으로 짧은 nudge
도착 반응: 0.98 scale + 2px down settle 후 복귀
```

### HealthBar

HealthBar는 카드가 배치될 때 표시하지 않는다.

```text
Board card deal
-> HealthBar 숨김 유지
-> PlayerTurn start
-> Turn banner와 HealthBar 동시 표시
```

표시 책임은 `AdventureBoardUIFlow.ShowHealthBars(viewTransitionManager)`에 있다.

### Intent

PlayerTurn 시작 시 Intent reveal은 시작하지만 현재 PlayerTurn ready UI를 막지 않는다.

```text
PlayerTurnStarted 수신
-> Intent reveal fire-and-forget
-> Turn banner + HealthBar 완료 대기
-> CoinStatus 표시
-> Pouch 표시
```

따라서 Intent reveal은 Turn banner/HealthBar 이후에 시작되는 것이 아니다.
Turn banner/HealthBar와 같은 PlayerTurn 시작 구간에서 먼저 시작되고, 완료만 기다리지 않는다.

반대로 EnemyTurn의 intent trigger는 여전히 blocking gate다.
적 행동은 intent trigger animation 완료 후 시작한다.
`IntentBadgeWidget.Trigger()`는 `TransitionEndEvent`를 기다려 이 gate를 보장한다.
`IntentBadgeWidget.Show()`는 이미 표시된 상태에서도 다시 transition을 만들기 위해 hidden 상태를 먼저 한 프레임 반영한다.
`IntentBadgeWidget.Refresh()`는 refresh class를 다시 제거해 배지가 커진 상태로 남지 않게 한다.

## Documents Updated

```text
docs/adventure-view-uiflow-redesign.md
-> Intro deal 흐름, PlayerTurn 시작 표시 순서, HealthBar/Intent timing 반영

docs/adventure-card-ui-redesign.md
-> CardDeck top-card 계약, ReplaceBoardWithEnter signature, deal animation target, HealthBar timing 반영

docs/intent-domain-redesign.md
-> PlayerTurn intent reveal non-blocking, EnemyTurn trigger blocking 기준 반영
```

## Needs Manual Verification

Unity 검증은 현재 수행하지 않는다.
다음 항목은 사용자가 Unity에서 확인하거나, 별도 검증 시간을 잡았을 때 확인한다.

```text
1. CardDeck top-card에서 실제 카드가 출발하는 것처럼 보이는가
2. 카드 body만 움직이고 Health/Intent 예약 공간은 흔들리지 않는가
3. left side 카드가 먼저 들어오고 right side 카드가 이어서 들어오는가
4. 여러 카드가 0.12초 stagger로 읽히는가
5. settle B 방식이 어색하지 않은가
6. HealthBar가 PlayerTurn banner와 함께 보이는가
7. Intent가 PlayerTurn 시작 후 표시되는가
8. Pouch/CoinStatus가 Intent reveal보다 너무 빨리 보여 UX가 깨지지 않는가
9. Scene 종료 중 transition await가 화면에 UI를 다시 붙이지 않는가
```

## Current Risks

```text
ReplaceBoardWithEnter는 cardDeck 인자가 필요하다.
CardDeck이 없는 화면에서 같은 API를 쓰면 설계가 깨진다.

TransitionEndEvent 기반 animation은 실제 computed transition이 없으면 완료 이벤트가 오지 않을 수 있다.
현재는 해당 USS class와 transition 계약을 validator/수동 확인으로 관리해야 한다.

Intent reveal을 fire-and-forget으로 분리했기 때문에,
Pouch가 너무 빨리 보이면 시각적 우선순위가 꼬일 수 있다.

Card body lookup은 widget 구조에 의존한다.
Player/Monster/Display/Choice widget UXML 구조가 바뀌면 AdventureCardDealAnimator의 deal target lookup도 같이 점검해야 한다.
```

## Deferred Work

```text
Unity PlayMode 검증
오래된 문서 절의 중복 설명 정리
CardDeck 없는 board enter API 분리 여부 결정
deal motion duration/easing 수치 튜닝
Intent reveal을 blocking으로 되돌릴지 UX 확인
Pooling 설계
```

## Next Recommended Step

현재 다음 작업은 Unity 실행 검증을 제외하면 animation 계약과 문서-코드 불일치를 계속 줄이는 것이다.

```text
1. 코드상 Intro -> Deal -> PlayerTurn 시작 순서가 깨지지 않는지 확인한다.
2. USS transition 계약이 validator로 충분히 잡히는지 확인한다.
3. sync 문서가 실제 코드와 다른 설명을 남기지 않는지 정리한다.
4. Unity 검증이 가능해지면 CardDeck top-card 출발, HealthBar, Intent/Pouch 타이밍을 확인한다.
5. 문제가 있으면 motion 수치 또는 PlayerTurn gate를 조정한다.
```

단, 현재 요청 기준에서는 Unity 검증을 수행하지 않는다.
