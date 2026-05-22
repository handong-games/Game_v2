using System.Collections.Generic;

namespace Game.Data
{
    public interface ICardModel
    {
        IReadOnlyList<GameplayTagReference> OwnedTags { get; }
        AbilitySetModel AbilitySet { get; }
        CardFaceModel Front { get; }
        CardFaceModel Back { get; }
    }
}
