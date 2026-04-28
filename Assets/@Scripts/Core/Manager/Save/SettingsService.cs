namespace Game.Core.Managers.Save
{
    public sealed class SettingsService : SaveService<SettingsState, SettingsSave>
    {
        public SettingsService() : base("settings.json")
        {
        }

        protected override SettingsState FromSave(SettingsSave save)
        {
            return SettingsState.FromSave(save);
        }
    }
}
