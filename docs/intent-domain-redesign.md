# Intent Domain Redesign

## Purpose

이 문서는 Adventure 전면 재설계 이후의 Intent 설계 기준을 기록한다.

기존 `Domains.Combat.Intent` 설계는 `CombatService`, `CombatCard` 중심의 과거 구조와 섞여 있다. 새 방향에서는 Intent를 Combat 하위 구현 세부사항이 아니라 별도 도메인으로 분리한다.

## Current Decision Summary

```text
Intent 목적:
- UI 예고 + 실행 계약 시스템
- 플레이어에게 보여준 최종 action이 EnemyTurn에서 실제로 실행되어야 한다.

갱신 시점:
- PlayerTurn 중에도 갱신 가능하다.
- EnemyTurn은 마지막으로 보여준 최신 resolved intent를 실행한다.

갱신 조건:
- 모든 상태 변화에 반응하지 않는다.
- Intent가 선언한 dependency 변화에만 반응한다.

실행 경계:
- Intent는 Ability를 직접 실행하지 않는다.
- AdventureEnemyActionFlow가 resolved intent action을 실행한다.
- 실행 후 Intent에 consume을 알려 sequence를 진행한다.

런타임 식별:
- CardId 기준으로 IntentRuntimeState를 관리한다.

표시 위치:
- 몬스터 카드 안 또는 위에 표시한다.

표시 개수:
- 모델/런타임 구조는 여러 intent item을 지원한다.
- V1 UI는 첫 번째 item만 표시해도 된다.

View 경계:
- Flow는 View를 직접 알지 않는다.
- View도 Flow를 직접 호출하지 않는다.
- Scenes.Adventure 이벤트와 Binding을 통해 연결한다.
```

## Final Runtime / Flow / Presentation Shape

Intent도 Adventure 재설계의 기본 구조인 `Runtime / Flow / Presentation / Scene`을 따른다.

```text
Data
- 정적 작성 데이터
- MonsterActionModel
- IntentDisplayModel
- IntentDisplayDefinition
- IntentNumberRuleModel
- IntentOverrideRule

Runtime
- 실행 중 변하는 Intent 상태
- CardId 기준 저장
- sequence index
- resolved action cache
- display cache

Flow
- Intent를 언제 계산/갱신/소비할지 결정
- PlayerTurn 시작 시 Prepare
- 상태 변화 시 Refresh
- EnemyAction 실행 후 Consume

Presentation
- Runtime 데이터를 UI용 ViewModel로 변환
- Sprite, NumberText, reveal sequence 생성
- 숫자 표현 규칙 담당

Scene
- VContainer 등록
- Controller, Events, View 연결
- Flow 결과를 View animation으로 전달
```

`IntentSystem`은 최종 유지 대상이 아니다.

```text
기존 IntentSystem 책임:
- runtime state 저장
- action resolve
- display cache 생성
- event 발행
- consume 처리

문제:
- Runtime / Flow / Presentation 책임이 섞여 있다.
- Card 객체 참조를 key로 사용한다.
- View로 전달될 변경 이벤트까지 도메인 객체가 발행한다.
```

최종 분해 방향:

```text
IntentSystem
-> IntentRuntime
-> IntentPrepareFlow
-> IntentRefreshFlow
-> IntentConsumeFlow
-> IntentPresenter
```

핵심 규칙:

```text
1. EnemyTurn에서는 Intent를 다시 계산하지 않는다.
2. View는 Intent 계산을 하지 않는다.
3. Runtime은 상태만 들고, 흐름 판단은 Flow가 한다.
4. Presentation은 숫자 문자열과 UI 표시 모델 생성을 담당한다.
5. Scene은 VContainer 등록과 View/Event 연결만 담당한다.
```

## Adventure Turn Integration

Intent는 Adventure 턴 흐름의 특정 지점에만 끼운다.

```text
Encounter 시작
-> 몬스터 카드 생성
-> MonsterActionModel.ExecutionAbility를 AbilitySystem에 부여
-> CardId + MonsterActionModel -> AbilitySpecHandle 저장

PlayerTurn 시작
-> IntentPrepareFlow
-> IntentRuntime에 ResolvedIntentActionData 캐시
-> IntentRuntime에 IntentDisplayData 캐시
-> IntentPresenter가 MonsterIntentRevealViewModel[] 생성
-> AdventureCombatEvents.IntentRevealRequested 발행
-> AdventureView가 IntentBadge reveal animation 재생
-> 완료 후 AdventureSceneController.OnIntentRevealCompleted
-> 플레이어 입력 허용

PlayerTurn 중 상태 변화
-> 선언된 dependency가 바뀐 몬스터만 IntentRefreshFlow
-> IntentRuntime 캐시 교체
-> Presentation ViewModel 생성
-> View refresh animation 요청

EndTurn
-> 플레이어 입력 차단
-> EnemyTurnBannerRequested
-> 배너 완료 후 EnemyTurn 시작

EnemyTurn
-> AdventureEnemyActionFlow
-> IntentRuntime의 cached FinalActionModel 읽기
-> IntentExecutionBindingStore에서 AbilitySpecHandle 조회
-> AdventureCombatEvents.IntentTriggeredRequested 발행
-> IntentBadge trigger animation과 적 action을 동시에 시작
-> 해당 Ability 실행
-> 실행 완료 후 IntentConsumeFlow
-> sequence index advance

EnemyTurn 종료
-> PlayerTurn 시작
-> 다시 IntentPrepareFlow
```

