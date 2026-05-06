# ScriptableObject 기반 DB 선택

## Related System

DB 시스템

## Context

다양한 파일 형식으로 DB를 구성할 수 있었지만, 이 프로젝트에서는 ScriptableObject 기반 asset 구조를 선택했다. 이 선택은 특정 문제를 피하기 위한 결정이라기보다 Unity 개발 흐름에서 데이터 로드, 데이터 생성, 유지보수, 업데이트 가능성을 편하게 가져갈 수 있다는 기대를 바탕으로 한 결정이다.

## Goal

데이터 로드, 데이터 생성/수정 유지보수, AI 보조 생성, 배포 후 데이터 업데이트 가능성을 함께 고려한 DB 구조를 만든다.

## Considerations

- Addressables를 통해 데이터를 로드할 수 있는가
- Table asset을 한 번 로드했을 때 관련 row asset들을 함께 가져올 수 있는가
- Unity Editor에서 데이터 추가와 수정이 편한가
- AI를 통해 ScriptableObject class와 asset 생성 흐름을 보조받기 쉬운가
- C# 타입과 필드 구조를 유지하기 쉬운가
- Unity asset 참조를 직접 들고 있을 수 있는가
- Unity가 공식 제공하는 신뢰 가능한 구조인가
- 배포 이후 데이터 에셋 업데이트 가능성을 열어둘 수 있는가
- JSON/CSV/SQLite 같은 대안 대비 현재 프로젝트 규모에 적합한가

## Decision

Unity `ScriptableObject` 기반 row asset + Table asset 구조를 선택한다.

## Rationale

### 데이터 로드 측면

ScriptableObject 에셋은 Addressables 대상으로 관리할 수 있다. 이 프로젝트에서는 DB 테이블 에셋을 `ModelTable` 라벨로 묶고, `DBManager`가 해당 라벨을 통해 테이블들을 로드한다.

또한 Table asset을 한 번 로드하면, 그 Table이 참조하는 관련 데이터 row asset들을 함께 로드할 수 있다는 점을 기대했다.

### 데이터 추가 및 수정 측면

Unity Editor Inspector에서 데이터를 직접 만들고 수정할 수 있어 유지보수가 편할 것으로 판단했다. 별도 JSON/CSV 파서나 외부 데이터 변환 파이프라인 없이 Unity 안에서 데이터 구조와 값을 확인할 수 있다.

AI를 통해 ScriptableObject class와 데이터 asset 생성 흐름을 보조받을 수 있다는 점도 선택 이유에 포함된다. ScriptableObject는 Unity가 공식 제공하는 asset 구조이므로 Unity 프로젝트 안에서 신뢰할 수 있는 방식으로 판단했다.

### 데이터 업데이트 측면

배포 후 데이터 업데이트 가능성을 고려했다. 단, 이는 ScriptableObject 자체의 기능이 아니라 ScriptableObject 기반 DB 에셋을 Addressables 원격 콘텐츠로 관리할 수 있기 때문에 가능한 방향성이다. 실제 원격 업데이트를 사용하려면 Addressables remote catalog/content update 설정이 별도로 필요하다.

## Alternatives Considered

- JSON: 텍스트 diff와 외부 편집은 쉽지만 Unity asset 참조와 Inspector 편집성이 약하다.
- CSV: 대량 편집은 쉽지만 타입 안정성, 복합 데이터, Unity asset 참조 관리가 약하다.
- Excel/Google Sheets export: 기획 데이터 대량 편집에는 좋지만 별도 export/import 파이프라인이 필요하다.
- SQLite: 구조적이고 강력하지만 현재 Unity 에디터 중심 데이터 관리 목적에는 과할 수 있다.

## Consequences

- Unity Editor 기반 데이터 작성과 수정이 쉬워진다.
- Addressables와 결합해 런타임 로딩 구조를 만들기 쉽다.
- Unity asset 참조를 직접 보관하기 쉽다.
- AI를 통해 ScriptableObject 생성과 데이터 asset 작성 흐름을 보조받기 쉽다.
- 텍스트 기반 diff/merge와 대량 편집은 JSON/CSV보다 불편할 수 있다.
- Unity Editor와 asset serialization에 의존한다.

## Status

Accepted

## AI Reading Notes

이 결정은 "왜 ScriptableObject를 DB 형식으로 선택했는가"에 대한 핵심 근거다. 배포 후 데이터 업데이트는 반드시 Addressables 원격 콘텐츠와 결합된 가능성으로 설명해야 한다.
