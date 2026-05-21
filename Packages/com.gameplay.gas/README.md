# Gameplay GAS

Gameplay Ability System inspired package for Unity.

This package is intentionally independent from the game project. Game-specific
types such as character models, monsters, cards, views, and services must live
outside this package and adapt into the GAS runtime.

## Structure

```text
Runtime
├─ Core
│  ├─ Actors
│  ├─ Abilities
│  ├─ Attributes
│  ├─ Effects
│  ├─ Tags
│  ├─ Events
│  ├─ Tasks
│  ├─ Specs
│  ├─ Contexts
│  └─ Utilities
└─ Authoring
   ├─ Abilities
   ├─ Effects
   └─ Tags

Editor
├─ Validation
└─ Generation

Tests
├─ Runtime
└─ Editor
```

## Layer Rules

- `Runtime/Core` contains pure runtime GAS logic.
- `Runtime/Authoring` contains Unity authoring assets such as ScriptableObject definitions.
- `Editor` contains validation and generation tools only.
- Game project code may reference this package.
- This package must not reference game project code.
