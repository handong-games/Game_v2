using System;
using System.Collections.Generic;
using Domains.Adventure;
using Domains.Character;
using Domains.Scene.Title;
using Game.Core.Adapters;
using Game.Core.SceneLoading;
using Game.AbilitySystem.Abilities;
using Game.AbilitySystem.Attributes;
using Game.Data;
using Game.Generated;

namespace Domains.CharacterSelect
{
    public sealed class CharacterSelectController : IDisposable
    {
        private readonly CharacterService _characterService;
        private readonly AdventureStartState _adventureStartState;
        private readonly SceneManagerEx _sceneManager;
        private readonly ICharacterUnlockGateway _unlockGateway;
        private readonly TitleSceneNavigator _navigator;

        public CharacterSelectController(
            CharacterService characterService,
            AdventureStartState adventureStartState,
            SceneManagerEx sceneManager,
            ICharacterUnlockGateway unlockGateway,
            TitleSceneNavigator navigator)
        {
            _characterService = characterService;
            _adventureStartState = adventureStartState;
            _sceneManager = sceneManager;
            _unlockGateway = unlockGateway;
            _navigator = navigator;
        }

        public CharacterSelectInitialViewModel CreateInitialViewModel()
        {
            IReadOnlyList<CharacterModel> characters = _characterService.GetAll();
            CharacterSelectCardViewModel[] viewModels = new CharacterSelectCardViewModel[characters.Count];

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterModel character = characters[i];
                bool isLocked = !_unlockGateway.IsUnlocked(character.Id);
                float maxHealth = 0f;
                _characterService.TryGetInitialAttributeValue(
                    character.Id,
                    VitalAttributeSet.MaxHealthAttribute,
                    out maxHealth);
                float coinCount = 0f;
                _characterService.TryGetInitialAttributeValue(
                    character.Id,
                    CostAttributeSet.CoinCountAttribute,
                    out coinCount);

                IReadOnlyList<SkillGameplayAbility> abilities =
                    _characterService.GetDefaultSkillAbilities(character.Id);

                viewModels[i] = new CharacterSelectCardViewModel(
                    character.Id,
                    isLocked,
                    CardFaceViewModelFactory.Create(
                        isLocked ? character.Back : character.Front),
                    character.LocalizedName,
                    maxHealth,
                    (int)coinCount,
                    CreateSkillSlotViewModels(abilities));
            }

            return new CharacterSelectInitialViewModel(viewModels);
        }

        public bool IsUnlocked(ECharacter characterId)
        {
            return _unlockGateway.IsUnlocked(characterId);
        }

        public void StartNewAdventure(ECharacter selectedCharacterId)
        {
            if (!IsUnlocked(selectedCharacterId))
                return;

            _adventureStartState.SelectedCharacterId = selectedCharacterId;
            _sceneManager.Load(GameSceneId.Adventure);
        }

        public void OnBackClosed()
        {
            _navigator.HideCurrent();
        }

        public void Dispose()
        {
        }

        private static IReadOnlyList<CharacterSelectSkillSlotViewModel> CreateSkillSlotViewModels(
            IReadOnlyList<SkillGameplayAbility> abilities)
        {
            if (abilities == null || abilities.Count == 0)
                return Array.Empty<CharacterSelectSkillSlotViewModel>();

            List<CharacterSelectSkillSlotViewModel> result = new(abilities.Count);
            for (int i = 0; i < abilities.Count; i++)
            {
                SkillGameplayAbility ability = abilities[i];
                if (ability == null)
                    continue;

                result.Add(new CharacterSelectSkillSlotViewModel(
                    ability.Name,
                    ability.Icon));
            }

            return result;
        }
    }
}
