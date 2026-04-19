# STS2 화면 비율 공통 프레임워크

## 현재 상태
- 구현된 것: `GraphicManager`가 fullscreen/windowed 적용, 해상도 목록, 창 위치/디스플레이 동기화를 담당한다.
- 관련 파일: `Assets/@Scripts/Core/Manager/Garphic/GraphicManager.cs:9`
- 관련 파일: `Assets/@Scripts/Core/Manager/Garphic/GraphicManagerBehaviour.cs:5`
- 구현된 것: `SettingsView`와 `TitleView`가 `SharedViewRoot`에 공통 aspect 클래스를 적용해 반응형 상태를 일부 소비한다.
- 관련 파일: `Assets/@Scripts/Core/Manager/View/BaseView.cs:39`
- 관련 파일: `Assets/@Scripts/Domains/Settings/View/SettingsView.cs:99`
- 관련 파일: `Assets/@Scripts/Domains/View/TitleView/TitleView.cs:33`
- 구현된 것: STS2 전역/화면별 비율 처리 방식에 대한 디컴파일 분석 자료가 확보됐다.
- 관련 파일: `C:/Users/reg24/AppData/Local/Temp/sts2_NGame_decompiled.cs:501`
- 관련 파일: `C:/Users/reg24/AppData/Local/Temp/sts2_NAspectRatioDropdown.cs:39`
- 관련 파일: `C:/Users/reg24/AppData/Local/Temp/sts2_aspect_search.txt:1`

## 할 일
- [ ] `SharedViewRoot` 아래 STS2식 `letterbox` / `content frame` 구조 정의 — Done: 전역 UXML/USS 구조만으로 검은 여백과 중앙 콘텐츠 영역이 분리됨
- [ ] 현재 비율 bucket(`narrow/standard/ultrawide`)과 기준 콘텐츠 크기(`1680x1260`, `1680x1080`, `2580x1080`)를 공통 규약으로 정리 — Done: View가 동일한 기준 클래스를 공유하고 개별 계산을 하지 않음
- [ ] `SettingsView`를 공통 프레임 안에서 동작하도록 맞추고 STS2식 폭/여백 정책 적용 — Done: 비율이 바뀌어도 설정 패널이 콘텐츠 프레임 안에서 안정적으로 유지됨
- [ ] `TitleView`와 배경 계열 노드에 STS2식 narrow/ultrawide 후처리 패턴 적용 — Done: 좁은 비율에서는 배경 확대/위치 보정, 초광폭에서는 중앙 프레임 유지가 반영됨
- [ ] 검은 여백(letterbox/pillarbox) 정책을 fullscreen/windowed 모두에서 검증 — Done: 비율 유지가 우선되고 남는 영역이 의도대로 검은 여백으로 표현됨
- [ ] `stage.style.scale` 기반 전역 축소를 제거하고 `PanelSettings.Scale With Screen Size` 기반 전역 스케일로 전환 — In Progress
- [ ] `TitleView`, `SettingsView`, `CharacterSelectView`의 화면별 responsive class 의존을 줄이고 논리 stage 좌표계 기준 레이아웃으로 정리
- [ ] `viewport` 바깥은 항상 `letterbox`만 보이도록, View 내부 배경과 빈 영역 경계가 섞이지 않게 구조 점검

## 의존성
- 선행: `systems/ui/windowed-display-scenarios.md` (창 크기/디스플레이 변화 감지와 저장 복원 구조가 먼저 안정적이어야 함)
