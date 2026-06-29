# VContainer Registration Map

## Current Status Note

Read this document together with `docs/vcontainer-adoption-doc-index.md`.

`RootLifetimeScope` is created by `GameBootstrap` after `ManagerRegistry` initialization. It is a root composition container, not the legacy manager initializer.

Current root rule:

```text
RootLifetimeScope registers root services and ports.
GameRootEntryPoint owns VContainer-root initialization order.
GameBootstrap still owns Unity application bootstrap and legacy ManagerRegistry initialization.
```

`TitleScene` may resolve root-level ports from `RootLifetimeScope` only because it is currently the scene composition boundary. Controllers, views, and domain services must not call `GameBootstrap.ResolveRoot`.

`TitleSceneScope` is placed in `TitleScene.unity` as `@TitleSceneScope` and is connected to runtime startup. A registration is still not runtime ownership unless the type is actually resolved by this connected scope. It uses the `GameBootstrap`-created `RootLifetimeScope` as its parent so root adapters are inherited from the intended container.

Unity Scene + VContainer `LifetimeScope` is the adopted target scene architecture. `TitleSceneScope` is the current TitleScene scene `LifetimeScope`, not a code-created child scope target.

Current TitleScene checkpoint:

```text
TitleSceneScope, TitleScene views, and TitleScene controllers do not directly access legacy managers.
Legacy manager access remains only in explicit Legacy* bridge registrations.
```

## Purpose

This document records the intended VContainer scope ownership for current game services.

This map records the current VContainer ownership surface so that old `DependencyManager` or service-boundary names are not accidentally reintroduced.

## Package / Content Boundary

Package area:

```text
Packages/
```

Use this area only for reusable, content-agnostic infrastructure. Do not place game-specific VContainer composition roots in `Packages/com.gameplay.gas` or `Packages/com.uitoolkit.timeline`.

Content area:

```text
Assets/@Scripts/
```

Game-specific scope skeletons, composition roots, and registration maps belong here.

## Phase 1 Registration Rule

```text
Do not register the same runtime state owner in both DependencyManager and VContainer.
Do not reintroduce old AdventureService/CardDeckService/CardService/PlayerService/CombatService boundaries without a fresh ownership reason.
Do not let views/controllers call GameBootstrap.ResolveRoot directly.
Do not let scene views use legacy ViewManager Push ownership.
```

This prevents duplicate runtime state and hidden scene lifetime leaks.

## Root Scope Candidates

Root scope is for app-wide state and infrastructure that should survive scene changes.

First-phase root scope does not create existing legacy managers. Existing legacy managers remain owned by `ManagerRegistry`.

Current exception:

```text
ViewManager is now VContainer-created as a root singleton.
It is not registered as its own entry point.
GameRootEntryPoint calls ViewManager.Initialize once during root container startup.
VContainer disposes ViewManager when RootLifetimeScope is disposed.
```

Root first-phase adapter candidates:

| Interface | First implementation | Purpose | Notes |
| --- | --- | --- | --- |
| `IViewHost` | `ViewManager` | Let scene-owned views attach/detach without transferring disposal ownership to `ViewManager`. | Host path only. It must not be used as a second owner for views already in the legacy stack. |
| `SceneManagerEx` | Root singleton | Let controllers request scene loads through one app-level scene transition boundary. | Coordinates fade-out, scene preload, and Unity `SceneManager.LoadSceneAsync` activation. It logs exceptions at the `async void` boundary, resets loading state through `sceneLoaded`, and releases pending `allowSceneActivation=false` operations on failure so Unity's async queue is not left stalled. |
| `ScenePreloadService` | Root singleton delegate registration | Dispatch scene-specific preload work before scene activation. | Uses explicit preloader registration. `AdventureSceneLoader` currently owns Adventure payload preparation. |
| `AdventureSceneLoader` | Root singleton | Preload AdventureScene startup models/templates and hand a payload to `AdventureSceneScope`. | Releases any unconsumed payload before a new preload or RootScope disposal, transfers Addressables handle ownership to `AdventureSceneScope` on `Consume`, and asks `ViewManager` to preload the `AdventureView` root UXML before scene activation. |
| `ISceneTransitionPlayer` | `ViewOverlaySceneTransitionPlayer` | Let scene objects request scene fade transitions without directly using `ViewManager.OverlayLayer` or constructing transition objects. | Legacy bridge. Keeps UI Toolkit overlay details behind a narrow transition boundary. |
| `SceneManagerEx` | Root singleton | Centralize scene transition, preload, and Unity scene activation. | This is the current root scene-loading boundary. It is not the old manager-owned SceneManagerEx. |

