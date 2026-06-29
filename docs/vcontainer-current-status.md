# VContainer Current Status

## Purpose

This document is the current checkpoint for the VContainer migration.

Use it to answer:

```text
1. What is the final goal?
2. What is already done?
3. What is intentionally incomplete?
4. What should happen next?
```

Detailed design history remains in the other VContainer documents. This file is the short current-state summary.

## Final Goal

The final goal is:

```text
Remove the custom DependencyManager as an object creation and ownership system.
```

VContainer is the replacement ownership model.

Related goals:

```text
- Put objects into clear lifetimes: Root, Scene, AdventureSession.
- Prevent duplicate object creation.
- Make scene unload and session end release the correct objects.
- Reduce stale references, double dispose, and event subscription leaks.
- Make ViewManager a UI host, not the owner of every scene view.
- Keep the old manager-owned SceneManagerEx removed; the current `Game.Core.Adapters.SceneManagerEx` is a root scene-loading boundary, not a legacy manager.
```

Cold assessment:

```text
SceneManagerEx removal, ViewManager cleanup, and SettingsView cleanup are not separate final goals.
They are supporting work for DependencyManager removal and lifetime clarity.
```

## Current Architecture Direction

Target direction:

```text
Unity Scene
-> scene LifetimeScope
-> VContainer EntryPoint
-> scene startup
-> scoped IDisposable owners
-> Unity scene unload
-> LifetimeScope disposal
```

Current root:

```text
GameBootstrap
-> ManagerRegistry still initializes legacy managers
-> RootLifetimeScope is created after legacy manager initialization
-> GameRootEntryPoint initializes VContainer-owned root objects
```

Current scene direction:

```text
TitleScene
-> TitleSceneScope is connected
-> TitleSceneEntryPoint owns startup ordering directly

AdventureScene
-> AdventureSceneScope is the Adventure scene composition boundary
-> Adventure scene-local objects are AdventureSceneScope-owned
-> Adventure runtime state is split into scene-scoped runtime/state/flow classes
-> Adventure startup data is prepared by AdventureSceneLoader and consumed by AdventureSceneScope
```

## Completed

### Package And Root

```text
VContainer package is installed.
RootLifetimeScope exists.
GameBootstrap creates RootLifetimeScope after ManagerRegistry initialization.
RootLifetimeScope registers root VContainer services and ports.
GameRootEntryPoint is the root VContainer entry point.
ViewManager is VContainer-created as a root singleton.
GameRootEntryPoint calls ViewManager.Initialize once.
ViewManager is no longer a BaseManager or ManagerRegistry-owned manager.
```

Current root adapters include:

```text
IViewHost
ISceneTransitionPlayer
SceneManagerEx
ScenePreloadService
AdventureSceneLoader
```

Important current boundary:

```text
TitleScene uses ITitleSceneNavigator -> ISceneViewNavigator -> IViewHost.
IViewHost is currently implemented by ViewManager directly.
The root IViewNavigator / ViewManagerAdapter legacy bridge has been removed.
ViewOverlaySceneTransitionPlayer still injects concrete ViewManager as a transitional root bridge.
```

### TitleScene Scope

```text
TitleSceneScope is placed in TitleScene.unity.
TitleSceneScope uses GameBootstrap.RootLifetimeScope as parent.
TitleSceneEntryPoint is registered.
TitleSceneStartup has been removed.
TitleScene.OnLoaded no longer starts the title flow directly.
```

TitleScene resource ownership:

```text
TitleSceneLocalizationOwner owns title localization preload/release.
TitleSceneBgmOwner owns title BGM Addressables handle and release.
```

### TitleScene Controllers

```text
TitleViewController is no longer DependencyManager-owned.
CharacterSelectController is no longer DependencyManager-owned.
Both are registered in TitleSceneScope.
```

Navigation:

```text
TitleSceneEntryPoint delegates initial TitleView display to ITitleSceneNavigator.
TitleViewController delegates CharacterSelect/Settings navigation to ITitleSceneNavigator.
CharacterSelectView reports completed back-close animation to CharacterSelectController, which delegates HideCurrent to ITitleSceneNavigator.
SettingsView delegates close to SettingsViewController, which saves settings and delegates HideCurrent to ITitleSceneNavigator.
CharacterSelectController writes the selected character id to AdventureStartState.
CharacterSelectController requests scene loading through SceneManagerEx.
CharacterSelectController reads character data through CharacterService.
CharacterSelectController reads character unlock state through ICharacterUnlockGateway.
```

