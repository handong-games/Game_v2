# DB 시스템 기술 문서 초안

## 문서 목적

이 문서는 DB 시스템을 왜 만들었는지, 어떻게 구현했는지, 왜 현재 구조를 선택했는지를 나중에 다시 이해하기 위한 기술 문서 초안이다.

이번 문서는 코드나 에셋을 수정하지 않는다. Notion 페이지도 아직 생성하지 않고, 이후 Notion의 `데이터 테이블 시스템` 페이지와 그 안의 `Decision Log DB`에 옮길 수 있는 초안으로 작성한다.

## 범위

포함한다:

- ScriptableObject 기반 row asset
- Table asset
- `_rows` 순서 기반 ID 부여
- enum 생성 흐름
- Addressables `ModelTable` 라벨 기반 로딩
- `DBManager`의 테이블 보관 구조
- 현재 확인된 테이블: `CharacterTable`, `CharacterSkillTable`, `RegionTable`
- 새 테이블을 추가할 때 지켜야 할 규칙 초안

제외한다:

- 실제 코드 수정
- 새 DB 테이블/에셋 생성
- 밸런스 수치나 개별 데이터 값의 상세 설명
- 세이브 시스템 연동
- Addressables 설정 방법
- Notion 페이지 직접 생성

## 현재 구조 요약

DB 시스템은 Unity `ScriptableObject`를 데이터 row와 table의 기본 단위로 사용한다.

현재 코드 기준으로 확인되는 흐름은 다음과 같다.

```txt
ScriptableObject row asset
→ Table asset의 _rows에 등록
→ AbstractTable.OnEnable에서 _rows 순서로 row ID 부여
→ ModelEnumGenerator가 _rows 순서로 E* enum 생성
→ Table asset에 Addressables ModelTable 라벨 부여
→ DBManager가 ModelTable 라벨로 테이블 asset들을 로드
→ DBManager.Character / CharacterSkill / Region 프로퍼티로 접근
```

## 주요 파일

| 파일 | 역할 |
| --- | --- |
| `Assets/@Scripts/Core/Manager/DB/DBManager.cs` | Addressables로 테이블 에셋을 로드하고 런타임 접근점을 제공 |
| `Assets/@Scripts/Data/AbstractModel.cs` | row asset의 공통 기반. enum ID와 이름 보관 |
| `Assets/@Scripts/Data/AbstractTable.cs` | table asset의 공통 기반. `_rows`와 ID 부여 규칙 보관 |
| `Assets/@Scripts/Core/Manager/Editor/ModelEnumGenerator.cs` | table `_rows` 순서에서 enum 코드 생성 |
| `Assets/@Resources/Data/Database/CharacterTable.asset` | Character 테이블 |
| `Assets/@Resources/Data/Database/CharacterSkillTable.asset` | CharacterSkill 테이블 |
| `Assets/@Resources/Data/Database/RegionTable.asset` | Region 테이블 |

## 왜 DB 시스템을 만들었는가

확정된 사실:

- 캐릭터, 캐릭터 스킬, 지역 같은 게임 데이터를 코드에 직접 하드코딩하지 않고 asset 기반으로 관리한다.
- 런타임에서는 `DBManager`를 통해 주요 테이블에 접근한다.
- 테이블의 `_rows` 순서는 런타임 ID와 generated enum의 기준으로 사용된다.

확인된 선택 동기:

- 데이터 로드 관점에서 Addressables API를 사용하여 손쉽게 로드할 수 있다는 점을 기대했다.
- 데이터 로드 관점에서 Table asset을 한 번 로드하면 관련된 모든 데이터 row asset을 한 번에 로드할 수 있다는 점을 기대했다.
- 데이터 생성 구조 관점에서 AI를 통해 ScriptableObject 데이터 에셋을 만들고 생성할 수 있다는 점을 기대했다.
- 데이터 업데이트 관점에서 Addressables 원격 콘텐츠를 통해 배포 후 데이터 업데이트 가능성을 열어둘 수 있다는 점을 기대했다.
- ScriptableObject가 Unity에서 공식 제공되는 구조라 신뢰할 수 있다는 점을 고려했다.

추정한 보조 의도:

- 게임 데이터의 공식 집합을 코드 밖에서 관리하려고 한 것으로 보인다.
- enum 생성기를 통해 데이터 순서와 코드 참조를 맞춰, 문자열 기반 참조보다 안전한 접근 방식을 만들려고 한 것으로 보인다.

확인 필요:

- 이 구조를 처음 도입할 때 JSON/CSV/SQLite 등과 직접 비교했는지 여부.
- 모든 게임 데이터가 이 구조로 이동할 예정이었는지, 핵심 데이터만 대상으로 했는지 여부.

## 왜 ScriptableObject를 선택했는가

ScriptableObject 기반 DB를 선택한 이유는 세 축으로 정리한다.

