# TitleScene VContainer Implementation Phases

## Current Status Note

Read this document together with `docs/vcontainer-adoption-doc-index.md`.

This document is the current authority for TitleScene implementation order.

Scene architecture decision:

```text
Unity Scene + VContainer LifetimeScope is the adopted target direction.
SceneManagerEx has been removed from code.
BaseSceneLifecycleRunner remains temporarily while BaseScene responsibilities move into scene LifetimeScope entry points and scene loader services.
```

## Purpose

This document defines the staged implementation order for moving TitleScene toward VContainer ownership.

This is a sequencing document. It exists to prevent the migration from accidentally mixing these separate concerns:

```text
Resource ownership
ViewManager redesign
View ownership
Controller ownership
DependencyManager removal
```

Cold assessment:

```text
The dangerous failure mode is not "VContainer is hard".
The dangerous failure mode is two systems owning the same object at the same time.
```

## Current Skeleton State

Already added as migration foundations:

```text
Root adapter boundaries
- IAudioPlayer
- IViewHost
- ISceneLoader

Legacy adapters
- AudioManagerAdapter
- UnitySceneLoader
- ViewManager implements IViewHost directly

TitleScene support classes
- TitleSceneBgmOwner
- TitleSceneLocalizationOwner
- TitleSceneNavigator

Scopes
- RootLifetimeScope
- TitleSceneScope
```

Current runtime status after Phase 1 resource-owner application:

```text
GameBootstrap creates RootLifetimeScope after ManagerRegistry initialization.
GameBootstrap exposes RootLifetimeScope for scene-scope parent lookup.
TitleScene contains a scene-placed @TitleSceneScope component.
TitleScene does not create or resolve TitleSceneScope from code.
ViewManager still owns transient view lifetime for unmigrated scenes.
CharacterSelectController is registered in TitleSceneScope.
TitleViewController has been removed from DependencyManager and is registered in TitleSceneScope.
TitleSceneLegacyRuntimeFactory and TitleSceneLegacyRuntime have been removed.
TitleSceneEntryPoint contains the startup sequence directly.
TitleScene.OnLoaded no longer starts the title flow directly.
SettingsView close no longer calls ViewManager.Pop directly; the close callback is wired by the creation path.
CharacterSelectView Back close no longer calls ViewManager.Pop directly; the back-close callback is wired by the creation path.
CharacterSelectView no longer calls SceneManagerEx.LoadScene directly.
CharacterSelectController AdventureScene loading goes through ISceneLoader.
CharacterSelectController adventure-start responsibility is split behind LegacyAdventureSessionStarter.
TitleScene views receive controllers through VContainer constructor injection, not custom DependencyManager field injection.
LegacyAdventureSessionStarter has a constructor-injection path for ISceneLoader and TitleSceneScope registers it directly as IAdventureSessionStarter.
LegacyAdventureSessionStarter still resolves legacy adventure/session services internally through DependencyManager.
LegacyAdventureSessionStarter has been removed from DependencyRegistry, so DependencyManager no longer creates the TitleScene bridge.
ViewManager Attach/Detach and SceneViewNavigator exist.
TitleScene runtime now uses SceneViewNavigator for TitleScene-scoped views.
TitleScene view registrations exist in TitleSceneScope and are runtime-owned by the connected scene scope.
ViewManager still owns legacy transient view lifetime for unmigrated scenes.
TitleSceneScope overrides parent lookup to use the GameBootstrap-created RootLifetimeScope.
TitleSceneEntryPoint is registered in TitleSceneScope and owns startup ordering directly.
TitleViewController delegates CharacterSelect/Settings navigation to ITitleSceneNavigator and is no longer DependencyManager-owned.
TitleSceneEntryPoint calls ITitleSceneNavigator.ShowTitle.
TitleSceneScope inherits root-level ports from RootLifetimeScope, but TitleScene view display uses the scene-scoped ISceneViewNavigator path.
TitleSceneScope registers ISceneViewNavigator for TitleScene-scoped views.
TitleScene resolves ISceneTransitionPlayer from RootLifetimeScope for before-unload transition.
```