Current character catalog path:

```text
CharacterSelectController
-> CharacterService
-> DBManager
```

Current unlock-state bridge:

```text
CharacterSelectController
-> ICharacterUnlockGateway
-> LegacyCharacterUnlockGateway
-> SaveManager / ProgressState
```

Risk:

```text
SaveManager is still the real state owner.
CharacterService is a TitleSceneScope dependency used by CharacterSelectController.
The important current improvement is that TitleScene controller/view creation is no longer DependencyManager-owned.
```

### TitleScene Views

```text
TitleView is TitleSceneScope-owned.
CharacterSelectView is TitleSceneScope-owned.
SettingsView is TitleSceneScope-owned.
```

Display path:

```text
TitleSceneNavigator resolves TitleScene scoped views at display time to avoid constructor cycles.
TitleSceneNavigator is the only TitleScene object allowed to use IObjectResolver directly, and only for TitleSceneScope-owned views.
Views and controllers must not use IObjectResolver directly.
TitleScene views receive controllers through VContainer constructor injection.
CharacterSelectView and SettingsView no longer receive close/back callbacks through post-construction Initialize methods.
SceneViewNavigator shows/hides TitleScene views through BaseView.Show/Hide.
ViewManager attaches/detaches TitleScene views but does not dispose them.
BaseView.OnVisualTreeCloned is for one-time UXML element caching and UI Toolkit event subscription after CloneTree.
BaseView.OnShown/OnHidden is for scene navigator visibility lifecycle.
TitleView plays its intro only on first show.
CharacterSelectView restarts the new-game selection flow every time it is shown.
SettingsView refreshes current settings every time it is shown and preserves its last selected tab inside the view instance.
SettingsView applies root-layer visual classes through IViewHost from the view layer, not from SettingsViewController.
TitleView, CharacterSelectView, and SettingsView keep view-local intro/close animation behavior in their own view partials and USS.
TitleView unregisters button callbacks defensively and preserves BaseView disposal through base.Dispose().
CharacterSelectView and SettingsView preserve BaseView disposal through base.Dispose().
```

### TitleScene Legacy Boundary Check

Current check result:

```text
TitleSceneScope has no direct DependencyManager.Resolve calls.
TitleScene View/Controller types do not directly call SaveManager, AudioManager, GraphicManager, LocaleManager, or ViewManager.
CharacterSelectController depends on the root scene-loading boundary SceneManagerEx.
Remaining TitleScene-related legacy access is intentionally isolated inside explicit gateway types.
```

Allowed current legacy bridges:

```text
LegacySettingsGateway
LegacyCharacterUnlockGateway
```

Cold assessment:

```text
TitleScene is not free of legacy managers.
It is free of direct legacy manager access from its scene scope, views, and controllers.
That is the useful checkpoint before touching AdventureScene.
```

### ViewManager First Split

Done:

```text
ViewManager.Attach
ViewManager.Detach
ViewManager.DetachAll
IViewHost
ISceneViewNavigator
SceneViewNavigator
ViewManager VContainer singleton registration
GameRootEntryPoint root initialization ordering
```

Legacy ownership names are explicit:

```text
PushAndOwn
PopAndDispose
ClearOwnedViewsAndDetachAll
```

Compatibility wrappers still exist:

```text
Push
Pop
Clear
```

Current meaning:

```text
TitleScene uses non-disposing attach/detach.
Unmigrated scenes still use legacy Push/Pop/Clear ownership.
```

### Scene Loading Surface

Done:

```text
GameSceneId exists.
GameSceneNames maps GameSceneId to Unity scene names.
SceneManagerEx exposes Load(GameSceneId).
CharacterSelectController calls SceneManagerEx.Load(GameSceneId.Adventure) after setting AdventureStartState.
ScenePreloadService dispatches scene preload work.
AdventureSceneLoader preloads Adventure startup data before activation.
```

Current runtime registration:

```text
RootLifetimeScope registers SceneManagerEx, ScenePreloadService, and AdventureSceneLoader.
SceneManagerEx starts fade-out, scene preload, and Unity SceneManager.LoadSceneAsync in parallel.
SceneManagerEx blocks activation until fade-out and preload have completed and load progress reaches Unity's activation threshold.
The old manager-owned SceneManagerEx namespace is no longer in the active runtime loading path.
```

Transition ownership:

```text
FadeOut transition moved out of TitleScene.OnBeforeUnload.
SceneManagerEx plays ISceneTransitionPlayer.FadeOut before scene activation.
```

### Adventure Skeletons

Adventure scene startup:

```text
AdventureStartState stores the selected character id before AdventureScene loading.
AdventureSceneLoader preloads AdventureSceneInitialData, card UI models, widget templates, and AdventureView root UXML.
AdventureScenePayload transfers preloaded data and Addressables handle ownership into AdventureSceneScope.
AdventureSceneScope registers scene-local runtime/state/flow/presentation/UI objects.
AdventureSceneLocalization is AdventureSceneScope-owned.
AdventureSceneEntryPoint is AdventureSceneScope-owned.
```

Current Adventure behavior:

```text
AdventureSceneScope owns AdventureSceneEntryPoint, AdventureScreenController, AdventureView, and AdventureSceneLocalization.
AdventureSceneEntryPoint attaches AdventureView through IViewHost, not legacy ViewManager Push ownership.
AdventureSceneScope registers AdventureScreenController directly, not through a DependencyManager-backed factory.
AdventureScreenController is not DependencyManager-owned.
AdventureScreenController receives scene-scoped runtime/flow/presenter/events through VContainer constructor injection.
AdventureView receives AdventureScreenController and UIFlow objects through constructor injection.
Adventure card deal motion is scene-scoped through AdventureCardDealAnimator; AdventureBoardLayout keeps board slot/anchor rules and delegates deck-to-board motion.
AdventureView defensively clears widget/cue/card bindings during detach and Dispose.
AdventureView no longer receives any controller through custom DependencyManager field injection.
```

## Current Incomplete Areas

### SettingsView

`SettingsView` is scoped by TitleSceneScope and no longer directly uses legacy managers.

Remaining bridge:

```text
SettingsView
-> SettingsViewController
-> ISettingsGateway
-> LegacySettingsGateway
-> SaveManager / AudioManager / GraphicManager / LocaleManager
```

Risk:

```text
The view no longer owns the direct manager calls, but settings policy is still backed by legacy managers.
This is acceptable until Save/Audio/Graphic/Locale ownership is redesigned.
```

### ViewManager

Current mixed responsibilities:

```text
Root UI host
UXML loading
BaseView binding
Legacy stack navigation
Legacy view disposal
Scene unload cleanup
```

Current status:

```text
TitleScene path is separated.
Global ViewManager cleanup is not complete.
AdventureScene scene view attachment has moved to IViewHost.
The legacy Push/Pop/Clear ownership path still exists as compatibility surface for unmigrated or future audited flows.
```

### Scene Loading

Current status:

```text
The old manager-owned SceneManagerEx has been removed.
The active SceneManagerEx is Game.Core.Adapters.SceneManagerEx and is VContainer root-owned.
RootLifetimeScope registers SceneManagerEx directly.
SceneManagerEx coordinates fade-out, scene preload, and Unity SceneManager.LoadSceneAsync activation.
SceneManagerAdapter has been removed.
```

Remaining SceneManagerEx responsibilities:

```text
Start fade-out, preload, and LoadSceneAsync in parallel.
Block scene activation until fade-out and preload complete.
Release pending scene activation if preload/loading fails after LoadSceneAsync has started.
Reset loading state when Unity reports sceneLoaded.
```

Remaining scene-loading cleanup:

```text
SceneManagerEx is still a broad concrete root service.
Later cleanup can split narrower ports if multiple callers need only preload, transition, or activation behavior.
```

Target:

```text
Unity Scene + LifetimeScope should own scene lifecycle.
BaseSceneLifecycleRunner should shrink as BaseScene startup/shutdown hooks move into scene LifetimeScopes.
```

### Adventure

Adventure run-state redesign is active and no longer follows the old service graph.

Do not reintroduce these old service boundaries as-is:

```text
AdventureService
CardDeckService
CardService
PlayerService
CombatService
```

