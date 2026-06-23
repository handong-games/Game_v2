# VContainer Adoption Document Index

## Purpose

This is the reading guide and current source-of-truth map for the VContainer adoption documents.

The migration has both design history and active implementation planning. Read the active documents first.

## Current Status

```text
VContainer package is installed.
Root/scene/session scope foundations exist.
Root adapter and TitleScene support skeletons exist.
Unity Scene + VContainer LifetimeScope has been adopted as the target scene architecture.
TitleScene contains a scene-placed @TitleSceneScope component.
TitleScene startup is now owned by TitleSceneScope / TitleSceneEntryPoint.
TitleScene resource owners are created through TitleSceneScope during TitleScene startup.
SettingsView close flow no longer calls ViewManager.Pop directly; close behavior is provided by its creation path.
CharacterSelectController scene loading now goes through ISceneLoader.
CharacterSelectController adventure-start responsibility now goes through LegacyAdventureSessionStarter.
CharacterSelectController character catalog lookup now goes through ICharacterSelectCatalog / LegacyCharacterSelectCatalog.
CharacterSelectController unlock-state lookup now goes through ICharacterUnlockGateway / LegacyCharacterUnlockGateway.
CharacterSelectController is registered in TitleSceneScope through its public constructor.
CharacterSelectView no longer receives CharacterSelectController through custom DependencyManager field injection.
CharacterSelectView and SettingsView dispose through BaseView by calling base.Dispose().
LegacyAdventureSessionStarter has a public constructor-injection path and is no longer registered in DependencyManager.
TitleScene legacy startup runtime/factory has been removed.
TitleSceneEntryPoint now owns TitleScene startup ordering directly.
ViewManager has a non-disposing Attach/Detach host path.
TitleScene runtime uses SceneViewNavigator for TitleScene-scoped views.
TitleScene view types are registered in TitleSceneScope and are runtime-owned by the connected scene scope.
Runtime view disposal for TitleScene no longer follows the legacy ViewManager Push/Pop path.
SettingsViewController no longer owns root-layer visual class expression; SettingsView applies it through IViewHost.
TitleView, CharacterSelectView, and SettingsView keep view-local animation behavior in their own view partials and USS.
TitleSceneEntryPoint is registered in TitleSceneScope and starts TitleScene directly.
TitleSceneViewFlow has been removed.
ITitleSceneNavigator / TitleSceneNavigator is the single TitleScene navigation boundary.
TitleScene root TitleView creation no longer depends on DependencyManager.Inject for TitleViewController.
TitleViewController delegates CharacterSelect/Settings navigation to ITitleSceneNavigator.
TitleViewController is no longer registered in DependencyManager.
TitleViewController and CharacterSelectController are registered in TitleSceneScope.
TitleScene.OnLoaded no longer starts the title flow directly.
TitleScene scene fade-out now goes through ISceneTransitionPlayer resolved from RootLifetimeScope.
GameBootstrap creates RootLifetimeScope after ManagerRegistry initialization.
GameBootstrap exposes RootLifetimeScope for scene-scope parent lookup.
GameRootEntryPoint is registered in RootLifetimeScope and initializes VContainer-owned root objects.
ViewManager is VContainer-created as a root singleton and initialized by GameRootEntryPoint.
TitleSceneScope composition verification passes in Unity batchmode.
TitleSceneScope filtered PlayMode verification passes through Game.PlayMode.Tests.
ViewManager legacy stack ownership is now exposed through explicit PushAndOwn / PopAndDispose / ClearOwnedViewsAndDetachAll methods.
ViewManager still owns transient view lifetime for unmigrated scenes.
LegacyAdventureSessionStarter is registered directly in TitleSceneScope and creates AdventureSessionLifetimeScope before scene load.
TitleSceneScope no longer resolves AdventureService/CardDeckService/PlayerService/CardService/CardBoardService directly.
TitleSceneScope, TitleScene views, and TitleScene controllers no longer directly call legacy managers; remaining access is isolated in Legacy* bridge types.
ISceneLoader now has a GameSceneId-based loading method.
LegacyAdventureSessionStarter now requests AdventureScene through GameSceneId.Adventure instead of generic BaseScene loading.
RootLifetimeScope now registers UnitySceneLoader as the SceneManagerEx-free loader.
SceneManagerEx and SceneManagerAdapter have been removed from code.
Scene fade-out transition is now owned by ISceneLoader implementations instead of TitleScene.OnBeforeUnload.
AdventureSceneScope is runtime-created by the remaining AdventureScene BaseScene bridge.
AdventureSceneScope owns AdventureSceneStartup, AdventureSceneLocalizationOwner, AdventureController, and AdventureView.
AdventureSceneStartup attaches AdventureView through IViewHost, not legacy ViewManager Push ownership.
AdventureSessionRuntime stores the active AdventureSessionLifetimeScope for AdventureSceneScope parent lookup.
LegacyAdventureSessionStarter creates AdventureSessionLifetimeScope at adventure start.
AdventureSessionLifetimeScope has RootLifetimeScope parent wiring and registers AdventureSessionInitializer, VContainer-owned Adventure/CardDeck/CardBoard/Card/Player run-state services, and remaining DependencyManager-created aliases.
AdventureSessionInitializer performs run-state initialization through VContainer resolution.
AdventureSessionState/AdventureSessionFactory/AdventureService, CardDeckState/CardDeckBuilder/CardDeckService, CardBoardState/CardBoardService, CardRegistry/CardFactory/CardService, and PlayerRunState/PlayerService are now AdventureSessionLifetimeScope-owned.
CombatService and CharacterService remain legacy aliases.
AdventureController duplicate StartAdventure event registration is guarded.
AdventureView defensive Dispose unregistration is in place for Adventure static/widget/cue events.
AdventureView receives AdventureController through constructor injection from AdventureSceneScope, not custom DependencyManager field injection.
AdventureController receives run-state services through VContainer constructor injection and is no longer registered in DependencyManager.
AdventureScene dependency map is documented; AdventureScene is the next blocker before broad manager cleanup.
```

