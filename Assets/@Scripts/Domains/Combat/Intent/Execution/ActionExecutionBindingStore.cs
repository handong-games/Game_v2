using System;
using System.Collections.Generic;
using Domains.Combat;
using Domains.Combat.Intent.Data;
using Gameplay.GAS;
using UnityEngine;

namespace Domains.Combat.Intent.Execution
{
    public sealed class ActionExecutionBindingStore
    {
        private readonly Dictionary<CombatCard, Dictionary<IntentActionModel, GameplayAbilitySpecHandle>>
            _handlesByMonster = new();

        public void Clear()
        {
            _handlesByMonster.Clear();
        }

        public bool Bind(
            CombatCard monster,
            IntentActionModel actionModel,
            GameplayAbilitySpecHandle abilitySpecHandle)
        {
            if (monster == null)
                throw new ArgumentNullException(nameof(monster));

            if (actionModel == null)
                throw new ArgumentNullException(nameof(actionModel));

            if (!abilitySpecHandle.IsValid)
            {
                Debug.LogError(
                    $"Cannot bind invalid ability spec handle. Monster: {monster.CardId}, Action: {actionModel.name}.");
                return false;
            }

            if (!_handlesByMonster.TryGetValue(monster, out Dictionary<IntentActionModel, GameplayAbilitySpecHandle> handles))
            {
                handles = new Dictionary<IntentActionModel, GameplayAbilitySpecHandle>();
                _handlesByMonster.Add(monster, handles);
            }

            if (handles.ContainsKey(actionModel))
            {
                Debug.LogError(
                    $"Duplicate action execution binding. Monster: {monster.CardId}, Action: {actionModel.name}.");
                return false;
            }

            handles.Add(actionModel, abilitySpecHandle);
            return true;
        }

        public bool TryGetHandle(
            CombatCard monster,
            IntentActionModel actionModel,
            out GameplayAbilitySpecHandle abilitySpecHandle)
        {
            abilitySpecHandle = GameplayAbilitySpecHandle.Invalid;

            if (monster == null || actionModel == null)
                return false;

            return _handlesByMonster.TryGetValue(monster, out Dictionary<IntentActionModel, GameplayAbilitySpecHandle> handles) &&
                   handles.TryGetValue(actionModel, out abilitySpecHandle);
        }
    }
}
