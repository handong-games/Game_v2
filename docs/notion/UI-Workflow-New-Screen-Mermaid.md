# UI Workflow - 01 새 화면 추가 Mermaid 사양

## Diagram Name

`UI Workflow - 01 새 화면 추가`

## Node Rule

- 모든 노드는 번호를 가진다.
- 노드에는 번호와 짧은 단계명만 둔다.
- 상세 설명은 Notion의 같은 번호 섹션에서 관리한다.
- 위에서 아래로 읽는 세로형 사고 흐름으로 배치한다.
- Figma/FigJam을 사용하지 않아도 Notion 코드 블록과 Markdown 문서에서 원본을 유지한다.

## Role In The Page

- 실제 작업 중에는 `Workflow Table`을 먼저 본다.
- Mermaid는 전체 흐름 확인과 외부 도구 이동을 위한 보조 원본이다.
- 노드 번호, Workflow Table 번호, 상세 섹션 번호는 항상 일치해야 한다.

## Node List

```txt
01 이 UI는 독립 화면인가?
02 새 View가 필요한가?
03 BaseView 상속 View 생성
04 같은 이름의 UXML 생성
05 USS 작성
06 코드에서 제어할 요소가 있는가?
07 UXML 요소에 name 지정
08 OnBind에서 Root.Q로 바인딩
09 도메인 행동이 필요한가?
10 Controller 분리
11 이벤트 등록
12 Dispose에서 이벤트 해제
13 Addressables에 UXML 등록
14 진입 지점에서 View 생성
15 ViewManager.Push 호출
16 표시 / 닫기 / 재진입 확인
```

## Mermaid Source

```mermaid
flowchart TB
    n01(["01<br/>이 UI는 독립 화면인가?"])
    n02{"02<br/>새 View가 필요한가?"}
    n03["03<br/>BaseView 상속 View 생성"]
    n04["04<br/>같은 이름의 UXML 생성"]
    n05["05<br/>USS 작성"]
    n06(["06<br/>코드에서 제어할 요소가 있는가?"])
    n07["07<br/>UXML 요소에 name 지정"]
    n08["08<br/>OnBind에서 Root.Q로 바인딩"]
    n09{"09<br/>도메인 행동이 필요한가?"}
    n10["10<br/>Controller 분리"]
    n11["11<br/>이벤트 등록"]
    n12["12<br/>Dispose에서 이벤트 해제"]
    n13["13<br/>Addressables에 UXML 등록"]
    n14["14<br/>진입 지점에서 View 생성"]
    n15["15<br/>ViewManager.Push 호출"]
    n16(["16<br/>표시 / 닫기 / 재진입 확인"])

    n01 -->|"Yes"| n02
    n02 -->|"Yes"| n03
    n03 --> n04
    n04 --> n05
    n05 --> n06
    n06 -->|"Yes"| n07
    n07 --> n08
    n08 --> n09
    n09 -->|"Yes"| n10
    n10 --> n11
    n11 --> n12
    n12 --> n13
    n13 --> n14
    n14 --> n15
    n15 --> n16

    n01 -. "No: 기존 화면에 UI 요소 추가" .-> alt01["다른 워크플로우로 이동"]
    n06 -. "No: UXML + USS만 작성" .-> n09
    n09 -. "No: View 내부 단순 UI 처리" .-> n11

    subgraph legend ["Legend"]
      l1["01-02 상황 판단"]
      l2["03-05 View / UI Asset 생성"]
      l3["06-08 코드 바인딩"]
      l4["09-12 행동 연결"]
      l5["13-15 등록 / 진입"]
      l6["16 검증"]
    end

    subgraph notion ["Notion Detail"]
      d1["각 번호의 상세 설명은 Notion 문서에서 확인"]
      d2["작업 중에는 Workflow Table을 먼저 확인"]
    end

    classDef q fill:#EDE9FE,stroke:#7C3AED,stroke-width:2px,color:#111827;
    classDef dec fill:#FEF3C7,stroke:#D97706,stroke-width:2px,color:#111827;
    classDef act fill:#DBEAFE,stroke:#2563EB,stroke-width:2px,color:#111827;
    classDef chk fill:#DCFCE7,stroke:#16A34A,stroke-width:2px,color:#111827;
    classDef risk fill:#DBEAFE,stroke:#DC2626,stroke-width:4px,color:#111827;
    classDef side fill:#F3F4F6,stroke:#6B7280,stroke-width:1px,color:#111827;

    class n01,n06 q;
    class n02,n09 dec;
    class n03,n04,n05,n10,n11,n14,n15 act;
    class n07,n08,n12,n13 risk;
    class n16 chk;
    class alt01,l1,l2,l3,l4,l5,l6,d1,d2 side;
```

## Mermaid Usage Status

Figma/FigJam 생성은 보류한다.
Notion 페이지와 로컬 Markdown 문서에서 Mermaid 원본을 관리한다.
