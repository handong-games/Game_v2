# Adventure Enemy Death Flow Design

## Goal

적이 사망했을 때 전투 상태, 보드 상태, UI 연출, 보상, 다음 선택지 표시 흐름을 명확히 분리한다.

최종 사용자 경험은 다음 순서다.

```text
적 사망
-> Board 레이어에서 Death animation
-> Death animation 완료
-> AdventureBoard에서 적 카드 제거
-> 임시 Reward UI 표시
-> Reward UI 패널 클릭으로 닫힘
-> AdventureStageContinuationFlow
-> 다음 선택지 카드 표시 또는 ImmediateEncounter 시작
```

## Current Problem

현재 구조에서는 `AdventureCombatDeathObserver`가 너무 많은 책임을 가진다.

```text
DeathMessage 감지
-> cardId 확인
-> 사망 중복 처리
-> player/enemy 분기
-> AdventureBoard에서 enemy card 제거
-> UI card remove 요청
-> AdventureCards에서 card 제거
-> CombatRuntime에서 card 제거
-> 모든 적 사망 판단
-> CombatResultFlow.CompleteCombat(Victory)
```

이 구조는 빠르게 동작할 수 있지만, 다음 문제가 있다.

```text
1. DeathObserver가 감지자 역할을 넘어서 게임 진행을 조율한다.
2. 일반 카드 제거와 사망 카드 제거가 같은 흐름에 섞인다.
3. 사망 애니메이션 완료 시점과 AdventureBoard 상태 제거 시점이 명확하지 않다.
4. Victory 배너, Reward, 다음 선택지 표시가 CombatResultFlow 안에 묶여 확장성이 낮다.
```

## Confirmed Decisions

### 1. Death animation의 의미

Death animation은 단순 사망 표현이 아니라, 카드가 보드에서 퇴장하는 표현까지 포함한다.

따라서 별도의 exit animation은 만들지 않는다.

```text
Death animation = 사망 표현 + 보드 퇴장 표현
```

### 2. AdventureBoard 제거 시점

`AdventureBoard`에서 적 카드는 Death animation이 완료된 뒤 제거한다.

```text
Death animation 완료 전:
- CombatRuntime은 enemy 사망을 반영할 수 있다.
- AdventureBoard는 enemy cardId를 유지한다.
- UI는 enemy card를 계속 표시한다.

Death animation 완료 후:
- UI card 제거
- AdventureBoard에서 enemy cardId 제거
- AdventureCards에서 enemy card 제거
```

### 3. Death animation 책임

Death animation은 개별 `AdventureMonsterCardWidget`이 아니라 Board 레이어에서 처리한다.

```text
AdventureBoardUIFlow
-> cardId로 binding 조회
-> binding.Element에 death class 적용
-> transition 완료 대기
-> UI binding 제거
```

이유:

```text
1. 사망은 "몬스터 위젯의 내부 상태"라기보다 "보드 카드가 사라지는 보드 연출"이다.
2. 나중에 다른 enemy card 타입이 생겨도 Board death animation을 재사용할 수 있다.
3. 일반 remove animation과 death animation을 분리할 수 있다.
```

### 4. Reward UI

이번 단계의 Reward UI는 임시 UI다.

```text
임시 Reward UI
-> 패널 아무 곳이나 클릭
-> 완료 결과 반환
```

보상 선택, 보상 모델, 보상 적용은 이후 설계로 미룬다.

### 5. Victory 배너

Victory 배너는 이번 흐름에서 제거한다.

```text
Victory
-> Reward UI
-> StageContinuation
```

Defeat 배너는 현재 흐름과 충돌하지 않으므로 유지 가능하다.

## Target Responsibilities

### AdventureCombatDeathObserver

역할: `GameplayDeathMessage` 감지만 담당한다.

```text
Player death
-> AdventureCombatResultFlow.CompleteCombat(Defeat)

Enemy death
-> AdventureEnemyDeathFlow.HandleEnemyDeath(cardId)
```

하지 않는 일:

```text
- AdventureBoard 직접 제거
- UI 제거 이벤트 직접 발행
- AdventureCards 직접 제거
- Victory 판단 직접 완료
```

### AdventureEnemyDeathFlow

역할: 적 사망 이후 전투/보드 상태와 UI 사망 연출을 연결한다.

예상 흐름:

