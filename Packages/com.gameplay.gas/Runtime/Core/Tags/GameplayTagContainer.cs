using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayTagContainer : IEnumerable<GameplayTag>
    {
        [SerializeField]
        private List<GameplayTag> _tags = new();

        public int Count => _tags.Count;
        public bool IsEmpty => _tags.Count == 0;

        public void Add(GameplayTag tag)
        {
            AddTag(tag);
        }

        public void AddTag(GameplayTag tag)
        {
            if (!tag.IsValid || _tags.Contains(tag))
                return;

            _tags.Add(tag);
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
            if (other == null)
                return;

            for (int i = 0; i < other._tags.Count; i++)
            {
                AddTag(other._tags[i]);
            }
        }

        public void Clear()
        {
            _tags.Clear();
        }

        public bool HasTag(GameplayTag tag)
        {
            if (!tag.IsValid || _tags.Count == 0)
                return false;

            for (int i = 0; i < _tags.Count; i++)
            {
                if (_tags[i].MatchesTag(tag))
                    return true;
            }

            return false;
        }

        public bool HasTagExact(GameplayTag tag)
        {
            if (!tag.IsValid)
                return false;

            return _tags.Contains(tag);
        }

        public bool HasAny(GameplayTagContainer other)
        {
            if (other == null || _tags.Count == 0 || other._tags.Count == 0)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            for (int i = 0; i < other._tags.Count; i++)
            {
                if (HasTag(other._tags[i]))
                    return true;
            }

            return false;
        }

        public bool HasAnyExact(GameplayTagContainer other)
        {
            if (other == null || _tags.Count == 0 || other._tags.Count == 0)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            for (int i = 0; i < other._tags.Count; i++)
            {
                if (HasTagExact(other._tags[i]))
                    return true;
            }

            return false;
        }

        public bool HasAll(GameplayTagContainer other)
        {
            if (other == null || other._tags.Count == 0)
                return true;

            if (_tags.Count == 0)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            for (int i = 0; i < other._tags.Count; i++)
            {
                if (!HasTag(other._tags[i]))
                    return false;
            }

            return true;
        }

        public bool HasAllExact(GameplayTagContainer other)
        {
            if (other == null || other._tags.Count == 0)
                return true;

            if (_tags.Count == 0)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            for (int i = 0; i < other._tags.Count; i++)
            {
                if (!HasTagExact(other._tags[i]))
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
