using System.Collections.Generic;

namespace Gameplay.GAS
{
    public sealed class GameplayTagCountContainer
    {
        private readonly Dictionary<GameplayTag, int> _explicitTagCounts = new();
        private readonly Dictionary<GameplayTag, int> _hierarchicalTagCounts = new();

        public int Count => _explicitTagCounts.Count;
        public bool IsEmpty => _explicitTagCounts.Count == 0;

        public void AddTag(GameplayTag tag, int count = 1)
        {
            if (!tag.IsValid || count <= 0)
                return;

            _explicitTagCounts.TryGetValue(tag, out int currentCount);
            _explicitTagCounts[tag] = currentCount + count;
            ApplyHierarchicalCountDelta(tag, count);
        }

        public void RemoveTag(GameplayTag tag, int count = 1)
        {
            if (!_explicitTagCounts.TryGetValue(tag, out int currentCount) || count <= 0)
                return;

            int nextCount = currentCount - count;
            if (nextCount > 0)
            {
                _explicitTagCounts[tag] = nextCount;
                ApplyHierarchicalCountDelta(tag, -count);
                return;
            }

            _explicitTagCounts.Remove(tag);
            ApplyHierarchicalCountDelta(tag, -currentCount);
        }

        public int GetCount(GameplayTag tag)
        {
            return _hierarchicalTagCounts.TryGetValue(tag, out int count) ? count : 0;
        }

        public int GetExplicitCount(GameplayTag tag)
        {
            return _explicitTagCounts.TryGetValue(tag, out int count) ? count : 0;
        }

        public bool HasTag(GameplayTag tag)
        {
            return tag.IsValid && GetCount(tag) > 0;
        }

        public bool HasTagExact(GameplayTag tag)
        {
            return tag.IsValid && GetExplicitCount(tag) > 0;
        }

        public bool HasAny(GameplayTagContainer tags)
        {
            if (tags == null || tags.Count == 0 || _hierarchicalTagCounts.Count == 0)
                return false;

            foreach (GameplayTag tag in tags)
            {
                if (HasTag(tag))
                    return true;
            }

            return false;
        }

        public bool HasAnyExact(GameplayTagContainer tags)
        {
            if (tags == null || tags.Count == 0 || _explicitTagCounts.Count == 0)
                return false;

            foreach (GameplayTag tag in tags)
            {
                if (HasTagExact(tag))
                    return true;
            }

            return false;
        }

        public bool HasAll(GameplayTagContainer tags)
        {
            if (tags == null || tags.Count == 0)
                return true;

            if (_hierarchicalTagCounts.Count == 0)
                return false;

            foreach (GameplayTag tag in tags)
            {
                if (!HasTag(tag))
                    return false;
            }

            return true;
        }

        public bool HasAllExact(GameplayTagContainer tags)
        {
            if (tags == null || tags.Count == 0)
                return true;

            if (_explicitTagCounts.Count == 0)
                return false;

            foreach (GameplayTag tag in tags)
            {
                if (!HasTagExact(tag))
                    return false;
            }

            return true;
        }

        public void Clear()
        {
            _explicitTagCounts.Clear();
            _hierarchicalTagCounts.Clear();
        }

        private void ApplyHierarchicalCountDelta(GameplayTag tag, int countDelta)
        {
            HashSet<GameplayTag> visitedTags = new();
            GameplayTag currentTag = tag;

            while (currentTag.IsValid && visitedTags.Add(currentTag))
            {
                _hierarchicalTagCounts.TryGetValue(currentTag, out int currentCount);
                int nextCount = currentCount + countDelta;
                if (nextCount > 0)
                    _hierarchicalTagCounts[currentTag] = nextCount;
                else
                    _hierarchicalTagCounts.Remove(currentTag);

                currentTag = currentTag.GetDirectParent();
            }
        }
    }
}
