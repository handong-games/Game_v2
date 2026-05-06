# DB 시스템

## Current Summary

DB 시스템은 Unity `ScriptableObject` 기반 row asset과 Table asset을 사용해 게임 데이터를 관리한다. Table asset은 Addressables `ModelTable` 라벨로 로드되며, `DBManager`가 `CharacterTable`, `CharacterSkillTable`, `RegionTable`에 대한 런타임 접근점을 제공한다.

## Current Responsibilities

- 게임 데이터를 코드에 직접 하드코딩하지 않고 asset 기반으로 관리한다.
- Table asset의 `_rows`를 공식 데이터 집합으로 사용한다.
- `_rows` 순서를 기준으로 런타임 ID를 부여한다.
- `_rows` 순서를 기준으로 generated enum을 생성한다.
- Addressables `ModelTable` 라벨로 DB 테이블 asset들을 로드한다.
- `DBManager`를 통해 다른 시스템이 주요 테이블에 접근하게 한다.

## Current Structure

```txt
ScriptableObject row asset
→ Table asset의 _rows에 등록
→ AbstractTable.OnEnable에서 _rows 순서로 row ID 부여
→ ModelEnumGenerator가 _rows 순서로 E* enum 생성
→ Table asset에 Addressables ModelTable 라벨 부여
→ DBManager가 ModelTable 라벨로 테이블 asset들을 로드
→ DBManager.Character / CharacterSkill / Region 프로퍼티로 접근
```

## Related Code

- `Assets/@Scripts/Core/Manager/DB/DBManager.cs`
- `Assets/@Scripts/Data/AbstractModel.cs`
- `Assets/@Scripts/Data/AbstractTable.cs`
- `Assets/@Scripts/Core/Manager/Editor/ModelEnumGenerator.cs`

## Related Assets

- `Assets/@Resources/Data/Database/CharacterTable.asset`
- `Assets/@Resources/Data/Database/CharacterSkillTable.asset`
- `Assets/@Resources/Data/Database/RegionTable.asset`

## Decision Log DB

이 Decision Log DB는 `Game Technical Wiki`의 최상위에 독립적으로 두지 않고, `데이터 테이블 시스템` 페이지 안에 둔다. 이 시스템을 이해할 때 필요한 의사결정 이력을 시스템 문맥 안에서 바로 추적하기 위한 구조다.

- `ScriptableObject 기반 DB 선택`
- `Row asset + Table asset 분리`
- `_rows 순서 기반 ID / enum 생성`
- `Addressables ModelTable 라벨 기반 로딩`

## Confirmed Facts

- `DBManager`는 Addressables `ModelTable` 라벨로 테이블 에셋들을 로드한다.
- `DBManager`는 로드된 에셋 목록에서 `CharacterTable`, `CharacterSkillTable`, `RegionTable` 타입을 찾아 프로퍼티에 보관한다.
- `AbstractTable.OnEnable()`은 `_rows` 순서에 따라 각 row의 ID를 설정한다.
- `ModelEnumGenerator`는 `_rows` 순서에 따라 `E*` enum을 생성한다.
- 기존 Character table 인터뷰 기록에는 `CharacterTable._rows` 순서가 `ECharacter` 생성 순서의 원본이라고 기록되어 있다.

## Confirmed Selection Drivers

- 데이터 로드 관점에서 Addressables API를 사용해 손쉽게 로드할 수 있다는 점을 기대했다.
- 데이터 로드 관점에서 Table asset을 한 번 로드하면 관련된 모든 데이터 row asset을 한 번에 로드할 수 있다는 점을 기대했다.
- 데이터 생성 구조 관점에서 AI를 통해 ScriptableObject 데이터 에셋을 만들고 생성할 수 있다는 점을 기대했다.
- 데이터 업데이트 관점에서 Addressables 원격 콘텐츠를 통해 배포 후 데이터 업데이트 가능성을 열어둘 수 있다는 점을 기대했다.
- ScriptableObject가 Unity에서 공식 제공되는 구조라 신뢰할 수 있다는 점을 고려했다.

## Inferred Rationale

- DB 시스템은 특정 문제 회피보다는 Unity 개발 흐름에서 데이터 관리와 로딩을 편하게 만들 수 있을 것이라는 기대를 바탕으로 선택된 구조다.
- `_rows` 순서와 enum 생성을 연결해 문자열 기반 참조보다 안전한 데이터 참조 방식을 만들려 한 것으로 보인다.

## Known Risks / Open Questions

- `_rows` 순서 변경은 generated enum 값 변경으로 이어질 수 있다.
- `First()` 기반 테이블 탐색은 테이블이 누락되었을 때 런타임 예외를 만들 수 있다.
- `WaitForCompletion()`을 사용한 이유가 부트스트랩 단계의 동기 초기화를 의도한 것인지 추가 확인이 필요하다.
- DB enum 값이 저장 데이터나 외부 참조와 연결될 경우 `_rows` 순서 변경 위험은 별도 문서로 다뤄야 한다.

## AI Reading Notes

- 현재 시스템 구조와 설계 의도를 분리해서 읽는다.
- 이 시스템의 Decision Log는 시스템 페이지 내부의 문맥으로 읽는다.
- 추정한 의도는 확정 사실처럼 말하지 않는다.
- 배포 후 데이터 업데이트 가능성은 ScriptableObject 단독 기능이 아니라 Addressables 원격 콘텐츠 배포와 결합된 방향성으로 설명한다.
- `_rows` 순서 변경은 ID/enum 변경 위험이 있으므로 주의한다.
