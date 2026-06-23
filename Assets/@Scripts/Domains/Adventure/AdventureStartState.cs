using Game.Generated;

namespace Domains.Adventure
{
    public sealed class AdventureStartState
    {
        // Scene-transition state for TitleScene -> AdventureScene.
        // CharacterSelectController must set this before loading AdventureScene.
        // This type intentionally does not track an "unset" state; direct AdventureScene entry uses default(ECharacter).
        public ECharacter SelectedCharacterId { get; set; }
    }
}
