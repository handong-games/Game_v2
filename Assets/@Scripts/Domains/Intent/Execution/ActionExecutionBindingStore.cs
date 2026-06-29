using System;
using System.Collections.Generic;
using Domains.Intent.Data;
using Gameplay.GAS;

namespace Domains.Intent.Execution
{
    // Role:
    // Stores runtime bindings between monster actions and granted Ability handles.
    public sealed class ActionExecutionBindingStore
    {
        private readonly Dictionary<uint, Dictionary<MonsterActionModel, GameplayAbilitySpecHandle>>
            _handlesByMonsterCardId = new();

        public void Clear()
        {
            _handlesByMonsterCardId.Clear();
        }

        public bool Bind(
            uint monsterCardId,
            MonsterActionModel actionModel,
            GameplayAbilitySpecHandle abilitySpecHandle)
        {
            if (actionModel == null)
                throw new ArgumentNullException(nameof(actionModel));

            if (!abilitySpecHandle.IsValid)
            {
                throw new InvalidOperationException(
                    $"Cannot bind invalid ability spec handle. Monster: {monsterCardId}, Action: {actionModel.name}.");
            }

            if (!_handlesByMonsterCardId.TryGetValue(
                    monsterCardId,
                    out Dictionary<MonsterActionModel, GameplayAbilitySpecHandle> handles))
            {
                handles = new Dictionary<MonsterActionModel, GameplayAbilitySpecHandle>();
                _handlesByMonsterCardId.Add(monsterCardId, handles);
            }

            if (handles.ContainsKey(actionModel))
            {
                throw new InvalidOperationException(
                    $"Duplicate action execution binding. Monster: {monsterCardId}, Action: {actionModel.name}.");
            }

            handles.Add(actionModel, abilitySpecHandle);
            return true;
        }

        public bool TryGetHandle(
            uint monsterCardId,
            MonsterActionModel actionModel,
            out GameplayAbilitySpecHandle abilitySpecHandle)
        {
            abilitySpecHandle = GameplayAbilitySpecHandle.Invalid;

            if (actionModel == null)
                return false;

            return _handlesByMonsterCardId.TryGetValue(monsterCardId, out Dictionary<MonsterActionModel, GameplayAbilitySpecHandle> handles) &&
                   handles.TryGetValue(actionModel, out abilitySpecHandle);
        }
    }
}
