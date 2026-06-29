using System;
using Domains.Adventure;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scenes.Adventure
{
    // Role:
    // Preloads and releases localization tables needed by one AdventureScene scope.
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
        private bool _disposed;

        public async Awaitable Preload()
        {
            if (_disposed || _isPreloaded)
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
                throw handle.OperationException ?? new InvalidOperationException("Failed to preload AdventureScene localization tables.");
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
