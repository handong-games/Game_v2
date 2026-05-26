using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Data;
using Game.Generated;
using Gameplay.GAS;

namespace Domains.Character
{
    [Dependency]
    public sealed class CharacterService
    {
        public CharacterModel Get(ECharacter id)
        {
            return DBManager.Instance.Character.Get(id);
        }

        public IReadOnlyList<CharacterModel> GetAll()
        {
            return DBManager.Instance.Character.GetAll();
        }

        public bool TryGetInitialAttributeValue(
            ECharacter id,
            GameplayAttribute attribute,
            out float value)
        {
            CharacterModel character = Get(id);
            return TryGetInitialAttributeValue(character, attribute, out value);
        }

        private static bool TryGetInitialAttributeValue(
            CharacterModel character,
            GameplayAttribute attribute,
            out float value)
        {
            value = 0f;

            if (character == null)
                return false;

            IReadOnlyList<AttributeSetDefaultsDefinition> definitions = character.AttributeSetDefaults;
            for (int i = 0; i < definitions.Count; i++)
            {
                AttributeSetDefaultsDefinition definition = definitions[i];
                if (definition == null)
                    continue;

                if (definition.TryGetValue(attribute, out value))
                    return true;
            }

            return false;
        }
    }
}
