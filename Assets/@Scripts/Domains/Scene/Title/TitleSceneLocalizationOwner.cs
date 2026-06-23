using System;
using Domains.CharacterSelect;
using Domains.Settings.View;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using Views.TitleView;

namespace Domains.Scene.Title
{
    public sealed class TitleSceneLocalizationOwner : IDisposable
    {
        private const string CharacterNamesTable = "CharacterNames";

        private static readonly TableReference[] Tables =
        {
            nameof(TitleView),
            nameof(CharacterSelectView),
            nameof(SettingsView),
            CharacterNamesTable,
        };

        private bool _isPreloaded;

        public void Preload()
        {
            if (_isPreloaded)
                return;

            LocalizationSettings.StringDatabase
                .PreloadTables(Tables)
                .WaitForCompletion();

            _isPreloaded = true;
        }

        public void Dispose()
        {
            if (!_isPreloaded)
                return;

            for (int i = 0; i < Tables.Length; i++)
            {
                LocalizationSettings.StringDatabase.ReleaseTable(Tables[i]);
            }

            _isPreloaded = false;
        }
    }
}