SceneManagerEx failure tradeoff:

```text
SceneManagerEx starts fade-out, scene preload, and LoadSceneAsync in parallel.
If preload fails after LoadSceneAsync has started, Unity scene loading cannot be cancelled.
The current implementation releases allowSceneActivation=false to avoid stalling Unity's async operation queue.
For AdventureScene this can still activate the scene without AdventureScenePayload, and AdventureSceneScope will fail fast.
This is intentional for the current implementation, but a later transition API should add an explicit failure-recovery path
such as fade-in to the current scene or a loading-error scene.
```

Do not create one adapter per manager by default. Add a root adapter only when a VContainer-owned object needs a narrow boundary.

Recommended folder placement for these boundaries:

```text
Assets/@Scripts/Core/Ports/
Assets/@Scripts/Core/Adapters/
Assets/@Scripts/Core/Composition/
```

No generic view factory is currently registered. Scene-specific navigators resolve their scoped views through their scene container.

Current root registrations:

| Type / Interface | Registration | Lifetime | Notes |
| --- | --- | --- | --- |
| `ViewManager` | `Register<ViewManager>().AsSelf().As<IViewHost>()` | Root singleton | VContainer-created root UI host. `AsSelf` remains because root startup and scene transition still inject the concrete type. It owns the root `PanelSettings`, theme, and root-loaded view UXML Addressables handles until Root disposal. |
| `ViewTransitionManager` | `Register<ViewTransitionManager>()` | Root singleton | VContainer-created transition executor. Static `Instance` access has been removed; callers must receive it through DI or a narrow adapter. |
| `GameRootEntryPoint` | `RegisterEntryPoint<GameRootEntryPoint>()` | Root singleton | Stateless root initialization orchestrator. Calls `ViewManager.Initialize()` once. |
| `ISceneTransitionPlayer` | `ViewOverlaySceneTransitionPlayer` | Root singleton | Uses concrete `ViewManager` until a narrower overlay host port exists. |

Current `ViewManager` concrete-type injection audit:

| Consumer | Why it injects `ViewManager` today | Preferred later boundary |
| --- | --- | --- |
| `GameRootEntryPoint` | Needs to call `ViewManager.Initialize()` during root startup. | Keep direct while `ViewManager` is the only root UI object needing ordered initialization. |
| `ViewOverlaySceneTransitionPlayer` | Needs `OverlayLayer` for scene fade transitions. | Split `IViewOverlayHost` exposing only overlay access. |
| `AdventureSceneLoader` | Needs ViewManager-owned view template handle caching so `AdventureView` root UXML can be preloaded before scene activation. | Split a narrow view-template preload port if more scene preloaders need the same operation. |
| `ViewManagerBehavior` | Unity MonoBehaviour forwards viewport changes to its owning `ViewManager`. | Acceptable internal helper; not a composition boundary leak. |

| Type | Current owner | Final target | Notes |
| --- | --- | --- | --- |
| `AdventureStartState` | `RootLifetimeScope` | Root singleton | Temporary handoff state from CharacterSelect to AdventureSceneLoader. |
| `AdventureSceneLoader` | `RootLifetimeScope` | Root singleton | Preloads Adventure startup data and transfers payload ownership to AdventureSceneScope. |
| `ScenePreloadService` | `RootLifetimeScope` | Root singleton | Dispatches scene-specific preloaders. |

## Scene Scope Candidates

Scene scope is for controllers and presenters whose lifetime follows a scene-level flow.

TitleScene-specific ownership and migration notes are tracked in `docs/title-scene-vcontainer-lifecycle-design.md`.

