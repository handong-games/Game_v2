namespace Domains.Adventure
{
    // Role:
    // Receives coin flip presentation cues from an Adventure card AbilitySystem avatar.
    public interface IAdventureCoinFlipCueReceiver
    {
        void HandleCoinFlipCue(CoinFlipCueData data);
    }

    // Role:
    // Receives coin count presentation cues from an Adventure card AbilitySystem avatar.
    public interface IAdventureCoinChangeCueReceiver
    {
        void HandleCoinChangeCue(CoinChangeCueData data);
    }

    // Role:
    // Receives damage presentation cues from an Adventure card AbilitySystem avatar.
    public interface IAdventureDamageCueReceiver
    {
        void HandleDamageCue(DamageCueData data);
    }
}
