# Unity Model / State 데이터 설계 원칙

## 현재 상태
- 구현된 것: `ScriptableObject` 기반 Model과 런타임 State가 일부 존재한다.
- 관련 파일:
  - `Assets/@Scripts/Data/AbstractModel.cs`
  - `Assets/@Scripts/Data/CharacterModel.cs`
  - `Assets/@Scripts/Data/MonsterModel.cs`
  - `Assets/@Scripts/Data/RegionModel.cs`
  - `Assets/@Scripts/Data/StageModel.cs`
  - `Assets/@Scripts/Domains/Adventure/AdventureSession.cs`
  - `Assets/@Scripts/Domains/Run/StageState.cs`

## 목적
Unity 데이터 설계에서 `Model`과 `State`를 왜 나누는지 설명하고, 새 데이터가 생겼을 때 어떤 기준으로 역할을 분리할지 판단할 수 있게 한다.

이 문서는 AAA 게임의 대규모 데이터 구조를 그대로 모방하기 위한 문서가 아니다. AAA가 데이터를 분리하는 의도를 이해한 뒤, 현재 프로젝트 규모에 맞게 축소 적용하기 위한 판단 기준이다.

## 핵심 원칙

### 1. 원본 정의와 런타임 상태를 분리한다
`Model`은 에디터에서 만든 원본 정의다. 런타임 중 변하지 않는 데이터를 가진다.

`State`는 플레이 중 변하는 현재 상태다. 저장/복원 대상이 될 수 있지만, 이 문서에서는 SaveManager, JSON, DTO 구조는 다루지 않는다.

```text
MonsterModel.MaxHp
→ 슬라임이라는 콘텐츠의 원본 체력

MonsterState.CurrentHp
→ 현재 전투 중인 슬라임 인스턴스의 체력
```

이 분리는 `ScriptableObject` 원본 데이터가 런타임 값으로 오염되는 것을 막기 위한 것이다.

### 2. 변경 이유가 다른 데이터는 분리한다
데이터를 나누는 가장 중요한 기준은 "왜 바뀌는가"다.

```text
몬스터 이름, 초상화, 기본 HP
→ 콘텐츠 정의가 바뀔 때 변경된다

현재 HP, 현재 버프, 사망 여부
→ 플레이 중 상태가 바뀔 때 변경된다

첫 스테이지는 몬스터만 등장
→ 진행 규칙이 바뀔 때 변경된다
```

변경 이유가 다른 데이터를 같은 곳에 넣으면 모델이 비대해지고, 수정 의도를 설명하기 어려워진다.

### 3. 콘텐츠와 규칙을 구분한다
콘텐츠는 "무엇이 존재하는가"를 설명한다.

```text
CharacterModel
MonsterModel
EventModel
ShopModel
RegionModel
StageModel
```

규칙은 "그 콘텐츠를 어떻게 고르고, 배치하고, 계산하는가"를 설명한다.

```text
첫 스테이지는 Monster만 등장한다
마지막 스테이지는 Boss만 등장한다
선택지는 항상 3장 표시한다
몬스터 카드 2장이 합쳐지면 엘리트 몬스터 카드가 된다
```

초기에는 단순 규칙을 가까운 Model에 둘 수 있다. 하지만 규칙이 재사용되거나, 조건문이 늘어나거나, 밸런싱 단위가 되면 `RuleModel` 후보로 분리한다.

### 4. AAA식 구조는 의도만 축소 적용한다
AAA 게임은 보통 원본 정의, 런타임 인스턴스, 규칙, UI 표시 데이터를 분리한다.

```text
Definition / Asset Data
Runtime Instance / State
Rule / System
Presentation / View Data
```

이유는 다음과 같다.

- 대량 콘텐츠를 관리해야 한다.
- 디자이너가 원본 데이터를 안전하게 편집해야 한다.
- 런타임 값이 원본 asset을 오염시키면 안 된다.
- 밸런스 규칙과 콘텐츠 정의의 변경 이유가 다르다.
- 저장/복원해야 할 현재 상태를 명확히 구분해야 한다.

우리 프로젝트는 이 구조를 모두 도입하지 않는다. 먼저 `Content Model`과 `Runtime State`를 명확히 나누고, 필요가 생길 때 `RuleModel`, `ViewData`, `Save DTO`를 점진적으로 분리한다.

## 권장 분류

### Content Model
게임에 존재하는 원본 콘텐츠를 정의한다.

판단 질문:
- 이 데이터는 게임 안에 존재하는 대상 자체를 설명하는가?
- Unity Inspector에서 기획자가 편집하는 원본 값인가?
- 런타임 중 변경되면 안 되는가?
- 여러 State나 시스템이 공유할 수 있는 정의인가?

예:
```text
CharacterModel
- 이름
- 초상화
- 기본 MaxHp
- 기본 CoinCount
- 기본 Skill 목록

MonsterModel
- 이름
- 초상화
- 기본 MaxHp
- Rank
```

### Rule Model
콘텐츠를 선택, 배치, 계산하는 정책을 정의한다.

