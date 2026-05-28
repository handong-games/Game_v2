using System;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayTagRequirements
    {
        [SerializeField]
        private GameplayTagContainer _requiredTags = new();

        [SerializeField]
        private GameplayTagContainer _blockedTags = new();

        public GameplayTagContainer RequiredTags => _requiredTags;
        public GameplayTagContainer BlockedTags => _blockedTags;

        public bool IsEmpty => RequiredTags.Count == 0 && BlockedTags.Count == 0;

        public bool RequirementsMet(GameplayTagCountContainer ownedTags)
        {
            return ownedTags.HasAll(RequiredTags) && !ownedTags.HasAny(BlockedTags);
        }

        public bool RequirementsMet(GameplayTagContainer ownedTags)
        {
            return ownedTags.HasAll(RequiredTags) && !ownedTags.HasAny(BlockedTags);
        }
    }
}