이 흐름에서 가장 중요한 계약:

```text
PlayerTurn에 보여준 ResolvedIntentActionData.FinalActionModel이
EnemyTurn에서 실행되는 FinalActionModel과 같아야 한다.
```

따라서 `AdventureEnemyActionFlow`는 원칙적으로 intent를 계산하지 않는다.

```text
AdventureEnemyActionFlow 역할:
- 살아있는 적 순회
- cached ResolvedIntentActionData 조회
- FinalActionModel 실행
- action 시작 시 해당 몬스터 intent trigger presentation 요청
- 실행 완료 후 consume 요청

AdventureEnemyActionFlow 비책임:
- 다음 행동 계산
- intent display 생성
- sequence index 직접 수정
- View reveal 요청
```

## Intent Trigger vs Consume

적 턴에서 intent badge는 적 행동 시작과 동시에 사라져야 한다.

하지만 UI에서 사라지는 것과 Runtime 계약을 소비하는 것은 같은 일이 아니다.

```text
Trigger:
- Presentation 동작
- 예고된 intent가 실제 행동으로 전환되는 순간을 표현한다.
- IntentBadge가 퇴장하며 enemy action이 시작된다.
- Runtime의 ResolvedIntentActionData를 바로 삭제하지 않는다.

Consume:
- Runtime 동작
- 해당 intent action이 실제로 처리되었음을 기록한다.
- sequence index를 advance한다.
- Enemy action 완료 이후에 호출한다.
```

이 둘을 분리하는 이유:

```text
행동 시작과 동시에 badge는 사라져야 한다.
하지만 action 실행 중에는 어떤 FinalActionModel을 실행 중인지 추적 가능해야 한다.
실행 실패, interrupt, death, combat end 같은 예외 흐름에서 Runtime 계약을 너무 빨리 지우면 원인 추적이 어려워진다.
```

따라서 적 행동 흐름은 다음 순서를 따른다.

```text
1. cached ResolvedIntentActionData 읽기
2. 실행할 FinalActionModel 확정
3. IntentTriggeredRequested(cardId) 발행
4. trigger exit animation 시작
5. enemy action 시작
6. enemy action 완료
7. IntentConsumeFlow.Consume(cardId)
8. sequence index advance
```

중요:

```text
IntentTriggeredRequested는 trigger animation 완료를 기다리지 않는다.
적 action과 trigger animation은 같은 시점에 시작될 수 있다.
```

## Target Package Structure

```text
Assets/@Scripts/Domains/Intent/
  Data/
  Runtime/
  Flow/
  Execution/
  Presentation/
  Validation/
```

### Data

정적 작성 데이터와 ScriptableObject 모델을 둔다.

예상 타입:

```text
MonsterActionModel
IntentDisplayModel
IntentDisplayDefinition
IntentNumberRuleModel
IntentOverrideRule
IntentOverrideRuleSetModel
```

`IntentActionModel`은 사용하지 않는다.

이유:

```text
이 모델은 단순한 intent 표시 action이 아니라 몬스터가 실제로 수행할 행동이다.
따라서 이름은 MonsterActionModel이 더 정확하다.
```

확정 rename:

```text
IntentActionModel -> MonsterActionModel
```

`MonsterActionModel`은 실행과 표시 정의를 함께 가진다.

테이블은 만들지 않는다.

```text
MonsterActionTable 없음
MonsterModel.ActionSequence가 MonsterActionModel을 직접 참조
```

이유:

```text
현재 action은 enum id로 전역 조회할 데이터가 아니라 몬스터가 직접 들고 있는 행동 정의다.
MonsterModel 하나를 열었을 때 이 몬스터가 어떤 행동을 하는지 바로 보여야 한다.
Table을 두면 enum, table 등록, asset 참조가 모두 늘어나고 초기 설계 목적보다 복잡해진다.
```

```csharp
using System;
using System.Collections.Generic;
using Gameplay.GAS;
using UnityEngine;

namespace Domains.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Monster/Action")]
    public sealed class MonsterActionModel : ScriptableObject
    {
        [SerializeField]
        private GameplayAbility _executionAbility;

        [SerializeField]
        private IntentDisplayDefinition[] _intentDisplays;

        public GameplayAbility ExecutionAbility => _executionAbility;

        public IReadOnlyList<IntentDisplayDefinition> IntentDisplays =>
            _intentDisplays ?? Array.Empty<IntentDisplayDefinition>();
    }
}
```

`MonsterModel` 예상 형태:

```csharp
using Domains.Intent.Data;
using UnityEngine;

namespace Game.Data
{
    public sealed class MonsterModel : ScriptableObject
    {
        [SerializeField]
        private MonsterActionModel[] _actionSequence;

        public IReadOnlyList<MonsterActionModel> ActionSequence =>
            _actionSequence ?? Array.Empty<MonsterActionModel>();
    }
}
```

`IntentDisplayModel`은 재설계 대상이다.

확정 방향:

```text
- Namespace: Domains.Intent.Data
- Icon은 AssetReferenceSprite가 아니라 Sprite 직접 참조
- Description은 LocalizedString
- RequiresNumber 유지
- IntentDisplayTable 없음
- MonsterActionModel.IntentDisplays가 IntentDisplayModel을 직접 참조
```

이유:

```text
IntentDisplayModel은 "공격", "방어", "독" 같은 재사용 가능한 표시 데이터 에셋이다.
하지만 지금 구조에서는 enum id로 전역 조회할 필요가 없다.
MonsterActionModel이 어떤 표시를 노출하는지 직접 참조로 보여주는 쪽이 더 읽기 쉽다.
```

예상 코드:

```csharp
using UnityEngine;
using UnityEngine.Localization;

namespace Domains.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Intent/Display")]
    public sealed class IntentDisplayModel : ScriptableObject
    {
        [SerializeField]
        private Sprite _icon;

        [SerializeField]
        private LocalizedString _description;

        [SerializeField]
        private bool _requiresNumber;

        public Sprite Icon => _icon;
        public LocalizedString Description => _description;
        public bool RequiresNumber => _requiresNumber;
    }
}
```

주의:

```text
IntentPresenter는 Addressables를 모른다.
IntentBadgeWidget도 Addressables를 모른다.
IntentDisplayModel은 이미 로드된 MonsterActionModel 그래프 안의 직접 참조로 접근한다.
Addressables 로딩은 AdventureSceneLoader가 필요한 루트 모델을 로드하는 경계에서 끝낸다.
```

책임:

```text
- 몬스터가 어떤 행동 sequence를 가지는지 표현
- 한 행동이 어떤 표시 정보를 가지는지 표현
- 상태 override rule을 표현
- 숫자 계산 rule을 표현
```

비책임:

```text
- 현재 전투 상태 저장
- UI 위젯 조작
- Ability 실행
```

### Runtime

전투 중 변경되는 intent 상태와 cache를 둔다.

예상 타입:

```text
IntentRuntime
IntentRuntimeState
ResolvedIntentActionData
IntentDisplayData
IntentNumberData
```

CardId 기준으로 상태를 관리한다.

```csharp
Dictionary<uint, IntentRuntimeState>
```

책임:

```text
- 몬스터별 current action sequence index 저장
- 현재 resolved action cache 저장
- 현재 display cache 저장
- dependency cache 저장
```

비책임:

```text
- Ability 실행
- 카드 보드 배치
- View 호출
```

### Flow

Intent use-case orchestration을 둔다.

예상 타입:

```text
IntentPrepareFlow
IntentRefreshFlow
IntentConsumeFlow
```

책임:

```text
- PlayerTurn 시작 시 살아있는 몬스터 intent 준비
- dependency 변화에 따른 intent refresh
- enemy action 실행 후 consume 처리
- sequence index advance 정책 적용
```

비책임:

```text
- View reveal animation 호출
- Ability 직접 실행
```

### Execution

MonsterActionModel과 실제 실행 가능한 Ability handle의 runtime binding을 둔다.

예상 타입:

```text
IntentExecutionBindingStore
IntentExecutionBindingData
```

책임:

```text
- monster CardId + MonsterActionModel -> AbilitySpecHandle 매핑
- EnemyActionFlow가 resolved action을 실행할 수 있도록 handle 제공
```

비책임:

```text
- 어떤 action을 실행할지 결정
- Ability 내부 효과 실행
```

### Presentation

Intent runtime data를 UI에 넘길 ViewModel로 변환한다.

예상 타입:

```text
IntentPresenter
MonsterIntentRevealViewModel
IntentItemViewModel
```

책임:

```text
- IntentDisplayData를 UI 표시용 데이터로 변환
- reveal sequence 배열 생성
- 배열 순서에 의미를 부여
- IntentItemViewModel은 Sprite를 직접 가진다.
```

`IntentPresenter`는 `IntentDisplayData.DisplayModel.Icon`에서 Sprite를 읽어 `IntentItemViewModel`을 만든다.

```csharp
public IntentItemViewModel CreateItem(IntentDisplayData data)
{
    return new IntentItemViewModel(
        data.DisplayModel.Icon,
        data.NumberValue,
        data.CountValue,
        FormatNumber(data.NumberValue, data.CountValue));
}
```

중요 경계:

```text
View는 전달받은 배열 순서대로 재생만 한다.
View가 "왼쪽부터 reveal" 같은 게임 규칙을 판단하지 않는다.
```

### Validation

작성 데이터 검증을 둔다.

예상 검증:

```text
- MonsterModel.ActionSequence가 비어 있지 않음
- MonsterActionModel.ExecutionAbility가 존재함
- MonsterActionModel.IntentDisplays가 비어 있지 않음
- IntentDisplayModel.Icon이 존재함
- IntentDisplayModel.Description이 존재함
- 숫자가 필요한 display에는 IntentNumberRuleModel이 존재함
- OverrideRule priority 충돌 없음
- executable action은 combat setup 시 binding 가능함
```

## Core Contract

Intent는 UI 예고와 실행 사이의 계약을 보관한다.

