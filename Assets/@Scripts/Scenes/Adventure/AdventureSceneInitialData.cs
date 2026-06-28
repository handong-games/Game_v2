using Game.Data;
using Game.Generated;

namespace Game.Scenes.Adventure
{
    // Role:
    // Holds immutable startup data required to initialize Adventure runtime state.
    // It does not own Addressables handles; AdventureSceneAssetHandles owns them.
    public sealed class AdventureSceneInitialData
    {
        public AdventureSceneInitialData(
            ECharacter selectedCharacterId,
            CharacterModel character,
            AdventureRegionModel adventure,
            uint seed)
        {
            SelectedCharacterId = selectedCharacterId;
            Character = character;
            Adventure = adventure;
            Seed = seed;
        }

        public ECharacter SelectedCharacterId { get; }
        public CharacterModel Character { get; }
        public AdventureRegionModel Adventure { get; }
        public uint Seed { get; }
    }
}
