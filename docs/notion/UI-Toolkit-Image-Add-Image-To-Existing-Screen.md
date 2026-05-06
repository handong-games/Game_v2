# [UI Toolkit/Image] 기존 화면에 이미지 추가

## 목차

01. 결과물
02. UXML에 Image 추가
03. 코드에서 이미지 설정
04. 참고 자료

## 01. 결과물

![UI Toolkit Add Image Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Add-Image-To-Existing-Screen-Placeholder.svg)

`Existing Screen` 위에 `Added Image`가 표시되는 상태

이 문서는 기존 화면에 이미지를 추가하는 방법을 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
   └─ Image
```

## 02. UXML에 Image 추가

01. 기존 화면 UXML을 연다.
02. 이미지를 추가할 위치를 정한다.
03. 해당 위치에 `Image`를 추가한다.
04. 화면에 이미지가 표시되는지 확인한다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement name="screen-root">
        <ui:Image name="icon-image" />
    </ui:VisualElement>
</ui:UXML>
```

`Image`는 이미지를 표시하는 기본 UI 요소입니다.

- `name`: 코드에서 요소를 찾을 때 사용하는 이름
- `Image`: 표시 위치와 구조를 정의하는 요소
- 실제 이미지 애셋: 이후 단계에서 `Texture2D`로 연결
- 이 문서는 `Texture2D`를 기준으로 설명합니다.

### 구조

```text
UIDocument
└─ ExistingScreen.uxml
   └─ Image
```

## 03. 코드에서 이미지 설정

### SerializeField로 설정하는 경우

**1.** 이미지를 표시할 `Image`를 찾는다.

```csharp
var uiDocument = GetComponent<UIDocument>();
var root = uiDocument.rootVisualElement;
var image = root.Q<Image>("icon-image");
```

**2.** `Image`에 표시할 텍스처를 설정한다.

```csharp
[SerializeField] private Texture2D iconTexture;

image.image = iconTexture;
```

**3.** 화면에 이미지가 표시되는지 확인한다.

### Addressables로 로드하는 경우

**1.** 이미지를 표시할 `Image`를 찾는다.

```csharp
var uiDocument = GetComponent<UIDocument>();
var root = uiDocument.rootVisualElement;
var image = root.Q<Image>("icon-image");
```

**2.** Addressables로 이미지를 로드해 적용한다.

```csharp
[SerializeField] private string iconAddress;
[SerializeField] private Texture2D fallbackTexture;

private AsyncOperationHandle<Texture2D> _iconHandle;

// Addressables key로 Texture2D를 비동기로 로드한다.
_iconHandle = Addressables.LoadAssetAsync<Texture2D>(iconAddress);
await _iconHandle.Task;

// 로드에 성공하면 Image에 적용한다.
if (_iconHandle.Status == AsyncOperationStatus.Succeeded)
{
    image.image = _iconHandle.Result;
}
// 실패하면 기본 이미지를 적용한다.
else
{
    image.image = fallbackTexture;
}
```

**3.** 사용이 끝나면 handle을 해제한다.

```csharp
if (_iconHandle.IsValid())
{
    Addressables.Release(_iconHandle);
}
```

**4.** 화면에 이미지가 표시되는지 확인한다.

### 구조

```text
UIDocument
└─ Existing Screen Root
   └─ Image
```

## 04. 참고 자료

- [Unity UI Toolkit](https://docs.unity3d.com/2023.2/Documentation/Manual/UIElements.html)
- [UXML element Image](https://docs.unity3d.com/2023.2/Documentation/Manual/UIE-uxml-element-Image.html)
- [Image API](https://docs.unity3d.com/ScriptReference/UIElements.Image.html)
- [Addressables](https://docs.unity3d.com/Packages/com.unity.addressables@latest)

