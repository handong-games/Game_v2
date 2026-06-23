# Adventure Run-State Service Redesign

## Purpose

This document defines how Adventure run-state services should be redesigned before more services are moved from `DependencyManager` to VContainer ownership.

The goal is not only to change who creates the objects. The goal is to make the runtime state boundary explicit.

Cold assessment:

```text
Moving the current services into VContainer as-is would mostly move the problem.
The container would improve lifetime ownership, but the services would still mix mutable state, DB lookup, object creation, and game progression.
That makes duplicate state bugs likely during the DependencyManager removal.
```

## Current Problem

Current Adventure services are named as services, but most of them are state holders.

| Current type | Mutable state currently held | Other responsibility mixed in | Risk |
| --- | --- | --- | --- |
| `AdventureService` | `CurrentAdventure`, stage number through `AdventureSession` | Creates new adventure session, reads DB, advances stage | Session state and session creation are mixed. |
| `CardDeckService` | Deck order, random state, draw index, remaining monster/event pools | Builds deck from DB models, resolves choice cards | Random/deck state can split if duplicated. |
| `CardService` | Created cards, next card id, ability systems on cards | Creates cards, applies tags/attributes/abilities | Must stay aligned with player card and board ids. |
| `CardBoardService` | Card ids by board zone | Moves/removes/places cards | Already VContainer-owned, but state is still internal to the service. |
| `PlayerService` | Current player state, current player card | Initializes player from character model | Must stay aligned with `CardService`. |
| `CombatService` | Combat cards, current side, round, resolved deaths, death subscription | Runs turn flow, subscribes to `GameplayMessageManager`, raises adventure events | Highest leak risk because it owns event subscription and combat lifecycle. |

The dangerous failure mode is split state:

```text
DependencyManager.CardService
-> PlayerService stores player card from this instance

VContainer.CardService
-> AdventureController reads a different card registry
```

This can compile and still be wrong at runtime.

## Design Principles

### 1. Scope Owns State

Runtime state that belongs to one adventure run must be owned by `AdventureSessionLifetimeScope`.

```text
AdventureSessionLifetimeScope
-> run state
-> services that mutate run state
```

Scene scopes may consume run state, but must not own it.

```text
AdventureSceneScope
-> AdventureController
-> AdventureView
-> scene startup/localization owners
-> consumes session services
```

### 2. State Is Data, Not Orchestration

State classes should hold mutable runtime data and simple reset operations.

They should not:

```text
- Load DB tables directly
- Subscribe to static/global events
- Load Unity assets
- Change scenes
- Create UI views
```

### 3. Services Mutate State

Services should express operations over state.

They can:

```text
- Start a session
- Advance stage
- Draw cards
- Place cards
- Initialize player run data
- Run combat turn logic
```

They should receive state through constructor injection.

### 4. Factories Create Runtime Objects

Object construction should be separated when creation has real rules.

Examples:

```text
CardFactory
-> creates Card from CardModelBase
-> applies tags, attributes, and ability grants

AdventureSessionFactory
-> creates AdventureSession from character/adventure model/seed
```

### 5. Content Lookup Is Not Run State

DB-backed content lookup is not Adventure session state.

Examples:

```text
CharacterService
MonsterService
DBManager table access
```

These should eventually become root-level catalog/repository boundaries, not session-scoped mutable services.

## Target Responsibility Split

### Adventure Session

Current:

```text
AdventureService
-> CurrentAdventure
-> StartNew(character)
-> AdvanceStage()
-> DB lookup
```

Target:

| Type | Responsibility | Scope |
| --- | --- | --- |
| `AdventureSessionState` | Holds current `AdventureSession`. | AdventureSession |
| `AdventureSessionFactory` | Creates `AdventureSession` from content data and seed. | AdventureSession or Root, depending on DB boundary |
| `AdventureSessionService` | Starts session, advances stage, exposes current stage. | AdventureSession |
| `IAdventureCatalog` | Reads adventure/character content. | Root or legacy bridge |

Do not move this alone if `CardDeckService` still depends on the same seed/session identity through another path.

### Card Deck

Current:

```text
CardDeckService
-> CurrentCardDeck
-> deck list
-> remaining monster/event pools
-> random
-> draw index
```

Target:

| Type | Responsibility | Scope |
| --- | --- | --- |
| `CardDeckState` | Holds current deck, draw index, remaining pools, random state. | AdventureSession |
| `CardDeckBuilder` | Builds initial deck/pools from content models and seed. | AdventureSession |
| `CardDeckService` | Draws cards and resolves choice cards by mutating `CardDeckState`. | AdventureSession |
| `ICardDeckCatalog` | Reads card deck content. | Root or legacy bridge |