This state is intentionally partial. Resource ownership has improved, but DI ownership has not migrated.
TitleScene scene startup and controller creation have migrated to VContainer; View ownership and AdventureSession ownership have not.

## Phase 0: Skeleton Only

Goal:

```text
Compile-safe foundation.
No runtime ownership transfer.
```

Allowed:

```text
- Add interfaces.
- Add adapters.
- Add owner skeletons.
- Add scope skeletons.
- Add documentation.
```

Not allowed:

```text
- Connect RootLifetimeScope to GameBootstrap.
- Place TitleSceneScope in the Unity TitleScene runtime.
- Register a type in VContainer while DependencyManager also creates the runtime instance.
- Change ViewManager Pop/Clear semantics.
```

Acceptance:

```text
Unity script compilation passes.
No runtime flow depends on the new scopes.
```

Current status:

```text
Done.
```

## Phase 1: Apply TitleScene Resource Owners Without VContainer Runtime

Goal:

```text
Fix resource ownership problems before DI ownership changes.
```

Candidate changes:

```text
TitleSceneEntryPoint
-> uses TitleSceneLocalizationOwner through TitleSceneScope
-> uses TitleSceneBgmOwner through TitleSceneScope
```

Expected improvement:

```text
Localization preload/release uses one table list.
CharacterNames release mismatch is fixed.
TitleMenuBgm Addressables handle is stored and released.
```

Risk:

```text
TitleSceneBgmOwner depends on IAudioPlayer inherited from RootLifetimeScope.
If RootLifetimeScope is not created before TitleSceneScope starts, TitleScene startup cannot build correctly.
```

Cold assessment:

```text
This phase improves memory/resource correctness without solving DI ownership.
It is worth doing before controller migration.
```

Acceptance:

```text
TitleScene loads and releases the exact same localization table list.
TitleScene stops BGM before releasing the Addressables handle.
No controller ownership changes.
Unity script compilation passes.
```

Current status:

```text
Done.
TitleSceneEntryPoint uses TitleSceneLocalizationOwner and TitleSceneBgmOwner through TitleSceneScope.
VContainer now owns TitleScene startup objects.
TitleScene views now use the scoped SceneViewNavigator path; unmigrated scenes still use ViewManager's transient path.
```

## Phase 2: Remove View Close/Navigation Intent From Views

Goal:

```text
Prepare for SceneScope-owned views by removing direct ViewManager/SceneManager calls from TitleScene-owned views.
```

Target call sites:

```text
Done:
- SettingsView direct ViewManager.Instance.Pop
- CharacterSelectView -> ViewManager.Instance.Pop
- CharacterSelectView -> SceneManagerEx.Instance.LoadScene<AdventureScene>
- CharacterSelectController -> SceneManagerEx.Instance.LoadScene<AdventureScene>

Remaining:
- SettingsView -> SettingsViewController -> ISettingsGateway -> LegacySettingsGateway -> SaveManager/AudioManager/GraphicManager/LocaleManager
- SettingsView -> IViewHost.RootLayer for view-local root-layer class expression
```

Preferred direction:

```text
View
-> observes UI event or transition completion
-> calls controller

Controller
-> validates intent
-> asks ITitleSceneNavigator or ISceneLoader
```

Recommended order:

```text
1. SettingsView close.
2. CharacterSelectView Back.
3. CharacterSelectView Start.
```

SettingsView sub-phases:

```text
Phase 2A:
-> Remove direct ViewManager.Pop from SettingsView close flow.
-> Preserve existing SaveManager/AudioManager/GraphicManager/LocaleManager behavior.

Phase 2B and later:
-> Move SaveManager.SaveAll out of SettingsView.
-> Move AudioManager calls out of SettingsView.
-> Move GraphicManager calls out of SettingsView.
-> Move LocaleManager calls out of SettingsView.
-> Keep root-layer class expression in the view unless it grows into a dedicated display policy.
```

Final SettingsView purpose:

```text
SettingsView displays settings and forwards user intent to a controller.
SettingsView should not directly change settings or close itself through ViewManager.
```

Risk:

```text
CharacterSelectView uses transition completion to decide Back vs Start.
Changing this too early can break close animation timing or double-load AdventureScene.
```

Acceptance:

```text
TitleScene-owned views no longer directly call ViewManager.Instance.Pop.
TitleScene-owned views no longer directly call SceneManagerEx.Instance.LoadScene.
Existing transient behavior is preserved through the controller/flow path.
```

Current Phase 2A status:

```text
SettingsView close direct Pop removal is done.
SettingsView no longer directly uses SaveManager, AudioManager, GraphicManager, LocaleManager, or ViewManager.
SettingsViewController delegates settings operations to ISettingsGateway.
LegacySettingsGateway contains the remaining SaveManager, AudioManager, GraphicManager, and LocaleManager bridge.
SettingsView reads IViewHost.RootLayer to apply view-local root-layer classes.
CharacterSelectView Back direct Pop removal is done.
CharacterSelectView direct SceneManagerEx.LoadScene removal is done.
CharacterSelectController direct SceneManagerEx.LoadScene removal is done.
CharacterSelectController is no longer DependencyManager-owned; TitleSceneScope registers it and VContainer injects it into CharacterSelectView through the constructor.
CharacterSelectController delegates adventure run-state initialization and scene loading to LegacyAdventureSessionStarter.
TitleViewController direct child view Create/Push removal is done.
TitleViewController no longer constructs TitleSceneViewFlow.
TitleViewController depends on ITitleSceneNavigator and is no longer DependencyManager-owned.
TitleScene direct root TitleView Create/Push removal is done.
TitleSceneScope now constructs TitleSceneNavigator through VContainer registration.
TitleScene direct ViewManager.OverlayLayer and ViewTransitionManager access removal is done.
TitleScene resolves ISceneTransitionPlayer from RootLifetimeScope instead of constructing the transition adapter manually.
```

## Phase 3: Redesign ViewManager Toward Root UI Host

Goal:

```text
Separate view host behavior from view ownership.
```

Current incompatible behavior:

```text
ViewManager.Pop disposes views.
ViewManager.Clear disposes views.
ViewManager.OnSceneUnloaded calls Clear.
```

Target:

```text
ViewManager
-> UIDocument and layers
-> attach/detach
-> overlay/root host access
-> viewport/aspect updates

SceneScope
-> view ownership and disposal
```

Migration shape:

```text
1. Add non-disposing Attach/Detach API.
2. Keep legacy Push/Pop/Clear behavior temporarily.
3. Introduce navigator that uses Attach/Detach.
4. Move TitleScene flow to the non-disposing path.
5. Only then consider scoped view registration.
```

Risk:

```text
Renaming or changing Pop semantics without changing call sites will create hidden ownership bugs.
```

Acceptance:

```text
There is a way to show/hide or attach/detach a view without disposing it.
Legacy Push/Pop/Clear still works for unmigrated scenes.
The disposal owner is explicit per path.
```

Current status:

```text
Done:
- ViewManager.Attach/Detach/DetachAll
- IViewHost / ViewManager direct implementation
- ISceneViewNavigator / SceneViewNavigator
- TitleSceneNavigator uses ISceneViewNavigator.
- TitleScene-scoped views are attached/detached without ViewManager disposal.

Still legacy outside TitleScene:
- The legacy ViewManager transient stack still exists as compatibility surface.
- ViewManager still keeps Push/Pop/Clear compatibility behavior.
- Addressables UXML asset ownership still lives inside ViewManager.
```

## Phase 4: Connect RootLifetimeScope

Goal:

```text
Create RootLifetimeScope at application lifetime without replacing ManagerRegistry.
```

Allowed:

```text
GameBootstrap
-> still calls ManagerRegistry.AllInit
-> creates RootLifetimeScope after or alongside legacy managers
```

Not allowed:

```text
- Move existing managers into VContainer.
- Remove ManagerRegistry.
- Replace BaseManager<T>.Instance access globally.
```

