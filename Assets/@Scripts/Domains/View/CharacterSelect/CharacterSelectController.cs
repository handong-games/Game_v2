using System;
using System.Collections.Generic;
using Domains.Adventure;
using Domains.Card;
using Domains.Combat;
using Domains.Player;
using Domains.Scene;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using Game.Core.Managers.Save;
using Game.Data;
using Game.Generated;

namespace Domains.CharacterSelect
{
    using Card = global::Domains.Card.Card;

    [Dependency(nameof(TitleScene))]
    public sealed class CharacterSelectController : IDisposable
    {
        [Inject]
        private AdventureService _adventureService;

        [Inject]
        private CardDeckService _cardDeckService;

        [Inject]
        private PlayerService _playerService;

        [Inject]
        private CardService _cardService;

        [Inject]
        private CardBoardService _cardBoardService;

        [Inject]
        private CombatService _combatService;

        public CharacterSelectInitialViewModel CreateInitialViewModel()
        {
            IReadOnlyList<CharacterModel> characters = DBManager.Instance.Character.GetAll();
            ProgressState progress = SaveManager.Instance.GetState<ProgressState>();
            CharacterSelectCardViewModel[] viewModels = new CharacterSelectCardViewModel[characters.Count];

            for (int i = 0; i < characters.Count; i++)
            {
                CharacterModel character = characters[i];
                bool isLocked = !progress.IsUnlocked(character.Id);

                viewModels[i] = new CharacterSelectCardViewModel(
                    character.Id,
                    isLocked,
                    CardFaceViewModelFactory.Create(
                        isLocked ? character.Back : character.Front),
                    character.LocalizedName,
                    character.CoinCount,
                    character.DefaultSkills);
            }

            return new CharacterSelectInitialViewModel(viewModels);
        }

        public bool IsUnlocked(ECharacter characterId)
        {
            ProgressState progress = SaveManager.Instance.GetState<ProgressState>();
            return progress.IsUnlocked(characterId);
        }

        public void StartNewAdventure(ECharacter selectedCharacterId)
        {
            if (!IsUnlocked(selectedCharacterId))
                return;

            AdventureSession adventure = _adventureService.StartNew(selectedCharacterId);
            CharacterModel characterModel = DBManager.Instance.Character.Get(selectedCharacterId);

            _cardService.Clear();
            _cardBoardService.Clear();
            _combatService.Initialize();
            Card playerCard = _cardService.Create(characterModel);
            _playerService.Initialize(characterModel, adventure.Seed);
            _playerService.SetPlayerCard(playerCard);
            _cardDeckService.Initialize(
                adventure.CardDeckId,
                adventure.Seed);

            SceneManagerEx.Instance.LoadScene<AdventureScene>();
        }

        public void Dispose()
        {
        }
    }
}
