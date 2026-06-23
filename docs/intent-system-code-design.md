# Intent System Code Design

This document records the code design interview after the game-rules design in
`docs/intent-system-game-rules.md`.

This document now uses the current implementation-oriented naming where names
have been decided. Names that are still unresolved remain marked as temporary or
deferred.

## Status

The content in this document is provisional. It captures the current working
design and may be revised as later viewpoints are designed.

## Naming And Placement

V1 intent code is grouped under the combat domain.

```text
Assets/@Scripts/Domains/Combat/Intent/
  Data/
  Execution/
  Events/
```

Namespaces follow the folder structure:

- `Domains.Combat.Intent`
- `Domains.Combat.Intent.Data`
- `Domains.Combat.Intent.Execution`
- `Domains.Combat.Intent.Events`

V1 file/class layout:

```text
Assets/@Scripts/Domains/Combat/Intent/
  IntentSystem.cs              -> Domains.Combat.Intent.IntentSystem
  IntentRuntimeState.cs        -> Domains.Combat.Intent.IntentRuntimeState
  IntentDisplayData.cs         -> Domains.Combat.Intent.IntentDisplayData
  IntentNumberData.cs          -> Domains.Combat.Intent.IntentNumberData
  ResolvedIntentActionData.cs  -> Domains.Combat.Intent.ResolvedIntentActionData

Assets/@Scripts/Domains/Combat/Intent/Data/
  EIntentAction.cs                         -> Domains.Combat.Intent.Data.EIntentAction
  EIntentDisplay.cs                        -> Domains.Combat.Intent.Data.EIntentDisplay
  IntentActionModel.cs                     -> Domains.Combat.Intent.Data.IntentActionModel
  IntentDisplayModel.cs                    -> Domains.Combat.Intent.Data.IntentDisplayModel
  IntentDisplayDefinition.cs               -> Domains.Combat.Intent.Data.IntentDisplayDefinition
  IntentNumberRuleModel.cs                 -> Domains.Combat.Intent.Data.IntentNumberRuleModel
  FixedIntentNumberRuleModel.cs            -> Domains.Combat.Intent.Data.FixedIntentNumberRuleModel
  FixedIntentNumberAndCountRuleModel.cs    -> Domains.Combat.Intent.Data.FixedIntentNumberAndCountRuleModel
  IntentOverrideRule.cs                    -> Domains.Combat.Intent.Data.IntentOverrideRule
  IntentOverrideRuleSetModel.cs            -> Domains.Combat.Intent.Data.IntentOverrideRuleSetModel

Assets/@Scripts/Domains/Combat/Intent/Execution/
  ActionExecutionBindingData.cs   -> Domains.Combat.Intent.Execution.ActionExecutionBindingData
  ActionExecutionBindingStore.cs  -> Domains.Combat.Intent.Execution.ActionExecutionBindingStore
```

Do not create separate `IntentActionResolver` or `IntentDisplayCalculator`
classes in v1. `IntentSystem` owns `ResolveAction` and `BuildDisplayCache`
internally until those responsibilities become large enough to split.

Naming rules:

- `Model`: an independent ScriptableObject asset or reusable static authored
  data. Examples: `IntentActionModel`, `IntentDisplayModel`,
  `IntentNumberRuleModel`, `IntentOverrideRuleSetModel`.
- `Definition` or `Rule`: a serialized item inside a parent model, not an
  independent asset. Examples: `IntentDisplayDefinition`,
  `IntentOverrideRule`.
- `Data`: immutable runtime value structs or snapshot-like data passed between
  systems. Examples: `IntentNumberData`, `IntentDisplayData`,
  `ResolvedIntentActionData`, `ActionExecutionBindingData`.
- No suffix: mutable runtime state, services, stores, and systems. Examples:
  `IntentRuntimeState`, `IntentSystem`, `ActionExecutionBindingStore`.

Confirmed authored data names:

- `IntentActionModel : AbstractModel<EIntentAction>`.
- `IntentDisplayModel : AbstractModel<EIntentDisplay>`.
- `MonsterModel.ActionSequence`: the authored repeated action order for that
  monster.

## Implementation Sketches

These sketches are not final code. They document the expected field shape before
implementation so later code can be checked against the design.

### IntentActionModel

```csharp
namespace Domains.Combat.Intent.Data
{
    using System;
    using System.Collections.Generic;
    using Game.Data;
    using Gameplay.GAS;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/Combat/Intent/Action")]
    public sealed class IntentActionModel : AbstractModel<EIntentAction>
    {
        [SerializeField]
        private GameplayAbility _executionAbility;

        [SerializeField]
        private IntentDisplayDefinition[] _displayDefinitions;

        public GameplayAbility ExecutionAbility => _executionAbility;

        public IReadOnlyList<IntentDisplayDefinition> DisplayDefinitions =>
            _displayDefinitions ?? Array.Empty<IntentDisplayDefinition>();
    }
}
```

Field roles:

- `Id` from `AbstractModel<EIntentAction>` is the stable action id used for
  authoring, validation, logs, and tests.
- `ExecutionAbility` is the GameplayAbility that enemy-turn execution will bind
  and activate.
- `DisplayDefinitions` is the authored list of player-facing intent display
  units for this action.

### IntentDisplayModel

```csharp
namespace Domains.Combat.Intent.Data
{
    using Game.Data;
    using UnityEngine;
    using UnityEngine.AddressableAssets;
    using UnityEngine.Localization;

    [CreateAssetMenu(menuName = "Game/Combat/Intent/Display")]
    public sealed class IntentDisplayModel : AbstractModel<EIntentDisplay>
    {
        [SerializeField]
        private AssetReferenceSprite _icon;

        [SerializeField]
        private LocalizedString _description;

        [SerializeField]
        private bool _requiresNumber;

        public AssetReferenceSprite Icon => _icon;
        public LocalizedString Description => _description;
        public bool RequiresNumber => _requiresNumber;
    }
}
```

Field roles:

- `Id` from `AbstractModel<EIntentDisplay>` is the stable display meaning id.
- `Icon` is the Addressables sprite reference shown by UI.
- `Description` is the localized text used by tooltip/detail UI.
- `RequiresNumber` says whether every use of this display model must provide a
  numeric rule.

### IntentDisplayDefinition

```csharp
namespace Domains.Combat.Intent.Data
{
    using System;
    using UnityEngine;

    [Serializable]
    public sealed class IntentDisplayDefinition
    {
        [SerializeField]
        private IntentDisplayModel _displayModel;

        [SerializeField]
        private IntentNumberRuleModel _numberRule;

        public IntentDisplayModel DisplayModel => _displayModel;
        public IntentNumberRuleModel NumberRule => _numberRule;
    }
}
```

