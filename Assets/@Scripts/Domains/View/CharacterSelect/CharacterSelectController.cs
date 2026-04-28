using System;
using System.Collections.Generic;
using Domains.Character;
using Domains.Run;
using Domains.Scene.CombatScene;
using Domains.Scene.TitleScene;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Scene;
using UnityEngine.SceneManagement;

namespace Domains.CharacterSelect
{
    [Dependency(nameof(TitleScene))]
    public sealed class CharacterSelectController : IDisposable
    {
        [Inject]
        private CharacterService _characterService;

        [Inject]
        private RunService _runService;

        public IReadOnlyList<CharacterState> GetAllCharacters()
        {
            return _characterService.Characters;
        }

        public bool CanSelect(CharacterState character)
        {
            return IsSelectable(character);
        }

        public void StartGame(CharacterState character)
        {
            if (!IsSelectable(character))
                return;
            
            _runService.StartNewRun(character);
            SceneManagerEx.Instance.LoadScene<CombatScene>();
        }

        public void Dispose()
        {
        }

        private bool IsSelectable(CharacterState character)
        {
            return character != null && character.IsUnlocked;
        }
    }
}