Risk:

```text
RootLifetimeScope creation order relative to ManagerRegistry matters.
Adapters call Manager.Instance, so legacy managers must already exist before adapter use.
```

Acceptance:

```text
RootLifetimeScope exists for application lifetime.
Existing managers remain ManagerRegistry-owned.
No gameplay flow changes yet.
```

Current status:

```text
Done.
GameBootstrap creates @RootLifetimeScope after ManagerRegistry.AllInit and gameplay cue initialization.
RootLifetimeScope registers only adapters and does not create existing managers.
TitleSceneScope inherits root-level ports from RootLifetimeScope at the scene composition boundary.
```

Resolve rule:

```text
Allowed:
TitleScene -> GameBootstrap.ResolveRoot<ISceneTransitionPlayer>

Not allowed:
TitleViewController -> ResolveRoot
CharacterSelectController -> ResolveRoot
Views -> ResolveRoot
Domain services -> ResolveRoot
```

Reason:

```text
TitleScene is currently the composition boundary for this migration step.
Letting controllers or views call ResolveRoot would recreate service-locator coupling under a VContainer name.
```

## Phase 5: Connect TitleSceneScope

Goal:

```text
Prepare TitleSceneScope as the Unity scene LifetimeScope for TitleScene.
```

Allowed:

```text
TitleSceneScope is placed in the Unity TitleScene as @TitleSceneScope.
TitleSceneEntryPoint starts TitleScene directly.
TitleScene.OnLoaded performs no direct startup work.
Keep ViewManager ownership intact until scoped view ownership is safe.
```

Resolve rule:

```text
ResolveRoot is a temporary bridge only.
The target is constructor injection through TitleSceneScope and TitleSceneEntryPoint.
```

Risk:

```text
CharacterSelectController is no longer direct-created by TitleScene; duplicate creation risk now moves to accidental reintroduction of a legacy factory path.
If TitleSceneScope is placed in the Unity scene before ViewManager ownership is ready, scoped views can be disposed by the wrong owner.
If LegacyAdventureSessionStarter is treated as final TitleSceneScope ownership, adventure session services remain in the wrong lifetime.
```

Acceptance:

```text
TitleSceneScope is documented as the Unity scene LifetimeScope target.
TitleSceneScope can find the GameBootstrap-created RootLifetimeScope as its parent.
TitleSceneEntryPoint is connected and owns startup ordering directly.
Controller ownership for TitleScene has moved to TitleSceneScope.
View disposal ownership has not moved to TitleSceneScope.
No controller type is owned by both DependencyManager and VContainer at runtime.
```

Cold assessment:

```text
Connecting the LifetimeScope component before removing DependencyManager ownership is not migration.
It is duplicate object creation with a more modern API.
```

## Phase 5D: Align Startup Contract Before EntryPoint Registration

Goal:

```text
Make the legacy TitleScene startup path and the future VContainer entry point call the same startup sequence.
```

Previous shape:

```text
TitleScene.OnLoaded
-> TitleSceneLegacyRuntimeFactory.Create()
-> TitleSceneLegacyRuntime.Start()
-> TitleScene startup object
```

Current shape:

```text
TitleSceneScope
-> TitleSceneEntryPoint.Start()
-> preload localization
-> load/play BGM
-> show TitleView
```

Done:

```text
TitleSceneStartup has been removed.
TitleSceneEntryPoint owns startup ordering directly.
TitleSceneScope registers TitleSceneEntryPoint.
TitleSceneLegacyRuntime and TitleSceneLegacyRuntimeFactory have been removed.
TitleScene.OnLoaded no longer starts TitleScene directly.
```

Scene connection checklist:

```text
1. TitleSceneScope is placed in the Unity TitleScene as @TitleSceneScope.
2. TitleSceneScope uses the GameBootstrap-created RootLifetimeScope as parent.
3. TitleSceneEntryPoint starts TitleScene exactly once.
4. ViewManager ownership is transient only for legacy Push/Pop paths; TitleScene uses SceneViewNavigator intentionally.
```

