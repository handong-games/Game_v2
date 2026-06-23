# CombatService Lifetime Inspection

## Purpose

This document records the lifetime and event-subscription risks that must be resolved before moving `CombatService` from `DependencyManager` ownership to `AdventureSessionLifetimeScope`.

Cold assessment:

```text
CombatService should not be moved as-is.
It owns mutable combat state, subscribes to GameplayMessageManager, and emits static AdventureEvents.
If the service lifetime is wrong, stale combat state can receive future death messages or notify dead scene views.
```

## Current Ownership

Current owner:

```text
DependencyManager
-> CombatService
```

Current VContainer path:

```text
AdventureSessionLifetimeScope
-> LegacyAdventureRunStateInstaller
-> RegisterInstance(DependencyManager.Resolve<CombatService>())
```

Current consumers:

```text
AdventureController
-> constructor CombatService
-> ReadyCombat(...)
-> NextTurn()
-> CurrentSide / RoundNumber
```

## Current CombatService Responsibilities

`CombatService` currently owns four responsibility groups:

| Responsibility | Current location | Risk |
| --- | --- | --- |
| Combat state | `_cardsBySide`, `_combatCardByAvatar`, `_resolvedDeaths`, `_currentSide`, `RoundNumber`, `_combatEnded` | Should be session/combat-scoped state, not global manager state. |
| Death subscription | `_combatDeathSubscription` from `GameplayMessageManager.Instance.Subscribe` | Must be disposed exactly once when combat/session ends. |
| Combat rule flow | `ReadyCombat`, `NextTurn`, turn start/end, death resolution | Can stay in service, but should operate on explicit state. |
| Static event emission | `AdventureEvents.CombatEnded`, `AdventureEvents.EnemyTurnBannerRequested` | Static delegate holders are not scoped and are easy to leak. |

## Current Subscription Flow

```text
AdventureController.ReadyCombat(...)
-> CombatService.ReadyCombat(...)
-> EnsureDeathSubscription()
-> GameplayMessageManager.Instance.Subscribe<GameplayDeathMessage>(
       GameplayMessageTags.CombatDeath,
       OnCombatDeathMessage)
```

Unsubscribe currently happens only here:

```text
CombatService.Dispose()
-> _combatDeathSubscription?.Dispose()
```

`GameplayMessageManager` returns a disposable subscription and removes the handler when disposed. That part is acceptable. The risk is ownership timing: if `CombatService.Dispose()` is not called at the right session boundary, the subscription remains active.

## Static AdventureEvents Risk

`AdventureEvents` is a public static delegate holder:

```text
AdventureEvents.CombatEnded
AdventureEvents.EnemyTurnBannerRequested
AdventureEvents.CardDealCompleted
AdventureEvents.StageCompleted
...
```

Current subscriptions:

```text
AdventureController
-> CardDealCompleted
-> StageCompleted

AdventureView
-> AdventureStarted
-> CardsDrawn
-> BoardChanged
-> TurnBannerRequested
-> EnemyTurnBannerRequested
-> CombatEnded
```

Current defensive cleanup:

```text
AdventureController.Dispose()
-> UnregisterEvents()

AdventureView.Dispose()
-> UnregisterEvents()
-> CoinFlipCueEventBus.Published -= ...
-> CoinChangeCueEventBus.Published -= ...
```

This is better than before, but still not a scoped event model. `AdventureEvents` itself has no `Clear()` or scoped owner.

## Disposal Order Risk

Current AdventureScene flow:

```text
AdventureScene.OnUnloaded()
-> AdventureSceneScope.Dispose()
-> AdventureSceneStartup.Dispose()
-> IViewHost.Detach(adventureView)
-> scoped AdventureView / AdventureController disposed by VContainer
```

Current CombatService flow:

```text
DependencyManager owns CombatService globally
-> CombatService.Dispose() only when DependencyManager disposes all globals
```

That means the view/controller may die at scene unload, but `CombatService` can remain alive unless it is moved to a session scope.

Moving it to `AdventureSessionLifetimeScope` will fix ownership only if its internal subscription and state are separated cleanly first.

## Required Refactor Before Ownership Move

Do this before moving `CombatService`:

```text
1. Introduce CombatState.
2. Move combat cards, side, round, resolved deaths, and ended flags into CombatState.
3. Introduce CombatDeathSubscriptionOwner.
4. Move GameplayMessageManager subscription ownership out of CombatService.
5. Keep CombatService public API unchanged.
6. Keep CombatService DependencyManager-owned until the split compiles.
```

Target split:

| Type | Responsibility | Target scope |
| --- | --- | --- |
| `CombatState` | Combat cards, avatar lookup, resolved deaths, side, round, ended flags. | AdventureSession first, possible combat sub-scope later |
| `CombatDeathSubscriptionOwner` | Owns `GameplayMessageManager` subscription and disposes it. | Same as `CombatService` |
| `CombatService` | Runs combat rules over `CombatState`. | AdventureSession first |
| Later `AdventureEventPort` | Replaces direct static `AdventureEvents` emission. | Later refactor |

## Immediate Next Patch

The next code patch should be:

```text
Introduce CombatState.
Move pure combat state fields from CombatService into CombatState.
Keep CombatService public API unchanged.
Keep CombatService DependencyManager-owned.
Run Unity batchmode compile.
```

Do not move `CombatService` to VContainer ownership yet.

## Move Criteria

`CombatService` can move to `AdventureSessionLifetimeScope` only after:

```text
CombatState exists.
GameplayMessageManager subscription ownership is explicit.
CombatService Dispose releases subscription exactly once.
AdventureView and AdventureController disposal are still deterministic.
No stale AdventureEvents subscriber survives AdventureSceneScope disposal.
```

Cold warning:

```text
The last item is not fully enforceable while AdventureEvents remains static.
For now, defensive unsubscribe is acceptable, but long term AdventureEvents should become a scoped event port/bus.
```
