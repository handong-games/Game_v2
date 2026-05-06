# [UI Toolkit/Text] 텍스트 정렬 기준

## 목차

01. 결과물
02. -unity-text-align
03. Label 예시
04. 참고 자료

## 01. 결과물

![UI Toolkit Text Alignment Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Text-Alignment-Placeholder.svg)

하나의 패널 안에서 여러 `Label`이 `-unity-text-align` 값에 따라 서로 다른 위치에 표시되는 상태

이 문서는 Unity UI Toolkit에서 USS로 `-unity-text-align`을 지정하는 방법을 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
   └─ Compare Panel
      ├─ Label
      ├─ Label
      ├─ Label
      ├─ Label
      ├─ Label
      ├─ Label
      ├─ Label
      ├─ Label
      └─ Label
```

## 02. -unity-text-align

`-unity-text-align`은 UI Toolkit에서 요소 박스 내부의 텍스트 정렬 위치를 지정하는 USS 속성입니다.

Unity 공식 문서 기준으로 다음 값을 사용할 수 있습니다.

- `upper-left`
- `upper-center`
- `upper-right`
- `middle-left`
- `middle-center`
- `middle-right`
- `lower-left`
- `lower-center`
- `lower-right`

```uss
.title-text {
    -unity-text-align: middle-center;
}
```

이 속성은 텍스트가 박스 안에서 어디에 그려질지를 결정합니다.

## 03. Label 예시

01. 여러 `Label`을 포함할 패널을 만든다.
02. 각 `Label`에 서로 다른 class를 지정한다.
03. USS에서 각 class의 `-unity-text-align` 값을 지정한다.
04. 화면에서 텍스트 위치가 달라지는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="compare-panel">
        <ui:Label text="Upper Left" class="align-upper-left" />
        <ui:Label text="Middle Center" class="align-middle-center" />
        <ui:Label text="Lower Right" class="align-lower-right" />
    </ui:VisualElement>
</ui:UXML>
```

```uss
.align-upper-left {
    -unity-text-align: upper-left;
}

.align-middle-center {
    -unity-text-align: middle-center;
}

.align-lower-right {
    -unity-text-align: lower-right;
}
```

`-unity-text-align`은 레이아웃 정렬이 아니라, 텍스트가 요소 내부에서 정렬되는 위치를 지정합니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
   └─ VisualElement.compare-panel
      ├─ Label.align-upper-left
      ├─ Label.align-middle-center
      └─ Label.align-lower-right
```

## 04. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)
- [Unity USS properties reference](https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html)
- [Unity USS common properties](https://docs.unity3d.com/2023.2/Documentation/Manual/UIE-USS-SupportedProperties.html)
- [Label API](https://docs.unity3d.com/ScriptReference/UIElements.Label.html)