```text
PlayerTurn start
-> Intent 계산
-> ResolvedIntentActionData cache
-> IntentDisplayData cache
-> UI reveal

PlayerTurn 중 dependency 변화
-> 해당 monster intent refresh
-> ResolvedIntentActionData 교체
-> IntentDisplayData 교체
-> UI 갱신

EnemyTurn
-> 마지막 cached ResolvedIntentActionData 읽기
-> AdventureEnemyActionFlow가 실행
-> 실행 완료 후 IntentConsumeFlow 호출
-> sequence index advance
-> resolved/display cache release
```

EnemyTurn은 action을 다시 계산하지 않는다.

```text
보여준 것과 실행되는 것이 다르면 refresh pipeline의 버그다.
```

## ResolvedIntentActionData

EnemyActionFlow가 Intent로부터 받는 계약 객체다.

```csharp
public readonly struct ResolvedIntentActionData
{
    public MonsterActionModel BaseActionModel { get; }
    public MonsterActionModel FinalActionModel { get; }
    public IntentOverrideRule AppliedOverrideRule { get; }
    public bool HasOverride => AppliedOverrideRule != null;
}
```

의미:

```text
BaseActionModel:
- 현재 sequence index에서 나온 원래 행동

FinalActionModel:
- override rule 적용 후 실제로 보여주고 실행할 행동

AppliedOverrideRule:
- 어떤 rule이 행동을 교체했는지
- consume 시 sequence advance 정책 판단에 사용
```

## Dependency Model

Intent refresh는 선언된 dependency 변화에만 반응한다.

### Action / Override Dependency

행동 자체가 바뀌는 조건이다.

예:

```text
Stun 적용
-> Strike 행동이 SkipTurn 행동으로 교체됨
```

위치는 `IntentOverrideRule` 또는 action resolution layer다.

`IntentOverrideRule`은 이름을 유지한다. 다만 replacement 대상은 `IntentActionModel`이 아니라 `MonsterActionModel`이다.

```text
IntentOverrideRule
- condition
- replacement MonsterActionModel
- priority
- sequence advance policy
```

### Number Dependency

행동은 그대로인데 표시 숫자만 바뀌는 조건이다.

예:

```text
Weak 적용
-> Strike 행동 유지
-> Damage 6이 Damage 3으로 변경
```

위치는 `IntentNumberRuleModel`이다.

## Presentation Rules

Intent는 몬스터 카드 안 또는 위에 표시한다.

```text
[Intent Badge]
[HP]
[Monster Card]
```

최종 UI는 `AdventureMonsterCardWidget` 안에서 HP 위에 표시한다.

```text
AdventureMonsterCardWidget
- IntentBadgeWidget
- HealthWidget
- CardFace
```

기존 `CombatCardWidget` 중심 구현은 마이그레이션 전 임시 경로로만 본다.

### Intent Badge V1

확정된 UI 방향:

```text
위치:
- AdventureMonsterCardWidget 내부
- HealthWidget 위

형태:
- 작은 badge 형태
- icon이 주 시각 요소
- number는 icon 좌하단 또는 전면에 overlay

표시 개수:
- 모델/런타임 구조는 여러 intent item을 지원
- V1 UI는 첫 번째 intent item만 표시

숫자:
- NumberValue = 21, CountValue = 1 -> "21"
- NumberValue = 3, CountValue = 2 -> "3x2"
- NumberValue = 0, CountValue = 0 -> number label 숨김

애니메이션:
- reveal: fade + scale
- refresh: pulse
- trigger: fade out / scale down / slight upward motion
```

### Intent Icon Style Guide

Intent icon은 개별 몬스터 행동의 그림 설명이 아니라, 플레이어가 즉시 판단해야 하는 의미를 전달한다.

핵심 원칙:

```text
1. 같은 캔버스 크기
   - 원본은 128x128 또는 256x256 중 하나로 통일한다.
   - 실제 UI 표시 크기는 IntentBadgeWidget USS에서 결정한다.

2. 같은 시점
   - 정면 또는 3/4뷰 중 하나로 고정한다.
   - 공격/방어/버프/디버프마다 시점이 달라지면 서로 다른 게임의 에셋처럼 보인다.

3. 같은 선 두께
   - 손그림 스타일을 사용한다면 outline 두께를 icon set 전체에서 맞춘다.
   - 작은 badge에서 보이는 실루엣이 우선이다.

4. 같은 색 체계
   - Attack: warm red / orange
   - Defense: steel blue / gray
   - Heal: green
   - Buff: golden yellow
   - Debuff: purple

5. 같은 여백
   - 실제 그림은 캔버스의 약 70~80%만 사용한다.
   - badge 안에서 숫자 overlay와 충돌하지 않도록 좌하단/하단 여백을 확보한다.

6. 같은 복잡도
   - 어떤 icon은 상세하고 어떤 icon은 단순하면 안 된다.
   - 32px 근처에서도 의미가 읽혀야 한다.

7. 숫자는 icon 이미지에 포함하지 않는다.
   - 숫자 표시, x2 같은 count 표현은 Presentation 계층의 NumberText와 USS가 담당한다.
```

V1 icon set은 의미 단위로 시작한다.

```text
Intent_Attack
Intent_Defense
Intent_Buff
Intent_Debuff
Intent_Heal
```