## Phase 5B: Move Scene Startup To EntryPoint

Goal:

```text
Replace TitleScene.OnLoaded responsibilities with a VContainer-owned TitleSceneEntryPoint.
```

Target:

```text
TitleSceneEntryPoint
-> TitleSceneLocalizationOwner.Preload
-> TitleSceneBgmOwner.LoadAndPlay
-> ITitleSceneNavigator.ShowTitle
```

Not target:

```text
TitleSceneEntryPoint should not own scene transition to the next scene.
Before-unload transition belongs to ISceneLoader / scene loading flow.
```

Acceptance:

```text
TitleScene startup responsibilities can be expressed through constructor-injected dependencies.
The old TitleScene.OnLoaded startup path is removed; TitleScene.OnLoaded is intentionally empty.
```

## Phase 5V: Verify Connected TitleSceneScope

Goal:

```text
Verify that TitleSceneScope is connected without reintroducing duplicate controller ownership.
```

Done:

```text
Added TitleSceneScopeCompositionVerifier under Assets/Editor.
Batchmode composition verification passes.
The verifier opens Assets/Scenes/TitleScene.unity.
It confirms exactly one @TitleSceneScope component exists.
It confirms DependencyManager registry does not contain TitleViewController or CharacterSelectController.
Added Game.PlayMode.Tests under Assets/Tests/PlayMode.
Filtered PlayMode test run passes for Game.PlayMode.Tests.
The PlayMode test loads TitleScene and verifies RootLifetimeScope, TitleSceneScope container, TitleSceneNavigator, TitleViewController, and CharacterSelectController resolution.
The PlayMode test verifies TitleViewController and CharacterSelectController resolve as scoped single instances.
```

Issue found during verification:

```text
Unity batchmode executeMethod was able to run structural verification but did not reliably enter Play Mode.
Unity Test Framework PlayMode execution is the reliable automated path for runtime verification.
SceneManagerEx threw when TestRunner's temporary scene did not map to a BaseScene type.
SceneManagerEx now ignores unknown scenes instead of calling Activator.CreateInstance(null).
```

Verification commands:

```text
Unity compile:
C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe -batchmode -quit -projectPath C:\Users\reg24\Favorites\Game

Composition verification:
C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe -batchmode -projectPath C:\Users\reg24\Favorites\Game -executeMethod TitleSceneScopeCompositionVerifier.Run

Filtered PlayMode verification:
C:\Program Files\Unity\Hub\Editor\6000.4.1f1\Editor\Unity.exe -batchmode -projectPath C:\Users\reg24\Favorites\Game -runTests -testPlatform PlayMode -assemblyNames Game.PlayMode.Tests
```

## Phase 5C: Shrink SceneManagerEx

Goal:

```text
Remove SceneManagerEx responsibility for BaseScene object creation and lifecycle mirroring.
```

Target direction:

```text
SceneManagerEx current:
-> new / Activator BaseScene
-> BeforeUnload
-> LoadScene
-> Loaded / Unloaded

Future:
-> either removed
-> or reduced to a temporary adapter behind ISceneLoader
```

Do not remove `SceneManagerEx` before there is a replacement for:

```text
1. transition before scene load
2. Unity SceneManager.LoadScene/LoadSceneAsync
3. current loading guard
4. legacy call sites using ISceneLoader
```

Current progress:

```text
GameSceneId and GameSceneNames exist.
ISceneLoader now exposes Load(GameSceneId).
LegacyAdventureSessionStarter uses Load(GameSceneId.Adventure).
UnitySceneLoader is now registered as the runtime SceneManagerEx-free implementation.
BaseSceneLifecycleRunner preserves the temporary BaseScene lifecycle bridge only for AdventureScene.
TitleScene is no longer created or mirrored by BaseSceneLifecycleRunner.
SceneManagerEx has been removed.
SceneManagerAdapter has been removed.
TitleScene.OnBeforeUnload no longer owns fade-out transition.
```

Reason:

```text
The loader now owns fade-out transition policy.
Root registration has been swapped because UnitySceneLoader now coordinates BaseSceneLifecycleRunner before loading.
```

