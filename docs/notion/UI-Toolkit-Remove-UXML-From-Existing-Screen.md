# [UI Toolkit] 기존 화면에서 UXML 제거

## 목차

01. 결과물
02. 기존 UXML에서 제거하는 경우
03. 런타임에서 제거하는 경우
04. 참고 자료

## 01. 결과물

![UI Toolkit Remove UXML Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Remove-UXML-From-Existing-Screen-Placeholder.svg)

`Existing Screen`만 남고 `Added Screen`이 제거된 상태

이 문서는 기존 화면에서 추가된 UXML을 제거하는 방법을 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
```

## 02. 기존 UXML에서 제거하는 경우

01. 기존 화면 UXML을 연다.
02. 제거할 `Instance` 위치를 찾는다.
03. 해당 `Instance`를 제거한다.
04. 기존 화면만 표시되는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root">
    </ui:VisualElement>
</ui:UXML>
```

`Instance`를 제거하면 해당 템플릿은 현재 UXML에 포함되지 않습니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
```

## 03. 런타임에서 제거하는 경우

**1.** 제거할 요소를 찾는다.

```csharp
var uiDocument = GetComponent<UIDocument>();
var root = uiDocument.rootVisualElement;
var addedScreen = root.Q<VisualElement>("added-screen");
```

**2.** 찾은 요소를 기존 화면에서 제거한다.

```csharp
addedScreen?.RemoveFromHierarchy();
```

**3.** 기존 화면만 남는지 확인한다.

### 구조

```text
UIDocument
└─ Existing Screen Root
```

## 04. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/2023.2/Documentation/Manual/UIElements.html)
- [RemoveFromHierarchy](https://docs.unity3d.com/ScriptReference/UIElements.VisualElement.RemoveFromHierarchy.html)
- [Load UXML and USS C# scripts](https://docs.unity3d.com/cn/2023.2/Manual/UIE-manage-asset-reference.html)
