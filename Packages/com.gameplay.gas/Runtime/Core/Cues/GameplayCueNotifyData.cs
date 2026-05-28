using System;
using UnityEngine;

namespace Gameplay.GAS
{
    [Serializable]
    public sealed class GameplayCueNotifyData
    {
        [SerializeField]
        [GameplayTagFilter("GameplayCue")]
        private GameplayTag _cueTag;

        [SerializeReference]
        private GameplayCueNotify _notify;

        public GameplayTag CueTag => _cueTag;
        public GameplayCueNotify Notify => _notify;

        public bool IsValid => CueTag.IsValid && _notify != null;
    }
}
