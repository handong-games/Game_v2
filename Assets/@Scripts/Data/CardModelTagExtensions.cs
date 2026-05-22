using System;
using Gameplay.GAS;

namespace Game.Data
{
    public static class CardModelTagExtensions
    {
        public static bool HasOwnedTag(this ICardModel model, GameplayTag tag)
        {
            if (model == null || !tag.IsValid)
                return false;

            for (int i = 0; i < model.OwnedTags.Count; i++)
            {
                if (model.OwnedTags[i].Tag.MatchesTag(tag))
                    return true;
            }

            return false;
        }

        public static bool HasOwnedTagExact(this ICardModel model, GameplayTag tag)
        {
            if (model == null || !tag.IsValid)
                return false;

            for (int i = 0; i < model.OwnedTags.Count; i++)
            {
                if (model.OwnedTags[i].Tag.MatchesTagExact(tag))
                    return true;
            }

            return false;
        }

        public static bool TryGetChoiceType(this ICardModel model, out EChoiceCardType choiceType)
        {
            if (model.HasOwnedTagExact(CardGameplayTags.ChoiceMonster))
            {
                choiceType = EChoiceCardType.Monster;
                return true;
            }

            if (model.HasOwnedTagExact(CardGameplayTags.ChoiceElite))
            {
                choiceType = EChoiceCardType.Elite;
                return true;
            }

            if (model.HasOwnedTagExact(CardGameplayTags.ChoiceBoss))
            {
                choiceType = EChoiceCardType.Boss;
                return true;
            }

            if (model.HasOwnedTagExact(CardGameplayTags.ChoiceEvent))
            {
                choiceType = EChoiceCardType.Event;
                return true;
            }

            if (model.HasOwnedTagExact(CardGameplayTags.ChoiceShop))
            {
                choiceType = EChoiceCardType.Shop;
                return true;
            }

            choiceType = default;
            return false;
        }
    }
}
