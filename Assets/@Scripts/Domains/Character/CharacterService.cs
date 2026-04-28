using System;
using System.Collections.Generic;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Core.Managers.Save;
using Game.Data;
using Game.Generated;

namespace Domains.Character
{
    [Dependency]
    public sealed class CharacterService : IDisposable
    {
        private readonly List<CharacterState> _characters = new();
        public IReadOnlyList<CharacterState> Characters => _characters;
        
        public CharacterService()
        {
            IReadOnlyList<CharacterModel> models = DBManager.Instance.Character.GetAll();
            for (int i = 0; i < models.Count; i++)
            {
                CharacterModel model = models[i];
                if (model == null)
                {
                    throw new InvalidOperationException($"Character model at index {i} is null.");
                }

                CharacterState state = new CharacterState(model)
                {
                    IsUnlocked = SaveManager.Instance.Progress.IsUnlocked(model.Key)
                };

                _characters.Add(state);
            }
        }

        public CharacterState Get(ECharacter character)
        {
            int index = (int)character;
            if (index < 0 || index >= _characters.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(character), character, null);
            }

            return _characters[index];
        }

        public void SetLock(ECharacter character, bool isLocked)
        {
            SaveManager.Instance.Progress.SetLock(character, isLocked);

            CharacterState state = Get(character);
            state.IsUnlocked = !isLocked;

            SaveManager.Instance.SaveProgress();
        }

        public void Dispose()
        {
            _characters.Clear();
        }
    }
}
