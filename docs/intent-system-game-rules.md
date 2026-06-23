# Intent System Game Rules

## Purpose

The intent system lets the player see what each monster is about to do and choose the current turn's strategy in response.

The intent is not primarily an action name. It is player-facing information about the effect or danger that matters for decision making, such as damage, healing, defense, buffs, debuffs, or action-prevention states.

## V1 Scope

V1 covers:

- Showing monster intents at the start of the player turn.
- Repeating each monster's authored action sequence in order.
- Showing one or more authored intent display units per action.
- Showing numeric values for damage, healing, and defense.
- Recalculating visible intent values immediately when relevant effects change.
- Replacing the final action through status-based override rules.
- Executing the latest resolved final action that produced the currently visible
  intent.
- Handling execution failure by skipping the action.

V1 does not cover:

- Random action selection.
- Health-condition action branches.
- Hidden or unknown intents.
- Monster encyclopedia, observation history, or persistent learning records.
- Combat save and restore.
- Debug/read-model tooling.
- Final code architecture.
- Detailed text copy, animation timing, sound, or UI presentation polish.

## Turn Timing

At combat start, every living monster begins at action sequence index `0`.

At the start of each player turn, every living monster has a prepared base action derived from its current sequence index.

Monster intents are revealed from left to right using a sequential reveal animation.

In the current v1 flow, the player cannot act while the reveal animation is still
running. Player actions begin after the reveal flow is complete.

After an intent has been revealed, it remains visible on screen until it changes or the monster is removed.

If a monster's intent changes during the player action phase, the visible intent
updates immediately.

There is no extra final reevaluation immediately before the enemy turn. All relevant state changes during the player turn must trigger immediate intent refresh.

## Monster Action Sequence

Each monster owns its action sequence directly in the monster data.

The sequence is deterministic and repeats in authored order.

When the sequence reaches the last action, the next normal advance wraps back to the first action.

Enemy actions execute in screen/placement order from left to right.

If a monster dies during the player turn, its intent is removed and it does not execute on the enemy turn. Its sequence state no longer matters.

## Action Data

Each action entry must have:

- `ActionId`
- An execution target, whose concrete implementation shape is deferred to code design.
- At least one intent entry.

`ActionId` is a stable developer-facing key used for logs, validation, tests, and future tooling.

The exact execution representation is not decided in this game-rules document.
The current code-design document may choose a provisional execution shape while
the gameplay rule remains: one authored action has one executable target.

## Intent Entries

Intent entries are display units, not raw effect lists.

One intent entry should represent what the player reads as one action meaning.
If a single monster action deals damage and applies poison, and the player
should read that as one "poison sting" action, it should usually be authored as
one intent entry rather than separate damage and poison entries.

Multiple intent entries are used only when the monster action should be read as
multiple distinct action meanings.

Each intent entry must have:

- Icon.
- Description field.
- Optional numeric data, depending on intent type.

Numeric intent entries require numeric calculation rules.

Non-numeric intent entries do not require numeric values by default.

When an action has multiple intent entries, they are shown in the order authored
in the action data.

Example:

```text
[Poison Sting 3 x 2] [Defense 5]
```

## Intent Icon Semantics

Intent icons are separated by player response, not by visual flavor.

Effects that require the same player response should use the same icon.

Effects that require meaningfully different responses should use different icons.

For example, physical damage and fire damage use the same damage icon if the player's response is the same. Poison sting may use one poison-sting icon if the player should read damage plus poison as one action meaning, rather than two separate actions.

Buff and debuff intents may use effect-specific icons, sourced from the actual effect data when appropriate.

## Numeric Display

Damage, healing, and defense values are displayed using the same rules as actual execution.

Displayed values must reflect current relevant buffs, debuffs, stacks, and other state modifiers.

Damage intent shows the amount the monster will attempt to deal, not the player's final expected HP loss after defense, block, shield, or mitigation.

Multi-hit values may be represented as base value plus count.

Example:

```text
3 x 2
```

Different numeric entries in the same action stay separate only when they are
authored as distinct display units.

Given the same state, displayed numeric intent and actual monster output must match. A mismatch is a bug unless a later design explicitly introduces an exception.

## State And Item Effects

Skills and items do not directly rewrite UI intent.

They apply gameplay effects or states. The combat intent rules then reevaluate the affected monster's final action and intent.

Effects that only change values, such as weakness or strength, keep the base prepared action and immediately recalculate numeric intent values.

Effects that prevent or replace behavior are handled by status override rules.

## Status Override Rules

The base prepared action is preserved.

Status override rules determine the final action shown and executed while applicable.

A status override rule defines:

- Matching status/effect tag.
- Replacement action.
- Numeric `priority`.
- Whether the base action sequence advances after the replacement action is processed.

When multiple override rules apply, the highest priority rule wins.

Duplicate priority among simultaneously applicable override rules is invalid data.

The default sequence-advance policy for an override rule is `do not advance`.

If the replacement action actually executes during the enemy turn, that rule's sequence-advance policy is applied.

If the status is removed before enemy execution and the replacement action does not execute, the sequence does not advance because of that replacement. The monster returns to the base prepared action.

Multi-turn statuses are supported. If a status remains across turns, it can replace the current base action again on each affected turn.

The status system owns duration. For a status that blocks two enemy actions, duration naturally decreases after that monster's enemy-turn processing.

## Enemy Turn Execution

On the enemy turn, each living monster executes its current final action in left-to-right order.

The final action is the base prepared action after applying the currently winning status override rule, if any.

If the final action cannot execute, v1 shows no special failure intent or message. The action is skipped.

An action that fails this way still advances past that action.

## Validation

Editor or data validation should catch invalid authoring as early as possible.

Required validation includes:

- Monster action sequence is not empty.
- Every action has a stable `ActionId`.
- Every action has an execution target.
- Every action has at least one intent entry.
- Every intent entry has an icon.
- Every intent entry has a description field.
- Numeric intent entries have numeric calculation rules.
- Override rule priorities do not conflict where conflicts would make resolution ambiguous.

Runtime handling for invalid data is decided in code design. Some invalid data is
an editor/build validation error that runtime code may trust, while recoverable
runtime failures should log an error and keep combat from crashing.

## V1 Completion Criteria

The game-rules design for v1 is satisfied when the following scenarios are covered:

- At combat start, each living monster prepares and reveals its first intent.
- At each player turn start, living monsters reveal intents from left to right.
- Revealed intents remain visible.
- The player can act after the reveal flow is complete.
- A monster's deterministic sequence repeats in authored order.
- An action can display one or more intent entries, where each entry is an
  authored player-facing action meaning.
- Damage, healing, and defense values reflect current state.
- Relevant state changes immediately refresh affected intents.
- Status override rules can replace the visible and executable final action.
- Removing a status before enemy execution restores the base prepared action.
- Multi-turn status overrides can apply across multiple turns.
- Override priority deterministically selects one replacement action.
- Enemy turn execution uses the same latest resolved final action that produced
  the currently visible intent.
- Failed execution skips the action and advances past it.

## Deferred To Code Design

The next design phase should decide:

- Runtime owner for sequence index, base prepared action, override result, and current intent entries.
- ScriptableObject/data classes for monster actions and override rules.
- Execution abstraction details and concrete activation API.
- Event flow for immediate intent refresh.
- Numeric calculation integration with actual execution.
- UI data transfer shape.
- Test cases and validation tooling.
