# Adventure Model Redesign Plan

## Current Direction

Adventure scene start data is moving from legacy service/table lookup to explicit scene preload data and scene-scoped runtime state.

The runtime path assumes required Adventure data exists. Do not add runtime fallback for missing entry presentation, encounter sequence, or selected character data. Missing data should fail during development so the asset configuration problem is visible.

## Completed Model Decisions

- `AdventureRegionModel` owns the static region definition.
- `AdventureEntryPresentationModel` owns first-screen presentation data.
- `AdventureStageDefinition` uses array order as stage order. It does not store a duplicated stage number.
- `AdventureEncounterDeckDefinition` owns start entries, shuffled entries, and boss entry directly.
- `CardDeckModel`, `CardDeckTable`, and `ECardDeck` are removed from the Adventure scene entry path.

## Stage Draw Rule

`AdventureStageDefinition.DrawCount` and `AdventureRegionModel.StartDrawCount` describe how many encounter/choice cards are drawn into the right board side.

They do not include the player card.

Reason:

- the player card is placed once on the left board side by `AdventureStageFlow`
- encounter candidates and choice cards live on the right board side
- subtracting one from `DrawCount` makes the model field mean "total board cards including player", which conflicts with the current board ownership model

Current rule:

```text
Choice stage
-> draw `DrawCount` right-side offer cards

No explicit stage definition
-> draw `StartDrawCount` right-side offer cards

ImmediateEncounter stage
-> draw exactly 1 right-side encounter card
```

`ImmediateEncounter` is forced to one card because there is no player choice UI in that mode. Drawing multiple right-side cards and then immediately committing only the first would create invisible discarded offers and make the opening board misleading.

## Pending Content Work

### Localization Table Work

Current implementation uses the existing `AdventureView` String Table for Adventure entry presentation text.

Current keys:

- `entry_title`
- `entry_subtitle`

These keys are assigned to `Adventure_Default.asset`:

- `AdventureEntryPresentationModel.Title`
- `AdventureEntryPresentationModel.Subtitle`

This is content work, not runtime fallback. The runtime code should continue to assume these localized strings are configured.

Future split option:

- create a dedicated Adventure region/entry Localization Table if entry presentation text grows beyond view-owned banner text
- migrate `Adventure_Default.asset` from `AdventureView` keys to the dedicated content table

### Entry Visual Assets

`AdventureEntryPresentationModel.Background` and `AdventureEntryPresentationModel.Emblem` are direct Sprite references, but the current Banner UI does not consume them yet.

Future work:

- decide which UI element owns the background and emblem
- assign sprite references to the Adventure asset
- apply those sprite references in the presentation layer