Reason:

```text
AdventureScreenController consumes scene-scoped flows through constructor injection from VContainer.
Adventure runtime is now split into state/runtime/flow/presenter classes under AdventureSceneScope.
Reintroducing broad legacy services would hide ownership again and make duplicate state likely.
Combat behavior should continue to move through explicit combat runtime/flow objects, not the old CombatService shape.
```

## Priority

Final priority axis:

```text
Move toward DependencyManager removal.
```

Supporting considerations:

```text
1. Memory/lifetime risk
2. Small changes that stay inside TitleScene
3. Old manager-owned SceneManagerEx removal remains protected
```

## Recommended Next Order

### 1. AdventureScene Dependency Boundary

Reason:

```text
AdventureScene now blocks BaseSceneLifecycleRunner removal and full DependencyManager removal.
AdventureScene owns the largest remaining legacy object graph.
Changing other managers first risks breaking AdventureScene through hidden DependencyManager coupling.
```

Current status:

```text
AdventureScene dependency map is documented.
AdventureView uses constructor injection and is AdventureSceneScope-owned.
AdventureScreenController uses constructor injection and is AdventureSceneScope-owned.
AdventureSceneEntryPoint attaches AdventureView through IViewHost, not legacy ViewManager Push ownership.
Adventure scene runtime/state/flow objects are AdventureSceneScope-owned.
AdventureSceneLoader prepares startup data before AdventureScene activation.
```

Remaining work:

```text
Finish dynamic UI interaction verification beyond AdventureScene startup.
Keep removing stale service/controller names from docs and tests.
Do not reintroduce broad AdventureService/CardDeckService/CardService/PlayerService/CombatService boundaries without a fresh ownership reason.
```

### 2. Scene Loading Boundary Cleanup

Reason:

```text
GameSceneId and SceneManagerEx.Load(GameSceneId) exist.
FadeOut and preload are centralized in SceneManagerEx.
```

Next safe step:

```text
Keep SceneManagerEx as the single app-level scene loading boundary until more scene flows exist.
If the class grows, split narrow ports for transition, preload, and activation rather than adding scene logic to controllers.
```

### 3. ViewManager Cleanup

Reason:

```text
TitleScene is already on attach/detach.
AdventureScene view attachment is also on IViewHost, but the global ViewManager compatibility stack still exists.
Full cleanup waits until all remaining view creation and disposal paths are audited.
```

Next safe step:

```text
Keep explicit ownership names.
Avoid changing Push/Pop semantics.
Reduce remaining concrete ViewManager root bridge dependencies.
Candidate ports: legacy owning stack port and overlay host port.
```

### 4. Adventure Run Lifetime

Status:

```text
Deferred.
```

Resume only when ready to touch:

```text
AdventureScreenController
AdventureView
AdventureScene startup
Adventure run-state objects
```

## Testing Policy

Use tests selectively.

```text
Documentation-only changes:
-> no tests

Disconnected skeleton or compile-surface changes:
-> Unity batchmode compile only

Runtime LifetimeScope connection:
-> compile + targeted PlayMode if ownership or scene loading changes

DependencyManager removal from runtime type:
-> compile + targeted composition/PlayMode verification
```

Do not run broad PlayMode tests by default because they are expensive.

## Current Next Decision

The next implementation decision is:

```text
Proceed with the next narrow boundary cleanup before broad manager cleanup.
```

Reason:

```text
SceneManagerEx is removed; remaining scene-lifecycle cleanup depends on BaseScene/Adventure cleanup.
ViewManager global cleanup cannot finish while the legacy stack compatibility path still exists.
DependencyManager cannot be safely reduced until remaining legacy service aliases and bridges are audited one by one.
```

## Active Documents

Read these in order:

```text
1. docs/vcontainer-current-status.md
2. docs/vcontainer-adoption-doc-index.md
3. docs/title-scene-vcontainer-implementation-phases.md
4. docs/title-scene-vcontainer-lifecycle-design.md
5. docs/view-manager-redesign-plan.md
6. docs/scene-lifetimescope-adoption-decision.md
7. docs/adventure-session-vcontainer-design.md
8. docs/vcontainer-registration-map.md
9. docs/vcontainer-memory-lifetime-checklist.md
```