## Active Reading Order

1. `docs/vcontainer-current-status.md`

   Use this as the short current checkpoint. It summarizes the final goal, completed work, incomplete areas, priority, and next recommended step.

2. `docs/title-scene-vcontainer-implementation-phases.md`

   Use this as the execution sequence. It explains what has been done, what should happen next, and which order must not be skipped.

3. `docs/title-scene-vcontainer-lifecycle-design.md`

   Use this as the TitleScene ownership design. It explains the current TitleScene boundary, resource owners, `ITitleSceneNavigator`, and which responsibilities are still outside full VContainer ownership.

4. `docs/scene-lifetimescope-adoption-decision.md`

   Use this as the scene architecture decision. It records that Unity Scene + VContainer `LifetimeScope` is the adopted target direction.

5. `docs/title-scene-controller-ownership-migration.md`

   Use this as the controller ownership transfer record and guardrail against reintroducing duplicate controller creation.

6. `docs/view-manager-redesign-plan.md`

   Use this before moving views into SceneScope ownership. It explains why `ViewManager.Pop/Clear` currently conflict with scoped view ownership.

7. `docs/adventure-session-vcontainer-design.md`

   Use this before moving adventure run-state services. It explains why those services belong to `AdventureSessionLifetimeScope`, not `TitleSceneScope`.

8. `docs/adventure-run-state-service-redesign.md`

   Use this before moving additional Adventure run-state services. It defines the State/Service/Factory split and the safe migration order after `CardBoardService`.

9. `docs/vcontainer-registration-map.md`

   Use this as the registration ownership map. It tracks which types are root, scene, or session candidates and which ones must not be duplicated.

10. `docs/combat-service-lifetime-inspection.md`

   Use this before touching CombatService ownership. It records the GameplayMessageManager subscription and static AdventureEvents risks.

11. `docs/vcontainer-memory-lifetime-checklist.md`

   Use this as a preflight checklist before moving a type from `DependencyManager` ownership to VContainer ownership.

12. `docs/unity-lifecycle-vcontainer-mapping.md`

   Use this as lifecycle learning material. It explains Unity lifecycle and VContainer scope mapping.

13. `docs/vcontainer-migration-plan.md`

   Historical baseline. This was the first broad migration plan and is useful for context, but the active TitleScene sequencing now lives in the documents above.

## Document Roles

