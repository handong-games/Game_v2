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
- Keep SceneManagerEx removed by moving scene lifecycle to Unity Scene + LifetimeScope.
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
-> AdventureSceneScope is created by the remaining BaseScene bridge
-> Adventure scene-local objects are AdventureSceneScope-owned
-> AdventureService, CardDeckService, CardBoardService, CardService, and PlayerService are AdventureSessionLifetimeScope-owned
-> remaining Adventure run-state services are still DependencyManager-owned aliases
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
IAudioPlayer
IViewHost
ISceneLoader
ISceneTransitionPlayer
IViewFactory
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
CharacterSelectController delegates adventure start to IAdventureSessionStarter.
CharacterSelectController reads character catalog data through ICharacterSelectCatalog.
CharacterSelectController reads character unlock state through ICharacterUnlockGateway.
```

Current character catalog bridge:

```text
CharacterSelectController
-> ICharacterSelectCatalog
-> LegacyCharacterSelectCatalog
-> DependencyManager / CharacterService / DBManager
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
CharacterService is still the real catalog owner.
The important current improvement is that the TitleScene controller no longer directly reaches into SaveManager or CharacterService.
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
TitleScene View/Controller types do not directly call SaveManager, AudioManager, GraphicManager, LocaleManager, ViewManager, or SceneManagerEx.
Remaining TitleScene-related legacy access is intentionally isolated inside Legacy* bridge types.
```

Allowed current legacy bridges:

```text
LegacySettingsGateway
LegacyCharacterUnlockGateway
LegacyCharacterSelectCatalog
LegacyAdventureSessionStarter
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

### LegacyAdventureSessionStarter

```text
LegacyAdventureSessionStarter is no longer in DependencyRegistry.
DependencyManager no longer creates LegacyAdventureSessionStarter.
TitleSceneScope registers it directly as IAdventureSessionStarter.
TitleSceneScope no longer resolves the adventure service list itself.
```

Still true:

```text
LegacyAdventureSessionStarter still resolves adventure run-state services from DependencyManager internally.
This is a bridge, not final ownership.
```

### Scene Loading Surface

Done:

```text
GameSceneId exists.
GameSceneNames maps GameSceneId to Unity scene names.
ISceneLoader exposes Load(GameSceneId).
LegacyAdventureSessionStarter calls Load(GameSceneId.Adventure).
UnitySceneLoader is registered as the runtime ISceneLoader.
```

Current runtime registration:

```text
RootLifetimeScope registers UnitySceneLoader.
UnitySceneLoader coordinates BaseSceneLifecycleRunner only for remaining legacy BaseScene scenes before SceneManager.LoadScene.
SceneManagerEx is no longer in the active runtime loading path.
```

Transition ownership:

```text
FadeOut transition moved out of TitleScene.OnBeforeUnload.
ISceneLoader implementations now play ISceneTransitionPlayer.FadeOut before loading.
```

### Adventure Skeletons

Adventure session bridge:

```text
AdventureSessionLifetimeScope has RootLifetimeScope parent wiring.
AdventureSessionRuntime exists and can store the current AdventureSessionLifetimeScope.
LegacyAdventureSessionStarter creates AdventureSessionLifetimeScope at adventure start.
AdventureSessionLifetimeScope registers AdventureSessionInitializer, VContainer-owned CardBoardState/CardBoardService, and remaining DependencyManager-created run-state aliases.
AdventureSessionInitializer runs the current adventure-start initialization sequence.
CardBoardService is VContainer-owned in AdventureSessionLifetimeScope.
Other run-state services are not VContainer-owned yet.
AdventureSceneScope exists and is runtime-created by AdventureScene.
AdventureSceneLocalizationOwner is AdventureSceneScope-owned.
AdventureSceneStartup is AdventureSceneScope-owned.
AdventureScene dependency map exists in docs/adventure-session-vcontainer-design.md.
```

Current Adventure behavior:

