using Game.Generated;

namespace Domains.Adventure
{
    public sealed class AdventureSession
    {
        public AdventureSession(ECharacter character)
        {
            SelectedCharacterId = character;
        }

        public ECharacter SelectedCharacterId { get; }
    }
}