판단 질문:
- 이 데이터는 "무엇인가"보다 "어떻게 할 것인가"를 설명하는가?
- 같은 규칙을 여러 Region, Stage, Encounter에서 재사용할 수 있는가?
- 규칙만 바꿔 밸런스를 조절하고 싶은가?
- 조건문이 Model 안에서 계속 늘어나고 있는가?

예:
```text
StageChoiceRule
- 첫 스테이지는 몬스터
- 마지막 스테이지는 보스
- 중간 선택지는 3장
- 몬스터 2장 병합 시 엘리트
```

초기에는 별도 `RuleModel`을 만들지 않아도 된다. 단순한 숫자나 정책은 콘텐츠 Model에 둘 수 있다. 다만 해당 값이 "콘텐츠의 정체성"이 아니라 "진행 방식"을 설명한다면, 이후 RuleModel로 분리될 수 있음을 명시한다.

### Runtime State
플레이 중 변하는 현재 상태를 보관한다.

판단 질문:
- 플레이 중 값이 바뀌는가?
- 저장/복원해야 할 수 있는 현재값인가?
- 같은 Model에서 생성된 여러 인스턴스가 서로 다른 값을 가져야 하는가?
- 원본 asset에 저장되면 안 되는 값인가?

예:
```text
PlayerState
- CharacterId
- CurrentHp
- CoinCount
- Skill 상태

MonsterState
- MonsterId
- CurrentHp
- Buffs

StageState
- StageId
- RemainingMonsterIds
- RemainingEventIds
- StageNumber
```

### Presentation Data
UI 표시를 위해 가공된 데이터다.

이 문서의 핵심 범위는 아니지만, 경계는 명확히 둔다.

판단 질문:
- 이 데이터는 화면 표시 편의를 위해 만들어졌는가?
- 텍스트, 아이콘, 색상, 표시 상태처럼 UI 중심인가?
- Model이나 State에 넣으면 게임 데이터가 UI에 오염되는가?

예:
```text
CardViewData
- DisplayName
- Portrait
- HpText
- IsTargetable
- VisualState
```

## 데이터 배치 체크리스트

새 필드나 새 클래스를 만들기 전에 아래 질문을 순서대로 확인한다.

```text
1. 이 데이터는 런타임 중 변하는가?
   - 예: State 후보
   - 아니오: Model 후보

2. 이 데이터는 존재 자체를 설명하는가?
   - 예: Content Model 후보

3. 이 데이터는 선택, 배치, 계산 방법을 설명하는가?
   - 예: Rule Model 후보

4. 이 데이터는 화면 표시 편의인가?
   - 예: Presentation Data 후보

5. 이 데이터가 바뀌는 이유는 무엇인가?
   - 콘텐츠 수정
   - 밸런스/규칙 수정
   - 플레이 진행
   - UI 표현 변경

6. 같은 이유로 바뀌는 데이터끼리 묶였는가?

7. 이 데이터는 누가 편집하는가?
   - 기획자 / 디자이너 / 프로그래머 / 런타임 시스템

8. 이 데이터는 저장될 수 있는 현재 상태인가?
   - 이 문서에서는 저장 구현은 다루지 않고, State 성격만 판단한다.

9. 이 데이터는 여러 곳에서 재사용되는가?

10. 지금 분리하지 않으면 나중에 왜 분리해야 하는지 설명할 수 있는가?
```

## 점진적 발전 기준

처음부터 모든 계층을 만들지 않는다. 구조는 아래 순서로 발전시킨다.

```text
1단계: Content Model과 Runtime State를 분리한다
2단계: 규칙이 커지면 RuleModel을 분리한다
3단계: UI 표시 요구가 커지면 ViewData를 분리한다
4단계: 저장 요구가 구체화되면 Save DTO를 분리한다
```

분리의 기준은 항상 "왜 분리하는가"다. 클래스 수를 늘리는 것이 목적이 아니라, 변경 이유와 책임을 설명 가능하게 만드는 것이 목적이다.

## 참고 자료
- Unity Manual: ScriptableObject  
  https://docs.unity.cn/Manual/class-ScriptableObject.html
- Unity: Architect game code with ScriptableObjects  
  https://unity.com/how-to/architect-game-code-scriptable-objects
- Game Programming Patterns: Type Object  
  https://gameprogrammingpatterns.com/type-object.html
- Microsoft: Tactical Domain-Driven Design  
  https://learn.microsoft.com/en-us/azure/architecture/microservices/model/tactical-domain-driven-design

## 할 일
- [ ] 현재 `CharacterModel`, `MonsterModel`, `RegionModel`, `StageModel`을 이 기준으로 다시 검토한다 — Done: 각 필드가 Content / Rule / State / Presentation 중 어디에 속하는지 분류됨
- [ ] 현재 `AdventureSession`, `StageState`를 이 기준으로 다시 검토한다 — Done: 각 State가 런타임 변화값만 가지는지 확인됨
- [ ] RuleModel 분리 후보를 표시한다 — Done: 아직 구현하지 않고 후보 목록만 정리됨

## 의존성
- 선행: 없음
