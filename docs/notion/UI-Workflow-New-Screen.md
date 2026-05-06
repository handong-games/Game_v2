# [UI Toolkit] 새 화면 추가

## 목차

01. 결과물
02. 씬에 배치하는 경우
03. 런타임에 생성하는 경우
04. 참고 자료

## 01. 결과물

![UI Toolkit Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-New-Screen-Placeholder.svg)

`UIDocument` 내부를 `New VisualElement` 하나가 채우는 상태

이 문서는 새 화면을 추가할 때 `씬에 배치하는 경우`와 `런타임에 생성하는 경우`를 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ New VisualElement
```

## 02. 씬에 배치하는 경우

01. Scene에 `GameObject`를 만든다.
02. `GameObject`에 `UIDocument`를 추가한다.
03. `FirstScreen.uxml`을 만든다.
04. `FirstScreen.uxml` 루트에 `VisualElement`를 추가한다.
05. `UIDocument`의 `Source Asset`에 `FirstScreen.uxml`을 연결한다.
06. `UIDocument` 내부에 `New VisualElement`가 표시되는지 확인한다.

### 02-1. 구조

```text
Scene
└─ GameObject
   └─ UIDocument
      └─ FirstScreen.uxml
         └─ New VisualElement
```

## 03. 런타임에 생성하는 경우

01. 화면 진입 시 `GameObject`를 생성한다.

```csharp
// 런타임 UI를 담을 GameObject를 만든다.
var go = new GameObject("Runtime UI");

// 생성한 GameObject에 UIDocument를 추가한다.
var uiDocument = go.AddComponent<UIDocument>();
```

02. 호출 클래스에 `VisualTreeAsset`과 `PanelSettings`를 선언하고 `UIDocument`에 연결한다.

```csharp
[SerializeField] private VisualTreeAsset firstScreenAsset;
[SerializeField] private PanelSettings panelSettings;
```

```csharp
// UIDocument가 사용할 PanelSettings를 지정한다.
uiDocument.panelSettings = panelSettings;

// 표시할 UXML VisualTreeAsset을 연결한다.
uiDocument.visualTreeAsset = firstScreenAsset;
```

03. `UIDocument` 내부에 `New VisualElement`가 표시되는지 확인한다.

### 03-1. 구조

```text
Runtime
└─ Created GameObject
   └─ UIDocument
      └─ FirstScreen.uxml
         └─ New VisualElement
```

## 04. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)
- [Unity UXML](https://docs.unity3d.com/Manual/UIE-UXML.html)
- [Unity VisualElement API](https://docs.unity3d.com/ScriptReference/UIElements.VisualElement.html)
- [Unity Runtime UI getting started](https://docs.unity3d.com/ja/6000.0/Manual/UIE-get-started-with-runtime-ui.html)