### 1. 데이터 로드 측면

ScriptableObject 에셋은 Addressables 대상으로 관리할 수 있다.

이 프로젝트에서는 `DBManager`가 `ModelTable` 라벨을 가진 에셋들을 로드하고, 그중 필요한 테이블 타입을 찾아 보관한다.

```csharp
_tableHandle = Addressables.LoadAssetsAsync<Object>(ModelTableLabel, null);
IList<Object> assets = _tableHandle.WaitForCompletion();

Character = assets.OfType<CharacterTable>().First();
CharacterSkill = assets.OfType<CharacterSkillTable>().First();
Region = assets.OfType<RegionTable>().First();
```

추정한 의도:

- 개별 테이블을 하나씩 직접 경로로 로드하기보다, DB용 테이블을 하나의 Addressables 라벨로 묶어 로드하려고 한 것으로 보인다.
- 테이블이 추가되더라도 같은 라벨 규칙을 따르면 `DBManager` 로딩 흐름에 편입할 수 있게 하려는 방향으로 보인다.
- Table asset을 한 번 로드하면 그 Table이 참조하는 관련 row asset까지 함께 로드되는 구조를 기대했다.

### 2. 데이터 추가 및 수정 측면

ScriptableObject는 Unity Editor Inspector에서 직접 만들고 수정할 수 있다.

이 선택의 장점:

- 별도 JSON/CSV 파서 없이 Unity 에디터 안에서 데이터 작성 가능
- Unity asset 참조를 직접 보관하기 쉬움
- 데이터 구조가 C# 타입과 연결되므로 필드 구조를 코드로 관리하기 쉬움
- 게임 개발 중 빠르게 데이터 항목을 추가하고 확인하기 쉬움
- AI를 통해 ScriptableObject class와 asset 생성 흐름을 보조받기 쉬움
- Unity가 공식 제공하는 asset 구조라 신뢰할 수 있음

감수한 단점:

- 텍스트 기반 diff와 merge는 JSON/CSV보다 불편할 수 있음
- 대량 편집은 스프레드시트 방식보다 불편할 수 있음
- Unity Editor와 asset serialization에 의존함

### 3. 배포 후 데이터 업데이트 측면

배포 후 데이터 업데이트 가능성도 고려했다.

단, 이 가능성은 ScriptableObject 자체의 기능이 아니라, ScriptableObject 기반 DB 에셋을 Addressables 원격 콘텐츠로 관리할 수 있기 때문에 가능한 방향성이다.

정확한 표현:

```txt
배포 후 데이터 업데이트 가능성을 고려했다.
단, 이는 ScriptableObject 자체의 기능이 아니라,
ScriptableObject 기반 DB 에셋을 Addressables 원격 콘텐츠로 관리할 수 있기 때문에 가능한 방향성이다.
실제 원격 업데이트를 사용하려면 Addressables remote catalog/content update 설정이 별도로 필요하다.
```

## Row asset과 Table asset 분리

확정된 사실:

- `AbstractModel<TKey>`는 개별 데이터 row의 공통 기반이다.
- `AbstractTable<TModel, TKey>`는 여러 row asset을 `_rows` 리스트로 보관한다.
- `CharacterTable`, `CharacterSkillTable`, `RegionTable`은 각각 전용 table asset이다.

추정한 의도:

- row asset은 개별 데이터 항목을 표현하고, table asset은 그 항목들의 공식 집합을 표현한다.
- table asset이 공식 집합이기 때문에, 어떤 데이터가 런타임 DB에 포함되는지 `_rows`로 명확히 통제할 수 있다.

## `_rows` 순서의 의미

`AbstractTable.OnEnable()`은 `_rows` 순서를 기준으로 각 row의 ID를 설정한다.

```csharp
for (int i = 0; i < _rows.Count; i++)
{
    if (_rows[i] == null)
        continue;

    _rows[i].SetId((TKey)Enum.ToObject(typeof(TKey), i));
}
```

`ModelEnumGenerator`도 같은 `_rows` 순서로 enum 값을 생성한다.

```txt
_rows[0] → ECharacter.Warrior = 0
_rows[1] → ECharacter.Mage = 1
_rows[2] → ECharacter.Rogue = 2
```

확정된 사실:

- 과거 Character table 인터뷰 기록에도 `CharacterTable._rows` 순서는 이후 `ECharacter` 생성 순서의 원본이라고 적혀 있다.

추정한 의도:

- 데이터 row의 런타임 ID와 코드 enum 값을 같은 순서로 맞추려는 구조다.
- 문자열 키보다 enum 기반 접근을 선호해 참조 안정성을 높이려 한 것으로 보인다.

주의점:

- `_rows` 순서 변경은 generated enum 값 변경으로 이어질 수 있다.
- 이미 저장 데이터나 외부 참조가 enum 값에 의존한다면 순서 변경은 위험할 수 있다.
- 이번 문서에서는 세이브 시스템 연동은 제외하지만, 향후 이 위험은 별도 의사결정 기록으로 다룰 필요가 있다.

## DBManager 역할

`DBManager`는 런타임에서 DB 테이블에 접근하는 중앙 진입점이다.

현재 확인된 프로퍼티:

```csharp
public CharacterTable Character { get; private set; }
public CharacterSkillTable CharacterSkill { get; private set; }
public RegionTable Region { get; private set; }
```

초기화 흐름:

```txt
OnInit
→ Addressables.LoadAssetsAsync<Object>("ModelTable", null)
→ WaitForCompletion
→ CharacterTable / CharacterSkillTable / RegionTable 타입 검색
→ 각 프로퍼티에 저장
```

해제 흐름:

```txt
OnDispose
→ 프로퍼티 null 처리
→ Addressables handle이 유효하면 Release
→ handle 초기화
```

추정한 의도:

- DB 테이블 로딩 책임을 한 곳에 모으려는 구조다.
- 다른 시스템은 Addressables를 직접 다루지 않고 `DBManager`의 프로퍼티를 통해 테이블에 접근하게 하려는 의도로 보인다.

확인 필요:

- `WaitForCompletion()`을 사용한 이유가 부트스트랩 단계에서 DB를 동기적으로 준비하기 위함인지 확인 필요.
- 테이블 누락 시 `First()`가 예외를 던지는 구조를 의도적으로 선택했는지 확인 필요.

## 새 테이블 추가 규칙 초안

새 테이블을 추가할 때는 다음 흐름을 따른다.

```txt
1. 새 Model class 작성
2. 새 Table class 작성
3. Model row asset 생성
4. Table asset 생성
5. Table asset의 _rows에 row asset 등록
6. Table asset에 ModelTable 라벨 부여
7. DBManager에 새 Table 프로퍼티 추가
8. DBManager.OnInit에서 새 Table 타입 할당
9. ModelEnumGenerator 실행
10. generated enum 확인
```

이번 문서에서는 실제 추가 작업은 하지 않는다.

## 의사결정 기록 후보

Notion으로 옮길 때는 `Decision Log DB`를 `Game Technical Wiki` 최상위에 독립적으로 두지 않고, `데이터 테이블 시스템` 페이지 안에 둔다. 아래 항목은 해당 시스템 페이지 내부의 결정 기록으로 만들 수 있다.

### 1. ScriptableObject 기반 DB 선택

- 왜: Addressables 로드, Unity Editor 유지보수, 배포 후 데이터 업데이트 가능성 고려
- 대안: JSON, CSV, Excel/Google Sheets export, SQLite
- 결과: Unity asset 기반 데이터 작성과 런타임 로딩 구조를 채택

### 2. Row asset + Table asset 분리

- 왜: 개별 데이터 항목과 공식 데이터 집합을 분리하기 위해
- 결과: Table `_rows`가 런타임 DB 포함 여부와 순서의 기준이 됨

### 3. `_rows` 순서 기반 ID / enum 생성

- 왜: 데이터 순서와 코드 enum을 일치시키기 위해
- 결과: enum 기반 참조 가능, 하지만 순서 변경 위험 존재

### 4. Addressables `ModelTable` 라벨 기반 로딩

- 왜: DB 테이블들을 라벨 단위로 묶어 로드하기 위해
- 결과: DBManager가 라벨로 테이블 에셋을 로드하고 타입별로 보관

## AI Reading Notes

AI가 이 문서를 읽을 때 지켜야 할 점:

- `데이터 테이블 시스템` 페이지에는 현재 DB 시스템 구조를 요약한다.
- `Decision Log DB`는 `데이터 테이블 시스템` 페이지 내부에 둔다.
- 해당 `Decision Log DB`에는 왜 ScriptableObject, Table asset, `_rows` 순서, Addressables 라벨 구조를 선택했는지 기록한다.
- 추정한 의도는 확정 사실처럼 말하지 않는다.
- 배포 후 데이터 업데이트는 ScriptableObject 단독 기능이 아니라 Addressables 원격 콘텐츠 배포와 결합된 가능성으로 설명한다.
- `_rows` 순서 변경은 ID/enum 변경 위험이 있으므로 주의한다.

## 참고 링크

- Unity Addressables content update workflow: https://docs.unity.cn/Packages/com.unity.addressables%401.17/manual/ContentUpdateWorkflow.html
- Unity Addressables content catalogs: https://docs.unity.cn/Packages/com.unity.addressables%402.2/manual/build-content-catalogs.html
- Unity Addressables remote content distribution: https://docs.unity.cn/Packages/com.unity.addressables%401.19/manual/RemoteContentDistribution.html
