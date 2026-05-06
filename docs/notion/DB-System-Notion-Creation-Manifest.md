# DB 시스템 Notion 생성 매니페스트

## 생성 대상

```txt
Game Technical Wiki
└─ 데이터 테이블 시스템
   └─ Decision Log DB
      ├─ ScriptableObject 기반 DB 선택
      ├─ Row asset + Table asset 분리
      ├─ _rows 순서 기반 ID / enum 생성
      └─ Addressables ModelTable 라벨 기반 로딩
```

## 생성 방식

- 먼저 `Game Technical Wiki`를 만든다.
- 그 아래에 `데이터 테이블 시스템` 시스템 페이지를 만든다.
- `데이터 테이블 시스템` 페이지 안에 `Decision Log DB` 섹션 또는 하위 DB를 만든다.
- `Decision Log DB` 안에 4개의 Decision Log 페이지를 만든다.
- 이번 생성은 Notion DB schema 생성이 아니라 page 기반 초안 생성이다.
- 이후 시스템 문서가 많아지면 `Game Technical Wiki` 홈에 시스템 인덱스 또는 linked database view를 추가한다.

## 로컬 원본 파일

- `docs/notion/DB-System-SystemsDB-Page.md`
- `docs/notion/DecisionLog-ScriptableObject-DB.md`
- `docs/notion/DecisionLog-Row-Table.md`
- `docs/notion/DecisionLog-Rows-ID-Enum.md`
- `docs/notion/DecisionLog-Addressables-ModelTable.md`

## Notion 생성 결과

- Game Technical Wiki: https://www.notion.so/356bca7e68b580cda557d58f9a319eb5
- Systems DB: deleted
- 데이터 테이블 시스템: https://www.notion.so/356bca7e68b58136a5a3feb0a1352c7e
- Decision Log DB: https://www.notion.so/356bca7e68b5815d8c5df394b0943ca9
- ScriptableObject 기반 DB 선택: https://www.notion.so/356bca7e68b581e5b8f6c621686dac51
- Row asset + Table asset 분리: https://www.notion.so/356bca7e68b58107b0a3f810b082387f
- _rows 순서 기반 ID / enum 생성: https://www.notion.so/356bca7e68b581b3918cd52bd603436f
- Addressables ModelTable 라벨 기반 로딩: https://www.notion.so/356bca7e68b581b1bfe5daace920a505
