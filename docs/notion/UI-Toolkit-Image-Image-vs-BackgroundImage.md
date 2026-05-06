# [UI Toolkit/Image] Image와 backgroundImage 구분

## 목차

01. 결과물
02. Image를 사용하는 경우
03. backgroundImage를 사용하는 경우
04. 선택 기준
05. 참고 자료

## 01. 결과물

![UI Toolkit Image vs BackgroundImage](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Image-Image-vs-BackgroundImage-Placeholder.svg)

이 문서는 `Image`와 `backgroundImage`를 어떤 기준으로 구분해서 사용하는지 기록합니다.

## 02. Image를 사용하는 경우

01. 독립적인 이미지 요소가 필요할 때 사용한다.
02. 아이콘, 초상화, 썸네일처럼 화면 안에 별도 요소로 배치할 때 사용한다.
03. 코드에서 `Q<Image>()`로 찾고 직접 제어할 수 있어야 할 때 사용한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root">
        <ui:Image name="icon-image" />
    </ui:VisualElement>
</ui:UXML>
```

```csharp
var uiDocument = GetComponent<UIDocument>();
var root = uiDocument.rootVisualElement;
var image = root.Q<Image>("icon-image");
```

### 구조

```text
UIDocument
└─ VisualElement
   └─ Image
```

## 03. backgroundImage를 사용하는 경우

01. 기존 요소의 배경 표현이 필요할 때 사용한다.
02. 패널, 카드, 배너처럼 이미 존재하는 요소의 뒤쪽에 이미지를 적용할 때 사용한다.
03. 별도 `Image` 요소를 추가하지 않고 스타일로 처리하고 싶을 때 사용한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="panel-root" />
</ui:UXML>
```

```csharp
var uiDocument = GetComponent<UIDocument>();
var root = uiDocument.rootVisualElement;
var panel = root.Q<VisualElement>("panel-root");

panel.style.backgroundImage = new StyleBackground(backgroundTexture);
```

`backgroundTexture`에는 보통 다음 타입을 사용할 수 있습니다.

- `Texture2D`
- `Sprite`
- `VectorImage`
- `RenderTexture`

### 구조

```text
UIDocument
└─ VisualElement
   └─ backgroundImage
```

## 04. 선택 기준

- 이미지 자체가 하나의 UI 요소면 `Image`
- 기존 요소의 배경 표현이면 `backgroundImage`
- 코드에서 독립적으로 찾고 제어해야 하면 `Image`
- 레이아웃 구조를 늘리지 않고 배경만 적용하면 `backgroundImage`

## 05. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/2023.2/Documentation/Manual/UIElements.html)
- [UXML element Image](https://docs.unity3d.com/ja/2023.2/Manual/UIE-uxml-element-Image.html)
- [Set background images](https://docs.unity3d.com/ja/current/Manual/UIB-styling-ui-backgrounds.html)
- [VisualElement backgroundImage](https://docs.unity3d.com/ScriptReference/UIElements.IStyle-backgroundImage.html)
