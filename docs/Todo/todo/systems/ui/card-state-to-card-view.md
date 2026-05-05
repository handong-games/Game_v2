# CardState 기반 실제 카드 UI 표시

## 현재 상태
- 구현된 것: `CardState`가 `AdventureEvents.CardsDrawn`을 통해 `AdventureView`와 `CardDealer`까지 전달됨
- 구현된 것: `CardDealer`는 현재 placeholder 카드를 생성하고 카드덱 위치에서 슬롯으로 이동시키는 연출을 담당함
- 관련 파일: `Assets/@Scripts/Domains/Adventure/CardState.cs`
- 관련 파일: `Assets/@Scripts/Domains/View/Adventure/CardDealer.cs`
- 관련 파일: `Assets/@Scripts/Domains/View/Common/Card.uxml`
- 관련 파일: `Assets/@Scripts/Domains/View/Common/Card.uss`

## 할 일
- [ ] `Card.uxml`을 실제 표시 구조로 확장 — Done: 카드 이름, 앞면/뒷면 표시 영역, portrait/icon 표시 영역이 UXML에 존재
- [ ] 플레이어 카드를 캐릭터 선택 화면 카드와 동일한 시각 규칙으로 표시 — Done: 선택 캐릭터의 이름, portrait, frame이 Adventure 카드에 표시됨
- [ ] 선택지 카드 앞면 표시 규칙 구현 — Done: 몬스터 선택지 앞면에서 이름, portrait, frame이 표시됨
- [ ] 선택지 카드 뒷면 표시 규칙 구현 — Done: Monster/Event/Shop/Boss 타입별 고정 이름, 타입 아이콘, frame이 표시됨
- [ ] `CardState.Face`에 따른 앞면/뒷면 class 적용 — Done: `Front`와 `Back` 상태가 같은 카드 인스턴스에서 서로 다른 시각 표현으로 전환됨
- [ ] `CardDealer`의 placeholder 생성 경로를 실제 카드 생성 경로로 교체 — Done: `CardDealer.DealAsync()`가 `CardState` 기반 카드 UI를 생성하고 기존 deal 이동 연출을 유지함

## 의존성
- 선행: 현재 1차 placeholder 검증 구현 (CardState가 UI까지 전달되는지 먼저 확인하기 위함)
