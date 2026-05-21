using System.Collections.Generic;

namespace Gameplay.GAS
{
    public class GameplayEffect
    {
        private readonly List<GameplayModifier> _modifiers = new();
        private readonly List<GameplayEffectExecution> _executions = new();
        private readonly List<GameplayEffectCue> _gameplayCues = new();

        public IReadOnlyList<GameplayModifier> Modifiers => _modifiers;
        public IReadOnlyList<GameplayEffectExecution> Executions => _executions;
        public IReadOnlyList<GameplayEffectCue> GameplayCues => _gameplayCues;
        public GameplayTagContainer GrantedTags { get; } = new();
        public GameplayTagRequirements ApplicationTagRequirements { get; } = new();
        public GameplayEffectDurationPolicy DurationPolicy { get; set; } = GameplayEffectDurationPolicy.Instant;
        public float DurationSeconds { get; set; }
        public float PeriodSeconds { get; set; }
        public bool ExecutePeriodicEffectOnApplication { get; set; }
        public GameplayEffectStackingType StackingType { get; set; } = GameplayEffectStackingType.None;
        public int StackLimitCount { get; set; }
        public GameplayEffectStackingDurationPolicy StackDurationRefreshPolicy { get; set; } =
            GameplayEffectStackingDurationPolicy.RefreshOnSuccessfulApplication;
        public GameplayEffectStackingPeriodPolicy StackPeriodResetPolicy { get; set; } =
            GameplayEffectStackingPeriodPolicy.ResetOnSuccessfulApplication;
        public GameplayEffectStackingExpirationPolicy StackExpirationPolicy { get; set; } =
            GameplayEffectStackingExpirationPolicy.ClearEntireStack;

        public bool IsPeriodic => PeriodSeconds > 0f;

        public void AddModifier(GameplayModifier modifier)
        {
            if (modifier.Attribute.IsValid)
                _modifiers.Add(modifier);
        }

        public void AddExecution(GameplayEffectExecution execution)
        {
            if (execution != null)
                _executions.Add(execution);
        }

        public void AddGameplayCue(GameplayEffectCue cue)
        {
            if (cue != null)
                _gameplayCues.Add(cue);
        }
    }
}