Field roles:

- `DisplayModel` points to the reusable static display source, such as attack,
  defense, heal, or stun.
- `NumberRule` is the action-specific numeric preview rule. It is required only
  when `DisplayModel.RequiresNumber` is true.

### IntentNumberRuleModel

```csharp
namespace Domains.Combat.Intent.Data
{
    using Domains.Combat.Intent;
    using UnityEngine;

    public abstract class IntentNumberRuleModel : ScriptableObject
    {
        public abstract IntentNumberData BuildNumber();
    }
}
```

V1 fixed rule types:

```csharp
namespace Domains.Combat.Intent.Data
{
    using Domains.Combat.Intent;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/Combat/Intent/Fixed Number Rule")]
    public sealed class FixedIntentNumberRuleModel : IntentNumberRuleModel
    {
        [SerializeField]
        private int _numberValue;

        public override IntentNumberData BuildNumber()
        {
            return new IntentNumberData(_numberValue, 1);
        }
    }
}
```

```csharp
namespace Domains.Combat.Intent.Data
{
    using Domains.Combat.Intent;
    using UnityEngine;

    [CreateAssetMenu(menuName = "Game/Combat/Intent/Fixed Number And Count Rule")]
    public sealed class FixedIntentNumberAndCountRuleModel : IntentNumberRuleModel
    {
        [SerializeField]
        private int _numberValue;

        [SerializeField]
        private int _countValue = 1;

        public override IntentNumberData BuildNumber()
        {
            return new IntentNumberData(_numberValue, _countValue);
        }
    }
}
```

Dependency return APIs are not added in the first code pass. V1 can build an
empty dependency cache, then add concrete dependency types when the low-level
ability/state event connection is designed.

### Runtime Values

```csharp
public readonly struct IntentNumberData
{
    public IntentNumberData(int numberValue, int countValue)
    {
        NumberValue = numberValue;
        CountValue = countValue;
    }

    public int NumberValue { get; }
    public int CountValue { get; }
}
```

```csharp
public readonly struct IntentDisplayData
{
    public IntentDisplayData(
        IntentDisplayModel displayModel,
        int numberValue,
        int countValue)
    {
        DisplayModel = displayModel;
        NumberValue = numberValue;
        CountValue = countValue;
    }

    public IntentDisplayModel DisplayModel { get; }
    public int NumberValue { get; }
    public int CountValue { get; }
}
```

```csharp
public readonly struct ResolvedIntentActionData
{
    public ResolvedIntentActionData(
        IntentActionModel baseActionModel,
        IntentActionModel finalActionModel,
        IntentOverrideRule appliedOverrideRule)
    {
        BaseActionModel = baseActionModel;
        FinalActionModel = finalActionModel;
        AppliedOverrideRule = appliedOverrideRule;
    }

    public IntentActionModel BaseActionModel { get; }
    public IntentActionModel FinalActionModel { get; }
    public IntentOverrideRule AppliedOverrideRule { get; }
    public bool HasOverride => AppliedOverrideRule != null;
}
```

### Service API Sketch

Initial `IntentSystem` public API:

```text
Clear()
Initialize(IEnumerable<CombatCard> monsters)
RebuildAll(IEnumerable<CombatCard> monsters)
Rebuild(CombatCard monster)
TryGetIntentDisplays(CombatCard monster, out IReadOnlyList<IntentDisplayData> displays)
TryGetResolvedAction(CombatCard monster, out ResolvedIntentActionData action)
ConsumeResolvedAction(CombatCard monster)
Remove(CombatCard monster)
event IntentChanged
event IntentCleared
```

Initial `ActionExecutionBindingStore` public API:

```text
Clear()
Bind(CombatCard monster, IntentActionModel actionModel, GameplayAbilitySpecHandle handle)
TryGetHandle(CombatCard monster, IntentActionModel actionModel, out GameplayAbilitySpecHandle handle)
```

## Core Types

### IntentRuntimeState

Runtime state held per monster for the intent/action sequence system.

Ownership:

- Owned by the intent system, or an equivalent combat-intent subsystem.
- Stored conceptually as `MonsterRef -> IntentRuntimeState`.
- Created when a monster combat runtime instance enters combat.
- Removed or made irrelevant when that monster is removed from combat.

Current fields:

- `currentIntentActionIndex`: current intent action index.
- `cachedIntentDisplays`: cached intent display list.
- `currentDependencies`: current refresh dependencies.
- `cachedResolvedAction`: cached resolved action.

Responsibilities:

- Store the current action sequence position for intent/action selection.
- Store the latest calculated display cache for UI consumption.
- Store the current dependency cache used for player-turn refresh filtering.
- Store the latest resolved action used as the combat execution source.
- Initialize `currentIntentActionIndex` to `0` at combat start.
- Advance `currentIntentActionIndex` when enemy-turn rules say the current
  action is consumed.

Non-responsibilities:

- Does not resolve status overrides.
- Does not calculate final action.
- Does not calculate numeric intent values.
- Does not know whether an intent is currently visible on screen.
- Is not exposed directly to outside systems as a mutable public object.

Access boundary:

- Other systems should request the specific values they need through intent-system
  query methods.
- They should not receive and freely inspect or mutate the full `IntentRuntimeState`
  object.
- This keeps cache ownership, refresh rules, and sequence-index mutation inside
  the intent system.

Example conceptual queries:

```text
GetResolvedAction(MonsterRef) -> ResolvedIntentActionData
GetIntentDisplays(MonsterRef) -> IntentDisplayData list
GetCurrentIntentActionIndex(MonsterRef) -> int
```

The exact return form for UI-facing display data is deferred to UI connection
design. For example, whether `GetIntentDisplays` returns an internal read-only
view, a copied snapshot, or another presentation model is not decided here.

Lookup failure policy:

- A living monster that participates in combat must have an `IntentRuntimeState`.
- If an external query asks for a living monster's intent state and no
  `IntentRuntimeState` exists, this is a programming/data-flow error.
- The intent system should log an error with enough context to identify the
  monster and requested value.
- The caller may still fail the current action safely to keep combat from
  crashing, but the missing state itself is not considered normal gameplay.
- Removed/dead monsters should normally be filtered out before querying the
  intent system.
- If a living monster has `IntentRuntimeState` but no `cachedResolvedAction`
  when enemy-turn execution asks for its resolved action, log an error, skip
  that monster's execution, do not call `ConsumeResolvedAction`, and do not
  advance `currentIntentActionIndex`.

### currentIntentActionIndex

The current action index used as the basis for this monster's intent.