| Type | Current owner | Final target | Notes |
| --- | --- | --- | --- |
| `TitleViewController` | `TitleSceneScope` registration | Title scene scope | Removed from DependencyManager. Created by VContainer when TitleSceneScope is present. |
| `CharacterSelectController` | `TitleSceneScope` registration | Title scene scope | Removed from DependencyManager registration. Created by VContainer when TitleSceneScope is present. |
| `ICharacterUnlockGateway` / `LegacyCharacterUnlockGateway` | `TitleSceneScope` registration | Transitional Title scene bridge, later save/progress boundary | Lets `CharacterSelectController` read unlock state without directly using `SaveManager`. The legacy gateway still uses `SaveManager` and `ProgressState`; do not treat save state as VContainer-owned yet. |
| `CharacterService` | `TitleSceneScope` registration | Title scene scope | Used by CharacterSelectController to build selection presentation. |
| `TitleView` | `TitleSceneScope` | Title scene scope | Scoped view. Receives `TitleViewController` through constructor injection; display goes through `ISceneViewNavigator`, not ViewManager-owned Push. |
| `SettingsView` | `TitleSceneScope` | Title scene scope | Scoped view. Display goes through `ISceneViewNavigator`; ViewManager attaches/detaches but does not dispose it. Close delegates to `SettingsViewController.OnClose`; there is no post-construction close callback injection. Internal manager dependencies are deferred cleanup. |
| `CharacterSelectView` | `TitleSceneScope` | Title scene scope | Scoped view. Receives `CharacterSelectController` through constructor injection; display goes through `ISceneViewNavigator`, not ViewManager-owned Push. Before display, `TitleSceneNavigator` injects `CardFaceWidgetTemplates` prepared by `TitleSceneCardFaceTemplateLoader` and the `SkillSlotWidget` template prepared by `TitleSceneSkillSlotTemplateLoader`. CharacterSelect dynamic widgets no longer use no-template `WaitForCompletion` fallback. |
| `TitleSceneCardFaceTemplateLoader` | `TitleSceneScope` registration | Title scene scope | Loads shared Portrait/Locked card face UXML templates for TitleScene-owned screens and releases the Addressables handles when TitleSceneScope is disposed. |
| `TitleSceneSkillSlotTemplateLoader` | `TitleSceneScope` registration | Title scene scope | Loads the shared SkillSlotWidget UXML template for CharacterSelect before the screen is shown and releases the Addressables handle when TitleSceneScope is disposed. |
| `TitleSceneLocalizationOwner` | `TitleSceneScope` registration | Title scene scope | Preloads Title/CharacterSelect/Settings localization tables asynchronously during `TitleSceneEntryPoint.Start`; releases the tables when TitleSceneScope is disposed. It must not use `WaitForCompletion`. |
| `TitleSceneBgmOwner` | `TitleSceneScope` registration | Title scene scope | Loads Title menu BGM asynchronously during `TitleSceneEntryPoint.Start`, plays it after load, and releases the Addressables handle when TitleSceneScope is disposed. It must not use `WaitForCompletion`. |
| `TitleSceneNavigator` | `TitleSceneScope` registration | Title scene scope | Single TitleScene navigation object. Shows Title, CharacterSelect, and Settings through `ISceneViewNavigator`. Resolves scoped views at display time with `IObjectResolver` to avoid the constructor cycle through `TitleViewController`. CharacterSelect display is asynchronous because required card face and skill slot templates are loaded before showing the screen. |
| `ISceneViewNavigator` / `SceneViewNavigator` | `TitleSceneScope` registration | Title scene scope | Uses `IViewHost` Attach/Detach and does not dispose views. Connected for TitleScene-scoped views only; do not reuse for legacy transient views without a disposal owner. |
| `AdventureScreenController` | `AdventureSceneScope` | Adventure scene scope | Constructed directly by VContainer. Owns Adventure screen/game startup and user interaction flow entry methods. |
| `AdventureStageAdvanceFlow` | `AdventureSceneScope` | Adventure scene scope | Centralizes post-encounter stage advance/complete branching. Returns `Advanced` only when the next stage should continue into immediate encounter checks. |
| `AdventureView` | `AdventureSceneScope` | Adventure scene scope | Receives `AdventureScreenController` and UI flows through constructor injection. Attached through `IViewHost`, not legacy `ViewManager.Push` ownership. |
| `AdventureSceneEntryPoint` | `AdventureSceneScope` | Adventure scene scope | VContainer entry point for AdventureScene startup. Binds screen event routes, initializes runtime, awaits async localization preload, attaches view, then starts the controller. |
| `AdventureSceneScope` | Adventure scene LifetimeScope | Adventure scene scope | Consumes AdventureScenePayload from AdventureSceneLoader and uses RootLifetimeScope as parent. |
| `AdventureSceneLocalization` | `AdventureSceneScope` | Adventure scene scope | Owns AdventureScene localization preload/release. Preload is async and must not use `WaitForCompletion`. |

## Adventure Scene Runtime Candidates

Adventure currently uses scene-scoped runtime/state/flow objects instead of the old broad service graph.

Current rule:

```text
Do not reintroduce AdventureService, CardDeckService, CardService, PlayerService, or CombatService as broad owners.
Keep mutable state in explicit runtime/state classes.
Keep orchestration in explicit flow classes.
Keep presentation conversion in AdventurePresenter.
```

Current AdventureSceneScope support:

```text
AdventureSceneLoader prepares AdventureScenePayload before scene activation.
AdventureSceneScope consumes the payload and registers scene-local runtime/state/flow/UI objects.
AdventureSceneEntryPoint binds event routes, initializes runtime, shows AdventureView, then starts AdventureScreenController.
AdventureScreenController coordinates game flows and emits presentation events.
AdventureView owns screen lifecycle and delegates UI operations to UIFlow objects.
```