몬스터별 icon은 기본값이 아니다.

```text
권장:
- TestSlime attack -> Intent_Attack
- Boss attack -> Intent_Attack
- Poison attack -> Intent_Poison 또는 Intent_Debuff

비권장:
- Intent_TestSlimeAttack
- Intent_GoblinAttack
- Intent_BossAttack
```

이유:

```text
플레이어가 해석해야 하는 것은 몬스터별 개성이 아니라 다음 턴에 대응해야 하는 의미다.
몬스터별 icon을 만들면 에셋 수가 빠르게 늘고, 같은 공격 의미가 서로 다른 그림으로 보인다.
```

현재 구현 단계의 임시 예외:

```text
IntentDisplay_Attack.asset의 Sprite는 아직 비어 있을 수 있다.
이는 Intent 계약, reveal, trigger, consume 흐름을 먼저 검증하기 위한 임시 상태다.
최종 검증 단계에서는 IntentDisplayModel.Icon이 null이면 실패로 본다.
```

예상 구조:

```text
AdventureMonsterCardWidget
- IntentBadgeWidget
- HealthWidget
- CardFace
```

`IntentBadgeWidget`은 별도 UI Toolkit widget으로 두는 방향을 우선한다.

이유:

```text
- CombatCardWidget에 health/card/cue/intent 책임이 과하게 섞이는 것을 줄인다.
- 이후 여러 intent item 표시로 확장하기 쉽다.
- intent badge animation을 독립적으로 관리할 수 있다.
```

최종 V1 관계:

```text
AdventureMonsterCardWidget
- 몬스터 카드 전용 UI 컨테이너
- IntentBadgeWidget을 가진다.

AdventurePlayerCardWidget
- 플레이어 카드 전용 UI 컨테이너
- IntentBadgeWidget을 가지지 않는다.

AdventureChoiceCardWidget
- 선택지 카드 전용 UI 컨테이너
- IntentBadgeWidget과 HealthWidget을 가지지 않는다.
```

Intent UI ViewModel은 Addressables를 노출하지 않는다.

```csharp
public sealed class IntentItemViewModel
{
    public Sprite Icon { get; }
    public int NumberValue { get; }
    public int CountValue { get; }
    public string NumberText { get; }
    public bool HasNumber => !string.IsNullOrEmpty(NumberText);
}
```

Sprite 로딩/변환과 숫자 표시 문자열 생성은 Presentation 계층에서 끝낸다.

`IntentBadgeWidget`은 `Sprite`, `NumberText`, 애니메이션만 처리한다. `IntentBadgeWidget`은 `NumberValue`, `CountValue`를 직접 포맷하지 않는다.

### IntentBadgeWidget Events

`IntentBadgeWidget`은 직접 Controller나 Flow를 알지 않는다.

Widget completion은 `AdventureWidgetEvents` 아래의 `IntentBadgeWidgetEvents`로 발행한다.

예상 구조:

```csharp
public sealed class AdventureWidgetEvents
{
    public AdventureTurnWidgetEvents Turn { get; }
    public AdventurePouchWidgetEvents Pouch { get; }
    public AdventureSkillSlotWidgetEvents SkillSlot { get; }
    public IntentBadgeWidgetEvents IntentBadge { get; }
}

public sealed class IntentBadgeWidgetEvents
{
    public Action RevealCompleted;
    public Action RefreshCompleted;
    public Action TriggerCompleted;
}
```

`IntentBadgeWidget`은 생성자 주입을 사용하지 않는다.

이유:

```text
- UI Toolkit VisualElement는 UXML/CloneTree로 생성된다.
- VContainer 생성자 주입 대상으로 보기 어렵다.
- 현재 프로젝트는 View의 OnVisualTreeCloned에서 widget을 찾고 Bind하는 흐름을 사용한다.
```

따라서 `Bind` 함수로 events를 전달한다.

```csharp
public sealed partial class IntentBadgeWidget : VisualElement
{
    private IntentBadgeWidgetEvents _events;

    public void Bind(IntentBadgeWidgetEvents events)
    {
        _events = events;
    }

    public async Awaitable Show(IntentItemViewModel item)
    {
        Apply(item);
        await PlayReveal();
        _events?.RevealCompleted?.Invoke();
    }

    public async Awaitable Refresh(IntentItemViewModel item)
    {
        Apply(item);
        await PlayRefresh();
        _events?.RefreshCompleted?.Invoke();
    }

    public async Awaitable Trigger()
    {
        await PlayTrigger();
        _events?.TriggerCompleted?.Invoke();
    }
}
```

`AdventureView`는 `OnVisualTreeCloned`에서 widget을 찾고 bind한다.

```csharp
protected override void OnVisualTreeCloned()
{
    QueryWidgets();
    BindWidgets();
    BindCombatEvents();
}

private void BindWidgets()
{
    _intentBadge.Bind(_widgetEvents.IntentBadge);
}
```

`AdventureView`는 `IntentBadgeWidgetEvents.RevealCompleted`를 직접 Controller로 전달할 수 있다.

```text
IntentBadgeWidget
-> IntentBadgeWidgetEvents.RevealCompleted
-> AdventureView
-> AdventureSceneController.OnIntentRevealCompleted()
```

