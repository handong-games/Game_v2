using System;
using Domains.Adventure;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace Domains.Scene.Adventure
{
    public sealed class AdventureSceneLocalization : IDisposable
    {
        private static readonly TableReference[] Tables =
        {
            nameof(AdventureView),
            "CharacterNames",
            "MonsterNames",
            "SkillNames",
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
