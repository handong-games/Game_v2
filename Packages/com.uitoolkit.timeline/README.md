# UIToolkit Timeline

`com.uitoolkit.timeline` is a minimal UI Toolkit timeline framework.

The package owns timeline authoring, compiled runtime tape data, UI Toolkit class playback, and editor preview. It does not depend on gameplay, cards, combat, abilities, or gameplay tags.

## Boundary

Framework concepts:

- Timeline
- Track
- Clip
- Notify
- Playback
- Preview
- Composition
- Runtime tape

Content concepts stay outside this package:

- Card
- Combat
- Gameplay Ability
- Gameplay Tag
- Damage
- Impact

## Runtime Playback

Runtime playback is intentionally small. A `UIToolkitTimelineComponent` targets one `VisualElement`, then plays a compiled `TimelineAsset`.

```csharp
var component = new UIToolkitTimelineComponent();
component.Init(visualElement);

if (component.CanPlay(timeline))
{
    TimelinePlayback playback = component.Play(timeline);
}
```

Playback uses the compiled runtime data in `TimelineAsset`. It evaluates value clips for translate, scale, and rotate, and raises notify events in tape order.

## Authoring

`TimelineAsset` stores two data areas:

- Runtime data: always serialized, used by playback.
- Authoring data: editor-only, compiled into runtime data through `OnValidate`.

Authoring value clips store start time, duration, from value, to value, and easing. Notify placements reference `TimelineNotifyDefinition` assets.

## Preview

Open `Window > UIToolkit Timeline > Timeline Asset Editor`, or double-click a `TimelineAsset`.

The editor preview instantiates the configured `VisualTreeAsset`, finds the target by name, and previews the compiled runtime tape. The preview player is editor-only, but its source of truth is the same compiled tape used by runtime playback.

## Composition Preview

Open `Window > UIToolkit Timeline > Timeline Composition Editor`, or double-click a `TimelineComposition`.

`TimelineComposition` is an editor-only observation asset. It assembles multiple independent `TimelineAsset` parts into one preview document with named element bindings and per-binding start offsets. Runtime playback remains independent; developers still play each timeline separately through `UIToolkitTimelineComponent`.
