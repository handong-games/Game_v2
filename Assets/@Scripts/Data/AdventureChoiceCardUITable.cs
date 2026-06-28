using UnityEngine;

namespace Game.Data
{
    // Role:
    // Resolves EChoiceCardType to the UI model used when rendering adventure choice cards.
    [CreateAssetMenu(menuName = "Game/Adventure/Cards/Choice Card UI Table")]
    public sealed class AdventureChoiceCardUITable
        : AbstractTable<AdventureChoiceCardUIModel, EChoiceCardType>
    {
    }
}
