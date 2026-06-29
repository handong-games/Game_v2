using System;
using Domains.CharacterSelect;
using Domains.Settings.View;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
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
        private bool _disposed;

        public async Awaitable Preload()
        {
            if (_isPreloaded)
                return;

            AsyncOperationHandle handle =
                LocalizationSettings.StringDatabase.PreloadTables(Tables);

            await WaitForAddressableLoad(handle);

            if (_disposed)
            {
                ReleaseTables();
                return;
            }

            _isPreloaded = true;
        }

        public void Dispose()
        {
            _disposed = true;
            if (!_isPreloaded)
                return;

            ReleaseTables();
            _isPreloaded = false;
        }

        private static async Awaitable WaitForAddressableLoad(AsyncOperationHandle handle)
        {
            while (!handle.IsDone)
            {
                await Awaitable.NextFrameAsync();
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw handle.OperationException ?? new InvalidOperationException("Failed to preload TitleScene localization tables.");
        }

        private static void ReleaseTables()
        {
            for (int i = 0; i < Tables.Length; i++)
            {
                LocalizationSettings.StringDatabase.ReleaseTable(Tables[i]);
            }
        }
    }
}
