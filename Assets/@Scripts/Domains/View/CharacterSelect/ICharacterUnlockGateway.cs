using Game.Generated;

namespace Domains.CharacterSelect
{
    public interface ICharacterUnlockGateway
    {
        bool IsUnlocked(ECharacter characterId);
    }
}
