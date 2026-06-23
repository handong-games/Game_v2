# Unity Lifecycle and VContainer Mapping

## Current Status Note

Read this document as lifecycle learning material. The active migration status is tracked in `docs/vcontainer-adoption-doc-index.md`.

Current applied state:

```text
VContainer package and scope skeletons exist.
Unity Scene + VContainer LifetimeScope is adopted as the target scene architecture.
TitleScene resource owners are now created through TitleSceneScope.
RootLifetimeScope is created by GameBootstrap after ManagerRegistry initialization.
TitleSceneScope inherits root-level ports from RootLifetimeScope at the scene composition boundary.
TitleSceneScope is connected to TitleScene runtime startup through the scene-placed @TitleSceneScope component.
Filtered PlayMode verification confirms TitleSceneScope builds and resolves scoped TitleScene controllers.
DependencyManager still owns most runtime services.
```

## Purpose

This document explains what Unity already owns, what VContainer should own, and how both lifecycles should be mapped in this project.

VContainer should not replace Unity's lifecycle. It should complement Unity by owning pure C# object creation, dependency wiring, and scope-based disposal.

## Core Rule

```text
Unity owns GameObject and Component lifecycles.
VContainer owns dependency graph and scope lifecycles.
```

Before adding a type to VContainer, answer:

```text
Does Unity already create and destroy this object?
```

If yes, VContainer should usually inject or register the existing object.

If no, VContainer can usually create and own the object.

## Unity-Owned Concepts

### Scene

Unity scenes own scene-local GameObjects.

```text
Scene load
→ scene GameObjects are loaded
→ MonoBehaviour lifecycle begins

Scene unload
→ scene GameObjects are destroyed
→ MonoBehaviour OnDestroy runs
```

Use Unity scene lifetime for:

- Camera rigs
- Scene roots
- UI document hosts
- Scene-only visual objects
- Effect layers
- Input adapters

Do not assume scene lifetime equals gameplay session lifetime.

### GameObject and Component

GameObject owns Components.

MonoBehaviour is a Component.

```text
GameObject destroyed
→ attached Components destroyed
→ MonoBehaviour.OnDestroy
```

Use MonoBehaviour when the object must exist in Unity's object hierarchy.

Do not use MonoBehaviour only because an object needs a lifetime. VContainer scopes are better for pure C# lifetime.

### MonoBehaviour Lifecycle

Important callbacks:

```text
Awake
OnEnable
Start
Update
OnDisable
OnDestroy
```

Use these for Unity boundary behavior:

- Reading serialized fields
- Connecting scene references
- Receiving Unity events
- Driving visual updates
- Handling collisions, animation, timeline, camera, audio, input

Avoid putting game rules and run state directly in these callbacks.

### Prefab

Prefab is a reusable GameObject template.

```text
Instantiate prefab
→ GameObject instance exists
Destroy instance
→ GameObject and components are destroyed
```

If VContainer should inject prefab components, instantiate the prefab through VContainer or inject the created GameObject.

### ScriptableObject

ScriptableObject is usually project asset data.

Use it for:

- Static data
- Authoring data
- Tables
- Ability definitions
- Card/character/monster models

Do not treat asset ScriptableObjects as session-owned runtime state unless they are explicitly cloned.

### Addressables

Addressables owns asset load handles and release timing.

```text
Addressables load
→ asset available

Addressables release
→ handle released
```

VContainer scope disposal does not automatically release Addressables handles unless an object explicitly owns and releases them.

Keep these responsibilities separate:

```text
Addressables: asset load/release
VContainer: object graph injection/disposal
Unity: GameObject/component destruction
```

### DontDestroyOnLoad

`DontDestroyOnLoad` keeps a GameObject alive across scene unload.

Use it sparingly.

Risk:

```text
Scene object becomes app-global object
→ lifetime is no longer obvious
→ stale state can survive unexpectedly
```

Current managers use persistent GameObjects for manager behaviors. Do not expand this pattern without explicit ownership review.

## VContainer-Owned Concepts

### LifetimeScope

`LifetimeScope` is VContainer's ownership boundary.

```text
Scope created
→ registered object graph can be resolved

Scope disposed
→ scope-owned IDisposable instances are disposed
```

Scope disposal is not the same as scene unload unless explicitly wired together.

### Root Scope

Root scope is app-wide.

Use for:

- App-wide settings state
- Static data lookup services
- Long-lived infrastructure bridges

Avoid putting run/session state in Root.

### Scene Scope

Scene scope follows a scene-level flow.

Use for:

- Scene controllers
- Scene presenters
- Scene navigation helpers
- Scene-specific adapters