Next:

```text
Verify remaining BaseScene lifecycle responsibilities.
Then shrink BaseSceneLifecycleRunner as scene startup/unload ownership moves into scene LifetimeScopes.
```

## Phase 6: Move TitleScene Controllers To VContainer

Goal:

```text
Move TitleViewController and CharacterSelectController from DependencyManager scene dependency to TitleSceneScope.
```

Steps:

```text
1. Convert controllers to constructor injection.
2. Register controllers in TitleSceneScope.
3. Ensure views receive the VContainer-owned controller instances.
4. Remove controller types from DependencyManager registration path.
5. Regenerate dependency registry if needed.
```

TitleViewController target:

```text
TitleViewController
-> depends on ITitleSceneNavigator
-> does not use ViewManager.Instance
-> does not create views directly
```

Current transitional status:

```text
TitleViewController does not directly create or push CharacterSelectView/SettingsView.
It depends on ITitleSceneNavigator through constructor injection.
It is TitleSceneScope-owned and no longer uses a temporary LegacyViewFactory/ViewManagerAdapter flow.
```

CharacterSelectController target for this phase:

```text
CharacterSelectController
-> remains TitleSceneScope-owned
-> character catalog lookup uses ICharacterSelectCatalog
-> scene loading uses ISceneLoader
-> character unlock lookup uses ICharacterUnlockGateway
-> adventure run-state service ownership remains deferred
```

Current character catalog bridge:

```text
CharacterSelectController
-> ICharacterSelectCatalog
-> LegacyCharacterSelectCatalog
-> DependencyManager / CharacterService / DBManager
```

This is intentionally not final character catalog ownership. It removes the direct `CharacterService` dependency from the TitleScene controller and removes `CharacterService` registration from `TitleSceneScope`.

Current unlock-state bridge:

```text
CharacterSelectController
-> ICharacterUnlockGateway
-> LegacyCharacterUnlockGateway
-> SaveManager / ProgressState
```

This is intentionally not final save ownership. It only removes the direct `SaveManager.Instance` dependency from the TitleScene controller.

Risk:

```text
Views currently use custom DependencyManager [Inject].
If views are still injected by LegacyViewFactory, they will not receive VContainer-owned controllers.
```

Acceptance:

```text
There is exactly one runtime instance of TitleViewController per TitleScene.
There is exactly one runtime instance of CharacterSelectController per TitleScene.
Views use those instances.
DependencyManager no longer creates those controller instances.
```

## Phase 7: Move TitleScene Views To SceneScope Ownership

Goal:

```text
Register TitleScene views as scoped objects owned by TitleSceneScope.
```

Target registrations:

```csharp
builder.Register<TitleView>(Lifetime.Scoped);
builder.Register<CharacterSelectView>(Lifetime.Scoped);
builder.Register<SettingsView>(Lifetime.Scoped);
```

Current registration status:

```text
Done for TitleScene:
- TitleView
- CharacterSelectView
- SettingsView

Runtime ownership has moved for TitleScene:
- TitleSceneScope creates the view instances.
- TitleScene views receive controllers through constructor injection.
- CharacterSelectView and SettingsView preserve BaseView cleanup by calling base.Dispose().
- TitleSceneNavigator resolves scoped views from the TitleSceneScope resolver at display time.
- TitleSceneNavigator shows TitleView, CharacterSelectView, and SettingsView through ISceneViewNavigator.
- ViewManager attaches/detaches these views but does not dispose them.
```

Target flow:

```text
TitleSceneScope owns views.
TitleSceneNavigator resolves scoped views at display time to avoid constructor cycles.
ViewManager attaches/detaches but does not dispose those views.
```

View injection direction:

```text
Controller/owner/flow use constructor injection.
Views should not rely on the old custom [Inject].
TitleScene-scoped views now use VContainer-created constructors.
```

Risk:

```text
If ViewManager still disposes views, SceneScope-owned views can be disposed twice.
If VContainer creates these views before their controllers are VContainer-owned, legacy custom [Inject] fields will not describe the actual runtime owner cleanly.
```

