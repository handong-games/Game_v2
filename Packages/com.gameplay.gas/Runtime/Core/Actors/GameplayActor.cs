namespace Gameplay.GAS
{
    public class GameplayActor
    {
        public GameplayActor()
        {
            AbilitySystem = new AbilitySystemComponent(this);
        }

        public AbilitySystemComponent AbilitySystem { get; }
    }
}