Working name from the interview: `currentIntentActionIndex`.

Rules:

- It points into `MonsterModel.ActionSequence`.
- It starts at `0` for each combat.
- Player-turn start reads it but does not advance it.
- Enemy-turn processing may advance it later.

### cachedIntentDisplays

The latest calculated intent display cache for this monster.

Working concept: `cachedIntentDisplays`.

It contains `IntentDisplayData` entries.

If the intent display cache has been cleared after `ConsumeResolvedAction`,
`GetIntentDisplays(MonsterRef)` returns an empty list. This is a normal
"nothing to display" state, not an error.

### currentDependencies

The current dependency cache for this monster's visible/calculated intent.

Working concept: `currentDependencies`.

Purpose:

- Let the intent system filter player-turn change events.
- Avoid recalculating intent displays for unrelated gameplay changes.

Current composition:

- Dependencies declared by the current final action's `IntentDisplayDefinition`
  entries.
- Conditions from the global `IntentOverrideRule` set, because those conditions can
  change the final action itself.

Monster-specific immunity or status applicability is not handled here. If a
monster is immune to a state, the ability/state system should prevent that state
from being applied in the first place.

### cachedResolvedAction

The latest resolved action for this monster.

Working concept: `cachedResolvedAction`.

Purpose:

- Preserve the exact action decision that produced the visible intent cache.
- Provide the combat execution source without re-resolving immediately
  before execution.

This exists because the game rule says there is no separate final reevaluation
right before the enemy turn. Player-turn start and player-turn change events are
responsible for keeping this cache current.

Contains a `ResolvedIntentActionData`.

If `cachedResolvedAction` has been cleared, `GetResolvedAction(MonsterRef)` is
an error for a living monster. The intent system should log the missing resolved
action and return a failure/no-action result to the caller.

### IntentDisplayData

One calculated UI-display item in the intent cache.

Fields:

- `DisplayModel`: reference to static display data.
- `NumberValue`: `int`.
- `CountValue`: `int`.

Numeric display rules:

- `NumberValue = 0`, `CountValue = 0`: show no number.
- `NumberValue = N`, `CountValue = 1`: show `N`.
- `NumberValue = N`, `CountValue > 1`: show `N x CountValue`.
- For intents whose display model requires a number, `NumberValue = 0` and
  `CountValue = 0` is invalid authoring/rule configuration. A real zero value
  should be represented as `NumberValue = 0`, `CountValue = 1`.

### IntentDisplayData.DisplayModel

A reference to an `IntentDisplayModel`.

Purpose:

- Keep runtime-changing data separate from static display data.
- Runtime cache stores numeric values and counts.
- Static data stores icon and description references through
  `IntentDisplayModel`.

Static display data must provide:

- Icon reference.
- Description reference.

Current direction:

- Icon references use Addressables `AssetReferenceSprite`.
- Description references use Unity Localization, likely `LocalizedString`.
- Static display data should share a common base/contract so UI can read all
  intent display units in the same way.

### IntentDisplayModel

Static display model for one player-facing intent meaning.

Purpose:

- Avoid duplicating icon and description references across actions.
- Let one authored intent entry point to a reusable display meaning.
- Keep runtime display cache focused on changing values.

Required fields:

- Icon reference.
- Description reference.

Authoring validation:

- Icon reference must not be null.
- Description reference must not be null.
- Null icon or description reference is an editor/build validation error.
- Runtime UI/data projection assumes both references exist.

Current optional fields:

- GameplayEffect reference, when this display meaning is tied to a concrete
  effect.
- Semantic/category tag, such as damage, heal, defense, poison, or stun.
- `RequiresNumber`: whether this display meaning requires a numeric preview.

Examples:

```text
IntentDisplayModel: Poison Sting
- icon: poison-sting icon
- description: poison-sting localized text
- semantic tag: Intent.PoisonSting

IntentDisplayModel: Defense
- icon: defense icon
- description: defense localized text
- semantic tag: Intent.Defense
- requires number: true
```

### IntentActionModel

One authored monster action entry.

Temporary decision:

- It provides both player-facing intent information and enemy-turn execution
  information.
- This decision is provisional and may be reopened.

Conceptual use:

- During player-turn display, `DisplayDefinitions` are read from
  `IntentActionModel`.
- During combat setup, `IntentActionModel` is bound to the granted
  AbilitySpecHandle that will execute it.
- During enemy-turn execution, the stored runtime binding for
  `IntentActionModel` is used.

Current required static fields:

- `IntentActionModel.Id / EIntentAction`: stable developer-facing action id for
  logs, validation, and tests.
- `ExecutionAbility`: execution Ability reference used during combat setup to
  bind this action to a granted runtime AbilitySpec.
- `DisplayDefinitions`: authored intent definitions used to build display
  cache.

Authoring validation:

- `ExecutionAbility` must not be null.
- Null `ExecutionAbility` is an editor/build validation error.
- `DisplayDefinitions` must not be null or empty.
- Null or empty `DisplayDefinitions` is an editor/build validation error.
- Runtime combat code assumes authored `IntentActionModel` has already passed this
  validation and can be used as valid executable action data.

### IntentDisplayDefinition

One authored display unit under an action.

An `IntentDisplayDefinition` is not a raw list of gameplay effects. It represents
one player-facing action meaning. If a monster action deals damage and applies
poison, and the player should read it as one "poison sting" action, it should be
one `IntentDisplayDefinition` that references a poison-sting display model.

Current fields:

- `DisplayModel`: reference to `IntentDisplayModel`.
- `NumberRule`: reference to `IntentNumberRuleModel`, required only when the
  display model requires a number.

Authoring validation:

- `DisplayModel` must not be null.
- Null `DisplayModel` is an editor/build validation error.
- If `DisplayModel.RequiresNumber` is true, the numeric rule reference must not be
  null.
- If `DisplayModel.RequiresNumber` is false, the numeric rule reference must be
  null.
- If `DisplayModel.RequiresNumber` is true, the referenced numeric rule must be
  validated in editor/build validation as a rule that produces a numeric display
  result. It must not be configured as a no-number rule.
- Runtime intent calculation assumes every `IntentDisplayDefinition` has a valid
  display model reference.

Current direction:

- All display definitions use `IntentDisplayModel` as their static display
  source.
- Basic/effect-based split has been removed.
- One intent definition produces one representative `NumberValue`/`CountValue`
  pair in `IntentDisplayData`.
- Display definitions should not manually duplicate refresh dependency lists
  when those dependencies come from numeric calculation.

Numeric display rule:

- `IntentDisplayDefinition` does not expose multiple independent numeric values.
- If the intent has no number, the runtime display uses `NumberValue = 0` and
  `CountValue = 0`.