```text
HandleEnemyDeath(cardId)
-> 중복 사망 처리 확인
-> enemy side 확인
-> CombatRuntime에서 enemy 제거
-> Board death animation 요청
-> Death animation 완료 후 AdventureBoard에서 enemy card 제거
-> AdventureCards에서 enemy card 제거
-> 모든 enemy가 제거되었으면 CombatResultFlow.CompleteCombat(Victory)
```

다중 적이 동시에 죽는 경우:

```text
여러 적 사망
-> 모든 사망 카드가 동시에 Death animation
-> 모든 사망 카드 제거 완료
-> 모든 적 사망이면 Victory
```

초기 구현은 단일 `cardId` 진입점으로 시작하더라도, 내부 API는 리스트 확장을 고려한다.

### AdventureBoardUIFlow

역할: Board에 배치된 카드의 UI death animation과 UI 제거를 담당한다.

새 API 후보:

```csharp
public Awaitable<bool> PlayCardDeathAndRemove(
    uint cardId,
    Func<bool> canContinue);
```

의미:

```text
1. binding 조회
2. death class 적용
3. transition 완료 대기
4. binding dispose
5. slot/remove from hierarchy
6. card event binding refresh
```

기존 `RemoveCardAfterExit`와 섞지 않는다.

```text
RemoveCardAfterExit
-> 일반 카드 제거

PlayCardDeathAndRemove
-> death animation이 포함된 적 사망 제거
```

### AdventureCombatResultFlow

역할: Combat 종료 결과 이후 흐름만 담당한다.

Victory 변경:

```text
기존:
Victory
-> ResultRequested(Victory)
-> RewardStarted
-> StageAdvance

변경:
Victory
-> RewardStarted
-> AdventureStageContinuationFlow.ContinueAfterReward()
```

Defeat:

```text
Defeat
-> ResultRequested(Defeat)
```

Victory 배너는 호출하지 않는다.

### AdventureStageContinuationFlow

역할: Reward 이후 Adventure 진행을 계속한다.

예상 흐름:

```text
ContinueAfterReward(claimedRewardIds)
-> AdventureStageAdvanceFlow.AdvanceOrComplete()
-> status == Advanced 이면 AdventureEncounterStartFlow.TryStartImmediateEncounter()
```

이 Flow를 분리하는 이유:

```text
1. CombatResultFlow가 StageAdvance와 ImmediateEncounter까지 알 필요가 없다.
2. Reward 이후뿐 아니라 Event/Shop 완료 이후에도 같은 "Adventure 진행 계속" 흐름을 재사용할 수 있다.
3. "다음 선택지 카드 표시"는 Reward UI가 아니라 Stage continuation의 결과다.
```

### AdventureRewardUIFlow

역할: 임시 Reward UI를 표시하고 클릭 완료를 반환한다.

현재는 empty result를 즉시 반환한다.

변경 방향:

```text
Play(viewModel, canContinue)
-> 임시 reward panel 생성
-> AdventureRoot 또는 reward overlay layer에 추가
-> 패널 클릭 대기
-> 패널 제거
-> AdventureRewardUIResult.Empty() 반환
```

주의:

`adventure-effect-layer`는 현재 `picking-mode="Ignore"`이므로 클릭 패널을 붙이기에는 부적합하다.

권장:

```text
1. AdventureRoot에 직접 임시 패널 추가
2. 또는 reward 전용 overlay layer를 UXML에 추가
```

## Event Design

기존 `AdventureBoardEvents.CardRemoveRequested`는 일반 카드 제거용으로 유지한다.

사망 제거는 별도 이벤트를 추가한다.

```csharp
public Func<AdventureBoardCardDeathViewModel, Awaitable<bool>> CardDeathRequested;
```

예상 ViewModel:

```csharp
public sealed class AdventureBoardCardDeathViewModel
{
    public AdventureBoardCardDeathViewModel(uint cardId)
    {
        CardId = cardId;
    }

    public uint CardId { get; }
}
```

여러 적 동시 사망 확장을 고려하면 다음 형태도 가능하다.

```csharp
public sealed class AdventureBoardCardsDeathViewModel
{
    public AdventureBoardCardsDeathViewModel(IReadOnlyList<uint> cardIds)
    {
        CardIds = cardIds ?? Array.Empty<uint>();
    }

    public IReadOnlyList<uint> CardIds { get; }
}
```

