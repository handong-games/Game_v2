using System;
using System.Collections.Generic;
using Game.Generated;

namespace Game.Core.Managers.Save
{
    public sealed class ProgressState : IState<ProgressSave>
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

        public static ProgressState FromSave(ProgressSave save)
        {
            ProgressState state = new ProgressState();
            state.CharacterUnlockIds.Clear();

            if (save.CharacterUnlockIds != null)
            {
                for (int i = 0; i < save.CharacterUnlockIds.Count; i++)
                {
                    if (Enum.TryParse(save.CharacterUnlockIds[i], out ECharacter character))
                    {
                        state.CharacterUnlockIds.Add(character);
                    }
                }
            }
            else
            {
                state.CharacterUnlockIds.Add(ECharacter.Warrior);
            }

            return state;
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