- If the intent has a number, the runtime display uses one `NumberValue` and one
  `CountValue`.
- The actual numeric calculation source is deferred to numeric preview design.
- If `DisplayModel.RequiresNumber` is false, `IntentDisplayData` is created with
  `NumberValue = 0` and `CountValue = 0`.

Temporary maintainability decision:

- Numeric calculation should provide its own refresh dependencies.
- `IntentDisplayDefinition` should reference a numeric rule/preview concept when it
  needs a number.
- That numeric rule/preview concept is responsible for both:
  - producing `NumberValue`/`CountValue`;
  - exposing the dependencies that should refresh the displayed value.
- Non-numeric display definitions do not need numeric dependencies.
- Override-rule dependencies remain separate because they can change the final
  action itself.

### IntentNumberRuleModel

Provisional contract for numeric intent preview.

V1 implementation direction:

- Start with fixed-value numeric rules.
- Actual buff/debuff/GameplayEffect-integrated numeric preview is deferred.
- This lets the intent data path, caching, display, and consume flow be
  implemented before full combat math integration.

Initial rule types:

```text
FixedIntentNumberRuleModel
- NumberValue
- CountValue = 1

FixedIntentNumberAndCountRuleModel
- NumberValue
- CountValue
```

Examples:

```text
Damage 6 -> NumberValue = 6, CountValue = 1
Damage 3 x 2 -> NumberValue = 3, CountValue = 2
Defense 5 -> NumberValue = 5, CountValue = 1
```

Initial validation:

- `FixedIntentNumberRuleModel` always produces `CountValue = 1`.
- `FixedIntentNumberRuleModel.NumberValue` may be any `int`, including `0`.
- `FixedIntentNumberAndCountRuleModel.CountValue` must be `>= 1`.
- `FixedIntentNumberAndCountRuleModel.NumberValue` may be any `int`, including
  `0`.
- `NumberValue = 0`, `CountValue = 1` means display the real value `0`.
- `NumberValue = 0`, `CountValue = 0` remains reserved for no-number display and
  is not produced by fixed numeric rules used by number-required intents.

Dependency behavior:

- Fixed numeric rules may have an authored dependency list.
- Their calculated number remains fixed, but relevant gameplay changes can still
  trigger the intent refresh path.
- This keeps the event reaction path alive in v1 even before modifier-aware
  numeric preview is implemented.

Example:

```text
FixedIntentNumberRuleModel
- NumberValue: 6
- Dependencies:
  - Attribute.AttackPower
  - Tag.State.Weakness
  - Tag.Buff.Strength
```

If `Tag.State.Weakness` changes, the intent system may refresh the intent even
though this v1 fixed rule still returns `6`.

Refresh emit policy:

- In v1, if a dependency match triggers an intent refresh, emit `IntentChanged`
  after rebuilding the cache even when the resulting display values are the same
  as before.
- Change-detection optimization can be added later.

Dependency validation:

- Authored dependency entries on numeric rules must be validated in editor/build
  validation.
- Null, invalid, or unresolved dependency entries are validation errors.
- Runtime dependency matching assumes authored numeric rule dependencies are
  valid.

Later ownership may move to the intent system, the Ability/GAS execution layer,
or a shared preview layer.

Responsibilities:

- Produce one `NumberValue` and one `CountValue` for an `IntentDisplayData`.
- Provide refresh dependencies required by that calculation.

Conceptual contract:

```text
BuildNumber(context) -> NumberValue, CountValue
GetDependencies() -> dependency list
```

`context` is not designed yet. It may include the acting monster, target,
AbilitySystem state, attributes, active effects, stacks, or action data.

Important constraints:

- One `IntentDisplayDefinition` can use at most one numeric rule.
- One numeric rule output maps to the single representative
  `NumberValue`/`CountValue` pair for that intent display unit.
- Numeric rule results must remain consistent with actual execution behavior.

Temporary refresh dependency kinds:

- Attribute.
- Tag.
- ActiveEffect.
- EffectStack.

V1 dependency matching is exact match only:

- Attribute dependency matches only the same Attribute id/reference.
- Tag dependency matches only the same exact GameplayTag.
- ActiveEffect dependency matches only the same effect id/reference.
- EffectStack dependency matches only the same effect id/reference.

Grouped, parent, or hierarchical matching is deferred.

### IntentOverrideRule

One status-based action replacement rule.

Fields:

- `ConditionTag`: GameplayTag condition, exact match.
- `ReplacementAction`: reference to a shared `IntentActionModel`.
- `Priority`: `int`.
- `Advance`: `bool`.

`ConditionTag` meaning:

- The rule applies when the monster AbilitySystem currently has the exact
  condition tag.
- Hierarchical/parent tag matching is deferred.

`ReplacementAction` meaning:

- The replacement is not a direct GameplayAbility reference.
- It is a normal `IntentActionModel` reference.
- Replacement actions are authored as shared action data, not inline inside the
  override rule.
- This keeps normal actions and replacement actions on the same validation,
  logging, execution, and display path.

`Priority` meaning:

- When multiple rules match, the rule with the highest priority wins.
- Priority ties are invalid data and should be caught by build/data validation.

`Advance` meaning:

- `true`: after the replacement action is actually processed on the enemy turn,
  the original action is considered consumed and the sequence advances.
- `false`: after the replacement action is processed, the original action is
  held and can be attempted again later.

Important:

- `advance` applies only if the replacement action actually processes.
- If the status is removed before enemy execution, the replacement does not
  process and `advance` does not apply.

### IntentOverrideRuleSetModel

Global/shared rule set used by combat intent resolution.

Current direction:

- Authored as a global ScriptableObject referenced by the combat system.
- Contains a list of `IntentOverrideRule` entries.
- Monster-specific immunity is not represented here. Immunity belongs to the
  AbilitySystem/state application layer.

Example:

```text
IntentOverrideRuleSetModel: DefaultCombatOverrideRules

Rule: Stun
- ConditionTag: State.Stun
- ReplacementAction: stunned_action IntentActionModel
- Priority: 100
- Advance: true

Rule: Sleep
- ConditionTag: State.Sleep
- ReplacementAction: sleep_action IntentActionModel
- Priority: 80
- Advance: false
```

Validation:

- `ConditionTag` is required.
- `ReplacementAction` is required.
- `Priority` must not duplicate another rule priority in the same RuleSet.
- Referenced replacement `IntentActionModel` must be valid.

### IntentSystem.ResolveAction

V1 keeps action resolution as an internal `IntentSystem` responsibility rather
than a separate resolver class.

