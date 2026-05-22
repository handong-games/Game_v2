using System;
using UnityEngine;
using Gameplay.GAS;

namespace Game.Data
{
    [Serializable]
    public struct GameplayTagReference
    {
        [SerializeField]
        private string _name;

        public GameplayTagReference(string name)
        {
            _name = name;
        }

        public string Name => _name;
        public GameplayTag Tag => string.IsNullOrWhiteSpace(_name)
            ? default
            : GameplayTag.Request(_name);
    }
}
