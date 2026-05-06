# Addressables ModelTable 라벨 기반 로딩

## Related System

DB 시스템

## Context

런타임에서 DB 테이블 에셋들을 일관된 방식으로 로드할 필요가 있었다.

## Goal

DB 테이블 asset들을 하나의 로딩 규칙으로 묶고, 다른 시스템이 Addressables를 직접 다루지 않아도 `DBManager`를 통해 테이블에 접근하게 한다.

## Decision

DB 테이블 asset에 Addressables `ModelTable` 라벨을 부여하고, `DBManager`가 해당 라벨로 테이블들을 로드한다.

## Rationale

개별 테이블 경로나 이름을 직접 관리하기보다, DB용 테이블을 하나의 Addressables 라벨로 묶으면 런타임 로딩 진입점을 단순화할 수 있다. `DBManager`는 로드된 에셋 목록에서 필요한 테이블 타입을 찾아 프로퍼티로 보관한다.

## Current Implementation

```csharp
private const string ModelTableLabel = "ModelTable";

_tableHandle = Addressables.LoadAssetsAsync<Object>(ModelTableLabel, null);
IList<Object> assets = _tableHandle.WaitForCompletion();

Character = assets.OfType<CharacterTable>().First();
CharacterSkill = assets.OfType<CharacterSkillTable>().First();
Region = assets.OfType<RegionTable>().First();
```

## Consequences

- 테이블 로딩 책임이 `DBManager`로 모인다.
- 새 테이블을 추가할 때 Addressables 라벨과 `DBManager` 등록 규칙이 필요하다.
- 테이블 asset이 누락되면 `First()`에서 예외가 발생할 수 있다.
- `WaitForCompletion()` 사용으로 초기화 흐름이 동기적으로 막힐 수 있다.

## Check Needed

- `WaitForCompletion()`을 사용한 이유가 부트스트랩 단계에서 DB를 반드시 동기 준비하려는 의도였는지 확인 필요.
- 테이블 누락을 예외로 빠르게 드러내려는 의도였는지, 아직 방어 코드가 없는 초기 구현인지 확인 필요.

## Status

Accepted

## AI Reading Notes

이 결정은 DB 로딩 진입점을 설명한다. Addressables 설정 방법 자체는 이번 문서 범위가 아니다.

