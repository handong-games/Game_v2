# Deep Interview Spec: Character Select Controller Flow

## Metadata
- Interview ID: character-select-controller-flow-20260425
- Rounds: 3
- Final Ambiguity Score: 8%
- Type: brownfield
- Generated: 2026-04-25T00:00:00+09:00
- Threshold: 20%
- Status: PASSED

## Clarity Breakdown
| Dimension | Score | Weight | Weighted |
|-----------|-------|--------|----------|
| Goal Clarity | 0.94 | 0.35 | 0.329 |
| Constraint Clarity | 0.92 | 0.25 | 0.230 |
| Success Criteria Clarity | 0.88 | 0.25 | 0.220 |
| Context Clarity | 0.94 | 0.15 | 0.141 |
| **Total Clarity** | | | **0.920** |
| **Ambiguity** | | | **0.080** |

## Goal
`CharacterSelectController`, `CharacterSelectView`, `DBManager`의 역할을 정리해, 캐릭터 조회/잠금 판단/선택 처리를 새 정적 데이터 및 저장 구조에 맞게 재구성한다.

## Constraints
- 이번 단계는 `캐릭터 조회 + 잠금 판단 + 선택 처리`까지만 다룬다.
- `CharacterSelectController`가 `SaveManager.Instance.Progress`를 직접 보고 잠금 여부를 판단한다.
- `CharacterModel`은 잠금 상태를 알지 않는다.
- `CharacterSelectView`는 직접 DB나 Save를 참조하지 않고 controller 결과만 사용한다.
- `DBManager.Instance.Character`는 `CharacterTable`을 통해 캐릭터 row 목록을 제공한다.

## Non-Goals
- 시작 버튼 활성/비활성 전체 흐름 마무리
- 상세 패널 표시 조건 마무리
- 잠긴 캐릭터 피드백 연출 전체 정리
- 스킬/카드 쪽 구조 확장

## Acceptance Criteria
- [ ] `CharacterSelectController`는 `DBManager.Instance.Character.GetAll()`로 캐릭터 목록을 조회한다.
- [ ] `CharacterSelectController`는 `SaveManager.Instance.Progress`를 이용해 `IsUnlocked`/`IsSelectable`를 판단한다.
- [ ] `CharacterModel`에는 잠금 상태 필드가 없다.
- [ ] `CharacterSelectView`는 잠금 여부를 controller API를 통해서만 판단한다.
- [ ] 선택 가능한 캐릭터만 `SelectCharacter`에서 `_selectedCharacter`로 설정된다.
- [ ] `CharacterSelectView`의 캐릭터 목록 바인딩은 controller가 제공한 목록 기준으로 동작한다.

## Assumptions Exposed & Resolved
| Assumption | Challenge | Resolution |
|------------|-----------|------------|
| 잠금 여부는 모델 안에 남겨둘 수도 있다 | `CharacterModel.IsLocked`를 유지할지 질문 | 완전히 제거 |
| 잠금 판단은 별도 UnlockState가 필요할 수 있다 | 누가 판단할지 질문 | `CharacterSelectController`가 직접 `SaveManager.Instance.Progress`를 본다 |
| 다음 단계에서 화면 전체 흐름까지 마무리해야 한다 | 이번 단계 범위 질문 | 캐릭터 조회/잠금 판단/선택 처리까지만 다룸 |

## Technical Context
- `DBManager`는 `CharacterTable`을 preload한 뒤 `DBManager.Instance.Character.GetAll()`을 제공한다.
- `ProgressState`는 `ECharacter` 기반 해금 상태를 보유한다.
- `CharacterSelectView`는 현재도 controller를 주입받고 있으며, 잠금 판단 로직만 controller 위임으로 전환하면 된다.

## Ontology (Key Entities)
| Entity | Fields | Relationships |
|--------|--------|---------------|
| CharacterModel | `CharacterName`, `Illustration`, `InitialCoinCount`, `InitialMaxHp`, `DefaultSkills` | 정적 캐릭터 row asset |
| CharacterTable | `_rows` | 캐릭터 원본 목록 |
| DBManager | `Character` | `CharacterTable` 접근 진입점 |
| ProgressState | `CharacterUnlockIds` | 해금 여부의 진실 원본 |
| CharacterSelectController | `_selectedCharacter` | 캐릭터 조회/잠금 판단/선택 처리 |
| CharacterSelectView | `_characters` | controller가 넘긴 캐릭터를 렌더링 |

## Interview Transcript
<details>
<summary>Full Q&A (3 rounds)</summary>

### Round 1
**Q:** 다음 단계는 `SaveManager` 정리와 `CharacterSelect` 흐름 정리 중 어느 쪽을 우선할까?
**A:** `CharacterSelectController/View/DBManager` 흐름을 먼저 정리한다.
**Ambiguity:** 15.8%

### Round 2
**Q:** 다음 단계는 어디까지 다룰까?
**A:** `캐릭터 조회 + 잠금 판단 + 선택 처리`까지 먼저 한다.
**Ambiguity:** 13.3%

### Round 3
**Q:** 잠금 여부 판단은 누가 맡을까?
**A:** `CharacterSelectController`가 `SaveManager.Instance.Progress`를 직접 보고 `IsSelectable()` / `IsUnlocked()`를 판단한다.
**Ambiguity:** 8.0%

</details>
