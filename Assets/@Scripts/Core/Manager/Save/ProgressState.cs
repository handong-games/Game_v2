using System;
using System.Collections.Generic;
using Game.Core.Managers.Dependency;
using Game.Generated;

namespace Game.Core.Managers.Save
{
    [Dependency]
    public sealed class ProgressState : ISave<ProgressSave>
    {
        public HashSet<ECharacter> CharacterUnlockIds { get; } = new()
        {
            ECharacter.Warrior
        };

        public bool IsUnlocked(ECharacter character)
        {
            return CharacterUnlockIds.Contains(character);
        }

        public void Unlock(ECharacter character)
        {
            CharacterUnlockIds.Add(character);
        }

        public void SetLock(ECharacter character, bool isLocked)
        {
            if (isLocked)
            {
                CharacterUnlockIds.Remove(character);
                return;
            }

            CharacterUnlockIds.Add(character);
        }

        public void LoadFrom(ProgressSave save)
        {
            CharacterUnlockIds.Clear();

            if (save.CharacterUnlockIds != null)
            {
                for (int i = 0; i < save.CharacterUnlockIds.Count; i++)
                {
                    if (Enum.TryParse(save.CharacterUnlockIds[i], out ECharacter character))
                    {
                        CharacterUnlockIds.Add(character);
                    }
                }
            }
            else
            {
                CharacterUnlockIds.Add(ECharacter.Warrior);
            }
        }

        public ProgressSave ToSave()
        {
            ProgressSave save = new ProgressSave();

            foreach (ECharacter character in CharacterUnlockIds)
            {
                save.CharacterUnlockIds.Add(character.ToString());
            }

            return save;
        }
    }
}