최종 결정은 A 방향이다.

```text
IntentBadgeWidgetEvents
- 개별 badge animation completion만 알린다.

AdventureView
- PlayIntentReveal(revealSequence) 내부에서 순차 reveal을 await한다.
- 전체 reveal sequence가 끝난 뒤 Controller.OnIntentRevealCompleted()를 호출한다.
```

따라서 개별 `RevealCompleted(cardId)` 하나가 전체 `IntentRevealCompleted`를 의미하지 않는다.

예상 흐름:

```csharp
private async void OnIntentRevealRequested(
    IReadOnlyList<MonsterIntentRevealViewModel> revealSequence)
{
    await PlayIntentReveal(revealSequence);
    _controller.OnIntentRevealCompleted();
}

private async Awaitable PlayIntentReveal(
    IReadOnlyList<MonsterIntentRevealViewModel> revealSequence)
{
    for (int i = 0; i < revealSequence.Count; i++)
    {
        MonsterIntentRevealViewModel item = revealSequence[i];
        AdventureMonsterCardWidget widget = _cardDealer.GetMonsterCard(item.CardId);
        await widget.ShowIntent(item.IntentItems);
    }
}
```

`IntentBadgeWidgetEvents.RevealCompleted`는 필요하면 `AdventureView` 내부 상태, 디버그, 개별 카드 animation chain에 사용할 수 있다. 하지만 PlayerTurn 입력 오픈 같은 전체 흐름 전환은 `PlayIntentReveal` 완료 기준으로 처리한다.

이 이벤트는 `AdventureWidgetEvents`에 속한다.

이유:

```text
- IntentBadgeWidgetEvents는 Flow -> View 요청이 아니다.
- Widget animation completion을 View에 알리는 widget-local event다.
- View는 이 completion을 받은 뒤 현재 프로젝트 흐름에 맞게 Controller로 전달한다.
```

주의:

```text
- IntentBadgeWidgetEvents는 widget completion event다.
- AdventureCombatEvents.IntentRevealRequested는 Flow -> View 요청 event다.
- 두 이벤트를 같은 객체에 섞지 않는다.
```

V1은 하나만 표시해도 된다.

```text
모델/런타임:
- 여러 item 지원

V1 View:
- 첫 번째 item만 표시 가능
```

숫자 표현은 `NumberValue + CountValue`를 사용한다.

```text
IntentDisplayData:
- NumberValue
- CountValue

IntentPresenter:
- NumberValue + CountValue를 NumberText로 변환

IntentItemViewModel:
- NumberText 보유

IntentBadgeWidget:
- NumberText 렌더링만 수행
```

표시 문자열 기본 규칙:

```text
NumberValue = 6, CountValue = 1 -> "6"
NumberValue = 3, CountValue = 2 -> "3x2"
NumberValue = 0, CountValue = 0 -> ""
```

## Adventure Scene Connection

Flow는 View를 직접 알지 않는다.

금지:

```text
AdventureTurnFlow -> AdventureView.PlayIntentReveal(...)
AdventureView -> AdventureTurnFlow.OnIntentRevealCompleted()
```

허용:

```text
AdventureTurnFlow
-> AdventureCombatEvents 요청 발행
-> AdventureView가 요청 수신
-> Widget/View animation 실행
-> Widget completion을 AdventureView가 수신
-> AdventureView가 AdventureSceneController에 전달
-> AdventureSceneController가 적절한 Flow로 위임
```

### Event Location

Intent reveal 요청 이벤트는 `Scenes.Adventure.Events.Flow.AdventureCombatEvents`에 둔다.

Intent reveal 완료 이벤트는 별도 scene event object로 만들지 않는다. Widget 또는 View animation completion은 `AdventureView`가 받고, `AdventureView`가 `AdventureSceneController`로 전달한다.

이유:

```text
Domains.Intent:
- 무엇을 보여줄지 계산
- 어떤 action을 실행해야 하는지 계약 보관

Scenes.Adventure:
- 계산된 intent를 실제 AdventureView에 어떻게 연결할지 조립
- combat presentation request를 AdventureCombatEvents로 발행
- View completion을 Controller로 전달
```

요청 이벤트:

```csharp
public sealed class AdventureCombatEvents
{
    public Action PlayerTurnBannerRequested;
    public Action<IReadOnlyList<MonsterIntentRevealViewModel>> IntentRevealRequested;
    public Action<uint> IntentTriggeredRequested;
    public Action EnemyTurnBannerRequested;
    public Action<ECombatEndResult> ResultRequested;
}
```

변경 결정:

```text
기존 EnemyTurnCompleted는 설계상 폐기한다.
다음 구현에서 EnemyTurnBannerRequested로 변경한다.
```

이름 규칙:

```text
Requested
- Flow -> View presentation request
- 예: PlayerTurnBannerRequested, IntentRevealRequested, IntentTriggeredRequested, EnemyTurnBannerRequested

Completed
- Widget/View animation completion
- View가 completion을 받아 Controller로 전달
```

`AdventureCombatEvents`의 역할:

```text
- Flow에서 AdventureView로 보내는 combat presentation request
- View animation을 직접 실행하지 않음
- View completion을 받지 않음
```

