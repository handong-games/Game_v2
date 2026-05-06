# [UI Toolkit/Text] 폰트가 보이는 원리

## 목차

01. 예시 이미지
02. 문자와 글리프
03. TTF와 OTF
04. 화면에 글자가 보이는 흐름
05. Unity UI Toolkit과의 연결
06. 참고 자료

## 01. 예시 이미지

![UI Toolkit Text Font Basics Result](C:/Users/reg24/Favorites/Game/docs/notion/UI-Toolkit-Text-Font-Glyph-Lookup-Placeholder.svg)

문자 데이터 `"A"`가 폰트 파일 안의 글리프 정보를 찾아 화면의 글자로 표시되는 상태

이 문서는 텍스트가 어떻게 폰트를 만나 화면에 보이는지에 대한 기초 원리를 기록합니다.

## 02. 문자와 글리프

텍스트 문자열의 `"A"`는 아직 그림이 아니라 문자 데이터입니다.

화면에 글자가 보이려면 이 문자에 대응하는 글자 모양 정보가 필요합니다. 이 실제 글자 모양을 `glyph`라고 부릅니다.

즉, `"A"` 자체가 바로 보이는 것이 아니라 `"A"`에 대응하는 글리프를 찾아서 그려야 합니다.

```text
"A"
-> character data
-> matching glyph
-> rendered text
```

## 03. TTF와 OTF

`TTF`와 `OTF`는 대표적인 폰트 파일 형식입니다.

이 파일들 안에는 문자에 대응하는 글리프 정보가 들어 있습니다.

즉, 폰트 파일은 단순한 스타일 이름이 아니라 `"어떤 문자를 어떤 모양으로 그릴지"`에 대한 데이터 모음입니다.

정리하면:

```text
TTF / OTF
= character to glyph data
```

## 04. 화면에 글자가 보이는 흐름

01. 문자열에서 문자를 읽는다.
02. 폰트 파일에서 해당 문자에 대응하는 글리프를 찾는다.
03. 찾은 글리프를 화면에 그린다.
04. 크기, 색상, 간격 같은 스타일을 함께 적용한다.

이 흐름을 간단히 쓰면:

```text
text
-> font file
-> glyph lookup
-> render on screen
```

## 05. Unity UI Toolkit과의 연결

Unity UI Toolkit에서는 이 과정이 `TextCore` 기반으로 동작합니다.

즉, UI Toolkit 텍스트도 결국 문자 데이터와 폰트 자산을 바탕으로 글리프를 찾아 화면에 그립니다.

이 원리를 이해하면 다음 문서에서 다루는 아래 개념이 자연스럽게 이어집니다.

- `Font Asset`
- `-unity-font`
- `-unity-font-definition`
- `-unity-font-style`

## 06. 참고 자료

- [Unity 6 Text](https://docs.unity3d.com/jp/current/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/text.html)
- [Unity UI Toolkit](https://docs.unity3d.com/Manual/UIElements.html)
