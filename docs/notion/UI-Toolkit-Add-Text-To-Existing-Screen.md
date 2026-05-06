# [UI Toolkit] 기존 화면에 텍스트 추가

## 목차

01. 결과물
02. UXML에 Label 추가
03. 코드에서 텍스트 설정
04. 참고 자료

## 01. 결과물

![UI Toolkit Add Text Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Add-Text-To-Existing-Screen-Placeholder.svg)

`Existing Screen` 위에 `Added Text`가 표시되는 상태

이 문서는 기존 화면에 텍스트를 추가하는 방법을 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
   └─ Label
```

## 02. UXML에 Label 추가

01. 기존 화면 UXML을 연다.
02. 텍스트를 추가할 위치를 정한다.
03. 해당 위치에 `Label`을 추가한다.
04. 화면에 텍스트가 표시되는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root">
        <ui:Label text="Added Text" />
    </ui:VisualElement>
</ui:UXML>
```

`Label`은 텍스트를 표시하는 기본 UI 요소입니다.
`text`는 화면에 표시할 문자열입니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
   └─ Label
```

## 03. 코드에서 텍스트 설정

**1.** 텍스트를 표시할 `Label`을 찾는다.

```csharp
var uiDocument = GetComponent<UIDocument>();
var root = uiDocument.rootVisualElement;
var label = root.Q<Label>("title-label");
```

**2.** `Label` 텍스트를 설정한다.

```csharp
label.text = "Added Text";
```

**3.** 화면에 텍스트가 표시되는지 확인한다.

### 구조

```text
UIDocument
└─ Existing Screen Root
   └─ Label
```

## 04. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/2023.2/Documentation/Manual/UIElements.html)
- [UXML element Label](https://docs.unity3d.com/2023.2/Documentation/Manual/UIE-uxml-element-Label.html)
- [Label API](https://docs.unity3d.com/ScriptReference/UIElements.Label.html)

