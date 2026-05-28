using System;
using System.Collections.Generic;
using Gameplay.GAS;

namespace Game.Data
{
    [GameplayTagProvider]
    public static class CardGameplayTags
    {
        public const string KindPlayerName = "Card.Kind.Player";
        public const string KindMonsterName = "Card.Kind.Monster";
        public const string KindEventName = "Card.Kind.Event";
        public const string KindShopName = "Card.Kind.Shop";
        public const string KindChoiceName = "Card.Kind.Choice";
        public const string CombatantName = "Card.Combatant";
        public const string SelectableName = "Card.Selectable";
        public const string TargetableName = "Card.Targetable";
        public const string FlippableName = "Card.Flippable";
        public const string ChoiceMonsterName = "Choice.Monster";
        public const string ChoiceEliteName = "Choice.Elite";
        public const string ChoiceBossName = "Choice.Boss";
        public const string ChoiceEventName = "Choice.Event";
        public const string ChoiceShopName = "Choice.Shop";
        public const string MonsterRankNormalName = "Monster.Rank.Normal";
        public const string MonsterRankEliteName = "Monster.Rank.Elite";
        public const string MonsterRankBossName = "Monster.Rank.Boss";

        public static readonly GameplayTag KindPlayer = GameplayTag.Define(KindPlayerName);
        public static readonly GameplayTag KindMonster = GameplayTag.Define(KindMonsterName);
        public static readonly GameplayTag KindEvent = GameplayTag.Define(KindEventName);
        public static readonly GameplayTag KindShop = GameplayTag.Define(KindShopName);
        public static readonly GameplayTag KindChoice = GameplayTag.Define(KindChoiceName);
        public static readonly GameplayTag Combatant = GameplayTag.Define(CombatantName);
        public static readonly GameplayTag Selectable = GameplayTag.Define(SelectableName);
        public static readonly GameplayTag Targetable = GameplayTag.Define(TargetableName);
        public static readonly GameplayTag Flippable = GameplayTag.Define(FlippableName);
        public static readonly GameplayTag ChoiceMonster = GameplayTag.Define(ChoiceMonsterName);
        public static readonly GameplayTag ChoiceElite = GameplayTag.Define(ChoiceEliteName);
        public static readonly GameplayTag ChoiceBoss = GameplayTag.Define(ChoiceBossName);
        public static readonly GameplayTag ChoiceEvent = GameplayTag.Define(ChoiceEventName);
        public static readonly GameplayTag ChoiceShop = GameplayTag.Define(ChoiceShopName);
        public static readonly GameplayTag MonsterRankNormal = GameplayTag.Define(MonsterRankNormalName);
        public static readonly GameplayTag MonsterRankElite = GameplayTag.Define(MonsterRankEliteName);
        public static readonly GameplayTag MonsterRankBoss = GameplayTag.Define(MonsterRankBossName);

        public static string GetChoiceName(EChoiceCardType choiceType)
        {
            return choiceType switch
            {
                EChoiceCardType.Monster => ChoiceMonsterName,
                EChoiceCardType.Elite => ChoiceEliteName,
                EChoiceCardType.Boss => ChoiceBossName,
                EChoiceCardType.Event => ChoiceEventName,
                EChoiceCardType.Shop => ChoiceShopName,
                _ => throw new ArgumentOutOfRangeException(nameof(choiceType)),
            };
        }

        public static string GetMonsterRankName(EMonsterRank rank)
        {
            return rank switch
            {
                EMonsterRank.Elite => MonsterRankEliteName,
                EMonsterRank.Boss => MonsterRankBossName,
                _ => MonsterRankNormalName,
            };
        }

        public static GameplayTag[] CreateTags(params string[] tagNames)
        {
            GameplayTag[] tags = new GameplayTag[tagNames.Length];
            for (int i = 0; i < tagNames.Length; i++)
            {
                tags[i] = GameplayTag.Define(tagNames[i]);
            }

            return tags;
        }

        public static IReadOnlyList<GameplayTag> Combine(
            IReadOnlyList<GameplayTag> source,
            params GameplayTag[] requiredTags)
        {
            int sourceCount = source?.Count ?? 0;
            GameplayTag[] tags = new GameplayTag[sourceCount + requiredTags.Length];

            for (int i = 0; i < sourceCount; i++)
            {
                tags[i] = source[i];
            }

            for (int i = 0; i < requiredTags.Length; i++)
            {
                tags[sourceCount + i] = requiredTags[i];
            }

            return tags;
        }
    }
}
