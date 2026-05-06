# _rows 순서 기반 ID / enum 생성

## Related System

DB 시스템

## Context

데이터 항목을 코드에서 안전하게 참조할 방법이 필요했다. 문자열 기반 참조는 오타나 이름 변경에 취약할 수 있으므로, 코드에서 사용할 enum과 런타임 데이터 ID를 맞추는 구조가 필요했다.

## Goal

Table asset의 `_rows` 순서를 기준으로 런타임 ID와 generated enum 값을 일치시킨다.

## Decision

`AbstractTable.OnEnable()`에서 `_rows` index를 row ID로 부여하고, `ModelEnumGenerator`가 같은 `_rows` 순서로 `E*` enum을 생성한다.

## Rationale

데이터 row의 런타임 ID와 코드 enum 값을 같은 순서로 맞추면 문자열 기반 참조보다 안전하게 데이터를 참조할 수 있다. Table asset의 `_rows`가 데이터 순서와 enum 생성의 단일 기준점이 된다.

## Evidence

- `AbstractTable.OnEnable()`은 `_rows` 순서대로 `row.SetId((TKey)index)`를 호출한다.
- `ModelEnumGenerator`는 Table asset의 `_rows` 순서대로 enum 값을 생성한다.
- 기존 Character table 인터뷰 기록에 `CharacterTable._rows` 순서는 `ECharacter` 생성 순서의 원본이라고 기록되어 있다.

## Consequences

- enum 기반 데이터 참조가 가능해진다.
- `_rows` 순서가 데이터 ID와 enum 값의 기준이 된다.
- `_rows` 순서 변경은 generated enum 값 변경으로 이어질 수 있다.
- 저장 데이터나 외부 참조가 enum 값에 의존하게 되면 순서 변경 위험이 커진다.

## Status

Accepted

## AI Reading Notes

`_rows` 순서는 단순 표시 순서가 아니라 ID/enum의 기준이다. 순서 변경은 데이터 호환성 위험으로 이어질 수 있으므로 주의해서 설명한다.

