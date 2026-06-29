# AdventureSession VContainer Design

## Purpose

This document defines the next migration boundary after TitleScene.

TitleScene migration moved menu controllers and views into `TitleSceneScope`.
The next risk is adventure run state:

```text
AdventureService
CardDeckService
CardService
CardBoardService
PlayerService
CombatService
```

These services are currently global `DependencyManager` dependencies, but their data is not application-global. They represent one adventure run.

Cold assessment:

```text
Moving these services into TitleSceneScope would be wrong.
TitleScene starts a run, but it does not own the run.
If TitleScene owns run state, either the run dies too early or TitleScene keeps references after it should be unloaded.
```

## Current Runtime Flow

Current start path:

```text
CharacterSelectController
-> IAdventureSessionStarter.Start(selectedCharacter)
-> LegacyAdventureSessionStarter
-> AdventureSessionLifetimeScope resolves AdventureService from VContainer
-> AdventureSessionLifetimeScope resolves CardDeckService from VContainer
-> AdventureSessionLifetimeScope resolves PlayerService from VContainer
-> AdventureSessionLifetimeScope resolves CardService from VContainer
-> AdventureSessionLifetimeScope resolves CardBoardService from VContainer
-> DependencyManager.Resolve<CharacterService>()
-> ISceneLoader.Load(GameSceneId.Adventure)
```

Current AdventureScene path:

```text
AdventureScene.OnLoaded
-> create @AdventureSceneScope
-> AdventureSceneScope builds with AdventureSessionRuntime.CurrentScope if available
-> otherwise AdventureSceneScope falls back to RootLifetimeScope
-> AdventureSceneEntryPoint.Start()
-> AdventureSceneLocalization.Preload()
-> AdventureSceneScope receives legacy run-state service aliases from parent session scope
-> AdventureScreenController(run-state services)
-> AdventureView(adventureScreenController)
-> AdventureGameToScreenEventBinder.Bind()
-> AdventureWidgetToScreenEventBinder.Bind()
-> IViewHost.Attach(adventureView)
-> AdventureScreenController.StartAdventure()

AdventureScene.OnUnloaded
-> AdventureSceneScope disposed
-> AdventureSceneEntryPoint.Dispose()
-> AdventureWidgetToScreenEventBinder.Dispose()
-> AdventureGameToScreenEventBinder.Dispose()
-> IViewHost.Detach(adventureView)
-> scoped AdventureView / AdventureScreenController / AdventureSceneLocalization disposed
```

Current BaseScene bridge:

```text
Unity loads AdventureScene
-> BaseSceneLifecycleRunner creates pure C# AdventureScene
-> AdventureScene.OnLoaded creates AdventureSceneScope
-> AdventureSceneEntryPoint starts the VContainer Adventure view/controller path
-> AdventureScene.OnBeforeUnload currently waits one frame
-> AdventureScene.OnUnloaded disposes AdventureSceneScope
-> AdventureSceneEntryPoint releases AdventureScene view event bindings
```

TitleScene no longer uses this pure C# BaseScene path. AdventureScene is now the remaining reason `BaseSceneLifecycleRunner` exists.

Current AdventureController path:

```text
AdventureController
-> constructor PlayerService
-> constructor AdventureService
-> constructor CardDeckService
-> constructor CardService
-> constructor CardBoardService
-> constructor CombatService
```

This means the current runtime still consumes `DependencyManager` run-state services, but `AdventureController` itself is no longer created by `DependencyManager`.

Current code scan checkpoint:

```text
AdventureScene creates and disposes AdventureSceneScope.
AdventureSceneScope owns AdventureSceneEntryPoint, AdventureSceneLocalization, AdventureScreenController, AdventureGameToScreenEventBinder, AdventureWidgetToScreenEventBinder, and AdventureView.
AdventureView receives AdventureScreenController through constructor injection.
AdventureScreenController receives scene flow dependencies through constructor injection.
AdventureEncounterStartFlow owns choice commit -> concrete encounter start transition.
AdventureGameEvents and AdventureWidgetEvents are scoped event holders.
CombatService owns a GameplayMessageManager death subscription and releases it in Dispose.
```

Stabilized legacy risks:

```text
AdventureGameToScreenEventBinder centralizes game-flow-to-screen subscriptions.
AdventureWidgetToScreenEventBinder centralizes widget-input-to-screen subscriptions.
AdventureView.Dispose unregisters cue handlers defensively and tolerates unbound widget fields.
```