Representative ownership map:

| Type | Current owner | Lifetime | Notes |
| --- | --- | --- | --- |
| `AdventureRunState` | `AdventureSceneScope` | Adventure scene | Holds the current run identity. |
| `AdventureProgress` | `AdventureSceneScope` | Adventure scene | Holds Adventure phase/intro completion. |
| `AdventureInputState` | `AdventureSceneScope` | Adventure scene | Holds game-side input mode. |
| `AdventureCards` / `CardRegistry` / `CardFactory` | `AdventureSceneScope` | Adventure scene | Owns runtime card instances by CardId. |
| `AdventureBoard` / `CardBoardState` | `AdventureSceneScope` | Adventure scene | Owns board-side CardId placement. |
| `AdventurePlayer` | `AdventureSceneScope` | Adventure scene | Holds selected character and player card. |
| `AdventureStageRuntime` | `AdventureSceneScope` | Adventure scene | Holds current stage offers and selected offer binding. |
| `AdventureCombatRuntime` | `AdventureSceneScope` | Adventure scene | Holds current combat participants and turn side. |
| `IntentRuntime` | `AdventureSceneScope` | Adventure scene | Holds current intent display/execution state. |
| `AdventureScreenController` | `AdventureSceneScope` | Adventure scene | Coordinates gameplay flows and presentation events. |
| `AdventureView` / UIFlow objects | `AdventureSceneScope` | Adventure scene | Owns screen lifecycle and UI presentation operations. |
| `AdventureCardDealAnimator` | `AdventureSceneScope` | Adventure scene | Plays intro deck-to-board card deal motion. `AdventureBoardLayout` owns slots/anchors and delegates this motion detail to the animator. |

## Object Creation Migration Order

Already established:

```text
1. Root adapter interfaces exist for view host and scene transition.
2. TitleScene resource owners exist and are created through TitleSceneScope.
3. RootLifetimeScope is created after ManagerRegistry initialization.
4. TitleSceneScope inherits root-level ports from RootLifetimeScope through its parent lookup.
5. Unity Scene + VContainer LifetimeScope is adopted as the target scene architecture.
6. TitleSceneScope composition verification passes.
7. ViewManager legacy stack ownership has explicit PushAndOwn / PopAndDispose / ClearOwnedViewsAndDetachAll methods.
8. TitleScene scoped views are displayed through SceneViewNavigator and are no longer disposed by ViewManager.
9. CharacterSelectController starts Adventure through AdventureStartState + SceneManagerEx.
10. AdventureSceneScope owns AdventureScreenController, AdventureView, runtime state, game flows, UI flows, and event binders.
```

Recommended remaining migration order:

```text
1. Remove direct close/navigation ownership from TitleScene views.
2. Add ViewManager Attach/Detach host path.
3. Introduce a navigator that uses the non-disposing host path.
4. Route TitleScene display through TitleSceneNavigator and the non-disposing navigator.
5. Prepare TitleSceneScope as the Unity scene LifetimeScope. Done.
6. Add TitleSceneEntryPoint for scene startup. Done.
7. Remove TitleScene-specific view factory / flow split in favor of TitleSceneNavigator. Done.
8. Move TitleScene navigation to TitleSceneScope ownership. Done.
9. Move TitleScene controllers to VContainer ownership. Done.
10. Move TitleScene views to SceneScope ownership. Done for TitleScene.
11. Place TitleSceneScope in the Unity TitleScene only after duplicate ownership is prevented. Done.
12. Replace the old adventure-start bridge with AdventureStartState + SceneManagerEx. Done.
13. Add AdventureSceneScope. Done.
14. Extract AdventureSceneLocalization. Done.
15. Add AdventureSceneEntryPoint. Done.
16. Move AdventureScreenController to VContainer ownership. Done.
17. Move Adventure runtime state into explicit scene-scoped runtime/state classes. Done for current AdventureScene path.
18. Continue removing stale service/controller names from docs, tests, and validation scripts.
19. Remove custom DependencyManager after all remaining ownership has an explicit VContainer replacement.
```

## MonoBehaviour Principle

The scene `LifetimeScope` MonoBehaviour is the adopted scene composition root.

TitleScene now contains a connected `TitleSceneScope` MonoBehaviour.

When MonoBehaviours are introduced:

```text
Existing scene component: RegisterComponent or InjectGameObject.
Prefab created by VContainer: container.Instantiate(prefab).
Scene hierarchy lookup: RegisterComponentInHierarchy only when the scene ownership is explicit.
```
