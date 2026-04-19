# 창모드 디스플레이 시나리오 정리

## 현재 상태
- 구현된 것: `GraphicManager`가 해상도 변경 이벤트와 해상도/비율 조회 일부를 담당한다.
- 관련 파일: `Assets/@Scripts/Core/Manager/Garphic/GraphicManager.cs:9`
- 관련 파일: `Assets/@Scripts/Core/Manager/Garphic/GraphicManagerBehaviour.cs:6`
- 구현된 것: `SettingsView`가 `GraphicManager.GetResolutions()`와 aspect 관련 값을 일부 소비한다.
- 관련 파일: `Assets/@Scripts/Domains/Settings/View/SettingsView.Graphic.cs:15`
- 관련 파일: `Assets/@Scripts/Domains/Settings/View/SettingsView.cs:124`

## 할 일
- [ ] 창모드에서 직접 리사이즈 시 현재 창 크기/비율을 정상 감지하고 UI 클래스에 반영 — Done: 마우스 드래그로 창 크기를 바꾸면 `Auto` 기준 narrow/standard/ultrawide 상태가 즉시 갱신됨
- [ ] OS 스냅(`Win + 방향키`) 후 현재 창이 속한 display와 비율을 다시 계산 — Done: 스냅 후 `GraphicManager`가 현재 display와 창 비율 변화를 감지하고 필요한 이벤트를 발행함
- [ ] 창이 다른 display로 이동했을 때 현재 display 기준 해상도 리스트를 다시 제공 — Done: resolution dropdown을 열면 현재 소속 display 기준의 목록이 보임
- [ ] 현재 저장 해상도가 새 리스트에 없을 때 `N/A`로 표시 — Done: display 이동 또는 비율 변경 후 기존 해상도가 유효하지 않으면 resolution 표시가 `N/A`가 됨
- [ ] fullscreen/windowed 전환 시 resolution UI 활성/비활성 상태 일관화 — Done: fullscreen이면 resolution UI 비활성, windowed면 활성 상태가 유지됨

## 의존성
- 선행: 없음