Enemy-turn execution reads `IntentRuntimeState.cachedResolvedAction`; it does not call
`ResolveAction` again immediately before running the action.

Responsibility:

- Read `currentIntentActionIndex`.
- Select the base action from `MonsterModel.ActionSequence`.
- Read current status/effect state from the monster's ability/state system.
- Apply `IntentOverrideRule` rules.
- Return a `ResolvedIntentActionData`.

### ResolvedIntentActionData

Output of `IntentSystem.ResolveAction`.

Temporary shape:

- `BaseActionModel`: action selected by `currentIntentActionIndex`.
- `FinalActionModel`: action after status override application.
- `AppliedOverrideRule`: override rule that selected the final action, or
  none.

### IntentSystem.BuildDisplayCache

V1 keeps display-cache calculation as an internal `IntentSystem` responsibility
rather than a separate calculator class.

Temporary responsibility:

- Use `ResolveAction` to get the current final action.
- Rebuild `IntentRuntimeState.cachedResolvedAction`.
- Read `DisplayDefinitions` from the final action.
- Calculate or request needed runtime numeric values.
- Rebuild `IntentRuntimeState.cachedIntentDisplays`.
- Rebuild `IntentRuntimeState.currentDependencies`.
- Cause an intent-changed notification after cache refresh.

Numeric calculation ownership is not yet final. The current document only
requires that displayed numeric values be cached in `cachedIntentDisplays`.

## Player Turn Start Flow

The player-turn-start viewpoint is currently designed as follows.

```text
1. Combat has already created IntentRuntimeState for each monster.
   - currentIntentActionIndex = 0 at combat start.
   - cachedIntentDisplays starts empty.

2. Player turn starts.

3. For every living monster:
   - `ResolveAction` selects the base action using `MonsterModel.ActionSequence`
     and `currentIntentActionIndex`.
   - `ResolveAction` checks current status/effects.
   - `ResolveAction` applies IntentOverrideRule rules and returns ResolvedIntentActionData.
   - IntentRuntimeState.cachedResolvedAction is replaced.
   - `BuildDisplayCache` builds new IntentDisplayData entries from the final action.
   - IntentRuntimeState.cachedIntentDisplays is replaced.
   - IntentRuntimeState.currentDependencies is replaced.
   - IntentChanged is emitted.

4. UI receives latest monster intent data.

5. UI reveals monsters from left to right.
   - The combat/intent system does not care whether a monster is visible.
   - Reveal state is a UI concern.
   - In v1, player actions are not accepted until the reveal flow is complete.
```

Player-turn start recalculation is authoritative. If `cachedResolvedAction`, `cachedIntentDisplays`, or
`currentDependencies` still contains previous data, the intent system replaces it without a
warning.

At player-turn start, the intent system calculates and emits `IntentChanged` for
all living monsters before or independently of UI reveal timing. The UI may queue
those updates and reveal them left to right. The intent system does not know
whether a specific intent has already appeared on screen. In v1, the case where
player action changes an unrevealed monster's queued intent is not considered,
because player actions are not accepted during reveal.

## Combat Setup And VContainer

V1 should use VContainer for object construction and dependency composition, but
not every combat-related object must be recreated for every combat.

Current project context:

- The project already uses VContainer `1.18.0`.
- Existing composition roots include:
  - `RootLifetimeScope`
  - `AdventureSessionLifetimeScope`
  - `AdventureSceneScope`
  - `TitleSceneScope`
- Existing scene/session startup uses `RegisterEntryPoint`, for example
  `AdventureSceneStartup`.
- Current combat flow is driven by `CombatService.ReadyCombat(...)` and
  `CombatService.NextTurn()`.

Current direction:

- Register intent/combat execution services through the existing VContainer
  composition pattern.
- Adventure scene-level services belong to the Adventure lifetime scope
  (`AdventureLifetimeScope` conceptually; current project naming may use
  `AdventureSceneScope`).
- Because one Adventure lifetime can contain multiple combats, reusable
  combat-related services can be created once in the Adventure lifetime and reset
  for each combat.
- Reusable services may include:
  - `IntentSystem` or equivalent intent subsystem.
  - `ActionExecutionBindingStore`.
  - `CombatService`, which owns combat state, orchestrates combat setup, handles
    turn flow, and executes enemy turns.
- These services should not be registered in `RootLifetimeScope` as global
  singletons.
- Per-combat data inside those services is cleared and rebuilt for each combat.
- Specific object types can still be pooled or created fresh inside the combat
  child scope if reset/reuse is unsafe, unclear, or more expensive to maintain.

Responsibility split:

- VContainer owns construction, injection, and Adventure-scope disposal for
  reusable services.
- `CombatService` owns combat setup order, validation orchestration, turn
  transition, and enemy-turn execution order.
- Constructors should not perform combat setup work that depends on monster
  runtime data being fully available.
- `BeginCombat`/`ReadyCombat` style methods should create or reset per-combat
  runtime data.
- `EndCombat`/`ClearCombat` style methods should clear per-combat runtime data
  and unsubscribe combat-specific subscriptions.

Conceptual setup flow:

```text
AdventureLifetimeScope exists for the Adventure scene
-> VContainer builds reusable combat-related services once
-> CombatService.ReadyCombat(...) receives combat cards
-> CombatService runs setup orchestration for this combat
-> Reusable services clear previous combat data
-> IntentSystem creates IntentRuntimeState for each living monster
-> ActionExecutionBindingStore is created/filled
-> Binding validation logs developer-facing errors
-> Player turn can start
-> Combat ends
-> Reusable services clear per-combat data
```

Lifetime rule:

- Adventure scope can own reusable combat-related service objects.
- Combat runtime data is per-combat and must be reset/cleared between combats.
- A combat-specific scope is still needed for per-combat event subscriptions and
  other disposable runtime bindings.
- The combat scope does not mean every service object must be recreated. It can
  represent a per-combat disposable/subscription lifetime while reusable service
  objects remain owned by the Adventure scope.
- Use the combat child `LifetimeScope` for per-combat context, events, and
  subscriptions. Move additional object types into that child scope, pooling, or
  fresh creation only when reset/reuse is unsafe, unclear, or more expensive to
  maintain.

Initial reuse classification:

Reusable service objects created in the Adventure lifetime:

- `IntentSystem` or equivalent intent subsystem.
- `ActionExecutionBindingStore`.
- `CombatService`.

Conceptual turn flow:

```text
CombatService.NextTurn()
-> CombatService handles ending the current turn phase
-> CombatService selects/starts the next turn phase
-> If the next phase is enemy turn, CombatService executes living enemies in
   screen order
```

