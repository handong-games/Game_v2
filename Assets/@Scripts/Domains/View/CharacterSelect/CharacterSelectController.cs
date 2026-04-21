using System;
using System.Collections.Generic;
using Domains.Scene.TitleScene;
using Game.Core.Managers.Dependency;
using Game.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Domains.CharacterSelect
{
    [Dependency(nameof(TitleScene))]
    public sealed class CharacterSelectController : IDisposable
    {
        private const string SceneLabel = "TitleScene";

        private AsyncOperationHandle<IList<Object>> _loadHandle;
        private SO_CharacterDatabase _characterDatabase;
        private SO_SkillDatabase _skillDatabase;
        private SO_CharacterData _selectedCharacter;
        private bool _loaded;

        public SO_CharacterData SelectedCharacter => _selectedCharacter;

        public IReadOnlyList<SO_CharacterData> GetAllCharacters()
        {
            EnsureLoaded();
            return _characterDatabase != null
                ? _characterDatabase.GetAllCharacters()
                : Array.Empty<SO_CharacterData>();
        }

        public IReadOnlyList<SO_SkillData> GetAllSkills()
        {
            EnsureLoaded();
            return _skillDatabase != null
                ? _skillDatabase.GetAllSkills()
                : Array.Empty<SO_SkillData>();
        }

        public bool IsSelectable(SO_CharacterData character)
        {
            return character != null && !character.IsLocked;
        }

        public void SelectCharacter(SO_CharacterData character)
        {
            if (!IsSelectable(character))
                return;

            _selectedCharacter = character;
        }

        public void ResetSelection()
        {
            _selectedCharacter = null;
        }

        public bool StartSelectedCharacter()
        {
            if (!IsSelectable(_selectedCharacter))
                return false;

            Debug.LogWarning($"[CharacterSelectController] '{_selectedCharacter.CharacterName}' selected. Runtime start flow is not connected yet.");
            return true;
        }

        public void Dispose()
        {
            if (_loadHandle.IsValid())
            {
                Addressables.Release(_loadHandle);
            }

            _loadHandle = default;
            _characterDatabase = null;
            _skillDatabase = null;
            _selectedCharacter = null;
            _loaded = false;
        }

        private void EnsureLoaded()
        {
            if (_loaded)
                return;

            _loadHandle = Addressables.LoadAssetsAsync<Object>(SceneLabel, null);
            IList<Object> assets = _loadHandle.WaitForCompletion();
            for (int i = 0; i < assets.Count; i++)
            {
                switch (assets[i])
                {
                    case SO_CharacterDatabase characterDatabase:
                        _characterDatabase = characterDatabase;
                        break;
                    case SO_SkillDatabase skillDatabase:
                        _skillDatabase = skillDatabase;
                        break;
                }
            }

            _loaded = true;

            if (_characterDatabase == null)
            {
                Debug.LogError("[CharacterSelectController] SO_CharacterDatabase could not be found under the TitleScene addressable label.");
            }

            if (_skillDatabase == null)
            {
                Debug.LogError("[CharacterSelectController] SO_SkillDatabase could not be found under the TitleScene addressable label.");
            }
        }
    }
}
