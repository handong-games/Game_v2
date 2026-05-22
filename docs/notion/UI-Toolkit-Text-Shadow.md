# [UI Toolkit/Text] 텍스트 그림자 기준

## 목차

01. 결과물
02. text-shadow
03. Label 예시
04. 상속 예시
05. TextCore와 Font Asset 맥락
06. 참고 자료

## 01. 결과물

![UI Toolkit Text Shadow Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Text-Shadow-Placeholder.svg)

그림자가 없는 텍스트와 그림자가 적용된 텍스트가 한 화면에서 비교되는 상태

이 문서는 Unity UI Toolkit에서 USS로 `text-shadow`를 지정하는 방법을 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
   └─ Label
```

## 02. text-shadow

`text-shadow`는 UI Toolkit에서 텍스트에 드롭 섀도우를 적용하는 USS 속성입니다.

Unity 공식 문서 기준으로 `text-shadow`는:

- 상속되는 속성
- 애니메이션 가능한 속성

즉, 부모 요소에 그림자 값을 지정하고 자식 텍스트에 별도 값을 주지 않으면 자식 텍스트는 그 그림자를 따라갑니다.

```uss
.title-text {
    text-shadow: 2px 2px 4px rgba(15, 23, 42, 0.45);
}
```

이 속성은 보통 아래 요소를 함께 봅니다.

- X offset
- Y offset
- blur radius
- shadow color

`text-shadow`는 요소 박스가 아니라 텍스트 자체에 적용됩니다.

## 03. Label 예시

01. `Label`에 class를 지정한다.
02. USS에서 해당 class의 `text-shadow`를 지정한다.
03. 화면에서 그림자가 보이는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root">
        <ui:Label name="title-label" text="Shadow Text" class="title-text" />
    </ui:VisualElement>
</ui:UXML>
```

```uss
.title-text {
    text-shadow: 2px 2px 4px rgba(15, 23, 42, 0.45);
}
```

`Label`은 텍스트를 표시하는 기본 UI 요소입니다.
`class`는 USS 선택자와 연결되는 이름입니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
   └─ Label.title-text
```

## 04. 상속 예시

01. 부모 요소에 class를 지정한다.
02. USS에서 부모 class의 `text-shadow`를 지정한다.
03. 자식 `Label`에 별도 `text-shadow`를 지정하지 않는다.
04. 자식 `Label`이 부모 그림자를 따라가는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root" class="text-group">
        <ui:Label text="Inherited Shadow" />
    </ui:VisualElement>
</ui:UXML>
```

```uss
.text-group {
    text-shadow: 2px 2px 4px rgba(15, 23, 42, 0.45);
}
```

텍스트 스타일 속성은 부모 요소에 적용해도 자식 텍스트 요소로 전파될 수 있습니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
   └─ VisualElement.text-group
      └─ Label
```

## 05. TextCore와 Font Asset 맥락

Unity 공식 문서에서 UI Toolkit 텍스트는 `TextCore` 기반으로 설명됩니다.

또한 Unity는 `Font Asset`을 통해 텍스트 하이라이트와 그림자 같은 고급 텍스트 스타일링이 가능하다고 설명합니다.

즉 `text-shadow`는 단순한 시각 효과라기보다, TextCore / Font Asset 기반 텍스트 스타일링 흐름 안에 있는 속성으로 보는 편이 정확합니다.

이 문서에서는 `text-shadow` 자체만 다루고, `Font Asset` 전체 설명은 별도 `폰트 기준` 문서에서 다룹니다.

## 06. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)
- [Unity USS properties reference](https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html)
- [Styling text](https://docs.unity3d.com/ja/2021.3/Manual/UIB-styling-ui-text.html)
- [Style text with USS](https://docs.unity3d.com/cn/2022.3/Manual/UIB-styling-ui-text.html)
- [Unity 6 Text](https://docs.unity3d.com/jp/current/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/text.html)
