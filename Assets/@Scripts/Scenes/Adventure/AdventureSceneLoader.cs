using System;
using Domains.Adventure;
using Game.Core.Managers.DB;
using Game.Core.SceneLoading;
using Game.Core.Utility;
using Game.Data;
using Game.Generated;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Game.Scenes.Adventure
{
    // Role:
    // Preloads AdventureScene startup models from DBManager tables before scene activation.
    // It owns the payload only until AdventureSceneScope consumes it.
    public sealed class AdventureSceneLoader : IScenePreloader
    {
        private readonly AdventureStartState _startState;
        private readonly DBManager _dbManager;
        private AdventureScenePayload _payload;

        public AdventureSceneLoader(
            AdventureStartState startState,
            DBManager dbManager)
        {
            _startState = startState;
            _dbManager = dbManager;
        }

        public GameSceneId SceneId => GameSceneId.Adventure;

        public async Awaitable Preload()
        {
            _payload = null;
            AdventureSceneAssetHandles assetHandles = new();

            try
            {
                ECharacter selectedCharacterId = _startState.SelectedCharacterId;

                AsyncOperationHandle<AdventureRegionModel> adventureHandle =
                    _dbManager.Adventure.LoadAsync(EAdventure.Default);
                AsyncOperationHandle<CharacterModel> characterHandle =
                    _dbManager.Character.LoadAsync(selectedCharacterId);

                assetHandles.Add(adventureHandle);
                assetHandles.Add(characterHandle);

                AdventureRegionModel adventure = await WaitForAddressableLoad(adventureHandle);
                CharacterModel character = await WaitForAddressableLoad(characterHandle);

                uint seed = RandomUtility.CreateSeed();

                AdventureSceneInitialData initialData = new(
                    selectedCharacterId,
                    character,
                    adventure,
                    seed);

                _payload = new AdventureScenePayload(
                    initialData,
                    assetHandles);
            }
            catch
            {
                assetHandles.Dispose();
                _payload = null;
                throw;
            }
        }

        public AdventureScenePayload Consume()
        {
            // TODO: Add an editor-only fallback if direct AdventureScene play becomes necessary.
            if (_payload == null)
                throw new InvalidOperationException("AdventureScenePayload does not exist. AdventureScene must be loaded through SceneManagerEx.");

            AdventureScenePayload payload = _payload;
            _payload = null;
            return payload;
        }

        private static async Awaitable<T> WaitForAddressableLoad<T>(AsyncOperationHandle<T> handle)
        {
            while (!handle.IsDone)
            {
                await Awaitable.NextFrameAsync();
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw handle.OperationException ?? new InvalidOperationException($"Failed to load {typeof(T).Name}.");

            return handle.Result;
        }
    }
}
