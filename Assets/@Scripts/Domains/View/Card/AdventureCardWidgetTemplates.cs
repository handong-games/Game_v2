using System;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    // Role:
    // Carries preloaded Adventure card widget UXML templates for one AdventureScene scope.
    public sealed class AdventureCardWidgetTemplates
    {
        public AdventureCardWidgetTemplates(
            VisualTreeAsset choiceCard,
            VisualTreeAsset playerCard,
            VisualTreeAsset monsterCard,
            VisualTreeAsset displayCard,
            CardFaceWidgetTemplates faceTemplates)
        {
            ChoiceCard = choiceCard ?? throw new ArgumentNullException(nameof(choiceCard));
            PlayerCard = playerCard ?? throw new ArgumentNullException(nameof(playerCard));
            MonsterCard = monsterCard ?? throw new ArgumentNullException(nameof(monsterCard));
            DisplayCard = displayCard ?? throw new ArgumentNullException(nameof(displayCard));
            FaceTemplates = faceTemplates ?? throw new ArgumentNullException(nameof(faceTemplates));
        }

        public VisualTreeAsset ChoiceCard { get; }
        public VisualTreeAsset PlayerCard { get; }
        public VisualTreeAsset MonsterCard { get; }
        public VisualTreeAsset DisplayCard { get; }
        public CardFaceWidgetTemplates FaceTemplates { get; }
    }
}
