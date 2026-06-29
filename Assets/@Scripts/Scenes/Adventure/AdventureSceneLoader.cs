using System;
using System.Collections.Generic;
using Domains.Adventure;
using Domains.View.Widgets;
using Game.Core.Managers.DB;
using Game.Core.Managers.View;
using Game.Core.SceneLoading;
using Game.Core.Utility;
using Game.Data;
using Game.Generated;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Preloads AdventureScene startup models from DBManager tables before scene activation.
    // It owns the payload only until AdventureSceneScope consumes it.
    public sealed class AdventureSceneLoader : IScenePreloader, IDisposable
    {
        private readonly AdventureStartState _startState;
        private readonly DBManager _dbManager;
        private readonly ViewManager _viewManager;
        private AdventureScenePayload _payload;

        public AdventureSceneLoader(
            AdventureStartState startState,
            DBManager dbManager,
            ViewManager viewManager)
        {
            _startState = startState ?? throw new ArgumentNullException(nameof(startState));
            _dbManager = dbManager ?? throw new ArgumentNullException(nameof(dbManager));
            _viewManager = viewManager ?? throw new ArgumentNullException(nameof(viewManager));
        }

        public GameSceneId SceneId => GameSceneId.Adventure;

        public async Awaitable Preload()
        {
            ReleasePendingPayload();
            AdventureSceneAssetHandles assetHandles = new();

            try
            {
                ECharacter selectedCharacterId = _startState.SelectedCharacterId;

                AsyncOperationHandle<AdventureRegionModel> adventureHandle =
                    _dbManager.Adventure.LoadAsync(EAdventure.Default);
                AsyncOperationHandle<CharacterModel> characterHandle =
                    _dbManager.Character.LoadAsync(selectedCharacterId);
                Dictionary<EChoiceCardType, AsyncOperationHandle<AdventureChoiceCardUIModel>> choiceCardUIHandles =
                    LoadChoiceCardUIModels(assetHandles);
                AsyncOperationHandle<VisualTreeAsset> choiceCardTemplateHandle =
                    Addressables.LoadAssetAsync<VisualTreeAsset>(AdventureSceneAddressables.AdventureChoiceCardWidget);
                AsyncOperationHandle<VisualTreeAsset> playerCardTemplateHandle =
                    Addressables.LoadAssetAsync<VisualTreeAsset>(AdventureSceneAddressables.AdventurePlayerCardWidget);
                AsyncOperationHandle<VisualTreeAsset> monsterCardTemplateHandle =
                    Addressables.LoadAssetAsync<VisualTreeAsset>(AdventureSceneAddressables.AdventureMonsterCardWidget);
                AsyncOperationHandle<VisualTreeAsset> displayCardTemplateHandle =
                    Addressables.LoadAssetAsync<VisualTreeAsset>(AdventureSceneAddressables.AdventureDisplayCardWidget);
                AsyncOperationHandle<VisualTreeAsset> portraitFaceTemplateHandle =
                    Addressables.LoadAssetAsync<VisualTreeAsset>(AdventureSceneAddressables.PortraitCardFaceWidget);
                AsyncOperationHandle<VisualTreeAsset> lockedFaceTemplateHandle =
                    Addressables.LoadAssetAsync<VisualTreeAsset>(AdventureSceneAddressables.LockedCardFaceWidget);
                AsyncOperationHandle<VisualTreeAsset> skillSlotTemplateHandle =
                    Addressables.LoadAssetAsync<VisualTreeAsset>(AdventureSceneAddressables.SkillSlotWidget);
                Awaitable adventureViewTemplatePreload =
                    _viewManager.PreloadViewTemplate(typeof(AdventureView));

                assetHandles.Add(adventureHandle);
                assetHandles.Add(characterHandle);
                assetHandles.Add(choiceCardTemplateHandle);
                assetHandles.Add(playerCardTemplateHandle);
                assetHandles.Add(monsterCardTemplateHandle);
                assetHandles.Add(displayCardTemplateHandle);
                assetHandles.Add(portraitFaceTemplateHandle);
                assetHandles.Add(lockedFaceTemplateHandle);
                assetHandles.Add(skillSlotTemplateHandle);

                AdventureRegionModel adventure = await WaitForAddressableLoad(adventureHandle);
                CharacterModel character = await WaitForAddressableLoad(characterHandle);
                AdventureChoiceCardUIModels choiceCardUIModels =
                    await WaitForChoiceCardUIModels(choiceCardUIHandles);
                VisualTreeAsset choiceCardTemplate = await WaitForAddressableLoad(choiceCardTemplateHandle);
                VisualTreeAsset playerCardTemplate = await WaitForAddressableLoad(playerCardTemplateHandle);
                VisualTreeAsset monsterCardTemplate = await WaitForAddressableLoad(monsterCardTemplateHandle);
                VisualTreeAsset displayCardTemplate = await WaitForAddressableLoad(displayCardTemplateHandle);
                VisualTreeAsset portraitFaceTemplate = await WaitForAddressableLoad(portraitFaceTemplateHandle);
                VisualTreeAsset lockedFaceTemplate = await WaitForAddressableLoad(lockedFaceTemplateHandle);
                VisualTreeAsset skillSlotTemplate = await WaitForAddressableLoad(skillSlotTemplateHandle);
                await adventureViewTemplatePreload;

                uint seed = RandomUtility.CreateSeed();

                AdventureSceneInitialData initialData = new(
                    selectedCharacterId,
                    character,
                    adventure,
                    seed);
                CardFaceWidgetTemplates faceWidgetTemplates = new(
                    portraitFaceTemplate,
                    lockedFaceTemplate);
                AdventureCardWidgetTemplates cardWidgetTemplates = new(
                    choiceCardTemplate,
                    playerCardTemplate,
                    monsterCardTemplate,
                    displayCardTemplate,
                    faceWidgetTemplates);
                AdventureScreenWidgetTemplates screenWidgetTemplates = new(
                    skillSlotTemplate);

                _payload = new AdventureScenePayload(
                    initialData,
                    choiceCardUIModels,
                    cardWidgetTemplates,
                    screenWidgetTemplates,
                    assetHandles);
            }
            catch
            {
                assetHandles.Dispose();
                _payload = null;
                throw;
            }
        }

        private void ReleasePendingPayload()
        {
            _payload?.AssetHandles?.Dispose();
            _payload = null;
        }

        public void Dispose()
        {
            ReleasePendingPayload();
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

        private Dictionary<EChoiceCardType, AsyncOperationHandle<AdventureChoiceCardUIModel>> LoadChoiceCardUIModels(
            AdventureSceneAssetHandles assetHandles)
        {
            if (_dbManager.ChoiceCardUI == null)
                throw new InvalidOperationException("AdventureChoiceCardUITable is not loaded.");

            Dictionary<EChoiceCardType, AsyncOperationHandle<AdventureChoiceCardUIModel>> handles = new();
            foreach (EChoiceCardType choiceType in Enum.GetValues(typeof(EChoiceCardType)))
            {
                AsyncOperationHandle<AdventureChoiceCardUIModel> handle =
                    _dbManager.ChoiceCardUI.LoadAsync(choiceType);

                assetHandles.Add(handle);
                handles.Add(choiceType, handle);
            }

            return handles;
        }

        private static async Awaitable<AdventureChoiceCardUIModels> WaitForChoiceCardUIModels(
            IReadOnlyDictionary<EChoiceCardType, AsyncOperationHandle<AdventureChoiceCardUIModel>> handles)
        {
            Dictionary<EChoiceCardType, AdventureChoiceCardUIModel> models = new();
            foreach ((EChoiceCardType choiceType, AsyncOperationHandle<AdventureChoiceCardUIModel> handle) in handles)
            {
                AdventureChoiceCardUIModel model = await WaitForAddressableLoad(handle);
                if (model == null)
                    throw new InvalidOperationException($"{nameof(AdventureChoiceCardUIModel)} is missing: {choiceType}");

                models.Add(choiceType, model);
            }

            return new AdventureChoiceCardUIModels(models);
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
