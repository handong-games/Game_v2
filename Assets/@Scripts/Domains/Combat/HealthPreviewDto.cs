namespace Domains.Combat
{
    public readonly struct HealthPreviewDto
    {
        public HealthPreviewDto(bool isEnabled, int currentHealth, int previewHealth, int amount)
        {
            IsEnabled = isEnabled;
            CurrentHealth = currentHealth;
            PreviewHealth = previewHealth;
            Amount = amount;
        }

        public bool IsEnabled { get; }
        public int CurrentHealth { get; }
        public int PreviewHealth { get; }
        public int Amount { get; }
    }
}
