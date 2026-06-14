using Gameplay.GAS;

namespace Game.Messages
{
    public readonly struct GameplayDeathMessage
    {
        public GameplayDeathMessage(object avatar)
        {
            Avatar = avatar;
        }

        public object Avatar { get; }
    }
}