```text
AdventureScene still inherits BaseScene.
AdventureScene creates @AdventureSceneScope from the remaining BaseScene bridge.
AdventureSceneScope owns AdventureSceneStartup, AdventureController, AdventureView, and AdventureSceneLocalizationOwner.
AdventureSceneStartup attaches AdventureView through IViewHost, not legacy ViewManager Push ownership.
AdventureSceneScope registers AdventureController directly, not through a DependencyManager-backed factory.
AdventureController is no longer DependencyManager-owned.
AdventureController receives run-state services through VContainer constructor injection.
AdventureService, CardDeckService, CardBoardService, CardService, and PlayerService are VContainer-owned by AdventureSessionLifetimeScope.
CombatService and CharacterService are still DependencyManager-owned and exposed to VContainer as aliases.
AdventureController guards duplicate StartAdventure event registration.
AdventureView defensively unregisters static/widget/cue handlers during Dispose.
AdventureView no longer receives AdventureController through custom DependencyManager field injection.
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
SceneManagerEx has been removed from code.
SceneManagerEx is no longer a BaseManager and is no longer in ManagerRegistry.
RootLifetimeScope now registers ISceneLoader as UnitySceneLoader.
UnitySceneLoader coordinates BaseSceneLifecycleRunner only for remaining legacy BaseScene scenes before Unity SceneManager.LoadScene.
SceneManagerAdapter has been removed.
```

Remaining SceneManagerEx responsibilities:

```text
None in active runtime flow.
None in compatibility flow.
```

Remaining BaseSceneLifecycleRunner responsibilities:

```text
AdventureScene BaseScene object creation
AdventureScene BaseScene.Loaded
AdventureScene BaseScene.BeforeUnload
AdventureScene BaseScene.Unloaded
current/previous scene tracking
Unity scene loaded/unloaded event subscription
```

Target:

```text
Unity Scene + LifetimeScope should own scene lifecycle.
BaseSceneLifecycleRunner should shrink as BaseScene startup/shutdown hooks move into scene LifetimeScopes.
```

### Adventure

Adventure run-state migration is active but must proceed through service redesign.

Do not move these as-is:

```text
AdventureService
CardDeckService
CardService
PlayerService
CombatService
AdventureView
```

Reason:

```text
AdventureController consumes services through constructor injection from VContainer.
AdventureService, CardDeckService, CardBoardService, CardService, and PlayerService are VContainer-owned; the other current service registrations are DependencyManager-created aliases.
AdventureView consumes AdventureController through constructor injection and is AdventureSceneScope-owned.
Moving more run-state services without first splitting state/service/factory responsibilities would create duplicate or drifting run state.
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
3. SceneManagerEx removal progress
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
AdventureScene still uses BaseScene.
AdventureSceneScope is runtime-created by AdventureScene.
AdventureView uses constructor injection and is AdventureSceneScope-owned.
AdventureController uses constructor injection and is AdventureSceneScope-owned.
AdventureSceneStartup attaches AdventureView through IViewHost, not legacy ViewManager Push ownership.
CardBoardService is AdventureSessionLifetimeScope-owned.
Remaining Adventure run-state services are still DependencyManager global dependencies exposed to VContainer as aliases.
```

Remaining work:

```text
Use docs/adventure-run-state-service-redesign.md as the service refactor order.
CardBoardState split is done.
CardRegistry/CardFactory split is done.
PlayerRunState split is done.
CardService and PlayerService ownership move is done.
AdventureSessionState/AdventureSessionFactory and CardDeckState/CardDeckBuilder splits are done.
AdventureService and CardDeckService ownership move is done.
CombatService lifetime and event subscription boundaries are documented in docs/combat-service-lifetime-inspection.md.
CombatService is intentionally deferred.
Do not move CombatService as-is.
```

### 2. SceneManagerEx Removal Prep

Reason:

```text
GameSceneId and UnitySceneLoader already exist.
FadeOut transition already moved to loader implementations.
```

Next safe step:

```text
Identify remaining BaseScene startup/unload responsibilities.
Keep BaseSceneLifecycleRunner limited to AdventureScene until AdventureSceneScope owns startup/unload.
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

### 4. AdventureSessionScope

Status:

```text
Deferred.
```

Resume only when ready to touch:

```text
AdventureController
AdventureView
AdventureScene startup
Adventure run-state services
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
