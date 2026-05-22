# CardModel GameplayTag Editor

## 목적
CardModel의 생성 태그를 Unity Inspector에서 안전하게 추가/제거하고, 소스코드에서 정의한 GameplayTag를 선택할 수 있는 에디터 시스템을 만든다.

## 할 일
- CardModel의 `OwnedTags`를 문자열 직접 입력이 아닌 태그 선택 UI로 편집할 수 있게 한다.
- 소스코드에서 생성한 GameplayTag 목록을 에디터가 수집하거나 참조할 수 있게 한다.
- 잘못된 태그 이름, 중복 태그, 빈 태그를 에디터 단계에서 확인할 수 있게 한다.
- Card 생성 시 CardModel의 태그가 `AbilitySystem.OwnedTags`에 적용되는 흐름을 검증한다.

## 메모
현재는 최소형 `GameplayTagReference`를 사용해 태그 이름을 저장하고 런타임에 `GameplayTag.Request(...)`로 변환한다.
