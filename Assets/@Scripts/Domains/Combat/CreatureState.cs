using System;

namespace Domains.Combat
{
    public sealed class CreatureState
    {
        public int Id { get; }
        public ECombatSide Side { get; }
        public ECreatureKind Kind { get; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; }

        public event Action<int, int> HealthChanged;
        public event Action<HealthPreviewDto> HealthPreviewChanged;

        public bool IsPlayer => Kind == ECreatureKind.Player;
        public bool IsMonster => Kind == ECreatureKind.Monster;
        public bool IsAlive => CurrentHealth > 0;
        public bool IsDead => !IsAlive;
        
        private static int _nextId;
        
        public CreatureState(ECombatSide side, ECreatureKind kind, int maxHealth)
        {
            Id = _nextId++;
            Side = side;
            Kind = kind;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public static void ResetId()
        {
            _nextId = 0;
        }

        public void ApplyDamage(int amount)
        {
            SetHealth(CurrentHealth - amount);
        }

        public void ApplyHeal(int amount)
        {
            SetHealth(CurrentHealth + amount);
        }

        public void ShowHealthPreview(int previewHealth, int amount)
        {
            HealthPreviewChanged?.Invoke(new HealthPreviewDto(
                true,
                CurrentHealth,
                Math.Clamp(previewHealth, 0, MaxHealth),
                Math.Max(0, amount)));
        }

        public void HideHealthPreview()
        {
            HealthPreviewChanged?.Invoke(new HealthPreviewDto(false, CurrentHealth, CurrentHealth, 0));
        }

        private void SetHealth(int currentHealth)
        {
            int nextHealth = Math.Clamp(currentHealth, 0, MaxHealth);
            if (CurrentHealth == nextHealth)
                return;

            CurrentHealth = nextHealth;
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }
    }
}