`AdventureWidgetEvents`의 역할:

```text
- Widget에서 AdventureView로 올라오는 입력 또는 animation completion
- Pouch clicked
- EndTurn clicked
- SkillSlot clicked
- IntentBadge reveal/refresh/trigger completed
```

`AdventureSceneController`의 역할:

```text
- AdventureView가 전달한 completion을 받아 적절한 Flow로 위임
- Flow와 View 사이의 직접 참조를 막는 중간 조정자
```

완료 흐름:

```text
PlayerTurnBannerCompleted
IntentRevealCompleted
EnemyTurnBannerCompleted

Widget 또는 View animation
-> AdventureView
-> AdventureSceneController
-> Flow
```

예상 `AdventureView` 흐름:

```csharp
public sealed partial class AdventureView
{
    private readonly AdventureCombatEvents _combatEvents;
    private readonly AdventureSceneController _controller;

    protected override void OnVisualTreeCloned()
    {
        QueryWidgets();
        BindCombatEvents();
        BindWidgetEvents();
    }

    private void BindCombatEvents()
    {
        _combatEvents.PlayerTurnBannerRequested += OnPlayerTurnBannerRequested;
        _combatEvents.IntentRevealRequested += OnIntentRevealRequested;
        _combatEvents.EnemyTurnBannerRequested += OnEnemyTurnBannerRequested;
        _combatEvents.ResultRequested += OnCombatResultRequested;
    }

    private async void OnIntentRevealRequested(
        IReadOnlyList<MonsterIntentRevealViewModel> revealSequence)
    {
        await PlayIntentReveal(revealSequence);
        _controller.OnIntentRevealCompleted();
    }
}
```

주의:

```text
- Flow는 AdventureView를 직접 알지 않는다.
- AdventureView는 Flow를 직접 호출하지 않는다.
- AdventureView는 현재 프로젝트 흐름에 맞게 AdventureSceneController에 완료를 전달한다.
- Completion 전용 scene event object를 추가하지 않는다.
```

## First Implementation Target

사용자는 첫 구현 범위로 `3번`을 선택했다.

```text
Adventure Intent Reveal 흐름까지 연결한다.
```

포함:

```text
- Intent 계산
- Presenter에서 reveal sequence 생성
- AdventureCombatEvents.IntentRevealRequested 발행
- AdventureView가 OnVisualTreeCloned에서 combat event를 bind
- AdventureMonsterCardWidget에 intent 표시
- Flow와 View는 직접 참조하지 않음
```

## Minimum Success Criteria

사용자는 첫 구현의 최소 성공 기준으로 `계약 검증 성공`을 선택했다.

첫 구현은 단순히 UI에 intent를 표시하는 데서 끝나면 안 된다.

완료 기준:

```text
1. PlayerTurn에 몬스터 카드에 Intent가 표시된다.
2. 표시된 Intent는 IntentRuntime에 cached ResolvedIntentActionData로 남아 있다.
3. EnemyTurn은 action을 다시 계산하지 않는다.
4. AdventureEnemyActionFlow는 action 시작 시 IntentTriggeredRequested(cardId)를 발행한다.
5. Intent trigger animation과 enemy action은 같은 시점에 시작될 수 있다.
6. AdventureEnemyActionFlow는 cached ResolvedIntentActionData.FinalActionModel을 실행한다.
7. 실행 후 Intent consume이 호출된다.
8. consume 결과로 sequence index가 advance된다.
9. 다음 PlayerTurn에서 advance된 sequence 기준으로 새 Intent가 표시된다.
```

허용하지 않는 완료 기준:

```text
- UI에 아이콘만 보이는 상태
- EnemyTurn에서 별도로 action을 다시 계산하는 상태
- 표시 데이터와 실행 데이터가 서로 다른 source에서 나오는 상태
- sequence advance가 IntentRuntime 밖에서 직접 수정되는 상태
```

## Planned Implementation Order

현재까지 확정된 구현 순서:

```text
1. Events 코드 정리
   - AdventureCombatEvents.IntentRevealRequested 추가
   - AdventureCombatEvents.IntentTriggeredRequested 추가
   - AdventureCombatEvents.EnemyTurnCompleted -> EnemyTurnBannerRequested 변경
   - IntentBadgeWidgetEvents 추가
   - AdventureWidgetEvents에 IntentBadgeWidgetEvents 추가

2. Intent Presentation ViewModel 추가
   - IntentItemViewModel
   - MonsterIntentRevealViewModel

3. IntentBadgeWidget 추가
   - Sprite icon 표시
   - NumberValue + CountValue 표시
   - reveal / refresh / trigger animation

4. Card UI 재설계 Phase A
   - AdventureChoiceCardWidget
   - AdventurePlayerCardWidget
   - AdventureMonsterCardWidget
   - 공통 AdventureCardFrame.uss
   - 역할별 UXML

5. Intent reveal 연결
   - AdventureCombatEvents.IntentRevealRequested
   - AdventureView.PlayIntentReveal(revealSequence)
   - AdventureMonsterCardWidget.ShowIntent(...)
   - 전체 sequence 완료 후 Controller.OnIntentRevealCompleted()

6. Intent trigger 연결
   - AdventureCombatEvents.IntentTriggeredRequested
   - AdventureMonsterCardWidget.TriggerIntent(...)
   - trigger animation은 enemy action을 block하지 않음

7. EnemyTurn 계약 연결
   - EnemyTurn은 cached ResolvedIntentActionData 실행
   - action 시작 시 trigger 요청
   - 실행 후 consume
   - sequence index advance
```

