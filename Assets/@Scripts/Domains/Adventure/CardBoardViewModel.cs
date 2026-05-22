using System.Collections.Generic;

namespace Domains.Adventure
{
    public sealed class CardBoardViewModel
    {
        public CardBoardViewModel(
            IReadOnlyList<BoardCardViewModel> leftCards,
            IReadOnlyList<BoardCardViewModel> rightCards)
        {
            LeftCards = leftCards;
            RightCards = rightCards;
        }

        public IReadOnlyList<BoardCardViewModel> LeftCards { get; }
        public IReadOnlyList<BoardCardViewModel> RightCards { get; }
    }
}
