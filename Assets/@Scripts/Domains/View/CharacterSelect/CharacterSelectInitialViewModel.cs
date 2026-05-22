using System.Collections.Generic;

namespace Domains.CharacterSelect
{
    public sealed class CharacterSelectInitialViewModel
    {
        public CharacterSelectInitialViewModel(IReadOnlyList<CharacterSelectCardViewModel> cards)
        {
            Cards = cards;
        }

        public IReadOnlyList<CharacterSelectCardViewModel> Cards { get; }
    }
}
