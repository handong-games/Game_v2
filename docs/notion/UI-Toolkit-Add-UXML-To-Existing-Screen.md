# [UI Toolkit] 기존 화면에 UXML 추가

## 목차

01. 결과물
02. 기존 UXML에 포함하는 경우
03. 런타임에 추가하는 경우
04. 참고 자료

## 01. 결과물

![UI Toolkit Overlay Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Add-UXML-To-Existing-Screen-Placeholder.svg)

`Existing Screen` 위에 `Added Screen`이 표시되는 상태

이 문서는 기존 화면 위에 새 UXML을 오버레이로 추가하는 방법을 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
   └─ Added Screen
```

## 02. 기존 UXML에 포함하는 경우

01. 기존 화면 UXML을 연다.
02. 오버레이를 표시할 위치를 정한다.
03. 추가할 UXML을 템플릿 인스턴스로 포함한다.
04. 기존 화면 위에 오버레이가 표시되는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root">
        <ui:Instance template="AddedScreen" />
    </ui:VisualElement>
</ui:UXML>
```

`Instance`는 다른 UXML 템플릿을 현재 UXML 안에 포함하는 요소입니다.
`template`은 포함할 UXML 템플릿의 이름입니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
   └─ AddedScreen.uxml
```

## 03. 런타임에 추가하는 경우

01. 추가할 UXML을 준비한다.

```csharp
[SerializeField] private VisualTreeAsset overlayAsset;
```

02. 오버레이를 추가할 대상을 찾는다.

```csharp
var uiDocument = GetComponent<UIDocument>();
var root = uiDocument.rootVisualElement;

// 기존 화면에서 오버레이를 붙일 루트 요소를 찾는다.
var overlayRoot = root.Q<VisualElement>("overlay-root");
```

03. 추가할 UXML을 생성해 기존 화면 위에 추가한다.

```csharp
// 추가할 UXML을 인스턴스화한다.
var overlay = overlayAsset.Instantiate();

// 인스턴스화한 오버레이를 기존 화면 루트에 추가한다.
overlayRoot.Add(overlay);
```

04. 기존 화면 위에 오버레이가 표시되는지 확인한다.

### 구조

```text
UIDocument
└─ Existing Screen Root
   └─ Overlay Root
      └─ Added Screen
```

## 04. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/2023.2/Documentation/Manual/UIElements.html)
- [Load UXML and USS C# scripts](https://docs.unity3d.com/cn/2023.2/Manual/UIE-manage-asset-reference.html)
- [Instantiate UXML from C# scripts](https://docs.unity3d.com/cn/2023.2/Manual/UIE-LoadingUXMLcsharp.html)
- [UXML element TemplateContainer](https://docs.unity3d.com/kr/2023.2/Manual/UIE-uxml-element-TemplateContainer.html)
