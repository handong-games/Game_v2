using System;
using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;

namespace Game.Data
{
    public abstract class CardModel<TKey> : AbstractModel<TKey>, ICardModel
        where TKey : Enum
    {
        [Header("Visual")]
        [SerializeField]
        private CardFaceModel _front;

        [SerializeField]
        private CardFaceModel _back;

        [Header("Gameplay")]
        [SerializeField]
        private AbilitySetModel _abilitySet;

        [SerializeField]
        private AttributeSetDefaultsDefinition[] _attributeSetDefaults;

        [SerializeField]
        private GameplayTag[] _ownedTags;

        public virtual IReadOnlyList<GameplayTag> OwnedTags =>
            _ownedTags ?? Array.Empty<GameplayTag>();

        public virtual IReadOnlyList<AttributeSetDefaultsDefinition> AttributeSetDefaults =>
            _attributeSetDefaults ?? Array.Empty<AttributeSetDefaultsDefinition>();
        public virtual AbilitySetModel AbilitySet => _abilitySet;
        public virtual CardFaceModel Front => _front;
        public virtual CardFaceModel Back => _back;
    }
}