Stabilized legacy controller/view pairing:

```text
AdventureSceneScope resolves AdventureController once and injects that same scoped instance into AdventureView.
AdventureView no longer receives AdventureController through custom DependencyManager field injection.
This is still partly legacy-backed because AdventureSessionLifetimeScope owns CardBoardService but registers the remaining DependencyManager-created run-state instances as VContainer aliases.
```

## AdventureScene Dependency Map

Current creation chain:

```text
Unity loads AdventureScene
-> BaseSceneLifecycleRunner creates pure C# AdventureScene
-> AdventureScene.OnLoaded()
-> creates @AdventureSceneScope
-> AdventureSceneScope builds
-> AdventureSceneEntryPoint.Start()
-> AdventureSceneLocalization.Preload()
-> AdventureSceneScope resolves run-state service aliases from AdventureSessionLifetimeScope
-> AdventureScreenController(run-state services)
-> AdventureView(adventureScreenController)
-> AdventureGameToScreenEventBinder.Bind()
-> AdventureWidgetToScreenEventBinder.Bind()
-> IViewHost.Attach(adventureView)
-> AdventureScreenController.StartAdventure()
```

Cold assessment:

```text
AdventureScene previously had two hidden creation paths for AdventureController:
1. AdventureView received AdventureController through DependencyManager.Inject.
2. AdventureSceneStartup explicitly resolved AdventureController.

Current status:
AdventureSceneScope now resolves AdventureController once and injects it into AdventureView.
This removes the split controller reference risk before VContainer ownership transfer.
AdventureController ownership has moved out of DependencyManager and into AdventureSceneScope.
AdventureSession, CardDeck, CardBoard, Card, and Player run-state ownership has moved to AdventureSessionLifetimeScope.
```

Current event/subscription chain:

```text
AdventureView.OnVisualTreeCloned()
-> caches UXML elements
-> binds widgets to AdventureWidgetEvents

AdventureGameToScreenEventBinder.Bind()
-> runs before AdventureSceneNavigator.ShowAdventure()
-> connects AdventureGameEvents.Screen.InitialPresentationPrepared to AdventureView.OnGameInitialPresentationPrepared
-> connects AdventureGameEvents.Screen.PlayerTurnStarted to AdventureView.OnGamePlayerTurnStarted
-> connects AdventureGameEvents.Screen.EnemyTurnStarted to AdventureView.OnGameEnemyTurnStarted
-> connects AdventureGameEvents.Screen.RewardStarted to AdventureView.OnGameRewardStarted
-> connects AdventureGameEvents.Screen.ChoiceRefreshStarted to AdventureView.OnGameChoiceRefreshStarted
-> connects AdventureGameEvents.Board.RefreshRequested to AdventureView.OnGameBoardRefreshRequested
-> connects AdventureGameEvents.Combat.ResultRequested to AdventureView.OnGameCombatEnded

AdventureWidgetToScreenEventBinder.Bind()
-> subscribes AdventureWidgetEvents.Turn.EndTurnClicked to AdventureView.OnWidgetEndTurnClicked
-> subscribes AdventureWidgetEvents.Pouch.Clicked to AdventureView.OnWidgetPouchClicked
-> subscribes AdventureWidgetEvents.Card.Clicked to AdventureView.OnWidgetCardClicked
-> subscribes AdventureWidgetEvents.SkillSlot.SelectionChanged to AdventureView.OnWidgetSkillSlotSelectionChanged

AdventureCombatResultFlow.NotifyResult()
-> publishes AdventureGameEvents.Combat.ResultRequested
```

Risk:

```text
AdventureGameEvents and AdventureWidgetEvents are scoped event holders in AdventureSceneScope.
AdventureGameToScreenEventBinder and AdventureWidgetToScreenEventBinder centralize View event subscription and unsubscription by direction.
If these binders are not disposed with AdventureSceneScope, stale subscribers can survive into the next scene flow.
```

Current stateful services:

| Type | State held | Current owner | Correct target |
| --- | --- | --- | --- |
| `AdventureService` | `CurrentAdventure`, stage progress, selected character, seed | AdventureSessionLifetimeScope | AdventureSessionLifetimeScope |
| `CardDeckService` | deck order, random state, draw index, remaining monster/event pools | AdventureSessionLifetimeScope | AdventureSessionLifetimeScope |
| `CardService` | created cards, next card id, ability systems on cards | AdventureSessionLifetimeScope | AdventureSessionLifetimeScope |
| `CardBoardService` | card ids by board zone | DependencyManager global | AdventureSessionLifetimeScope |
| `PlayerService` | current player state, current player card | AdventureSessionLifetimeScope | AdventureSessionLifetimeScope |
| `CombatService` | combat cards, current side, round, death subscription | DependencyManager global | AdventureSessionLifetimeScope first, combat sub-scope later only if needed |
| `AdventureController` | event subscriptions and run orchestration | DependencyManager scene | AdventureSceneScope |
| `AdventureView` | UI element bindings and event subscriptions | ViewManager legacy stack | AdventureSceneScope |
| `AdventureSceneLocalizationOwner` | localization preload/release | direct-created by AdventureScene | AdventureSceneScope |

Important distinction:

```text
AdventureSceneScope owns scene presentation.
AdventureSessionLifetimeScope owns run state.

AdventureController belongs to AdventureSceneScope because it coordinates the scene's view flow.
AdventureService/CardDeckService/CardService/CardBoardService/PlayerService/CombatService belong to AdventureSessionLifetimeScope because they represent the current run.
```

## Target Ownership

Target scope hierarchy:

```text
RootLifetimeScope
-> AdventureSessionLifetimeScope
   -> AdventureSceneScope
   -> future CombatSceneScope
```

Current bridge support:

```text
AdventureSessionRuntime stores the currently active AdventureSessionLifetimeScope.
AdventureSessionLifetimeScope already uses RootLifetimeScope as parent.
AdventureSceneScope can use AdventureSessionRuntime.CurrentScope as parent when a session scope exists.
CardBoardService is VContainer-owned in AdventureSessionLifetimeScope.
AdventureSessionLifetimeScope registers AdventureSessionInitializer and remaining DependencyManager-created run-state aliases.
LegacyAdventureSessionStarter creates AdventureSessionLifetimeScope, resolves AdventureSessionInitializer, and starts legacy run-state initialization.
```

Target responsibility:

```text
AdventureSessionLifetimeScope
-> owns one run's stateful services
-> starts when a new adventure starts
-> survives scene changes inside that run
-> disposes when the run ends or returns to title
```

Scene scopes should not own run state:

```text
AdventureSceneScope
-> owns AdventureView, AdventureController, scene-local owners
-> consumes session services from AdventureSessionLifetimeScope

CombatSceneScope
-> owns combat scene view/controller/presenters
-> consumes session services from AdventureSessionLifetimeScope
```

## Service Classification

For detailed service redesign, read `docs/adventure-run-state-service-redesign.md` before moving additional services.

| Type | Current owner | Target owner | Reason |
| --- | --- | --- | --- |
| `AdventureService` | `AdventureSessionLifetimeScope` | `AdventureSessionLifetimeScope` | Holds `CurrentAdventure`, stage number, selected character, seed through AdventureSessionState. |
| `CardDeckService` | `AdventureSessionLifetimeScope` | `AdventureSessionLifetimeScope` | Holds deck order, random state, draw index, remaining pools through CardDeckState. |
| `CardService` | `AdventureSessionLifetimeScope` | `AdventureSessionLifetimeScope` | Holds created card instances and next card id through CardRegistry. |
| `CardBoardService` | `AdventureSessionLifetimeScope` | `AdventureSessionLifetimeScope` | Holds card placement by board zone. First moved run-state service. |
| `PlayerService` | `AdventureSessionLifetimeScope` | `AdventureSessionLifetimeScope` | Holds current player state and player card through PlayerRunState. |
| `CombatService` | `DependencyManager` global | `AdventureSessionLifetimeScope` or later combat sub-scope decision | Holds combat cards, current side, round number, death subscription. |
| `CharacterService` | `DependencyManager` global | Root or legacy root bridge | Static data lookup. It is not run state. |

`CombatService` is the most dangerous candidate.

Reason:

```text
CombatService subscribes to GameplayMessageManager.
If session scope disposal timing is wrong, combat death messages can leak into a dead run.
If combat can restart multiple times in one adventure, CombatService may need per-combat reset discipline even if its owner is AdventureSessionLifetimeScope.
```

## Non-Negotiable Guardrails

```text
1. Do not register CombatService as a VContainer-created service while DependencyManager still owns it.
2. Do not make TitleSceneScope the parent or owner of run-state services.
3. Do not remove the DependencyManager registrations for run-state services until AdventureController has a VContainer-owned path.
4. Do not connect AdventureSessionLifetimeScope to runtime before there is exactly one consumer path for session services.
5. Do not treat CharacterService as session state.
```

