# Scene LifetimeScope Adoption Decision

## Decision

Adopt the Unity Scene + VContainer `LifetimeScope` direction as the target scene architecture.

This means the final scene boundary is:

```text
Unity scene load
-> scene GameObjects are created by Unity
-> scene LifetimeScope MonoBehaviour builds the VContainer scope
-> scene entry point starts the scene flow

Unity scene unload
-> scene GameObjects are destroyed by Unity
-> scene LifetimeScope is destroyed
-> VContainer disposes scoped IDisposable objects
```

## What This Replaces

The target architecture replaces the current `SceneManagerEx`-owned pure C# scene object lifecycle.

Current runtime shape:

```text
SceneManagerEx
-> creates BaseScene with new / Activator
-> calls BaseScene.BeforeUnload
-> calls Unity SceneManager.LoadScene
-> receives sceneLoaded / sceneUnloaded
-> calls BaseScene.Loaded / Unloaded
```

Target runtime shape:

```text
ISceneLoader
-> handles transition policy
-> calls Unity SceneManager.LoadSceneAsync or LoadScene

Unity scene
-> contains TitleSceneScope / AdventureSceneScope / CombatSceneScope

Scene LifetimeScope
-> owns scene controllers, presenters, view-flow objects, and resource owners
-> registers scene MonoBehaviours with RegisterComponentInHierarchy only when needed

Scene entry point
-> replaces BaseScene.OnLoaded for scene startup
-> implements IDisposable or owns scoped disposables for scene shutdown
```

## Why This Direction Was Chosen

Scene lifetime is already a Unity concept. Letting a scene `LifetimeScope` live inside the Unity scene makes the ownership boundary visible and mechanically tied to scene unload.

This is better than making `SceneManagerEx` create a separate pure C# `BaseScene` object and then manually mirror Unity scene events into that object.

Cold assessment:

```text
The current BaseScene system duplicates Unity scene lifecycle in pure C#.
That duplication made sense before VContainer, but it becomes a liability once scene-scoped DI is the goal.
```

## Immediate Implication

Do not extend the current `ResolveRoot` pattern.

`TitleScene` currently resolves root-level ports as a temporary bridge. Under the adopted direction, that bridge should be removed by moving scene startup logic into a scene entry point owned by the scene `LifetimeScope`.

Temporary:

```text
TitleScene -> GameBootstrap.ResolveRoot<T>
```

Target:

```text
TitleSceneScope
-> RegisterEntryPoint<TitleSceneEntryPoint>
-> constructor injection
```

## TitleScene Target Shape

Unity hierarchy target:

```text
TitleScene.unity
-> TitleSceneScope GameObject
   -> TitleSceneScope : LifetimeScope
```

VContainer target:

```csharp
public sealed class TitleSceneScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<TitleSceneEntryPoint>();

        builder.Register<TitleSceneLocalizationOwner>(Lifetime.Scoped);
        builder.Register<TitleSceneBgmOwner>(Lifetime.Scoped);
        builder.Register<ITitleSceneNavigator, TitleSceneNavigator>(Lifetime.Scoped);
        builder.Register<ISceneViewNavigator, SceneViewNavigator>(Lifetime.Scoped);

        builder.Register<TitleViewController>(Lifetime.Scoped);
        builder.Register<CharacterSelectController>(Lifetime.Scoped);
        builder.Register<SettingsViewController>(Lifetime.Scoped);

        builder.Register<TitleView>(Lifetime.Scoped);
        builder.Register<CharacterSelectView>(Lifetime.Scoped);
        builder.Register<SettingsView>(Lifetime.Scoped);
    }
}
```

Entry point target:

```csharp
public sealed class TitleSceneEntryPoint : IStartable
{
    public void Start()
    {
        // preload localization
        // play title BGM
        // show TitleView
    }
}
```

## Transition Ownership

`BaseScene.OnBeforeUnload` must not be copied blindly into a scene entry point.

Scene transition belongs to scene loading flow:

```text
ISceneLoader.LoadTitle/LoadAdventure
-> ISceneTransitionPlayer.FadeOut
-> Unity SceneManager.LoadScene
```

Reason:

```text
The outgoing scene should not be responsible for knowing every future load path.
The loader owns the transition from one Unity scene to another.
```

## Migration Order

1. Keep current runtime working.
2. Record `TitleSceneScope` as the adopted Unity scene `LifetimeScope`.
3. Add `TitleSceneEntryPoint` skeleton without connecting it.
4. Move Title scene startup responsibilities from `TitleScene.OnLoaded` to `TitleSceneEntryPoint`.
5. Remove duplicate `DependencyManager` ownership for Title scene controllers before resolving them from VContainer.
6. Place `TitleSceneScope` in the Unity TitleScene only after controller ownership is safe.
7. Replace `SceneManagerEx` scene creation responsibility.
8. Remove or shrink `SceneManagerEx` after all `BaseScene` responsibilities are gone.

## SceneManagerEx Removal Track

Final goal:

```text
SceneManagerEx is removed.
BaseScene generic scene loading is removed.
Unity Scene + LifetimeScope owns scene lifecycle.
```

Current first code step:

```text
GameSceneId exists.
GameSceneNames maps GameSceneId to Unity scene asset names.
ISceneLoader now exposes Load(GameSceneId).
LegacyAdventureSessionStarter calls Load(GameSceneId.Adventure).
UnitySceneLoader exists as a SceneManagerEx-free implementation.
RootLifetimeScope now registers UnitySceneLoader as ISceneLoader.
SceneManagerAdapter has been removed.
TitleScene.OnBeforeUnload no longer plays FadeOut directly.
BaseSceneLifecycleRunner now owns only the remaining AdventureScene BaseScene creation and Loaded/BeforeUnload/Unloaded mirroring.
SceneManagerEx has been removed and is no longer registered in ManagerRegistry.
```

Reason:

```text
The root registration can now use UnitySceneLoader because UnitySceneLoader coordinates BaseSceneLifecycleRunner only for remaining legacy BaseScene scenes before loading a Unity scene.
This keeps the old BaseScene lifecycle bridge alive while removing SceneManagerEx from the active loading path.
```

Current safe next step:

```text
Keep shrinking BaseSceneLifecycleRunner by moving scene startup/shutdown hooks into scene LifetimeScopes.
Do not migrate Adventure run-state ownership until AdventureController and AdventureView leave DependencyManager in the same step.
```

## Hard Rules

```text
1. A scene LifetimeScope must not resolve a controller still owned by DependencyManager.
2. ViewManager must not dispose views owned by a scene LifetimeScope.
3. Scene transition logic moves to ISceneLoader, not to random scene entry points.
4. Scene MonoBehaviours are registered only when they actually exist in the Unity scene.
5. BaseScene removal is a later cleanup, not the first step.
```

## Current Status

```text
Adopted as target architecture.
Connected to TitleScene runtime startup.
TitleSceneScope exists in TitleScene.unity as @TitleSceneScope.
SceneManagerEx no longer drives current scene flow.
SceneManagerEx has been removed from code.
AdventureScene BaseScene lifecycle mirroring is owned by BaseSceneLifecycleRunner and coordinated by UnitySceneLoader.
TitleScene is no longer created or mirrored by BaseSceneLifecycleRunner.
GameSceneId-based scene loading surface exists and RootLifetimeScope registers UnitySceneLoader.
Scene transition FadeOut is now owned by ISceneLoader implementations, not TitleScene.OnBeforeUnload.
AdventureScene localization preload/release is now behind AdventureSceneLocalizationOwner, but AdventureScene still creates it directly until AdventureSceneScope is connected.
```
