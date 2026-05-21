using System.Collections.Generic;
using Gameplay.GAS;
using UnityEngine;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Data/Ability Set")]
    public sealed class AbilitySetModel : ScriptableObject
    {
        [SerializeField]
        private GameplayAbility[] _abilities;

        public IReadOnlyList<GameplayAbility> Abilities => _abilities;

        public void GiveAbilities(AbilitySystemComponent abilitySystem)
        {
            if (abilitySystem == null || _abilities == null)
                return;

            for (int i = 0; i < _abilities.Length; i++)
            {
                GameplayAbility ability = _abilities[i];
                if (ability != null)
                    abilitySystem.GiveAbility(ability, 1);
            }
        }
    }
}
