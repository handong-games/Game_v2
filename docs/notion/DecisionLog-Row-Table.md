# Row asset + Table asset 분리

## Related System

DB 시스템

## Context

개별 데이터 항목과 런타임에서 사용할 공식 데이터 집합을 구분할 필요가 있었다.

## Goal

각 데이터 항목은 독립적인 asset으로 관리하면서, 런타임에 포함되는 공식 목록은 Table asset에서 통제한다.

## Decision

개별 데이터는 ScriptableObject row asset으로 만들고, 공식 데이터 목록은 Table asset의 `_rows`에 등록한다.

## Rationale

row asset은 개별 데이터 항목을 표현한다. Table asset은 그 row asset들을 모아 런타임 DB에 포함되는 공식 집합을 표현한다. 이 구조를 사용하면 데이터 항목 자체와 "현재 사용할 데이터 목록"을 분리할 수 있다.

## Evidence

- `AbstractModel<TKey>`는 개별 row asset의 공통 기반이다.
- `AbstractTable<TModel, TKey>`는 여러 row asset을 `_rows` 리스트로 보관한다.
- 현재 `CharacterTable`, `CharacterSkillTable`, `RegionTable` asset이 존재한다.

## Consequences

- 어떤 데이터가 실제 DB에 포함되는지 Table asset에서 명확히 통제할 수 있다.
- Table asset의 `_rows` 관리가 중요해진다.
- row asset을 만들어도 Table asset에 등록하지 않으면 공식 DB 집합에 포함되지 않을 수 있다.

## Status

Accepted

## AI Reading Notes

시스템을 설명할 때 row asset은 "개별 데이터 항목", Table asset은 "공식 데이터 집합"으로 구분한다.

