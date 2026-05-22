# ArrowWidget Targeting Redesign

## Goal

Keep the current Arrow targeting UX exactly as-is while making the code easier to understand, easier to extend, and better aligned with Unity UI Toolkit patterns.

Current fixed behavior:

- Clicking a skill slot starts targeting and shows the arrow.
- Holding and moving the pointer updates the arrow.
- Releasing the mouse keeps the arrow active.
- Moving the pointer after release keeps updating the arrow.
- Clicking empty space cancels targeting.
- Right-clicking cancels targeting.
- Clicking another skill slot switches targeting to that slot.

## Unity UI Toolkit Basis

- `ArrowWidget` remains a custom `VisualElement` exposed through `[UxmlElement]`. Unity 6 custom control guidance recommends standalone reusable controls for self-contained UI behavior.
- `SkillTargetingManipulator` remains a `PointerManipulator`. Unity recommends manipulators when input logic should be separated from UI setup code.
- Pointer capture remains in the manipulator. This is the correct pattern when pointer movement must keep flowing to the original element after the pointer leaves its bounds.
- `ArrowWidget` keeps receiving panel-space positions and converts them with `WorldToLocal`.
- Runtime rendering should prefer transform changes over layout changes. Unity UI Toolkit performance guidance recommends `translate`, `scale`, and `rotate` over frequent `left`, `top`, `width`, or `height` changes.

References:

- https://docs.unity.cn/6000.5/Documentation/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/custom-controls.html
- https://docs.unity.cn/Manual/UIE-manipulators.html
- https://docs.unity3d.com/ja/2023.2/Manual/UIE-capture-the-pointer.html
- https://docs.unity3d.com/ja/2023.2/Manual/UIE-coordinate-and-position-system.html
- https://docs.unity3d.com/ja/current/Manual/best-practice-guides/ui-toolkit-for-advanced-unity-developers/optimizing-performance.html

## Responsibility Split

```text
AdventureView
- Finds UXML elements.
- Creates and disposes SkillTargetingController.
- Does not own targeting state.

SkillTargetingController
- Owns current targeting state.
- Registers root pointer callbacks.
- Attaches/removes slot manipulators.
- Handles cancel, blank-click cancel, right-click cancel, and slot switching.
- Controls ArrowWidget show/update/hide.

SkillTargetingManipulator
- Owns pointer input for one skill slot.
- Handles pointer capture.
- Reports start/move/release/cancel to SkillTargetingController.

ArrowWidget
- Draws the arrow only.
- Does not know skill slots, targeting state, AdventureView, or game rules.
```

## Implementation Plan

1. Add `SkillTargetingController`.
   - Move the state and root pointer routing currently in `AdventureView.Targeting` into this class.
   - Keep `SkillTargetingManipulator` focused on per-slot pointer capture.

2. Reduce `AdventureView.Targeting`.
   - Keep only `RegisterTargetingEvents()` and `UnregisterTargetingEvents()`.
   - Construct/dispose `SkillTargetingController`.

3. Refactor `ArrowWidget` rendering updates.
   - Keep the same visual result.
   - Precompute body segment sizes.
   - Set element size and center offset once.
   - During pointer movement, update `translate` and `rotate` instead of repeatedly changing `left`, `top`, `width`, and `height`.
   - Keep `display: none` for hidden state because the arrow is temporary and should not render while inactive.

## Non-goals

- No Arrow UX change.
- No new art assets.
- No `Painter2D`, Vector API, or `generateVisualContent` rewrite.
- No Sprite Atlas or Addressables restructure.
- No mobile touch or gamepad targeting support in this pass.
- No real monster target confirmation logic in this pass.

## Expected Follow-up

After this refactor, the next useful step is a visual profiling pass in Unity:

- Confirm the arrow textures enter the dynamic atlas or a sprite atlas.
- Check UI Toolkit Debugger and Frame Debugger while moving the pointer.
- Confirm the arrow update no longer dirties layout every pointer move.