## Proposed Migration Order

### Phase A: Session Boundary Documented

Done by this document.

Acceptance:

```text
The session-owned service list is explicit.
The target parent/child scope hierarchy is explicit.
The current blockers are explicit.
```

### Phase B: Introduce Session Starter Boundary

Replace the legacy bridge conceptually:

```text
IAdventureSessionStarter
-> VContainerAdventureSessionStarter
-> AdventureSessionLifetimeScopeFactory
-> creates AdventureSessionLifetimeScope
-> initializes run state
-> loads AdventureScene
```

Do not connect this until `AdventureScene` can consume the session scope.

### Phase C: Add AdventureSceneScope

Target:

```text
AdventureSceneScope
-> parent = RootLifetimeScope for the current bridge phase
-> registers AdventureController
-> registers AdventureView
-> registers AdventureSceneStartup
-> registers AdventureSceneLocalizationOwner
```

Risk:

```text
AdventureSceneScope consumes run-state services through VContainer constructor resolution.
The remaining legacy service instances are still DependencyManager-created aliases.
It must not register CombatService yet.
The final parent should become AdventureSessionLifetimeScope after session ownership exists.
```

Current status:

```text
AdventureSceneScope exists under Assets/@Scripts/Core/Composition.
AdventureScene creates @AdventureSceneScope from the remaining BaseScene bridge.
AdventureSceneScope registers scene-local objects and entry point.
AdventureSceneScope does not create run-state services.
If no AdventureSessionLifetimeScope exists, AdventureSceneScope registers legacy aliases only as an editor/direct-scene fallback.
```

### Phase C2: AdventureScene Dependency Map

Current status:

```text
Done by this document.
AdventureScene creation, view/controller injection, run-state services, and event subscriptions are mapped.
```

Acceptance:

```text
There is a clear split between AdventureSceneScope and AdventureSessionLifetimeScope candidates.
Adventure scene-local ownership has moved to AdventureSceneScope.
CardBoardService has moved to AdventureSessionLifetimeScope.
CombatService remains the only run-state service still exposed as a legacy alias.
```

### Phase C3: Move AdventureView To Constructor Injection Without Changing Owner

Target:

```csharp
public AdventureView(AdventureController controller)
```

But do not connect it immediately through `LegacyViewFactory`, because `LegacyViewFactory.Create<T>()` currently requires `new()`.

Earlier intermediate:

```text
Add a dedicated AdventureScene view factory/bridge that resolves the current DependencyManager-owned AdventureController
and creates AdventureView explicitly.
```

Current implementation:

```text
AdventureSceneScope directly registers AdventureController and AdventureView.
AdventureController receives CardBoardService from VContainer and the remaining DependencyManager-created run-state aliases through constructor injection.
```

Risk:

```text
Changing AdventureView to constructor injection before replacing LegacyViewFactory breaks Activator.CreateInstance<T>().
Changing AdventureController owner first while AdventureView still uses DependencyManager.Inject creates split ownership.
```

Practical adjustment:

```text
Do not start Phase C3 by editing AdventureView's constructor.
First create a single legacy startup path that owns "create view, push view, resolve controller, start controller".
Only after that path is isolated should AdventureView constructor injection be attempted.
```

### Phase C4: Move AdventureScene Startup Behind A Scene Startup Class

Target:

```text
AdventureSceneStartup
-> AdventureSceneLocalizationOwner.Preload()
-> create/show AdventureView
-> AdventureController.StartAdventure()
```

Reason:

```text
This mirrors the successful TitleScene migration.
It extracts BaseScene.OnLoaded responsibility before replacing the owner.
```

Do not place `AdventureSceneScope` into the Unity scene until the startup class has exactly one creation path for `AdventureView` and `AdventureController`.

First implementation target:

```text
AdventureSceneStartup can still be legacy-backed.
It should not register AdventureController or run-state services in VContainer yet.
Its first value is reducing AdventureScene.OnLoaded to one delegated startup call.
Current implementation is legacy-backed and does not change ownership.
```

### Phase C1: Extract AdventureScene Resource Owners

Goal:

```text
Move scene resource preload/release out of AdventureScene itself before moving startup into VContainer.
```

Current status:

```text
AdventureSceneLocalizationOwner exists.
It owns AdventureScene localization table preload/release.
AdventureSceneScope creates and disposes the owner.
AdventureSceneStartup receives the owner through VContainer.
```

