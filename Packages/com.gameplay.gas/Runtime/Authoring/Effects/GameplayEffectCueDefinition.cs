using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayEffectCueDefinition
    {
        [SerializeField]
        [GameplayTagFilter("GameplayCue")]
        private GameplayTagContainer _gameplayCueTags = new();

        [SerializeField]
        private float _minLevel;

        [SerializeField]
        private float _maxLevel;

        public GameplayTagContainer GameplayCueTags => _gameplayCueTags;
        public float MinLevel => _minLevel;
        public float MaxLevel => _maxLevel;

        public bool TryBuild(out GameplayEffectCue cue)
        {
            cue = new GameplayEffectCue(_minLevel, _maxLevel);
            bool hasTag = false;

            foreach (GameplayTag tag in _gameplayCueTags)
            {
                if (!tag.IsValid)
                    continue;

                cue.GameplayCueTags.AddTag(tag);
                hasTag = true;
            }

            return hasTag;
        }
    }
}