Acceptance:

```text
TitleScene views are owned by TitleSceneScope.
ViewManager does not dispose TitleScene-scoped views.
TitleSceneScope disposal disposes TitleScene views exactly once.
```

Current verification:

```text
Unity batchmode compile passes after scoped view handoff.
Filtered PlayMode verification passes through Game.PlayMode.Tests.
The first PlayMode run exposed a circular dependency:
TitleSceneViews -> TitleViewController -> TitleSceneChildNavigator -> TitleSceneViews.
The cycle was removed by making TitleSceneChildNavigator depend directly on scoped child views instead of the factory.
The current `TitleSceneViews` holder keeps the same rule: `TitleSceneChildNavigator` must not depend on `TitleSceneViews`, otherwise the cycle returns through `TitleView`.
```

## Phase 8: Remove Legacy TitleScene DependencyManager Ownership

Goal:

```text
Remove TitleScene-specific controller/view ownership from DependencyManager.
```

Candidates:

```text
TitleViewController
CharacterSelectController
TitleScene view injection path
LegacyAdventureSessionStarter registry path
```

Not included:

```text
AdventureService
CardDeckService
CardService
CardBoardService
PlayerService
CombatService
```

Those belong to the later AdventureSession migration.

Acceptance:

```text
TitleScene no longer depends on DependencyManager for TitleScene controller/view creation.
Existing Adventure run-state services may still be legacy-owned until AdventureSessionScope is designed.
```

Current status:

```text
Done for TitleScene controller/view/bridge creation.
TitleViewController is not in DependencyRegistry.
CharacterSelectController is not in DependencyRegistry.
LegacyAdventureSessionStarter is not in DependencyRegistry.
TitleSceneScope registers IAdventureSessionStarter directly.
LegacyAdventureSessionStarter still resolves adventure/session services from DependencyManager internally.
This is still a bridge, not final AdventureSession ownership.
```

## Phase 9: Start AdventureSessionScope Design

Goal:

```text
Move from TitleScene ownership migration to adventure run-state ownership design.
```

Current decision:

```text
AdventureSessionLifetimeScope remains disconnected.
AdventureService/CardDeckService/CardService/CardBoardService/PlayerService/CombatService remain DependencyManager-owned for now.
LegacyAdventureSessionStarter remains a TitleSceneScope-created bridge.
AdventureController and AdventureView still use the legacy DependencyManager/ViewManager path.
AdventureSceneLocalizationOwner owns AdventureScene localization preload/release, but is still created directly by AdventureScene.
```

Reason:

```text
AdventureController currently receives run-state services through `[Inject]`.
AdventureView currently receives AdventureController through `[Inject]`.
AdventureScene currently creates AdventureView through LegacyViewFactory and pushes it through ViewManager.
Registering run-state services in VContainer before moving these consumers would create duplicate run state.
```

Next safe step:

```text
AdventureSceneScope skeleton exists.
AdventureSceneLocalizationOwner exists.
Next: design AdventureScene startup entry point for view creation and AdventureController.StartAdventure.
Do not register AdventureController or run-state services until their DependencyManager ownership is removed in the same migration step.
```

Detailed AdventureSession design is tracked in:

```text
docs/adventure-session-vcontainer-design.md
```

## Phase Order Warning

Do not swap the order casually.

Known bad shortcuts:

```text
Register controllers in VContainer before views use VContainer-owned controllers.
-> duplicate controller instances.

Register views as Scoped before ViewManager stops disposing them.
-> double Dispose or stale stack references.

Replace RootLifetimeScope manager ownership before adapter boundaries are stable.
-> boot order failures.

Introduce cached views before Hide/Detach semantics exist.
-> hidden views may still receive input or remain in layout.
```

## Related Documents

- `docs/title-scene-vcontainer-lifecycle-design.md`
- `docs/view-manager-redesign-plan.md`
- `docs/vcontainer-registration-map.md`
- `docs/vcontainer-memory-lifetime-checklist.md`
