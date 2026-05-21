using System.Collections;
using System.Collections.Generic;

namespace Gameplay.GAS
{
    public sealed class GameplayTagContainer : IEnumerable<GameplayTag>
    {
        private readonly HashSet<GameplayTag> _tags = new();

        public int Count => _tags.Count;
        public bool IsEmpty => _tags.Count == 0;

        public void Add(GameplayTag tag)
        {
            AddTag(tag);
        }

        public void AddTag(GameplayTag tag)
        {
            if (tag.IsValid)
                _tags.Add(tag);
        }

        public void AddTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return;

            AddTag(GameplayTag.Request(tagName));
        }

        public void Remove(GameplayTag tag)
        {
            RemoveTag(tag);
        }

        public void RemoveTag(GameplayTag tag)
        {
            _tags.Remove(tag);
        }

        public void AppendTags(GameplayTagContainer other)
        {
            foreach (GameplayTag tag in other._tags)
            {
                AddTag(tag);
            }
        }

        public void Clear()
        {
            _tags.Clear();
        }

        public bool HasTag(GameplayTag tag)
        {
            foreach (GameplayTag ownedTag in _tags)
            {
                if (ownedTag.MatchesTag(tag))
                    return true;
            }

            return false;
        }

        public bool HasTagExact(GameplayTag tag)
        {
            return _tags.Contains(tag);
        }

        public bool HasAny(GameplayTagContainer other)
        {
            foreach (GameplayTag tag in other._tags)
            {
                if (HasTag(tag))
                    return true;
            }

            return false;
        }

        public bool HasAnyExact(GameplayTagContainer other)
        {
            foreach (GameplayTag tag in other._tags)
            {
                if (HasTagExact(tag))
                    return true;
            }

            return false;
        }

        public bool HasAll(GameplayTagContainer other)
        {
            foreach (GameplayTag tag in other._tags)
            {
                if (!HasTag(tag))
                    return false;
            }

            return true;
        }

        public bool HasAllExact(GameplayTagContainer other)
        {
            foreach (GameplayTag tag in other._tags)
            {
                if (!HasTagExact(tag))
                    return false;
            }

            return true;
        }

        public IEnumerator<GameplayTag> GetEnumerator()
        {
            return _tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
