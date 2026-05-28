using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.GAS
{
    [CreateAssetMenu(menuName = "Gameplay/GAS/Gameplay Cue Set")]
    public sealed class GameplayCueSet : ScriptableObject
    {
        [SerializeField]
        private List<GameplayCueNotifyData> _cues = new();

        private readonly Dictionary<GameplayTag, int> _cueDataMap = new();

        public void BuildAccelerationMap()
        {
            _cueDataMap.Clear();

            for (int i = 0; i < _cues.Count; i++)
            {
                GameplayCueNotifyData cue = _cues[i];
                if (cue == null || !cue.IsValid)
                    continue;

                _cueDataMap.TryAdd(cue.CueTag, i);
            }
        }

        public bool HandleGameplayCue(
            AbilitySystemComponent target,
            GameplayTag cueTag,
            GameplayCueEvent eventType,
            GameplayCueParameters parameters)
        {
            if (!TryFindCueIndex(cueTag, out int cueIndex))
                return false;

            GameplayCueNotifyData cue = _cues[cueIndex];
            cue.Notify?.HandleGameplayCue(target, cueTag, eventType, parameters);
            return true;
        }

        private bool TryFindCueIndex(GameplayTag cueTag, out int cueIndex)
        {
            GameplayTag currentTag = cueTag;

            while (currentTag.IsValid)
            {
                if (_cueDataMap.TryGetValue(currentTag, out cueIndex))
                    return cueIndex >= 0 && cueIndex < _cues.Count;

                currentTag = currentTag.GetDirectParent();
            }

            cueIndex = -1;
            return false;
        }
    }
}
