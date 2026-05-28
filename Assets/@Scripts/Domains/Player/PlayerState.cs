using Game.Data;

namespace Domains.Player
{
    public sealed class PlayerState
    {
        public PlayerState(CharacterModel character)
        {
            Character = character;
        }

        public CharacterModel Character { get; }
    }
}