## Implementation Progress

현재 코드 반영 상태:

```text
완료:
- Domains.Intent.Data 생성
- Domains.Intent.Runtime 생성
- Domains.Intent.Flow 생성
- Domains.Intent.Execution 생성
- MonsterActionModel 추가
- IntentDisplayModel을 Sprite 직접 참조 ScriptableObject로 전환
- IntentDisplayDefinition / IntentNumberRuleModel / IntentOverrideRule 계열을 Domains.Intent.Data로 이동
- MonsterModel.ActionSequence를 MonsterActionModel[] 기준으로 전환
- ResolvedIntentActionData / IntentDisplayData / IntentNumberData / IntentRuntimeState를 Domains.Intent.Runtime으로 이동
- IntentRuntime 추가
- ActionExecutionBindingStore를 Domains.Intent.Execution으로 이동
- IntentActionResolver / IntentDisplayBuilder 추가
- IntentPrepareFlow / IntentRefreshFlow / IntentConsumeFlow 추가
- IntentItemViewModel / MonsterIntentRevealViewModel / IntentPresenter 추가
- AdventureSceneScope에 Intent Runtime / Flow / Execution 등록
- AdventureSceneScope에 IntentPresenter 등록
- IntentActionModel / EIntentAction / EIntentDisplay 제거
- 기존 Domains.Combat.Intent 제거
- IntentBadgeWidget 추가
- AdventureMonsterCardWidget 추가
- CardDealer가 CombatCardWidget 대신 AdventureMonsterCardWidget을 생성하도록 전환
- CombatCardWidget 제거
- AdventureCombatEvents.IntentRevealRequested / IntentTriggeredRequested 추가
- AdventureViewEventBinder가 Intent reveal / trigger request를 View에 연결
- AdventureView가 CardDealer를 통해 intent reveal / trigger animation 요청을 받을 수 있음
- 전투 인카운터 시작 후 IntentPrepareFlow.PrepareAll 실행
- 전투 인카운터 시작 후 IntentPresenter.CreateRevealSequence 결과로 IntentRevealRequested 발행
- 적 행동 시작 시 IntentTriggeredRequested 발행
- 적 행동 완료 시 IntentConsumeFlow.Consume 호출
- Monster_TestSlime용 첫 MonsterActionModel asset 추가
- IntentDisplay_Attack asset 추가
- IntentNumber_TestSlime_Attack asset 추가
- Monster_TestSlime.ActionSequence에 첫 action 연결

아직 남음:
- Intent icon Sprite 제작 및 IntentDisplay_Attack에 연결
- 나머지 MonsterActionModel / IntentDisplay asset 재작성
- EnemyTurn이 cached FinalActionModel을 직접 실행하도록 연결
- PlayerTurn 재진입 시점의 reveal 순서 정교화
- AdventurePlayerCardWidget / AdventureChoiceCardWidget 분리
```

## Open Questions

아직 구현 전에 결정이 필요한 항목:

```text
1. AdventureSceneController가 IntentRevealCompleted를 받은 뒤 어느 Flow method로 위임하는가
   - AdventureTurnFlow.OnIntentRevealCompleted
   - AdventureSceneController 내부에서 입력 오픈
   - 별도 PlayerInputFlow로 위임

2. AdventureMonsterCardWidget의 Intent UI 구조
   - 단일 아이콘 슬롯
   - 리스트 컨테이너
   - tooltip/detail 지원 여부

3. 기존 MonsterModel.ActionSequence asset들을 새 MonsterActionModel 구조로 어떻게 재작성할지

4. Intent dependency 이벤트를 V1에서 실제 자동 연결할지, 구조만 열어두고 수동 refresh로 시작할지
```

## Cold Assessment

이 설계의 핵심 리스크는 복잡도다.

```text
Intent가 UI 예고 + 실행 계약이 되면,
표시, refresh, 실행, consume 중 하나라도 빠지면 버그가 난다.
```

따라서 구현 시 금지해야 할 shortcut:

```text
- View가 직접 intent 계산
- EnemyTurn이 action을 다시 계산
- Flow가 View를 직접 호출
- IntentRuntime이 Ability를 직접 실행
- 모든 상태 변경마다 무조건 refresh
- Card 객체 참조를 runtime key로 사용
```

권장 순서:

```text
1. 기존 Intent 타입을 새 경계에 맞게 분류
2. CardId 기반 IntentRuntime 설계
3. ResolvedIntentActionData를 계약 객체로 확정
4. IntentDisplayData -> IntentViewModel projection 작성
5. Scenes.Adventure request/result event 작성
6. Binding으로 View 연결
7. AdventureMonsterCardWidget에 intent 표시/trigger 슬롯 추가
8. EnemyActionFlow가 cached ResolvedIntentActionData를 실행하도록 연결
9. Trigger와 Consume 분리 검증
10. Consume으로 sequence advance 검증
```
