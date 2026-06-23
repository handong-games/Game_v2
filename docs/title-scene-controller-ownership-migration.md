# TitleScene Controller Ownership Migration

## Purpose

This document defines how `TitleViewController` and `CharacterSelectController` move from `DependencyManager` ownership to the adopted Unity Scene + VContainer `LifetimeScope` direction.

It exists because `TitleSceneEntryPoint` could not be safely registered while Title scene controller ownership was split across `DependencyManager`, direct scene bridges, and future VContainer scopes.

## Current Runtime Ownership

```text
TitleView
-> constructor injection: TitleViewController
-> TitleSceneScope creates TitleViewController

CharacterSelectView
-> constructor injection: CharacterSelectController
-> TitleSceneScope creates CharacterSelectController
```

Current registered scene dependencies:

```text
Removed:
TitleViewController
CharacterSelectController

[Dependency(nameof(TitleScene))]
LegacyAdventureSessionStarter
```

Current `TitleSceneScope` registration:

```text
builder.Register<TitleViewController>(Lifetime.Scoped);
builder.Register<CharacterSelectController>(Lifetime.Scoped);
```

`TitleSceneScope` is placed in the Unity TitleScene, so the registered graph can start the scene.

## Hard Problem

The problem is not controller syntax.

The remaining problem is ownership.

```text
If a legacy bridge is reintroduced and creates CharacterSelectController
while VContainer also creates CharacterSelectController,
then CharacterSelectView and future scene entry points can talk to different controller instances.
```

That is worse than the current custom DI because it creates invisible split state.

## TitleViewController Assessment

Current responsibilities:

```text
OnNewGame -> TitleSceneViewFlow.ShowCharacterSelect
OnSettings -> TitleSceneViewFlow.ShowSettings
OnQuit -> Application.Quit
```

Current dependency problem:

```text
TitleViewController used to manually create TitleSceneViewFlow with:
-> LegacyViewFactory
-> ViewManagerAdapter
```

Target:

```text
TitleViewController
-> constructor receives ITitleSceneChildNavigator
-> no DependencyManager attribute
-> current owner is TitleSceneScope
```

Current bridge:

```text
TitleViewController
-> public constructor receives ITitleSceneChildNavigator
-> delegates NewGame/Settings to ITitleSceneChildNavigator
-> no parameterless DependencyManager constructor
```

`TitleSceneChildNavigator` owns child screen navigation:

```text
ShowCharacterSelect
ShowSettings
```

This prevents `TitleViewController` from depending on the full `TitleSceneViewFlow`.

Cold assessment:

```text
TitleViewController is low risk.
It is a good first controller to move after TitleView can receive a VContainer-owned controller.
```

## CharacterSelectController Assessment

Current responsibilities:

```text
Create character select view model
Check unlock state
Ask LegacyAdventureSessionStarter to start the adventure
```

Current injected dependencies:

```text
CharacterService
IAdventureSessionStarter
```

Current construction shape:

```text
public CharacterSelectController(CharacterService, IAdventureSessionStarter)
-> current VContainer path through TitleSceneScope
```

Problem:

```text
LegacyAdventureSessionStarter still consumes AdventureSession candidate services.
Those services should not become TitleSceneScope-owned long term.
It now has a public constructor-injection path, but the supplied services still come from the legacy DependencyManager bridge.
```

Target:

```text
CharacterSelectController
-> constructor receives CharacterService
-> constructor receives progress/read model boundary
-> constructor receives adventure session starter/factory
-> constructor receives ISceneLoader
```

Do not move `LegacyAdventureSessionStarter` as-is into the final TitleSceneScope design.
It is a transition adapter around legacy global/session services.

Cold assessment:

```text
CharacterSelectController migration is now less risky than before, but the starter still marks the TitleScene to AdventureSession boundary.
```

## View Controller Wiring

Previous views used custom field injection:

```text
TitleView._controller [Inject]
CharacterSelectView._controller [Inject]
```

Current TitleScene views use constructor injection:

```text
TitleView(TitleViewController)
CharacterSelectView(CharacterSelectController)
```

Earlier transition options:

```text
Option A: Constructor injection into views.
Option B: Explicit Initialize(controller) before Bind.
```

Applied direction:

