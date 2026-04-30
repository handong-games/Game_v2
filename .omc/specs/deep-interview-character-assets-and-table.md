# Deep Interview Spec: Character Assets And Table Setup

## Metadata
- Interview ID: character-assets-table-20260425
- Rounds: 2
- Final Ambiguity Score: 12%
- Type: brownfield
- Generated: 2026-04-25T00:00:00+09:00
- Threshold: 20%
- Status: PASSED

## Clarity Breakdown
| Dimension | Score | Weight | Weighted |
|-----------|-------|--------|----------|
| Goal Clarity | 0.93 | 0.35 | 0.326 |
| Constraint Clarity | 0.90 | 0.25 | 0.225 |
| Success Criteria Clarity | 0.88 | 0.25 | 0.220 |
| Context Clarity | 0.94 | 0.15 | 0.141 |
| **Total Clarity** | | | **0.912** |
| **Ambiguity** | | | **0.088** |

## Goal
새 구조 기준으로 `CharacterModel` row asset들과 `CharacterTable` asset을 다시 만들고, `CharacterTable.asset`을 Addressables `ModelTable` 라벨로 등록한다.

## Constraints
- 이번 턴 작업 범위는 `Character`만 대상으로 한다.
- `CharacterSkillModel` 및 `CharacterSkillTable`은 이번 턴에 만들지 않는다.
- `CharacterModel.DefaultSkills`는 일단 빈 상태로 둔다.
- `CharacterTable._rows` 순서는 이후 `ECharacter` 생성 순서의 원본이 된다.

## Non-Goals
- 스킬 asset 재구성
- 카드 asset 재구성
- 진행 상태/잠금 해제 시스템 구현
- enum 생성기 실행 결과 검증

## Acceptance Criteria
- [ ] `CharacterModel` asset 3개가 존재한다.
- [ ] 생성 대상은 `Warrior`, `Mage`, `Rogue`다.
- [ ] 각 `CharacterModel`의 `DefaultSkills`는 비어 있다.
- [ ] `CharacterTable.asset` 1개가 존재한다.
- [ ] `CharacterTable._rows`에 `Warrior`, `Mage`, `Rogue`가 순서대로 등록되어 있다.
- [ ] `CharacterTable.asset`에 Addressables 라벨 `ModelTable`이 부여되어 있다.

## Assumptions Exposed & Resolved
| Assumption | Challenge | Resolution |
|------------|-----------|------------|
| Character와 Skill을 같이 재구성해야 할 수 있다 | 이번 턴 범위 확인 | Character만 진행 |
| 스킬 참조도 같이 채워야 할 수 있다 | CharacterModel 스킬 필드 처리 기준 확인 | DefaultSkills는 빈 상태 유지 |
| 완료 기준이 모호할 수 있다 | 생성/등록 대상 수량과 순서를 확인 | CharacterModel 3개 + CharacterTable 1개 + ModelTable 라벨 부여로 고정 |

## Technical Context
- 기존 `SO_CharacterDatabase.asset`, `SO_SkillDatabase.asset`, 예전 character row asset은 삭제된 상태다.
- 현재 코드 구조는 `CharacterModel`, `CharacterTable`, `DBManager`, `ModelTable` 라벨을 전제로 한다.
- `CharacterTable._rows` 순서는 이후 enum 생성기의 입력 순서가 된다.

## Ontology (Key Entities)
| Entity | Fields | Relationships |
|--------|--------|---------------|
| CharacterModel | `CharacterName`, `Illustration`, `InitialCoinCount`, `InitialMaxHp`, `DefaultSkills` | CharacterTable `_rows`에 등록되는 row asset |
| CharacterTable | `_rows` | CharacterModel들의 공식 집합 원본 |
| DBManager | `Character` | `ModelTable` 라벨의 CharacterTable을 로드 |

## Interview Transcript
<details>
<summary>Full Q&A (2 rounds)</summary>

### Round 1
**Q:** 이번 턴에서 Character만 만들지, Skill까지 같이 만들지?
**A:** Character만 / 대신 CharacterModel의 스킬은 일단 빈 상태로 둔다.
**Ambiguity:** 15.8%

### Round 2
**Q:** 완료 기준을 CharacterModel 3개 생성 + CharacterTable 1개 생성 + _rows 등록 + ModelTable 라벨 부여로 봐도 되는가?
**A:** 네.
**Ambiguity:** 8.8%

</details>
