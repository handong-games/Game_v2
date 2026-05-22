# [UI Toolkit/Text] 폰트 기준

## 목차

01. 예시 이미지
02. TextCore와 Font Asset
03. 소스 폰트와 Font Asset
04. 폰트 지정 속성
05. 폰트 스타일
06. 부모 요소 전파
07. 참고 자료

## 01. 예시 이미지

![UI Toolkit Text Font Reference Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Text-Font-Reference-Placeholder.svg)

같은 `Label`이라도 Font Asset과 폰트 스타일 값에 따라 화면에서 보이는 텍스트 모양이 달라지는 상태

이 문서는 Unity UI Toolkit에서 폰트를 어떤 자산과 속성으로 다루는지 기록합니다.

결과물은 아래 구조를 목표로 합니다.

```text
UIDocument
└─ Existing Screen
   └─ Label
```

## 02. TextCore와 Font Asset

Unity 공식 문서 기준으로 UI Toolkit 텍스트는 `TextCore` 기반으로 렌더링됩니다.

즉, UI Toolkit에서 텍스트를 표시할 때는 단순히 폰트 파일 이름만 보는 것이 아니라, 텍스트 렌더링용 자산 구조를 함께 이해해야 합니다.

여기서 핵심 개념은 다음 두 가지입니다.

- `Source Font`
- `Font Asset`

## 03. 소스 폰트와 Font Asset

소스 폰트는 보통 다음 파일 형식을 의미합니다.

- `TTF`
- `OTF`

하지만 UI Toolkit에서 실제 텍스트 렌더링에 사용되는 것은 이 원본 파일 자체보다, 이를 기반으로 사용하는 `Font Asset` 개념입니다.

즉 구분은 이렇게 됩니다.

```text
Source Font
- TTF
- OTF

UI Toolkit Rendering
- Font Asset
```

## 04. 폰트 지정 속성

Unity UI Toolkit에서 폰트를 지정할 때는 주로 다음 속성을 봅니다.

- `-unity-font`
- `-unity-font-definition`

```uss
.title-text {
    -unity-font-definition: resource("Fonts/MainFontAsset");
}
```

또는

```uss
.title-text {
    -unity-font: resource("Fonts/MainFont");
}
```

핵심은 다음과 같습니다.

- `-unity-font-definition`이 `-unity-font`보다 우선합니다.
- 둘 다 지정되면 `-unity-font-definition` 기준으로 동작할 수 있습니다.

## 05. 폰트 스타일

폰트 스타일은 다음 속성으로 지정합니다.

- `-unity-font-style`

사용 가능한 값:

- `normal`
- `italic`
- `bold`
- `bold-and-italic`

```uss
.title-text {
    -unity-font-style: bold;
}
```

## 06. 부모 요소 전파

텍스트 관련 속성은 `Label`에 직접 줄 수도 있고, 부모 `VisualElement`에 적용해서 자식 텍스트 요소로 전달할 수도 있습니다.

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <ui:VisualElement class="text-group">
        <ui:Label text="Inherited Font" />
    </ui:VisualElement>
</ui:UXML>
```

```uss
.text-group {
    -unity-font-style: italic;
}
```

즉, 폰트 속성은 개별 텍스트 요소뿐 아니라 상위 요소 기준으로도 관리할 수 있습니다.

## 07. 참고 자료

- [Unity 6 Text](https://docs.unity3d.com/jp/current/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/text.html)
- [Unity UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)
- [Unity USS common properties](https://docs.unity3d.com/2023.2/Documentation/Manual/UIE-USS-SupportedProperties.html)
- [Unity USS properties reference](https://docs.unity3d.com/Manual/UIE-USS-Properties-Reference.html)
