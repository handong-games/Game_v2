# [UI Toolkit/Text] 글자 간격 기준

## 목차

01. 결과물
02. letter-spacing
03. Label 예시
04. 상속 예시
05. 관련 속성
06. 참고 자료

## 01. 결과물

![UI Toolkit Text Letter Spacing Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Text-Letter-Spacing-Placeholder.svg)

같은 `Label`이라도 USS의 `letter-spacing` 값과 부모 요소 상속 여부에 따라 화면에서 보이는 글자 간격이 달라지는 상태

이 문서는 Unity UI Toolkit에서 USS로 `letter-spacing`을 지정하는 방법을 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
   └─ Label
```

## 02. letter-spacing

`letter-spacing`은 UI Toolkit에서 텍스트의 글자 사이 간격을 지정하는 USS 속성입니다.

Unity 공식 문서 기준으로 `letter-spacing`은 상속되는 텍스트 속성입니다. 부모에 값을 지정하고 자식에 별도 값을 지정하지 않으면 자식 텍스트는 부모 간격 값을 따라갑니다.

또한 `letter-spacing`은 애니메이션 가능한 속성입니다.

텍스트 속성은 `Label` 같은 텍스트 요소에 직접 줄 수도 있고, 부모 `VisualElement`에 적용해 자식 텍스트 요소로 전달할 수도 있습니다.

```uss
.title-text {
    letter-spacing: 6px;
}
```

## 03. Label 예시

01. `Label`에 class를 지정한다.
02. USS에서 해당 class의 `letter-spacing`을 지정한다.
03. 화면에서 글자 간격이 변경되었는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root">
        <ui:Label name="title-label" text="SPACED" class="title-text" />
    </ui:VisualElement>
</ui:UXML>
```

```uss
.title-text {
    letter-spacing: 6px;
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
02. USS에서 부모 class의 `letter-spacing`을 지정한다.
03. 자식 `Label`에 별도 `letter-spacing`을 지정하지 않는다.
04. 자식 `Label`이 부모 간격을 따라가는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root" class="text-group">
        <ui:Label text="INHERITED" />
    </ui:VisualElement>
</ui:UXML>
```

```uss
.text-group {
    letter-spacing: 4px;
}
```

부모에 `letter-spacing`을 지정하면 자식 `Label`은 별도 값이 없는 한 그 값을 상속받습니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
   └─ VisualElement.text-group
      └─ Label
```

## 05. 관련 속성

- `word-spacing`
- `-unity-paragraph-spacing`
- `white-space`
- `text-overflow`

이 속성들은 모두 텍스트 표시 방식과 함께 검토할 수 있지만, 이번 문서에서는 `letter-spacing`만 다룹니다.

## 06. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)
- [Unity USS properties reference](https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html)
- [Unity USS common properties](https://docs.unity3d.com/2023.2/Documentation/Manual/UIE-USS-SupportedProperties.html)
- [Label API](https://docs.unity3d.com/ScriptReference/UIElements.Label.html)
- [Styling text](https://docs.unity3d.com/2021.3/Documentation/Manual/UIB-styling-ui-text.html)