```text
Use constructor injection for TitleScene-scoped views.
```

Reason:

```text
TitleScene-scoped views are now VContainer-owned and are not created by LegacyViewFactory.
Constructor injection makes missing controller ownership fail during scope construction instead of later during Bind.
```

Current bridge:

```text
TitleSceneViews
-> receives scoped TitleView, CharacterSelectView, and SettingsView
-> returns already-composed view instances
```

This holder exists so `TitleSceneViewFlow` can access the root `TitleView` without asking `LegacyViewFactory` to inject a `DependencyManager`-owned `TitleViewController`.

`TitleSceneChildNavigator` intentionally receives `CharacterSelectView` and `SettingsView` directly instead of receiving `TitleSceneViews`.
This avoids the cycle `TitleSceneViews -> TitleView -> TitleViewController -> TitleSceneChildNavigator -> TitleSceneViews`.

`CharacterSelectView` no longer uses the legacy factory for TitleScene runtime construction.

## Safe Migration Order

1. Add explicit controller initialization methods to `TitleView` and `CharacterSelectView`. Done earlier, then replaced by constructor injection.
2. Keep `[Inject]` fields temporarily so current runtime still works. Done, then removed for TitleView and CharacterSelectView.
3. Introduce a VContainer-owned view creation path that calls these initialization methods. Done.
4. Split child screen navigation behind `ITitleSceneChildNavigator`. Done with legacy runtime bridge.
5. Move `TitleViewController` out of DependencyManager ownership. Done.
6. Remove `TitleViewController` from `DependencyManager` registration. Done.
7. Only then register `TitleSceneEntryPoint` in `TitleSceneScope`.
8. Split `CharacterSelectController` adventure-start responsibility behind an adventure session starter/factory. Done with LegacyAdventureSessionStarter.
9. Convert `CharacterSelectController` to constructor injection while preserving DependencyManager ownership. Done.
10. Move `CharacterSelectController` out of DependencyManager ownership. Done.
11. Remove `CharacterSelectController` from `DependencyManager` registration. Done.
12. Move `CharacterSelectController` to VContainer ownership through `TitleSceneScope`. Done.
13. Remove old `[Inject]` fields from Title scene views. Done for TitleView and CharacterSelectView.
14. Replace TitleScene view `InitializeController` methods with constructor injection. Done for TitleView, CharacterSelectView, and SettingsView.

## Do Not Do

```text
Do not register TitleSceneEntryPoint while CharacterSelectController ownership is still unresolved.
Do not register CharacterSelectController as scoped while it directly consumes AdventureSession candidates.
Do not remove [Dependency] attributes without replacing view injection.
Do not reintroduce a legacy TitleScene bridge that also creates TitleViewController or CharacterSelectController.
```

## Current Status

```text
Documented.
TitleView, CharacterSelectView, and SettingsView receive controllers through constructor injection.
TitleSceneViewFlow has been removed.
ITitleSceneNavigator and TitleSceneNavigator exist as the single TitleScene navigation boundary.
TitleScene root TitleView creation no longer depends on DependencyManager.Inject for TitleViewController.
TitleSceneEntryPoint delegates initial TitleView display to ITitleSceneNavigator.
TitleViewController delegates child screen navigation to ITitleSceneNavigator.
TitleView no longer has `[Inject] TitleViewController`.
TitleViewController no longer has `[Dependency(nameof(TitleScene))]`.
DependencyRegistry no longer contains TitleViewController.
CharacterSelectController delegates adventure start to LegacyAdventureSessionStarter.
CharacterSelectController has a public constructor for future VContainer injection.
CharacterSelectController is registered in TitleSceneScope through the public constructor.
LegacyAdventureSessionStarter contains the legacy adventure/session service setup and scene load request.
LegacyAdventureSessionStarter has a public constructor path for VContainer/direct bridge creation.
TitleSceneEntryPoint is registered in TitleSceneScope.
TitleSceneScope is the code owner for TitleScene startup and is present in the Unity scene asset.
DependencyManager no longer owns CharacterSelectController.
DependencyManager no longer owns LegacyAdventureSessionStarter.
TitleSceneScope owns the IAdventureSessionStarter bridge registration, but the bridge still resolves adventure/session services from DependencyManager.
```
