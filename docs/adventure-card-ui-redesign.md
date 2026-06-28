# Adventure Card UI Redesign

## Purpose

이 문서는 Intent UI 구현 전에 Adventure 카드 UI 구조를 재정리하기 위한 설계 기록이다.

현재 `CombatCardWidget` 하나로 선택지 카드, 플레이어 카드, 몬스터 카드를 모두 감당하려 하면 IntentBadge, Health, CardFace의 존재 여부가 mode 분기로 섞인다. 큰 변경이 허용되므로 역할별 UXML/Widget으로 분리하는 방향을 우선 설계한다.

## Current Decision

역할별 카드 위젯을 만든다.

```text
AdventureChoiceCardWidget
AdventurePlayerCardWidget
AdventureMonsterCardWidget
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
```

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

현재 `IntentDisplay_Attack`의 Sprite는 임시로 비어 있을 수 있다. 이는 계약 흐름 검증을 먼저 하기 위한 예외이며, 최종 검증에서는 icon Sprite가 반드시 연결되어야 한다.

## CardDealer Direction

CardDealer는 런타임 decorator 조립을 하지 않는다.

대신 역할별 완성형 UXML template을 생성하고 bind한다.

```text
CreateChoiceCard()
-> AdventureChoiceCardWidget

CreatePlayerCard()
-> AdventurePlayerCardWidget

CreateMonsterCard()
-> AdventureMonsterCardWidget
```

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

```text
사용하지 않는 곳:
- CardDealer가 카드 widget 하나를 생성해 반환하는 흐름

사용 가능한 곳:
- 이미 존재하는 parent slot에 UXML 내용을 직접 붙이는 흐름
```

주의:

```text
Instantiate()는 TemplateContainer를 생성한다.
따라서 CardDealer가 실제로 다룰 root widget은 TemplateContainer 안에서 Q로 찾아 분리한다.
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
- CardDealer 변경량이 크다.
- 기존 CombatCardWidget / CardWidget 참조 정리가 필요하다.
- AbilitySystem avatar binding 위치를 다시 정해야 한다.
- Pooling/reuse 시 역할별 unbind 규칙이 필요하다.
```

## Implementation Phases

추천 단계:

```text
Phase A: Card UI 재설계
- 공통 AdventureCardFrame.uss 작성
- 역할별 Adventure*CardWidget UXML/CS 작성
- 기존 CardWidget/CardFace 역할 정리
- CardDealer가 역할별 widget을 생성하도록 변경

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
AdventureChoiceCardWidget.Create()
AdventurePlayerCardWidget.Create()
AdventureMonsterCardWidget.Create()
```

각 `Create()`는 `VisualTreeAsset.Instantiate()`를 사용한다.

```text
1. Addressables에서 VisualTreeAsset 로드
2. Instantiate() 호출
3. TemplateContainer에서 root widget Q
4. root widget RemoveFromHierarchy()
5. root widget 반환
```

`CloneTree(parent)`는 카드 위젯 하나를 생성해서 반환하는 흐름에서는 사용하지 않는다.

## Pending Before Implementation

구현 직전 확인할 항목:

```text
1. 기존 CardWidget을 바로 CardFaceWidget으로 rename할지
2. 기존 CombatCardWidget을 즉시 제거할지, 일정 기간 병행할지
3. CardDealer가 역할별 widget mapping을 어떻게 보관할지
4. AbilitySystem avatar를 어떤 widget에 bind할지
5. Pooling/reuse를 이번 phase에 포함할지
```

## Cold Assessment

이 방향은 단기 변경량이 크다.

하지만 `CombatCardWidget` 하나에 선택지/플레이어/몬스터 분기를 계속 넣는 것보다 장기 유지보수성이 좋다.

구현 중 가장 위험한 부분은 UI 재설계 문제와 Intent 계약 문제를 동시에 디버깅하게 되는 것이다. 따라서 실제 구현에서는 Card UI 재설계와 Intent 계약 연결을 분리해서 진행하는 편이 좋다.
