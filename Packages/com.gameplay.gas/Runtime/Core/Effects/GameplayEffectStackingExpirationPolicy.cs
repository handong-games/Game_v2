namespace Gameplay.GAS
{
    public enum GameplayEffectStackingExpirationPolicy
    {
        ClearEntireStack,
        RemoveSingleStackAndRefreshDuration,
        RefreshDuration
    }
}