Player-turn-start intent trigger:

```text
CombatService.StartPlayerTurn()
-> update player-turn state such as round number if required
-> IntentSystem resolves all living monsters
-> IntentChanged events are emitted
-> UI reveal flow can start
```

The player-turn-start intent calculation trigger belongs to `CombatService`.

`CombatService` owns combat state such as current side, round number, combat
ended flag, and participant lists unless implementation later moves those fields
into a dedicated combat state object.

Per-combat data cleared and rebuilt for each combat:

- `IntentSystem`'s `MonsterRef -> IntentRuntimeState` entries.
- `ActionExecutionBindingStore`'s `MonsterRef -> IntentActionModel ->
  AbilitySpecHandle` entries.
- `CombatService`'s enemy-turn execution queue and in-progress execution state.
- Combat-specific state/effect/event subscriptions.
- Presentation subscriptions that listen to combat/turn logic events for the
  current combat.

This classification is the initial v1 direction. Specific reusable service
objects can be moved into the combat child scope or created fresh later if
reset/reuse becomes unsafe.

Combat event lifetime:

- Turn/combat logic events should be scoped to one combat.
- Subscriptions created for a combat must be disposed when that combat ends or
  before the next combat starts.
- The combat scope should be implemented as a VContainer child `LifetimeScope`.
- The combat child scope owns per-combat context, combat events/subscriptions, and
  other disposable runtime bindings.
- `CombatContext` is a per-combat object owned by the combat child
  `LifetimeScope`.
- `CombatContext` may contain references to reusable objects or static assets,
  such as `IntentOverrideRuleSetModel`, but the context object itself represents one
  combat.
- `CombatContext.PlayerCards` and `CombatContext.EnemyCards` reference the
  participant lists owned by `CombatService`; they are not snapshots in v1.
- `CombatService` remains the authoritative owner of combat participant lists.
- `CombatContext` exposes participant lists as read-only views, such as
  `IReadOnlyList<CombatCard>`.
- Systems that receive `CombatContext` must not mutate participant lists directly.
- `CombatContext` does not contain a `CombatService` reference.
- Objects that need to call `CombatService`, such as `CombatEventBridge`, should
  receive it through VContainer constructor injection instead of through
  `CombatContext`.
- Working name for the per-combat event collection: `CombatEvents`.
- Use `CombatEventBus` only if the implementation becomes a generic
  publish/subscribe bus. For the current design, `CombatEvents` is preferred
  because it represents the current combat's explicit events.
- `CombatEvents` is a per-combat object owned by the combat child
  `LifetimeScope`.
- With a simple C# event container shape, such as
  `public event Action EnemyTurnLogicCompleted`, `CombatEvents` should be
  created fresh for each combat rather than reused across combats.
- Because C# events can only be invoked by the declaring type, `CombatEvents`
  should expose explicit raise methods for logic owners such as `CombatService`.

```csharp
public sealed class CombatEvents
{
    public event Action EnemyTurnLogicCompleted;

    public void RaiseEnemyTurnLogicCompleted()
    {
        EnemyTurnLogicCompleted?.Invoke();
    }
}
```

- Working name for the bridge that subscribes to combat events and calls
  presentation/application callbacks: `CombatEventBridge`.
- `CombatEventBridge` is also a per-combat object owned by the combat child
  `LifetimeScope`.
- It subscribes to the current combat's `CombatEvents`, requests presentation
  work, calls back into `CombatService` when presentation completes, and
  unsubscribes/disposes with the combat scope.
- `CombatEventBridge` should not depend on `IntentSystem` or
  `ActionExecutionBindingStore`.
- It reports presentation completion to `CombatService`; `CombatService` then
  continues turn progression.
- Reusable service instances can remain owned by the Adventure lifetime while
  receiving the current combat context/scope during `ReadyCombat` setup.
- This is especially important for the redesigned enemy-turn-completion flow:

```text
CombatService calls CombatEvents.RaiseEnemyTurnLogicCompleted()
-> CombatEventBridge listens and requests enemy-turn completion presentation
-> Presentation completion calls back into CombatService
-> CombatService advances to the next player turn
-> Combat end disposes the current combat event subscriptions
```

VContainer combat child scope flow:

```text
AdventureLifetimeScope
-> CombatService.ReadyCombat(...)
-> Dispose previous CombatLifetimeScope if one exists
-> Create CombatLifetimeScope child
-> Register per-combat context and event/subscription objects
-> Reusable Adventure-scope services initialize per-combat data from that context
-> Combat ends
-> Dispose CombatLifetimeScope
```

The combat child scope is primarily for per-combat lifetime management. It does
not require all combat-related services to be recreated for each combat.

## Intent Changed Notification

Temporary event payload:

- `MonsterRef`
- `cachedIntentDisplays`

This is not final. It may change when UI ownership and lookup paths are
designed.

Intent clear notification:

- When `ConsumeResolvedAction` clears `cachedResolvedAction`, `cachedIntentDisplays`, and `currentDependencies`, the
  intent system emits a clear notification for that monster.
- The clear notification is emitted at consume time, after the Ability has ended,
  been cancelled, or failed activation. V1 does not clear the intent before
  Ability activation starts.
- Temporary event name: `IntentCleared`.
- Temporary payload:
  - `MonsterRef`
- UI animation and exact removal timing are deferred to UI design.

## State Change During Player Turn

When player actions, items, effects, stacks, buffs, debuffs, or status changes
affect a monster's final action or displayed numeric values:

```text
1. Ability/state system emits a relevant change event.
2. The affected monster is identified.
3. The event is compared against IntentRuntimeState.currentDependencies.
4. If the event matches current dependencies:
   - `ResolveAction` and `BuildDisplayCache` rerun for that monster.
   - IntentRuntimeState.cachedResolvedAction is replaced.
   - IntentRuntimeState.cachedIntentDisplays is replaced.
   - IntentRuntimeState.currentDependencies is replaced.
   - IntentChanged is emitted.
