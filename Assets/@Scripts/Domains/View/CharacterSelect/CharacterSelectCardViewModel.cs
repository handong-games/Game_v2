using System.Collections.Generic;
using Game.Data;
using Game.Generated;
using UnityEngine.Localization;

namespace Domains.CharacterSelect
{
    public sealed class CharacterSelectCardViewModel
    {
        public CharacterSelectCardViewModel(
            ECharacter characterId,
            bool isLocked,
            CardFaceViewModel face,
            LocalizedString localizedName,
            int coinCount,
            IReadOnlyList<CharacterSkillModel> skills)
        {
            CharacterId = characterId;
            IsLocked = isLocked;
            Face = face;
            LocalizedName = localizedName;
            CoinCount = coinCount;
            Skills = skills;
        }

        public ECharacter CharacterId { get; }
        public bool IsLocked { get; }
        public CardFaceViewModel Face { get; }
        public LocalizedString LocalizedName { get; }
        public int CoinCount { get; }
        public IReadOnlyList<CharacterSkillModel> Skills { get; }
    }
}
