using System;
using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;
using UnityEngine;

namespace Game.Data
{
    public abstract class CardModelBase : AbstractModel
    {
        [Header("Visual")]
        [SerializeField]
        private CardFaceModel _front;

        [SerializeField]
        private CardFaceModel _back;

        [Header("Gameplay")]
        // Default abilities granted when a card is created.
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

    public abstract class CardModel<TKey> : CardModelBase, IKeyAssignable<TKey>
        where TKey : Enum
    {
        [NonSerialized]
        private TKey _id;

        public TKey Id => _id;
        public override ModelKey ModelKey => new(typeof(TKey), Convert.ToInt32(_id));

        public void SetId(TKey id)
        {
            _id = id;
        }
    }
}