5. If the event does not match current dependencies, no intent refresh occurs.
6. UI decides how to render any change based on its own reveal state.
```

The combat/intent system does not branch on whether the intent is currently
visible.

In v1, player action phase starts only after the reveal flow is complete.
Therefore, when a player-action-phase refresh emits `IntentChanged`, UI can
update the currently visible intent immediately. The unrevealed queued-intent
replacement case is not part of v1.

Enemy-turn intent refresh:

- V1 does not refresh intents during the enemy turn.
- State changes caused by enemy actions are reflected when the next player turn
  starts and all living monsters are resolved again.
- Updating not-yet-executed monsters' intents during the enemy turn is deferred
  to v2.
- Enemy-turn execution uses the `cachedResolvedAction` cached before the enemy turn for each
  monster unless that monster is dead/removed before its execution slot.

## Ability-System Change Events

The Unity-side ability system should use Unreal GAS as a design reference for
change notifications.

Relevant Unreal GAS patterns found in local source:

- `RegisterGameplayTagEvent(...)` for gameplay tag count changes.
- `GetGameplayAttributeValueChangeDelegate(...)` for attribute value changes.
- `OnActiveGameplayEffectAddedDelegateToSelf` for active effect addition.
- `OnAnyGameplayEffectRemovedDelegate()` for active effect removal.
- `OnGameplayEffectStackChangeDelegate(...)` for stack count changes.

Current intent refresh event axes:

- `TagChanged`
- `AttributeChanged`
- `ActiveEffectAddedOrRemoved`
- `EffectStackChanged`

The intent system should not refresh on every gameplay change. It should filter
these events through `IntentRuntimeState.currentDependencies`.

## Status Immunity Boundary

Monster-specific immunity or status applicability belongs to the ability/state
system, not the intent system.

Example:

```text
Player attempts to stun a boss.
Boss ability/state system rejects the stun effect.
State.Stun is not applied.
No TagChanged(State.Stun) event is emitted.
Intent system does not need a boss-specific override-rule exception.
```

The intent system uses the global override rule set and the monster's actually
applied states/effects.

## Player Turn Start Inputs

The player-turn-start calculation currently needs these conceptual inputs:

- `MonsterModel.ActionSequence`.
- `IntentRuntimeState.currentIntentActionIndex`.
- Current status/effect state from the monster's ability/state system.
- `IntentOverrideRule` set.
- Static display data referenced by the final action's `DisplayDefinitions`.
- Runtime numeric calculation context, not yet designed.

## Enemy Turn Execution Flow

The enemy-turn execution viewpoint is currently designed as follows.

```text
1. Enemy turn starts.

2. CombatService iterates living monsters from left to right.

3. For each living monster:
   - Read IntentRuntimeState.cachedResolvedAction.
   - Read cachedResolvedAction.FinalActionModel.
   - Look up the stored runtime AbilitySpecHandle binding for that IntentActionModel.
   - Start that handle with `TryActivateAbility(handle, onEnded)`.
   - Wait until the Ability ends before continuing to the next monster.

4. When the Ability ends:
   - Completion includes whether the Ability was cancelled.
   - Cancellation still counts as action processing for sequence-advance
     purposes.
   - If combat has ended or the monster has been removed, any index change is
     not meaningfully observed.

5. If Ability activation fails before it starts:
   - Treat it as an immediate failed completion.
   - Do not show a special failure intent or message.

6. After completion/failure/cancellation:
   - CombatService reports the result to the intent system.
   - The intent system applies sequence-advance rules and updates
     `currentIntentActionIndex`.
```

Enemy-turn execution must not use `cachedIntentDisplays` as the source of
behavior. `cachedIntentDisplays` is UI display cache only.

Enemy-turn execution also does not call `ResolveAction` again immediately before
running the action. It executes the last `cachedResolvedAction` produced by
player-turn start or player-turn change refresh. If displayed intent and
executed behavior diverge because a required refresh event was missed, that is a
bug in the refresh pipeline.

Sequence advancement boundary:

- `IntentRuntimeState.currentIntentActionIndex` is mutated only by the intent
  system.
- `CombatService` does not directly modify `currentIntentActionIndex`.
- After an action completes, is cancelled, or fails activation,
  `CombatService` calls an intent-system command such as:

```text
ConsumeResolvedAction(MonsterRef)
```

- The intent system reads the cached `ResolvedIntentActionData`, applies base/override
  advance policy, and updates `currentIntentActionIndex`.
- `ConsumeResolvedAction` does not immediately resolve or cache the next intent.
- The next intent is resolved at the next player-turn start.
- After applying sequence advancement, `ConsumeResolvedAction` clears the old
  action/intent cache so the advanced sequence index is not paired with stale
  display or resolved-action data.

### Consumed Action Rule

For v1, `ConsumeResolvedAction(MonsterRef)` has no execution-result argument.
Calling it means the currently cached resolved action is considered processed for
sequence-advance purposes.

Current consume mapping:

```text
Ability started and ended normally -> call ConsumeResolvedAction
Ability started and was cancelled -> call ConsumeResolvedAction
Ability activation failed before start -> call ConsumeResolvedAction
Monster was already removed before execution -> do not call ConsumeResolvedAction
```

Activation failure handling:

- If `cachedResolvedAction` exists and a runtime binding exists, but
  `TryActivateAbility(handle, onEnded)` fails before the Ability starts, log an
  error or warning.
- Do not show a special failure intent or message in v1.
- Call `ConsumeResolvedAction(MonsterRef)` because the action attempt is
  considered processed for sequence-advance purposes.
- Runtime binding lookup failure is not treated as normal activation failure.
  The binding layer must detect and reject missing or duplicate bindings during
  combat setup.

Death/removal handling:

- Monster death or removal should be detected through the AbilityComponent/state
  system and reflected in combat participant state.
- When a monster dies or is removed, its intent/runtime state is removed or made
  irrelevant and the UI intent is removed.
- `CombatService` should filter dead/removed monsters before querying the
  intent system.
- For dead/removed monsters, `CombatService` should not call
  `GetResolvedAction` or `ConsumeResolvedAction`.
- Therefore death/removal is not reported through an execution result in v1; it
  is handled as a participant state change before execution.

When `ConsumeResolvedAction` is called:

- Base action: advance `currentIntentActionIndex`.
- Override action: apply `AppliedOverrideRule.Advance`.
- Do not rebuild `cachedResolvedAction`, `cachedIntentDisplays`, or
  `currentDependencies` for the next action immediately.
- Clear `cachedResolvedAction`, `cachedIntentDisplays`, and
  `currentDependencies` after the consume operation.
- Emit an intent-clear notification for that monster after the cache is cleared.
- If `cachedResolvedAction` is missing when `ConsumeResolvedAction` is called,
  log an error, do not advance `currentIntentActionIndex`, and do not create a
  new cache.

When `ConsumeResolvedAction` is not called:

- Do not advance `currentIntentActionIndex`.
- Do not apply `AppliedOverrideRule.Advance`.

### GAS Execution Boundary

`FinalActionModel.ExecutionAbility` is an execution Ability reference used
during combat setup to identify which Ability should be granted and bound to this
action.

Ability grant timing:

- Enemy action Abilities are granted during combat setup/preparation.
- Enemy-turn execution does not give a new Ability.
- Combat setup stores a runtime binding from action data to the granted
  AbilitySpecHandle.
- Enemy-turn execution uses the stored binding and activates that handle.

Runtime execution binding:

```text
ActionExecutionBindingData
- IntentActionModel
- AbilitySpecHandle
```

Binding ownership:

- Runtime execution bindings are owned by the combat execution layer, not by the
  generic AbilitySystem.
- Runtime execution bindings are also not owned by the intent system.
- The intent system decides and caches the final action.
- `CombatService` uses that final action to look up the bound
  AbilitySpecHandle.
- Bindings are created and filled during combat setup/preparation, when monster
  action Abilities are granted.
- `CombatService` consumes existing bindings; it does not create them lazily
  during enemy-turn execution.
- The AbilitySystem gives Abilities and activates AbilitySpecHandles, but it does
  not need to understand monster `IntentActionModel`.
- Conceptually, the binding store can be represented as:

```text
ActionExecutionBindingStore
- MonsterRef
  - IntentActionModel -> AbilitySpecHandle