Reason:

```text
This reduces BaseScene.OnLoaded/OnUnloaded responsibility without moving controller/view ownership prematurely.
```

### Phase D: Move AdventureController To Constructor Injection

Target constructor:

```csharp
public AdventureController(
    PlayerService playerService,
    AdventureService adventureService,
    CardDeckService cardDeckService,
    CardService cardService,
    CardBoardService cardBoardService,
    CombatService combatService)
```

Acceptance:

```text
AdventureController is not in DependencyRegistry.
AdventureController has a constructor-injection shape.
AdventureView receives the same AdventureController instance through constructor injection.
```

Current status:

```text
Done for constructor-injection shape.
Done for DependencyRegistry removal.
Done for AdventureSceneScope ownership.
Not done for session-scoped services.
AdventureSceneScope no longer constructs AdventureController through a DependencyManager-backed factory.
AdventureController is registered directly and receives run-state services through VContainer constructor resolution.
```

### Phase E: Move Run-State Services To AdventureSessionLifetimeScope

Only after Phase D:

```csharp
builder.Register<AdventureService>(Lifetime.Scoped);
builder.Register<CardDeckService>(Lifetime.Scoped);
builder.Register<CardService>(Lifetime.Scoped);
builder.Register<CardBoardService>(Lifetime.Scoped);
builder.Register<PlayerService>(Lifetime.Scoped);
builder.Register<CombatService>(Lifetime.Scoped);
```

Acceptance:

```text
DependencyRegistry no longer contains the moved run-state services.
AdventureSessionLifetimeScope resolves one instance per run.
AdventureSceneScope receives the same session instances.
Returning to Title disposes the session and clears run state.
```

### Phase F: Replace LegacyAdventureSessionStarter

Remove:

```text
LegacyAdventureSessionStarter resolving run-state services from DependencyManager
```

Replace with:

```text
VContainerAdventureSessionStarter
-> creates/configures AdventureSessionLifetimeScope
-> initializes the run through session-scoped services
-> loads AdventureScene
```

## Current Decision

For now:

```text
AdventureSessionLifetimeScope has parent wiring and runtime holder support.
AdventureSessionRuntime can store the current AdventureSessionLifetimeScope.
CardBoardService is VContainer-owned there.
AdventureSessionLifetimeScope registers AdventureSessionInitializer and remaining DependencyManager-created run-state aliases.
LegacyAdventureSessionStarter creates and stores AdventureSessionLifetimeScope at adventure start.
LegacyAdventureSessionStarter remains a scope creation and scene-load bridge.
AdventureSessionInitializer performs the current run-state initialization sequence.
The initializer is VContainer-owned. It receives CardBoardService from VContainer and the remaining legacy run-state services through alias registrations backed by DependencyManager instances.
TitleSceneScope registers LegacyAdventureSessionStarter directly and no longer resolves the adventure service list itself.
```

Reason:

```text
AdventureView and AdventureController no longer use DependencyManager field injection.
CardBoardService is now a VContainer-created object.
The remaining run-state services are still DependencyManager-created objects.
Registering those remaining services in VContainer before removing their DependencyManager ownership would create duplicate state.
```

## Next Concrete Step

The current safe implementation step is done:

```text
Extract AdventureScene startup responsibilities behind an AdventureSceneStartup-style boundary without changing ownership yet.
```

Acceptance for that step:

```text
AdventureScene.OnLoaded creates @AdventureSceneScope.
AdventureScene.OnUnloaded disposes @AdventureSceneScope.
AdventureView is AdventureSceneScope-owned.
AdventureController is AdventureSceneScope-owned.
CombatService and CharacterService remain DependencyManager-owned, but are visible to VContainer as aliases.
There is still exactly one AdventureController consumer path.
```

Do not register run-state services as VContainer-created objects unless the matching legacy DependencyManager ownership is removed in the same step.

Current safe implementation step:

```text
Harden AdventureSceneStartup / AdventureController / AdventureView disposal and duplicate-start behavior before moving ownership.
```

Status:

```text
Done for AdventureController duplicate StartAdventure event registration.
Done for AdventureView defensive event/cue unregistration.
Done for AdventureView constructor injection from the legacy startup path.
Done for AdventureSceneScope ownership transfer of scene-local objects.
```

Next safe implementation step:

```text
Replace LegacyAdventureSessionStarter's DependencyManager run-state initialization with session-scoped services only after AdventureSessionLifetimeScope registrations are ready.
```
