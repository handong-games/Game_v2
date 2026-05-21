using System.Collections.Generic;

namespace Gameplay.GAS
{
    public sealed class GameplayTagCountContainer
    {
        private readonly Dictionary<GameplayTag, int> _tagCounts = new();

        public int Count => _tagCounts.Count;
        public bool IsEmpty => _tagCounts.Count == 0;

        public void AddTag(GameplayTag tag, int count = 1)
        {
            if (!tag.IsValid || count <= 0)
                return;

            _tagCounts.TryGetValue(tag, out int currentCount);
            _tagCounts[tag] = currentCount + count;
        }

        public void RemoveTag(GameplayTag tag, int count = 1)
        {
            if (!_tagCounts.TryGetValue(tag, out int currentCount) || count <= 0)
                return;

            int nextCount = currentCount - count;
            if (nextCount > 0)
            {
                _tagCounts[tag] = nextCount;
                return;
            }

            _tagCounts.Remove(tag);
        }

        public int GetCount(GameplayTag tag)
        {
            return _tagCounts.TryGetValue(tag, out int count) ? count : 0;
        }

        public bool HasTag(GameplayTag tag)
        {
            foreach (GameplayTag ownedTag in _tagCounts.Keys)
            {
                if (ownedTag.MatchesTag(tag))
                    return true;
            }

            return false;
        }

        public bool HasTagExact(GameplayTag tag)
        {
            return GetCount(tag) > 0;
        }

        public bool HasAny(GameplayTagContainer tags)
        {
            foreach (GameplayTag tag in tags)
            {
                if (HasTag(tag))
                    return true;
            }

            return false;
        }

        public bool HasAnyExact(GameplayTagContainer tags)
        {
            foreach (GameplayTag tag in tags)
            {
                if (HasTagExact(tag))
                    return true;
            }

            return false;
        }

        public bool HasAll(GameplayTagContainer tags)
        {
            foreach (GameplayTag tag in tags)
            {
                if (!HasTag(tag))
                    return false;
            }

            return true;
        }

        public bool HasAllExact(GameplayTagContainer tags)
        {
            foreach (GameplayTag tag in tags)
            {
                if (!HasTagExact(tag))
                    return false;
            }

            return true;
        }

        public void Clear()
        {
            _tagCounts.Clear();
        }
    }
}
