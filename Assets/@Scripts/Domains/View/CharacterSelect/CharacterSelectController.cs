using System;
using System.Collections.Generic;
using Domains.Adventure;
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

        public IReadOnlyList<CharacterModel> GetAllCharacters()
        {
            return DBManager.Instance.Character.GetAll();
        }

        public bool CanSelect(ECharacter character)
        {
            return SaveManager.Instance.Progress.IsUnlocked(character);
        }

        public void StartNewAdventure(ECharacter character)
        {
            if (!CanSelect(character))
                return;
            
            _adventureService.StartNew(character);
            SceneManagerEx.Instance.LoadScene<AdventureScene>();
        }

        public void Dispose()
        {
        }
    }
}
