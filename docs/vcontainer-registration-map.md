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

Phase 1 does not transfer existing runtime services/controllers to VContainer ownership. The map exists to prevent accidental duplicate creation while the existing `DependencyManager` still owns runtime instances.

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
Do not connect VContainer registrations for existing [Dependency] services/controllers to runtime flow.
Do not connect SceneLifetimeScope to runtime flow.
Do not create AdventureSessionLifetimeScope in runtime flow until session ownership is ready.
Do not resolve game services from VContainer.
```

This prevents duplicate instances between VContainer and `DependencyManager`.

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
| `IAudioPlayer` | `AudioManagerAdapter` | Let VContainer-owned objects request BGM play/stop without directly using `AudioManager.Instance`. | Legacy bridge. Replace with a VContainer-native audio player only after audio ownership is redesigned. |
| `IViewHost` | `ViewManager` | Let scene-owned views attach/detach without transferring disposal ownership to `ViewManager`. | Host path only. It must not be used as a second owner for views already in the legacy stack. |
| `ISceneLoader` | `UnitySceneLoader` | Let controllers request scene loads without directly depending on Unity scene APIs. | New call sites should use `Load(GameSceneId)`. `UnitySceneLoader` owns fade-out and coordinates `BaseSceneLifecycleRunner` only for remaining legacy BaseScene scenes before calling Unity `SceneManager.LoadScene`. `SceneManagerEx` and `SceneManagerAdapter` have been removed. |
| `ISceneTransitionPlayer` | `ViewOverlaySceneTransitionPlayer` | Let scene objects request scene fade transitions without directly using `ViewManager.OverlayLayer` or `ViewTransitionManager.Instance`. | Legacy bridge. Keeps UI Toolkit overlay details behind a narrow transition boundary. |
| `IViewFactory` | `LegacyViewFactory` or later `VContainerViewFactory` | Centralize view creation. | Factory choice must not duplicate controller/view creation between `DependencyManager` and VContainer. |

Do not create one adapter per manager by default. Add a root adapter only when a VContainer-owned object needs a narrow boundary.

Recommended folder placement for these boundaries:

```text
Assets/@Scripts/Core/Ports/
Assets/@Scripts/Core/Adapters/
Assets/@Scripts/Core/Composition/
```

Existing `IViewFactory` and `LegacyViewFactory` remain under `Assets/@Scripts/Core/Manager/View/` for now to avoid unnecessary file movement.

Current root registrations:

| Type / Interface | Registration | Lifetime | Notes |
| --- | --- | --- | --- |
| `ViewManager` | `Register<ViewManager>().AsSelf().As<IViewHost>()` | Root singleton | VContainer-created root UI host. `AsSelf` remains because root startup and scene transition still inject the concrete type. |
| `GameRootEntryPoint` | `RegisterEntryPoint<GameRootEntryPoint>()` | Root singleton | Stateless root initialization orchestrator. Calls `ViewManager.Initialize()` once. |
| `ISceneTransitionPlayer` | `ViewOverlaySceneTransitionPlayer` | Root singleton | Uses concrete `ViewManager` until a narrower overlay host port exists. |

Current `ViewManager` concrete-type injection audit:

| Consumer | Why it injects `ViewManager` today | Preferred later boundary |
| --- | --- | --- |
| `GameRootEntryPoint` | Needs to call `ViewManager.Initialize()` during root startup. | Keep direct while `ViewManager` is the only root UI object needing ordered initialization. |
| `ViewOverlaySceneTransitionPlayer` | Needs `OverlayLayer` for scene fade transitions. | Split `IViewOverlayHost` exposing only overlay access. |
| `ViewManagerBehavior` | Unity MonoBehaviour forwards viewport changes to its owning `ViewManager`. | Acceptable internal helper; not a composition boundary leak. |

| Type | Current owner | Final target | Notes |
| --- | --- | --- | --- |
| `ProgressState` | `DependencyManager` | Root | Save state. Must not be duplicated. |
| `AudioSettingsState` | `DependencyManager` | Root | Save state. Must be shared with `AudioManager`. |
| `GraphicSettingsState` | `DependencyManager` | Root | Save state. Must be shared with `GraphicManager`. |
| `LocalizationSettingsState` | `DependencyManager` | Root | Save state. Must be shared with `LocaleManager`. |
| `CharacterService` | `DependencyManager` | Root | Static data lookup through `DBManager`. |
| `MonsterService` | `DependencyManager` | Root | Static data lookup through `DBManager`. |

## Scene Scope Candidates

Scene scope is for controllers and presenters whose lifetime follows a scene-level flow.

TitleScene-specific ownership and migration notes are tracked in `docs/title-scene-vcontainer-lifecycle-design.md`.

| Type | Current owner | Final target | Notes |
| --- | --- | --- | --- |
| `TitleViewController` | `TitleSceneScope` registration | Title scene scope | Removed from DependencyManager. Created by VContainer when TitleSceneScope is present. |
| `CharacterSelectController` | `TitleSceneScope` registration | Title scene scope | Removed from DependencyManager registration. Created by VContainer when TitleSceneScope is present. |
| `ICharacterSelectCatalog` / `LegacyCharacterSelectCatalog` | `TitleSceneScope` registration | Transitional Title scene bridge, later character catalog boundary | Lets `CharacterSelectController` read character selection data without directly using `CharacterService`. The legacy catalog still resolves `CharacterService` from `DependencyManager`; do not treat character catalog ownership as VContainer-owned yet. |
| `ICharacterUnlockGateway` / `LegacyCharacterUnlockGateway` | `TitleSceneScope` registration | Transitional Title scene bridge, later save/progress boundary | Lets `CharacterSelectController` read unlock state without directly using `SaveManager`. The legacy gateway still uses `SaveManager` and `ProgressState`; do not treat save state as VContainer-owned yet. |
| `IAdventureSessionStarter` / `LegacyAdventureSessionStarter` | `TitleSceneScope` direct registration | Transitional boundary, later AdventureSession factory/starter | Creates AdventureSessionLifetimeScope and loads AdventureScene. `TitleSceneScope` no longer resolves the adventure service list directly. CardBoardService is VContainer-owned; the remaining run-state services are exposed through DependencyManager-created aliases in AdventureSessionLifetimeScope. Removed from DependencyRegistry so DependencyManager no longer creates this TitleScene bridge. |
| `TitleView` | `TitleSceneScope` | Title scene scope | Scoped view. Receives `TitleViewController` through constructor injection; display goes through `ISceneViewNavigator`, not ViewManager-owned Push. |
| `SettingsView` | `TitleSceneScope` | Title scene scope | Scoped view. Display goes through `ISceneViewNavigator`; ViewManager attaches/detaches but does not dispose it. Close delegates to `SettingsViewController.OnClose`; there is no post-construction close callback injection. Internal manager dependencies are deferred cleanup. |
| `CharacterSelectView` | `TitleSceneScope` | Title scene scope | Scoped view. Receives `CharacterSelectController` through constructor injection; display goes through `ISceneViewNavigator`, not ViewManager-owned Push. Back close animation completion delegates to `CharacterSelectController.OnBackClosed`; there is no post-construction back callback injection. |
| `ITitleSceneNavigator` / `TitleSceneNavigator` | `TitleSceneScope` registration | Title scene scope | Single TitleScene navigation port. Shows Title, CharacterSelect, and Settings through `ISceneViewNavigator`. Resolves scoped views at display time with `IObjectResolver` to avoid the constructor cycle through `TitleViewController`. |
| `ISceneViewNavigator` / `SceneViewNavigator` | `TitleSceneScope` registration | Title scene scope | Uses `IViewHost` Attach/Detach and does not dispose views. Connected for TitleScene-scoped views only; do not reuse for legacy transient views without a disposal owner. |
| `AdventureController` | `AdventureSceneScope` | Adventure scene scope | Removed from DependencyManager registration. Constructed directly by VContainer with session-owned CardBoard/Card/Player services and remaining legacy aliases. |
| `AdventureView` | `AdventureSceneScope` | Adventure scene scope | Receives `AdventureController` through constructor injection. Attached through `IViewHost`, not legacy `ViewManager.Push` ownership. |
| `AdventureSceneStartup` | `AdventureSceneScope` | Adventure scene scope | VContainer entry point for AdventureScene startup. Preloads localization, attaches view, and starts controller. |
| `AdventureSceneScope` | Runtime-created by remaining `AdventureScene` BaseScene bridge | Adventure scene scope | Connected as an interim bridge. It uses `AdventureSessionRuntime.CurrentScope` as parent when available, otherwise falls back to `RootLifetimeScope`. Final target is a scene-placed/session-child scope after AdventureSessionLifetimeScope owns run state. |
| `AdventureSceneLocalizationOwner` | `AdventureSceneScope` | Adventure scene scope | Owns AdventureScene localization preload/release. |

## Adventure Session Scope Candidates

Adventure session scope is for run-specific state. These are currently global but should not remain root-owned long term.

Read `docs/adventure-session-vcontainer-design.md` before moving any of these services.

Current rule:

```text
Do not register these services as VContainer-created services while DependencyManager still owns them.
```

Current session-scope support:

```text
AdventureSessionRuntime can store the active AdventureSessionLifetimeScope.
AdventureSessionLifetimeScope uses RootLifetimeScope as parent.
AdventureSceneScope can use AdventureSessionRuntime.CurrentScope as parent when it exists.
LegacyAdventureSessionStarter creates the session scope at adventure start.
AdventureSessionLifetimeScope registers AdventureSessionInitializer, VContainer-owned Adventure/CardDeck/CardBoard/Card/Player run-state services, and remaining DependencyManager-created aliases.
AdventureSessionInitializer receives AdventureService, CardDeckService, CardBoardService, CardService, and PlayerService from VContainer and the remaining legacy dependencies through alias registrations.
AdventureService, CardDeckService, CardBoardService, CardService, and PlayerService are VContainer-owned in AdventureSessionLifetimeScope.
```

| Type | Current owner | Final target | Why not root long term | Migration blocker |
| --- | --- | --- | --- | --- |
| `AdventureSessionState` | `AdventureSessionLifetimeScope` | AdventureSession | Holds current adventure session. | Moved with AdventureService and CardDeckService to keep seed/deck identity aligned. |
| `AdventureSessionFactory` | `AdventureSessionLifetimeScope` | AdventureSession | Creates AdventureSession from content data and seed. | Moved with AdventureService and CardDeckService. |
| `AdventureService` | `AdventureSessionLifetimeScope` | AdventureSession | Public session API over AdventureSessionState/AdventureSessionFactory. | Removed from DependencyRegistry and no longer registered by LegacyAdventureRunStateInstaller. |
| `CardDeckState` | `AdventureSessionLifetimeScope` | AdventureSession | Holds deck, pools, random state, draw index. | Moved with AdventureService and CardDeckService. |
| `CardDeckBuilder` | `AdventureSessionLifetimeScope` | AdventureSession | Builds deck and initial pools from content data and seed. | Moved with AdventureService and CardDeckService. |
| `CardDeckService` | `AdventureSessionLifetimeScope` | AdventureSession | Draws cards and resolves choice cards through CardDeckState. | Removed from DependencyRegistry and no longer registered by LegacyAdventureRunStateInstaller. |
| `CardRegistry` | `AdventureSessionLifetimeScope` | AdventureSession | Holds card dictionary and next card id. | Moved with CardService and PlayerService to avoid split player-card state. |
| `CardFactory` | `AdventureSessionLifetimeScope` | AdventureSession | Creates and mutates Card model/face/tag/attribute setup. | Moved with CardService and PlayerService. |
| `CardService` | `AdventureSessionLifetimeScope` | AdventureSession | Public card API over CardRegistry/CardFactory. | Removed from DependencyRegistry and no longer registered by LegacyAdventureRunStateInstaller. |
| `CardBoardState` | `AdventureSessionLifetimeScope` | AdventureSession | Holds card zone placement. | Split from CardBoardService so state and mutation logic are separate. |
| `CardBoardService` | `AdventureSessionLifetimeScope` | AdventureSession | Mutates card board placement state. | First moved run-state service. Removed from DependencyRegistry and no longer registered by LegacyAdventureRunStateInstaller. |
| `PlayerRunState` | `AdventureSessionLifetimeScope` | AdventureSession | Holds current player and player card. | Moved with CardService to avoid split player-card state. |
| `PlayerService` | `AdventureSessionLifetimeScope` | AdventureSession | Public player run API over PlayerRunState. | Removed from DependencyRegistry and no longer registered by LegacyAdventureRunStateInstaller. |
| `CombatService` | `DependencyManager` | AdventureSession or combat sub-scope decision | Holds combat state and `GameplayMessageManager` death subscription. | Disposal timing must be explicit before scope migration. |

## Object Creation Migration Order

Already established:

```text
1. IViewFactory boundary exists.
2. LegacyViewFactory exists.
3. Root adapter interfaces exist.
4. TitleScene resource owners exist and are created through TitleSceneScope.
5. RootLifetimeScope is created after ManagerRegistry initialization.
6. TitleSceneScope inherits root-level ports from RootLifetimeScope through its parent lookup.
7. Unity Scene + VContainer LifetimeScope is adopted as the target scene architecture.
8. TitleSceneScope composition verification passes in batchmode.
9. TitleSceneScope filtered PlayMode verification passes through Game.PlayMode.Tests.
10. ViewManager legacy stack ownership has explicit PushAndOwn / PopAndDispose / ClearOwnedViewsAndDetachAll methods.
11. TitleScene scoped views are displayed through SceneViewNavigator and are no longer disposed by ViewManager.
12. LegacyAdventureSessionStarter is removed from DependencyRegistry and resolved through TitleSceneScope as IAdventureSessionStarter.
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
12. Remove LegacyAdventureSessionStarter from DependencyRegistry. Done.
13. Document AdventureSessionScope ownership boundary. Done.
14. Add AdventureSceneScope skeleton. Done.
15. Extract AdventureScene localization owner. Done.
16. Design AdventureScene startup entry point.
17. Move AdventureController to VContainer ownership.
18. Move stateful adventure services into AdventureSessionScope.
19. Remove custom DependencyManager after all ownership is transferred.
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
