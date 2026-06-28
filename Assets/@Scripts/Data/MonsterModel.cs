using System;
using System.Collections.Generic;
using Domains.Intent.Data;
using Game.AbilitySystem.Attributes;
using Game.Core.Managers.DB;
using Game.Generated;
using Gameplay.GAS;
using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Monster")]
    public sealed class MonsterModel : CardModel<EMonster>
    {
        private IReadOnlyList<GameplayTag> _runtimeOwnedTags;

        [Header("Visual")]
        [SerializeField]
        private LocalizedString _localizedName;

        [Header("Intent")]
        [SerializeField]
        private MonsterActionModel[] _actionSequence;

        public LocalizedString LocalizedName => _localizedName;
        public IReadOnlyList<MonsterActionModel> ActionSequence =>
            _actionSequence ?? Array.Empty<MonsterActionModel>();

        public override IReadOnlyList<GameplayTag> OwnedTags => _runtimeOwnedTags ??=
            CardGameplayTags.Combine(
                base.OwnedTags,
                CardGameplayTags.KindMonster,
                CardGameplayTags.Combatant,
                CardGameplayTags.Targetable);
    }
}

namespace Domains.Monster
{
    using Game.Data;

    public sealed class MonsterService
    {
        private readonly DBManager _dbManager;

        public MonsterService(DBManager dbManager)
        {
            _dbManager = dbManager;
        }

        public MonsterModel Get(EMonster id)
        {
            return _dbManager.Monster.Get(id);
        }

        public IReadOnlyList<MonsterModel> GetAll()
        {
            return _dbManager.Monster.GetAll();
        }

        public bool TryGetInitialAttributeValue(
            EMonster id,
            GameplayAttribute attribute,
            out float value)
        {
            MonsterModel monster = Get(id);
            return TryGetInitialAttributeValue(monster, attribute, out value);
        }

        public static bool TryGetInitialAttributeValue(
            MonsterModel monster,
            GameplayAttribute attribute,
            out float value)
        {
            value = 0f;

            if (monster == null)
                return false;

            IReadOnlyList<AttributeSetDefaultsDefinition> definitions = monster.AttributeSetDefaults;
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
