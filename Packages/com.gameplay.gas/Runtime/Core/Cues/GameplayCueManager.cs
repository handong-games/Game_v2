namespace Gameplay.GAS
{
    public sealed class GameplayCueManager
    {
        public static GameplayCueManager Instance { get; } = new();

        private GameplayCueSet _runtimeCueSet;

        public void Initialize(GameplayCueSet runtimeCueSet)
        {
            _runtimeCueSet = runtimeCueSet;
            _runtimeCueSet?.BuildAccelerationMap();
        }

        public void HandleGameplayCue(GameplayCueEventData cueEventData)
        {
            _runtimeCueSet?.HandleGameplayCue(
                cueEventData.Target,
                cueEventData.CueTag,
                cueEventData.EventType,
                cueEventData.Parameters);
        }
    }
}
