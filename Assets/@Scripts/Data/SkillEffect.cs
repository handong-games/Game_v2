using System;
using UnityEngine;

namespace Game.Data
{
    [Serializable]
    public sealed class SkillEffect
    {
        [SerializeField]
        private string _effectId;

        [SerializeField]
        private int _amount;

        public string EffectId => _effectId;
        public int Amount => _amount;
    }
}
