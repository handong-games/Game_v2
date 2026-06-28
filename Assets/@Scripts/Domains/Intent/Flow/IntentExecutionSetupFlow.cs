using System.Collections.Generic;
using Domains.Adventure;
using Domains.Intent.Data;
using Domains.Intent.Execution;
using Game.Data;
using Gameplay.GAS;
using UnityEngine;
using CardActor = Domains.Card.Card;

namespace Domains.Intent.Flow
{
    // Role:
    // Grants authored monster action abilities and binds them to CardId-scoped intent actions for the current combat.
    public sealed class IntentExecutionSetupFlow
    {
        private readonly AdventureCards _cards;
        private readonly ActionExecutionBindingStore _bindings;

        public IntentExecutionSetupFlow(
            AdventureCards cards,
            ActionExecutionBindingStore bindings)
        {
            _cards = cards;
            _bindings = bindings;
        }

        public void Setup(uint enemyCardId)
        {
            if (!_cards.TryGet(enemyCardId, out CardActor enemyCard))
                return;

            if (enemyCard.Model is not MonsterModel monsterModel)
            {
                Debug.LogError($"Intent execution setup requires MonsterModel. Card: {enemyCardId}.");
                return;
            }

            IReadOnlyList<MonsterActionModel> actionSequence = monsterModel.ActionSequence;
            for (int i = 0; i < actionSequence.Count; i++)
            {
                BindAction(enemyCard, actionSequence[i]);
            }
        }

        private void BindAction(CardActor enemyCard, MonsterActionModel actionModel)
        {
            if (actionModel == null)
            {
                Debug.LogError($"Monster action sequence contains null action. Card: {enemyCard.CardId}.");
                return;
            }

            if (actionModel.ExecutionAbility == null)
            {
                Debug.LogError(
                    $"Monster action has no execution ability. Card: {enemyCard.CardId}, Action: {actionModel.name}.");
                return;
            }

            if (_bindings.TryGetHandle(enemyCard.CardId, actionModel, out _))
                return;

            GameplayAbilitySpecHandle handle =
                enemyCard.AbilitySystem.GiveAbility(actionModel.ExecutionAbility);

            _bindings.Bind(enemyCard.CardId, actionModel, handle);
        }
    }
}
