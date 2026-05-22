using System;
using System.Collections.Generic;
using Gameplay.GAS;

namespace Game.Data
{
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

        public static readonly GameplayTag KindPlayer = GameplayTag.Request(KindPlayerName);
        public static readonly GameplayTag KindMonster = GameplayTag.Request(KindMonsterName);
        public static readonly GameplayTag KindEvent = GameplayTag.Request(KindEventName);
        public static readonly GameplayTag KindShop = GameplayTag.Request(KindShopName);
        public static readonly GameplayTag KindChoice = GameplayTag.Request(KindChoiceName);
        public static readonly GameplayTag Combatant = GameplayTag.Request(CombatantName);
        public static readonly GameplayTag Selectable = GameplayTag.Request(SelectableName);
        public static readonly GameplayTag Targetable = GameplayTag.Request(TargetableName);
        public static readonly GameplayTag Flippable = GameplayTag.Request(FlippableName);
        public static readonly GameplayTag ChoiceMonster = GameplayTag.Request(ChoiceMonsterName);
        public static readonly GameplayTag ChoiceElite = GameplayTag.Request(ChoiceEliteName);
        public static readonly GameplayTag ChoiceBoss = GameplayTag.Request(ChoiceBossName);
        public static readonly GameplayTag ChoiceEvent = GameplayTag.Request(ChoiceEventName);
        public static readonly GameplayTag ChoiceShop = GameplayTag.Request(ChoiceShopName);
        public static readonly GameplayTag MonsterRankNormal = GameplayTag.Request(MonsterRankNormalName);
        public static readonly GameplayTag MonsterRankElite = GameplayTag.Request(MonsterRankEliteName);
        public static readonly GameplayTag MonsterRankBoss = GameplayTag.Request(MonsterRankBossName);

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

        public static GameplayTagReference[] CreateReferences(params string[] tagNames)
        {
            GameplayTagReference[] references = new GameplayTagReference[tagNames.Length];
            for (int i = 0; i < tagNames.Length; i++)
            {
                references[i] = new GameplayTagReference(tagNames[i]);
            }

            return references;
        }

        public static IReadOnlyList<GameplayTagReference> Combine(
            IReadOnlyList<GameplayTagReference> source,
            params string[] requiredTagNames)
        {
            int sourceCount = source?.Count ?? 0;
            GameplayTagReference[] references = new GameplayTagReference[sourceCount + requiredTagNames.Length];

            for (int i = 0; i < sourceCount; i++)
            {
                references[i] = source[i];
            }

            for (int i = 0; i < requiredTagNames.Length; i++)
            {
                references[sourceCount + i] = new GameplayTagReference(requiredTagNames[i]);
            }

            return references;
        }
    }
}