초기 구현은 단일 `cardId`로 시작해도 된다. 단, `AdventureEnemyDeathFlow` 내부는 리스트 확장 가능성을 열어둔다.

## Expected Sequence

```text
GameplayDeathMessage
-> AdventureCombatDeathObserver
-> AdventureEnemyDeathFlow.HandleEnemyDeath(cardId)
-> AdventureCombatRuntime.RemoveCard(cardId)
-> AdventureBoardEvents.CardDeathRequested(cardId)
-> AdventureView.OnGameBoardCardDeathRequested
-> AdventureBoardUIFlow.PlayCardDeathAndRemove(cardId)
-> AdventureBoard.RemoveCard(ECardZone.Right, cardId)
-> AdventureCards.Remove(cardId)
-> if no enemies remain:
       AdventureCombatResultFlow.CompleteCombat(Victory)
-> AdventureRewardUIFlow.Play()
-> AdventureStageContinuationFlow.ContinueAfterReward()
-> AdventureStageAdvanceFlow.AdvanceOrComplete()
-> ChoiceRefreshStarted or Complete
-> TryStartImmediateEncounter if needed
```

## Implementation Feasibility

현재 프로젝트에는 구현에 필요한 기반이 이미 있다.

```text
Existing:
- AdventureCombatDeathObserver
- AdventureCombatResultFlow
- AdventureStageAdvanceFlow
- AdventureRewardUIFlow
- AdventureBoardUIFlow
- AdventureBoardEvents
- AdventureScreenEvents
- AdventureGameToScreenEventBinder
- AdventureView.OnGameBoardCardRemoveRequested
- AdventureChoiceRefreshUIFlow
```

필요한 추가/변경:

```text
New:
- AdventureEnemyDeathFlow
- AdventureStageContinuationFlow
- AdventureBoardCardDeathViewModel

Modify:
- AdventureCombatDeathObserver
- AdventureCombatResultFlow
- AdventureBoardEvents
- AdventureGameToScreenEventBinder
- AdventureView.GameEvent.cs
- AdventureView.Card.cs
- AdventureBoardUIFlow
- AdventureBoardLayout or board card animator class
- AdventureRewardUIFlow
- AdventureSceneScope registrations
```

## Risks

### 1. Death event 중복

`GameplayDeathMessage`가 같은 cardId에 대해 여러 번 올 수 있다.

대응:

```text
AdventureCombatRuntime.MarkDeathResolved(cardId)
```

중복 방지는 유지한다.

### 2. Runtime 제거와 UI 제거 순서

`CombatRuntime`에서는 적 사망을 먼저 반영해도 된다.

그러나 `AdventureBoard`와 UI 제거는 Death animation 완료 후 처리한다.

```text
CombatRuntime: 전투 판정용 상태
AdventureBoard: 현재 보드 표시 상태
```

두 상태의 역할을 혼동하지 않는다.

### 3. Reward UI 클릭 레이어

`adventure-effect-layer`는 `picking-mode="Ignore"`이므로 클릭 대기 UI에는 부적합하다.

대응:

```text
AdventureRoot에 직접 panel 추가
또는 reward overlay layer 추가
```

### 4. Victory 배너 제거 영향

현재 `AdventureCombatResultFlow`는 Victory에서도 `ResultRequested`를 호출한다.

변경 후 Victory에서는 호출하지 않는다.

Defeat는 유지할 수 있다.

## First Implementation Plan

```text
1. AdventureBoardCardDeathViewModel 추가
2. AdventureBoardEvents에 CardDeathRequested 추가
3. AdventureGameToScreenEventBinder에 CardDeathRequested 연결/해제 추가
4. AdventureView.OnGameBoardCardDeathRequested 추가
5. AdventureBoardUIFlow.PlayCardDeathAndRemove 추가
6. Board death USS class 추가
7. AdventureEnemyDeathFlow 추가
8. AdventureCombatDeathObserver 축소
9. AdventureStageContinuationFlow 추가
10. AdventureCombatResultFlow에서 Victory 배너 제거 및 continuation 호출
11. AdventureRewardUIFlow에 클릭으로 닫히는 임시 패널 추가
12. AdventureSceneScope 등록 추가
```

## Non-goals For This Pass

```text
- 실제 보상 선택/적용 시스템 구현
- 보상 아이템 모델 설계
- 보스 전용 death animation
- 다중 적 전투 UI 완성
- Defeat flow 재설계
```