| Document | Role | Current authority |
| --- | --- | --- |
| `vcontainer-current-status.md` | Short current checkpoint | Highest for current state and next priority |
| `title-scene-vcontainer-implementation-phases.md` | Step-by-step migration order for TitleScene | Highest for implementation order |
| `title-scene-vcontainer-lifecycle-design.md` | TitleScene ownership and lifecycle design | Highest for TitleScene scope decisions |
| `scene-lifetimescope-adoption-decision.md` | Adopted scene architecture direction | Highest for SceneManagerEx/BaseScene replacement direction |
| `title-scene-controller-ownership-migration.md` | TitleScene controller ownership transfer | Highest for controller ownership history and duplicate-creation guardrails |
| `view-manager-redesign-plan.md` | View ownership and ViewManager redesign plan | Highest for view ownership decisions |
| `adventure-session-vcontainer-design.md` | Adventure run-state scope design | Highest for AdventureSessionScope decisions |
| `adventure-run-state-service-redesign.md` | Adventure run-state State/Service/Factory redesign | Highest for Adventure service refactoring order |
| `combat-service-lifetime-inspection.md` | CombatService lifetime and subscription inspection | Highest for CombatService move guardrails |
| `vcontainer-registration-map.md` | Scope ownership candidates and duplicate-creation guard | Highest for registration ownership |
| `vcontainer-memory-lifetime-checklist.md` | Safety checklist | Supporting guardrail |
| `unity-lifecycle-vcontainer-mapping.md` | Learning/reference document | Supporting reference |
| `vcontainer-migration-plan.md` | Initial broad migration plan | Historical/background |

## Current Next Step

The current completed phase is:

```text
Phase 7 completed for TitleScene
-> TitleSceneScope is placed in the Unity TitleScene as @TitleSceneScope.
-> TitleSceneEntryPoint is registered and starts TitleScene directly.
-> Move TitleScene view controller dependencies to constructor injection. Done.
-> ITitleSceneNavigator / TitleSceneNavigator is TitleSceneScope-owned.
-> Batchmode composition verification passes.
-> Filtered PlayMode runtime verification passes through Game.PlayMode.Tests.
-> Legacy ViewManager ownership API names are explicit.
-> TitleScene navigation uses SceneViewNavigator through TitleSceneNavigator.
-> TitleScene views have moved to TitleSceneScope ownership.
-> Preserve existing Push/Pop/Clear transient behavior for unmigrated flows.
-> Do not apply SceneViewNavigator to unmigrated transient views.
```

The next planned phase is:

```text
Phase 8
-> Remove the remaining TitleScene legacy DependencyManager registry path. Done for LegacyAdventureSessionStarter.
-> LegacyAdventureSessionStarter stays as a temporary bridge that creates AdventureSessionLifetimeScope and loads AdventureScene.
-> Do not move AdventureService/CardDeckService/CardService/CardBoardService/PlayerService into TitleSceneScope.
```

The next safe implementation phase is:

```text
Phase 9
-> Create AdventureSessionLifetimeScope at adventure start. Done.
-> Extract AdventureSessionInitializer. Done.
-> Add AdventureSceneScope skeleton. Done.
-> Extract AdventureScene localization owner. Done.
-> Map AdventureScene dependencies. Done.
-> Add AdventureSceneStartup entry point. Done.
-> Attach AdventureView through IViewHost. Done.
-> Continue Adventure/BaseScene cleanup only after the next narrow boundary is selected.
```

SceneManagerEx removal track:

```text
-> GameSceneId and GameSceneNames exist.
-> ISceneLoader.Load(GameSceneId) exists.
-> RootLifetimeScope registers UnitySceneLoader as ISceneLoader.
-> UnitySceneLoader owns FadeOut and coordinates BaseSceneLifecycleRunner only for remaining legacy BaseScene scenes before SceneManager.LoadScene.
-> TitleScene.OnBeforeUnload no longer plays FadeOut directly.
-> SceneManagerEx is no longer a BaseManager and is no longer in ManagerRegistry.
-> SceneManagerEx has been removed from code.
-> SceneManagerAdapter has been removed from code.
-> TitleScene is no longer created or mirrored by BaseSceneLifecycleRunner.
```

Do not start these before controller scene-loading and ViewManager ownership decisions are explicit:

```text
Change ViewManager.Pop/Clear semantics.
Promote LegacyAdventureSessionStarter's adventure/session services into TitleSceneScope ownership.
```

## Hard Rules

```text
1. A runtime type is owned by either DependencyManager or VContainer, never both.
2. RootLifetimeScope does not create existing managers in the first migration phase.
3. ViewManager remains transient view owner only for legacy Push/Pop paths.
4. SceneScope-owned views must not be disposed by ViewManager.
5. Adapters are legacy bridges, not final architecture.
6. VContainer registration code is not runtime ownership unless the type is actually resolved by the connected scope.
7. ResolveRoot is allowed only at composition boundaries during migration.
8. Unity Scene + LifetimeScope is the adopted target; SceneManagerEx is out of the active loading path.
9. Scene-loading call sites use GameSceneId, not generic BaseScene loading.
```
