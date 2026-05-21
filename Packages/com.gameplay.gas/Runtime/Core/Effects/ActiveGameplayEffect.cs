namespace Gameplay.GAS
{
    public sealed class ActiveGameplayEffect
    {
        public ActiveGameplayEffect(
            ActiveGameplayEffectHandle handle,
            GameplayEffectSpec spec,
            GameplayEffectContext context)
        {
            Handle = handle;
            Spec = spec;
            Context = context;
        }

        public ActiveGameplayEffectHandle Handle { get; }
        public GameplayEffectSpec Spec { get; }
        public GameplayEffectContext Context { get; }
        public int StackCount { get; private set; } = 1;
        public float ElapsedSeconds { get; private set; }
        public float PeriodElapsedSeconds { get; private set; }

        public bool IsExpired =>
            Spec.Effect.DurationPolicy == GameplayEffectDurationPolicy.Duration &&
            ElapsedSeconds >= Spec.Effect.DurationSeconds;

        public int Tick(float deltaSeconds)
        {
            if (deltaSeconds <= 0f)
                return 0;

            float periodDeltaSeconds = GetPeriodDeltaSeconds(deltaSeconds);
            ElapsedSeconds += deltaSeconds;
            return TickPeriod(periodDeltaSeconds);
        }

        public bool TryAddStack()
        {
            int stackLimitCount = Spec.Effect.StackLimitCount;
            if (stackLimitCount > 0 && StackCount >= stackLimitCount)
                return false;

            StackCount++;
            return true;
        }

        public void RemoveStack()
        {
            if (StackCount > 1)
                StackCount--;
        }

        public void ResetDuration()
        {
            ElapsedSeconds = 0f;
        }

        public void ResetPeriod()
        {
            PeriodElapsedSeconds = 0f;
        }

        private float GetPeriodDeltaSeconds(float deltaSeconds)
        {
            if (Spec.Effect.DurationPolicy != GameplayEffectDurationPolicy.Duration)
                return deltaSeconds;

            float remainingSeconds = Spec.Effect.DurationSeconds - ElapsedSeconds;
            if (remainingSeconds <= 0f)
                return 0f;

            return deltaSeconds < remainingSeconds ? deltaSeconds : remainingSeconds;
        }

        private int TickPeriod(float deltaSeconds)
        {
            if (!Spec.Effect.IsPeriodic)
                return 0;

            PeriodElapsedSeconds += deltaSeconds;

            int tickCount = 0;
            while (PeriodElapsedSeconds >= Spec.Effect.PeriodSeconds)
            {
                PeriodElapsedSeconds -= Spec.Effect.PeriodSeconds;
                tickCount++;
            }

            return tickCount;
        }
    }
}
