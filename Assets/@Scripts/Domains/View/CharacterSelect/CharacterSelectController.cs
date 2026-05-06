using System;
using System.Collections.Generic;
using Domains.Adventure;
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
    [Dependency(nameof(TitleScene))]
    public sealed class CharacterSelectController : IDisposable
    {
        [Inject]
        private AdventureService _adventureService;

        [Inject]
        private CardDeckService _cardDeckService;

        [Inject]
        private PlayerService _playerService;

        public IReadOnlyList<CharacterModel> GetAllCharacters()
        {
            return DBManager.Instance.Character.GetAll();
        }

        public bool CanSelect(ECharacter character)
        {
            ProgressState progress = DependencyManager.Instance.Resolve<ProgressState>();
            return progress.IsUnlocked(character);
        }

        public void StartNewAdventure(ECharacter character)
        {
            if (!CanSelect(character))
                return;

            AdventureSession adventure = _adventureService.StartNew(character);
            CharacterModel characterModel = DBManager.Instance.Character.Get(character);

            _playerService.Initialize(characterModel, adventure.Seed);
            _cardDeckService.Initialize(
                adventure.CardDeckId,
                adventure.SelectedCharacterId,
                adventure.Seed);

            SceneManagerEx.Instance.LoadScene<AdventureScene>();
        }

        public void Dispose()
        {
        }
    }
}
