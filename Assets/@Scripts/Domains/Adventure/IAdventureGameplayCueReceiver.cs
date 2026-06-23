namespace Domains.Adventure
{
    public interface IAdventureGameplayCueReceiver
    {
        void HandleCoinFlipCue(CoinFlipCueData data);
        void HandleCoinChangeCue(CoinChangeCueData data);
        void HandleDamageCue(DamageCueData data);
    }
}
