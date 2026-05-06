using System.Collections.Generic;
using Domains.Player;
using Domains.Scene;
using Game.Core.Managers.Dependency;
using Game.Data;

namespace Domains.Adventure
{
    [Dependency(nameof(AdventureScene))]
    public sealed class AdventureController
    {
        [Inject]
        private AdventureService _adventureService;

        [Inject]
        private PlayerService _playerService;

        public void StartFirstStage()
        {
            _adventureService.StartFirstStage();
        }

        public CoinFlipDto OnPouchClicked()
        {
            return _playerService.OpenPouch();
        }

        public IReadOnlyList<CharacterSkillModel> GetSkillSlots()
        {
            return _playerService.CurrentPlayer.SkillSlots;
        }
    }
}
