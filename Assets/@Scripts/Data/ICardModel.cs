using System.Collections.Generic;
using Game.AbilitySystem.Attributes;
using Gameplay.GAS;

namespace Game.Data
{
    public interface ICardModel
    {
        IReadOnlyList<GameplayTagReference> OwnedTags { get; }
        IReadOnlyList<AttributeSetDefaultsDefinition> AttributeSetDefaults { get; }
        AbilitySetModel AbilitySet { get; }
        CardFaceModel Front { get; }
        CardFaceModel Back { get; }
    }
}
