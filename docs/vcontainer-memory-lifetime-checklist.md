# VContainer Memory and Lifetime Checklist

## Current Status Note

Use this checklist with `docs/vcontainer-adoption-doc-index.md`.

This checklist is for future ownership transfers. It does not mean a type is already VContainer-owned.

## Purpose

Use this checklist before moving any existing object from `DependencyManager` ownership to VContainer ownership.

Phase 1 keeps existing services/controllers owned by `DependencyManager`.

Exception:

```text
TitleScene now direct-creates TitleSceneLocalizationOwner and TitleSceneBgmOwner.
These are new resource-owner objects, not migrated DependencyManager services/controllers.
```

## Duplicate Creation Gate

Before registering a type in VContainer, answer:

- Is this type still registered by `[Dependency]`?
- Can `DependencyManager.Instance.Resolve<T>()` still create it?
- Does the type hold mutable runtime state?
- Does any manager cache the current instance?
- Would two instances cause save, combat, card, player, or UI state divergence?

If any answer is yes, do not register the type until ownership transfer is planned.

## Scope Selection

Use Root when:

- The object is app-wide.
- The object survives scene changes.
- The object has no run-specific mutable state, or that state is intentionally global.
- The object is required by long-lived managers.

Use Scene when:

- The object coordinates a scene flow.
- The object subscribes to scene UI events.
- The object should be disposed when the scene flow ends.

Use AdventureSession when:

- The object stores a run, deck, card, player, or combat state.
- The object should reset by creating a new session.
- The object currently needs manual `Clear()` calls to avoid state leakage.

Use Transient when:

- The object is short-lived.
- The object has no independent ownership.
- Disposal is either unnecessary or handled by its owner.

## Disposal Checklist

Before changing a type's lifetime:

- Identify all event subscriptions.
- Identify all `IDisposable` implementations.
- Confirm which scope will call `Dispose`.
- Confirm scene unload and session end dispose timing.
- Confirm Addressables or Unity objects are released by the correct owner.
- Confirm no manager keeps a disposed instance.

## Existing Stateful Services

These types must not be duplicated during migration:

```text
AdventureService
CardDeckService
CardService
CardBoardService
PlayerService
CombatService
ProgressState
AudioSettingsState
GraphicSettingsState
LocalizationSettingsState
```

## Object Creation Checklist

Before moving creation to VContainer:

- Is the type pure C# or MonoBehaviour?
- If pure C#, can it use constructor injection?
- If MonoBehaviour, who creates the GameObject?
- If created by Unity, should VContainer inject it instead of create it?
- If created from prefab, should VContainer call `Instantiate`?
- If loaded from Addressables, who owns the load and release handles?

## MonoBehaviour Rule

Scene `LifetimeScope` MonoBehaviour is the adopted target composition root for scene lifetime.

TitleScene now contains a connected `TitleSceneScope` MonoBehaviour.

Future rules:

```text
Scene LifetimeScope: place in the Unity scene only after duplicate ownership is prevented.
Unity-created component: inject or register the existing instance.
VContainer-created prefab: instantiate through VContainer.
Scene component search: use only inside an explicit scene scope.
```

## Migration Safety Checks

Every migration PR should prove:

- Current owner before the change.
- New owner after the change.
- Scope lifetime.
- Creation path.
- Disposal path.
- Whether duplicate creation is impossible.
- What smoke test covers the change.
