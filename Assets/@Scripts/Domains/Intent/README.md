# Intent Domain Structure

This package is the target home for the intent system after it is separated from the combat implementation details.

## Package Layout

- `Data`: authored ScriptableObject models and serializable definitions.
- `Runtime`: scene/combat runtime state and caches.
- `Flow`: use-case orchestration such as prepare, refresh, consume, and clear.
- `Execution`: runtime bindings between resolved intent actions and executable abilities.
- `Presentation`: projection from intent runtime data to UI-facing view models.
- `Validation`: editor/build-time data validation rules.

## Boundary

`Domains.Intent` owns intent decision, cache, display data, and execution binding concepts.

It should not own:

- Adventure scene progression
- card board placement
- Unity UI widgets
- concrete animation timing
- raw gameplay effect execution

Those responsibilities remain in Adventure flow, View, or AbilitySystem layers.