In this project, `TitleViewController` belongs to Title scene scope, not Root.

### AdventureSession Scope

Adventure session scope follows one adventure run.

Use for:

- `AdventureService`
- `CardDeckService`
- `CardService`
- `CardBoardService`
- `PlayerService`
- `CombatService`

These are currently global dependencies, but their final target should be session scope because they hold mutable run state.

### Transient

Transient is for short-lived objects.

Use for:

- Temporary view models
- Commands
- Request objects
- Pure objects with no independent ownership

Do not use transient for stateful services that must be shared during a session.

## Mapping Table

| Object kind | Unity owner | VContainer owner | Recommended handling |
| --- | --- | --- | --- |
| Scene GameObject | Yes | No | Unity creates/destroys; VContainer may inject/register |
| MonoBehaviour View Adapter | Yes | Partial | Register or inject in scene scope |
| Prefab instance | Yes | Partial | Instantiate through VContainer if injection is needed |
| ScriptableObject asset | Asset database/Addressables | No | Register instance only when needed |
| Pure C# controller | No | Yes | Register in scene scope |
| Pure C# service with app state | No | Yes | Register in root scope |
| Pure C# service with run state | No | Yes | Register in adventure session scope |
| Save state | No | Yes | Root scope candidate; avoid duplication |
| Addressables handle owner | No | Maybe | Owner must release handles explicitly |

## Project-Specific Rules

### Current Phase

Current phase has VContainer installed, scope skeletons added, and RootLifetimeScope created at application lifetime.
Unity Scene + VContainer LifetimeScope is adopted as the target direction and is now connected for TitleScene startup.
TitleScene may resolve root-level ports from RootLifetimeScope during the migration.
TitleSceneScope owns TitleScene startup/controllers, but scene view disposal and adventure session ownership are not fully migrated.

Current rule:

```text
Existing DependencyManager still owns current runtime objects.
VContainer must not own existing [Dependency] service runtime instances yet.
ResolveRoot must not spread into controllers, views, or domain services.
```

### View Creation

`IViewFactory` is a boundary for view creation, but it does not decide view reuse policy by itself.

Questions to answer per view:

```text
Should this view be created every time?
Should it be cached for the scene?
Should it survive scene changes?
Who disposes it?
```

`CharacterSelectView` is intended to be created once per Title scene, so it needs a scene-cached view policy before the factory becomes final.

### TitleViewController

`TitleViewController` belongs to Title scene scope.

It should be destroyed when `TitleScene` ends.

It should not be Root scoped.

### MonoBehaviour Strategy

Scene `LifetimeScope` MonoBehaviour is the adopted target composition root for scene lifetime.

Do not connect MonoBehaviour-specific registration code to runtime until duplicate ownership with `DependencyManager` and `ViewManager` is removed.

Principle:

```text
Scene LifetimeScope MonoBehaviour = scene composition root
MonoBehaviour = Unity boundary adapter
Pure C# = game rules, state, controller logic
VContainer = dependency and scope ownership
```

This means:

```text
Use MonoBehaviour for LifetimeScope because Unity owns scene lifetime.
Do not convert pure C# controllers/services into MonoBehaviours only to get Start/OnDestroy.
```

## Decision Checklist

Before assigning a class to MonoBehaviour or VContainer, ask:

```text
1. Does it need a GameObject transform, serialized field, Unity callback, physics, animation, audio, or UI component?
   → If yes, MonoBehaviour may be appropriate.

2. Does it hold gameplay state that should survive or reset independently of scene unload?
   → If yes, prefer pure C# plus VContainer scope.

3. Does it subscribe to events?
   → Identify which owner calls Dispose or OnDestroy.

4. Does it need Addressables assets?
   → Identify who releases handles.

5. Can it be tested without Unity scene objects?
   → If yes, keep it pure C#.
```

## Bad Smells

Avoid:

- Making stateful services MonoBehaviours only to get `OnDestroy`.
- Using `DontDestroyOnLoad` to avoid designing ownership.
- Resolving dependencies from arbitrary methods instead of constructor/factory boundaries.
- Registering the same stateful type in both `DependencyManager` and VContainer.
- Treating ScriptableObject assets as mutable session state.
- Letting scene unload reset gameplay session state accidentally.

## Target Mental Model

```text
Unity answers:
Where is this object in the scene?
When is this GameObject destroyed?

VContainer answers:
Who creates this C# object?
Which dependencies does it need?
Which scope owns it?
When is it disposed?

Game architecture answers:
Is this state app-wide, scene-wide, or session-wide?
```