```

Binding key rule:

- `AbilitySpecHandle` is interpreted in the context of that monster's
  AbilitySystem.
- The same shared `IntentActionModel` may be used by multiple monsters.
- Therefore the binding store is grouped by `MonsterRef`; each monster has
  its own `IntentActionModel -> AbilitySpecHandle` mappings.
- Even if two monsters have handles with the same apparent value, they are used
  against different AbilitySystem instances.
- If the same `IntentActionModel` appears multiple times in one monster's authored
  action sequence, combat setup gives the execution Ability once for that
  monster and reuses the same `AbilitySpecHandle` binding for every occurrence.
- If different `IntentActionModel` entries point to the same execution Ability for
  the same monster, combat setup also gives that Ability once and allows multiple
  `IntentActionModel` bindings to share the same `AbilitySpecHandle`.
- For now, the shared execution Ability does not need to know which
  `IntentActionModel` caused activation. Different actions that share the same
  execution Ability are treated as the same execution behavior.
- Therefore, if two actions need different execution results, such as different
  damage, effects, target handling, or animation behavior, they should use
  different execution Abilities in the current design.

Conceptual setup flow:

```text
For each executable action candidate:
1. Read IntentActionModel.ExecutionAbility as the execution Ability reference.
2. Check the per-monster setup cache:
   - ExecutionAbility -> AbilitySpecHandle.
3. If the execution Ability is not already granted for this monster, give it to
   the monster AbilitySystem and store the returned AbilitySpecHandle in the
   setup cache.
4. If the execution Ability is already granted for this monster, reuse the cached
   AbilitySpecHandle.
5. Store IntentActionModel -> AbilitySpecHandle in runtime execution bindings.
```

Executable action candidates include:

- Actions in the monster's authored base action sequence.
- Replacement actions referenced by the active combat `IntentOverrideRuleSetModel`.

Replacement actions may not appear in the monster's base sequence, but they can
become the final action through status override resolution. They must therefore
be granted and bound during combat setup.

For v1, combat setup binds all replacement actions in the active
`IntentOverrideRuleSetModel` for each monster, even if some of those statuses may
never apply to that monster due to immunity or state-application rules. Setup
does not analyze monster-specific status applicability.

Conceptual enemy-turn execution flow:

```text
1. Read cachedResolvedAction.FinalActionModel.
2. Look up the stored AbilitySpecHandle for that IntentActionModel.
3. Start that handle with TryActivateAbility(handle, onEnded).
```

Binding lookup policy:

- `0` matching runtime bindings: setup invariant violation.
- `1` matching runtime binding: activate that handle.
- `2+` matching runtime bindings for the same action: setup invariant violation.

The intent system does not own Ability grant internals, but the combat setup or
grant/data layer must ensure each executable `IntentActionModel` has exactly one
runtime AbilitySpecHandle binding per monster at runtime.

Current validation policy:

- Combat setup must verify that every executable action candidate can produce
  exactly one runtime binding for that monster.
- If a binding cannot be created or duplicates are produced, combat setup should
  log an error to notify the developer before enemy-turn execution can reach that
  action.
- Binding setup error logs should include at least:
  - Monster reference or monster id.
  - `IntentActionModel` reference or `IntentActionModel.Id / EIntentAction`.
  - `ExecutionAbility`.
  - Failure reason, such as Ability give failure, missing handle, or duplicate
    binding.
- Enemy-turn execution may still keep a defensive binding check, but reaching
  `0` or `2+` bindings at enemy-turn time means the setup invariant was broken,
  not that the action normally failed to activate.

Execution responsibility split:

```text
Intent/execution flow:
- Chooses which final action should run.
- Looks up the final action's stored runtime AbilitySpecHandle binding.
- Starts the bound AbilitySpec through the monster AbilitySystem.
- Waits for completion.
- Applies sequence advancement.

GAS Ability:
- Performs the actual behavior.
- Applies gameplay effects.
- Changes attributes/tags/effects.
- Runs animation/timeline tasks as needed.
- Ends itself when complete or cancelled.
```

The design follows Unreal GAS's pattern where Ability activation can receive a
one-shot end delegate and ASC also exposes broader ability-ended callbacks.

Current activation contract:

```text
TryActivateAbility(handle, onEnded)

Start success:
- Returns true.
- Later calls onEnded with cancellation information.

Start failure:
- Returns false.
- Does not call onEnded.
- CombatService treats this as immediate failed completion.
```

### Sequential Enemy Execution

Enemy actions are sequential, not parallel.

```text
Monster A runs -> completes/cancels/fails
Monster B runs -> completes/cancels/fails
Monster C runs -> completes/cancels/fails
```

If a monster is already dead or removed when its turn in the sequence is
reached, it is skipped and its action index does not advance.

If an earlier monster's action kills a later monster, the later monster is
skipped when reached and its action index does not advance.

## Deferred

The following are intentionally not final:

- Whether `IntentSystem.ResolveAction` and `IntentSystem.BuildDisplayCache`
  should later move into dedicated classes such as `IntentActionResolver` and
  `IntentDisplayCalculator`.
- Concrete field names for still-provisional dependency and event payload
  structures.
- Whether `IntentActionModel` ultimately keeps execution and display data together.
- Numeric calculation ownership and method signatures.
- UI data lookup path.
- UI-facing display query return shape.
- UI behavior if `IntentChanged` arrives for a monster whose visible intent was
  already cleared.
- Low-level event implementation for ability/state change notifications.
- Sequence advancement implementation details.
- Dependency match granularity.
- Concrete dependency data structures.
- Exact Ability activation API shape.
- Exact Ability completion callback type.

