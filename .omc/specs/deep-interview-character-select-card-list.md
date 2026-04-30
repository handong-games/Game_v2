# Deep Interview Spec: CharacterSelect Card List Binding

## Metadata
- Interview ID: character-select-card-list-20260424
- Rounds: 3
- Final Ambiguity Score: 15%
- Type: brownfield
- Generated: 2026-04-24T17:34:21+09:00
- Threshold: 20%
- Status: PASSED

## Clarity Breakdown
| Dimension | Score | Weight | Weighted |
|-----------|-------|--------|----------|
| Goal Clarity | 0.84 | 0.35 | 0.294 |
| Constraint Clarity | 0.87 | 0.25 | 0.218 |
| Success Criteria Clarity | 0.82 | 0.25 | 0.205 |
| Context Clarity | 0.90 | 0.15 | 0.135 |
| **Total Clarity** | | | **0.852** |
| **Ambiguity** | | | **0.148** |

## Goal
`CharacterSelectView.OnBind()` 시점에 `CharacterSelectController`로부터 현재 캐릭터 목록을 조회하고, `card-list`에 카드들을 동적으로 생성하여 표시한다.

## Constraints
- 기존 `card-0`, `card-1`, `card-2` 고정 슬롯 구조는 제거하거나 더 이상 전제로 삼지 않는다.
- 카드 개수는 `CharacterSelectController`가 반환하는 캐릭터 개수에 따라 결정된다.
- 뷰는 컨트롤러가 반환한 `IReadOnlyList<SO_CharacterData>`를 직접 사용해 카드 UI를 채운다.
- `OnBind()` 직후에는 카드만 생성되어 있어야 하며, 어떤 카드도 자동 선택되지 않는다.
- 상세 패널과 시작 버튼은 선택 전 비활성/미표시 상태를 유지한다.

## Non-Goals
- 캐릭터 선택 이후 실제 게임 시작 런타임 플로우 연결
- `SO_CharacterData`를 별도 DTO 또는 뷰모델로 래핑하는 작업
- 캐릭터 데이터 소스 자체의 구조 변경

## Acceptance Criteria
- [ ] `CharacterSelectView.uxml`의 `card-list`는 고정 카드 슬롯 없이 동적 카드 컨테이너로 동작한다.
- [ ] `CharacterSelectView.OnBind()`는 `CharacterSelectController`에서 `IReadOnlyList<SO_CharacterData>`를 받아 카드 개수만큼 UI를 생성한다.
- [ ] 각 카드는 최소한 이름, 일러스트, 잠금 상태를 `SO_CharacterData` 기준으로 바인딩한다.
- [ ] `OnBind()` 직후 `_selectedIndex`는 미선택 상태이며 상세 패널 텍스트와 스킬 목록은 비어 있다.
- [ ] 잠금되지 않은 카드를 클릭하면 해당 캐릭터가 선택되고 상세 패널이 갱신된다.
- [ ] 잠긴 카드를 클릭하면 기존 잠금 피드백 동작이 유지된다.

## Assumptions Exposed & Resolved
| Assumption | Challenge | Resolution |
|------------|-----------|------------|
| 카드 슬롯은 3개 고정이어야 한다 | 동적 생성으로 재설계 가능한지 확인 | 컨트롤러 데이터 개수만큼 동적 생성으로 변경 |
| 첫 캐릭터를 자동 선택하는 편이 좋을 수 있다 | `OnBind()` 직후 기대 상태 확인 | 초기 상태는 카드만 표시되고 아무것도 선택되지 않음 |
| 뷰에 표시용 DTO가 필요하다 | 데이터 전달 책임을 다시 확인 | `SO_CharacterData`를 `IReadOnlyList`로 직접 전달하여 사용 |

## Technical Context
- `CharacterSelectView.cs`는 현재 `_cards = new VisualElement[3]` 기반의 고정 슬롯 캐싱과 `LoadCharacters()`/`ApplyCardData()` 흐름을 사용한다.
- `CharacterSelectView.uxml`은 현재 `card-list` 아래에 `card-0~2`를 직접 선언하고 있다.
- `CharacterSelectController.cs`는 이미 `GetAllCharacters()`를 통해 캐릭터 목록을 제공한다.
- 공용 카드 템플릿 `Assets/@Scripts/Domains/View/Common/Card.uxml`은 `card-illustration`, `card-lock`, `card-name` 구조를 갖고 있어 동적 생성 재사용에 적합하다.

## Ontology (Key Entities)
| Entity | Fields | Relationships |
|--------|--------|---------------|
| CharacterSelectView | `card-list`, `_cards`, `_selectedIndex`, `detail-panel` | 컨트롤러에서 받은 캐릭터 목록으로 카드 생성 및 선택 상태 관리 |
| CharacterSelectController | `GetAllCharacters()`, `SelectCharacter()`, `ResetSelection()` | 뷰에 캐릭터 데이터 공급, 선택 상태 유지 |
| SO_CharacterData | `CharacterName`, `Illustration`, `IsLocked`, `InitialMaxHp`, `InitialCoinCount`, `DefaultSkills` | 카드 표시와 상세 패널 표시의 원본 데이터 |
| CardTemplate | `card-illustration`, `card-lock`, `card-name` | 각 캐릭터 카드의 시각적 구조 |

## Interview Transcript
<details>
<summary>Full Q&A (3 rounds)</summary>

### Round 1
**Q:** `card-list`는 앞으로도 지금처럼 3개의 고정 슬롯을 유지할지, 아니면 `OnBind` 때 개수만큼 동적 생성할지?
**A:** 현재 구조를 재설계해도 되며, `OnBind` 때 컨트롤러가 준 개수만큼 카드를 동적으로 생성하는 구조로 바꿀 생각이다.
**Ambiguity:** 36.8%

### Round 2
**Q:** `OnBind` 직후 기대 화면은 카드만 생성된 미선택 상태인지, 첫 선택 가능 캐릭터 자동 선택 상태인지?
**A:** 카드들만 생성되어 있고 아무것도 선택되지 않은 상태다.
**Ambiguity:** 21.6%

### Round 3
**Q:** 컨트롤러가 DTO/뷰모델을 만들어 넘길지, `SO_CharacterData`를 직접 넘길지?
**A:** `SO_CharacterData`를 직접 사용해 카드 UI를 채우는 방향으로, `IReadOnlyList` 형태로 사용한다.
**Ambiguity:** 15%

</details>