This should move with `AdventureSessionState` or immediately after it. Moving it without a stable session seed boundary is brittle.

### Card Registry And Card Creation

Current:

```text
CardService
-> Dictionary<uint, Card>
-> next card id
-> Create(model)
-> applies tags/attributes/abilities
```

Target:

| Type | Responsibility | Scope |
| --- | --- | --- |
| `CardRegistry` | Holds created cards and next card id. | AdventureSession |
| `CardFactory` | Creates `Card` and applies tags, attributes, abilities. | AdventureSession |
| `CardService` | Coordinates create/replace/remove/query through registry/factory. | AdventureSession |

This should not move separately from `PlayerService`.

Reason:

```text
PlayerService currently stores the player card.
If CardService and PlayerService use different card instances or registries, the run is corrupt.
```

### Card Board

Current:

```text
CardBoardService
-> Dictionary<ECardZone, List<uint>>
```

Current migration status:

```text
CardBoardService is already VContainer-owned in AdventureSessionLifetimeScope.
It is removed from DependencyRegistry.
```

Target:

| Type | Responsibility | Scope |
| --- | --- | --- |
| `CardBoardState` | Holds card ids by zone. | AdventureSession |
| `CardBoardService` | Places, moves, removes, and clears board ids. | AdventureSession |

This is the safest state/service split to implement first because it has no DB lookup, no Unity object ownership, and no static event subscription.

### Player Run

Current:

```text
PlayerService
-> CurrentPlayer
-> current player card
```

Target:

| Type | Responsibility | Scope |
| --- | --- | --- |
| `PlayerRunState` | Holds current `PlayerState` and player card id/reference. | AdventureSession |
| `PlayerRunService` | Initializes player run data and exposes player card. | AdventureSession |

This should move with `CardRegistry`/`CardService`.

Cold assessment:

```text
PlayerRunState should probably store a card id instead of a direct Card reference long term.
Direct references are convenient, but card id keeps the ownership boundary clearer because CardRegistry remains authoritative.
```

### Combat

Current:

```text
CombatService
-> combat cards by side
-> avatar lookup
-> resolved deaths
-> round/current side
-> GameplayMessageManager subscription
-> AdventureEvents.CombatEnded
```

Target:

| Type | Responsibility | Scope |
| --- | --- | --- |
| `CombatState` | Holds combat cards, side, round, resolved deaths. | AdventureSession or future Combat sub-scope |
| `CombatService` | Runs turn flow and combat completion logic. | AdventureSession first, Combat sub-scope later if needed |
| `CombatDeathSubscriptionOwner` | Owns `GameplayMessageManager` subscription. | Same as `CombatService` |
| `CombatEventPort` | Emits combat result without direct static event coupling. | Later refactor |

Do not move `CombatService` early.

Reason:

```text
It owns a global message subscription.
If dispose timing is wrong, dead combat state can receive future death messages.
It also raises static AdventureEvents, so stale subscribers are harder to diagnose.
```

## Proposed Target Registration

Final shape:

```csharp
protected override void Configure(IContainerBuilder builder)
{
    builder.Register<AdventureSessionState>(Lifetime.Scoped);
    builder.Register<AdventureSessionFactory>(Lifetime.Scoped);
    builder.Register<AdventureSessionService>(Lifetime.Scoped);

    builder.Register<CardDeckState>(Lifetime.Scoped);
    builder.Register<CardDeckBuilder>(Lifetime.Scoped);
    builder.Register<CardDeckService>(Lifetime.Scoped);

    builder.Register<CardRegistry>(Lifetime.Scoped);
    builder.Register<CardFactory>(Lifetime.Scoped);
    builder.Register<CardService>(Lifetime.Scoped);

    builder.Register<CardBoardState>(Lifetime.Scoped);
    builder.Register<CardBoardService>(Lifetime.Scoped);

    builder.Register<PlayerRunState>(Lifetime.Scoped);
    builder.Register<PlayerRunService>(Lifetime.Scoped);

    builder.Register<CombatState>(Lifetime.Scoped);
    builder.Register<CombatService>(Lifetime.Scoped);
    builder.Register<CombatDeathSubscriptionOwner>(Lifetime.Scoped);

    builder.Register<AdventureSessionInitializer>(Lifetime.Scoped);
}
```

Do not implement all of this in one step. This is the target map, not the next patch.

## Migration Order

### Phase 1: Split CardBoardState

Reason:

```text
CardBoardService is already VContainer-owned.
It has the smallest blast radius.
```

Change:

```text
Add CardBoardState.
Move Dictionary<ECardZone, List<uint>> into CardBoardState.
Make CardBoardService operate on CardBoardState.
Keep public CardBoardService API unchanged.
```

Acceptance:

```text
AdventureController and AdventureSessionInitializer keep compiling unchanged.
Unity batchmode compile passes.
```

Status:

```text
Done.
CardBoardState exists.
CardBoardService public API stayed unchanged.
AdventureSessionLifetimeScope and AdventureSceneScope fallback register CardBoardState and CardBoardService.
```

### Phase 2: Split CardRegistry And CardFactory

Reason:

```text
CardService combines registry and creation rules.
This must be split before PlayerService is moved.
```

Change:

```text
Add CardRegistry.
Add CardFactory.
Make CardService delegate id/card storage to CardRegistry and construction to CardFactory.
Keep CardService public API unchanged.
```

Acceptance:

```text
No consumer outside CardService needs to know CardRegistry exists yet.
DependencyManager still owns CardService until the split is stable.
```

Status:

```text
Done.
CardRegistry exists and owns card dictionary plus next card id.
CardFactory exists and owns Card creation/replacement rules.
CardService public API stayed unchanged.
CardService moved to AdventureSessionLifetimeScope after the CardService/PlayerService ownership step.
```

### Phase 3: Split PlayerRunState

Reason:

```text
Player run data depends on card creation.
Move only after CardService internals are stable.
```

Change:

```text
Add PlayerRunState.
Make PlayerService operate on PlayerRunState.
Prefer storing player card id long term, but keep direct Card reference initially if needed to avoid broad changes.
```

Acceptance:

```text
PlayerService API remains stable.
No duplicate Card/Player state appears.
```

Status:

```text
Done.
PlayerRunState exists and holds CurrentPlayer plus CurrentPlayerCard.
PlayerService public API stayed unchanged.
PlayerService moved to AdventureSessionLifetimeScope after the CardService/PlayerService ownership step.
```

### Phase 4: Move CardService And PlayerService Together

Reason:

```text
CardService and PlayerService share the player card boundary.
Moving one without the other risks split state.
```

Change:

```text
Remove CardService and PlayerService from DependencyRegistry.
Register CardRegistry, CardFactory, CardService, PlayerRunState, and PlayerService/PlayerRunService in AdventureSessionLifetimeScope.
Remove their aliases from LegacyAdventureRunStateInstaller.
```

Acceptance:

```text
AdventureSessionInitializer and AdventureController resolve both from AdventureSessionLifetimeScope.
No DependencyManager.Resolve<CardService>() or Resolve<PlayerService>() remains in the Adventure path.
```

Status:

```text
Done.
CardRegistry, CardFactory, CardService, PlayerRunState, and PlayerService are registered in AdventureSessionLifetimeScope.
CardService and PlayerService were removed from DependencyRegistry.
CardService and PlayerService aliases were removed from LegacyAdventureRunStateInstaller.
```

### Phase 5: Split And Move AdventureSessionService With CardDeckService

Reason:

```text
Adventure seed/session identity drives card deck initialization.
These should not drift.
```

Change:

```text
Add AdventureSessionState.
Add AdventureSessionFactory.
Refactor AdventureService to use AdventureSessionState and AdventureSessionFactory.
Add CardDeckState and CardDeckBuilder.
Refactor CardDeckService to operate on CardDeckState.
Move AdventureService/CardDeckService ownership together or in two tightly verified steps.
```

Acceptance:

```text
AdventureSessionInitializer starts one session and initializes one deck from the same session seed.
No duplicate AdventureService/CardDeckService instances exist.
```

Status:

```text
Split done.
AdventureSessionState and AdventureSessionFactory exist.
AdventureService uses AdventureSessionState and AdventureSessionFactory internally.
CardDeckState and CardDeckBuilder exist.
CardDeckService uses CardDeckState and CardDeckBuilder internally.
AdventureService and CardDeckService moved to AdventureSessionLifetimeScope after the ownership step.
AdventureService and CardDeckService were removed from DependencyRegistry.
AdventureService and CardDeckService aliases were removed from LegacyAdventureRunStateInstaller.
```

### Phase 6: Combat Last

Reason:

```text
CombatService has global message subscription and static event coupling.
It is the most expensive failure if lifetime is wrong.
```

Change:

```text
Add CombatState.
Separate death subscription ownership.
Move CombatService only after AdventureView/AdventureController disposal and event unsubscription have been verified.
```

Acceptance:

```text
Combat dispose releases GameplayMessageManager subscription exactly once.
No static AdventureEvents subscribers survive session disposal.
```

## Immediate Next Patch

The next code patch should be:

```text
Introduce CombatState.
Move pure combat state fields from CombatService into CombatState.
Keep CombatService public API unchanged.
Keep CombatService DependencyManager-owned.
Run Unity batchmode compile.
```

CombatService is the highest-risk remaining run-state service because it owns a global message subscription and raises static AdventureEvents.
