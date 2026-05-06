# [UI Toolkit/Text] 텍스트 색상 기준

## 목차

01. 결과물
02. color
03. Label 예시
04. 상속 예시
05. 참고 자료

## 01. 결과물

![UI Toolkit Text Color Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Text-Color-Placeholder.svg)

같은 `Label`이라도 USS의 `color` 값에 따라 화면에서 보이는 텍스트 색상이 달라지는 상태

이 문서는 Unity UI Toolkit에서 USS로 `color`를 지정하는 방법을 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
   └─ Label
```

## 02. color

`color`는 UI Toolkit에서 요소의 텍스트를 그릴 때 사용할 색을 지정하는 USS 속성입니다.

Unity 공식 문서 기준으로 `color`는 상속되는 텍스트 속성입니다. 부모에 값을 지정하고 자식에 별도 값을 지정하지 않으면 자식 텍스트는 부모 색을 따라갑니다.

```uss
.title-text {
    color: rgb(37, 99, 235);
}
```

## 03. Label 예시

01. `Label`에 class를 지정한다.
02. USS에서 해당 class의 `color`를 지정한다.
03. 화면에서 텍스트 색상이 변경되었는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root">
        <ui:Label name="title-label" text="Styled Label" class="title-text" />
    </ui:VisualElement>
</ui:UXML>
```

```uss
.title-text {
    color: rgb(37, 99, 235);
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
02. USS에서 부모 class의 `color`를 지정한다.
03. 자식 `Label`에 별도 `color`를 지정하지 않는다.
04. 자식 `Label`이 부모 색을 따라가는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root" class="text-group">
        <ui:Label text="Inherited Label" />
    </ui:VisualElement>
</ui:UXML>
```

```uss
.text-group {
    color: rgb(15, 23, 42);
}
```

부모에 `color`를 지정하면 자식 `Label`은 별도 값이 없는 한 그 값을 상속받습니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
   └─ VisualElement.text-group
      └─ Label
```

## 05. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)
- [Unity USS properties reference](https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html)
- [Unity USS common properties](https://docs.unity3d.com/2023.2/Documentation/Manual/UIE-USS-SupportedProperties.html)
- [Label API](https://docs.unity3d.com/ScriptReference/UIElements.Label.html)
