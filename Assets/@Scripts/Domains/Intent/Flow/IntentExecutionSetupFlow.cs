using System.Collections.Generic;
using Domains.Adventure;
using Domains.Intent.Data;
using Domains.Intent.Execution;
using Game.Data;
using Gameplay.GAS;
using System;
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
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _bindings = bindings ?? throw new ArgumentNullException(nameof(bindings));
        }

        public void Setup(uint enemyCardId)
        {
            if (!_cards.TryGet(enemyCardId, out CardActor enemyCard))
                throw new InvalidOperationException(
                    $"Cannot setup intent execution because enemy card is missing. Card: {enemyCardId}.");

            if (enemyCard.Model is not MonsterModel monsterModel)
                throw new InvalidOperationException(
                    $"Intent execution setup requires MonsterModel. Card: {enemyCardId}.");

            IReadOnlyList<MonsterActionModel> actionSequence = monsterModel.ActionSequence;
            if (actionSequence.Count == 0)
                throw new InvalidOperationException(
                    $"Monster action sequence is empty. Card: {enemyCardId}, Monster: {monsterModel.name}.");

            for (int i = 0; i < actionSequence.Count; i++)
            {
                BindAction(enemyCard, actionSequence[i]);
            }
        }

        private void BindAction(CardActor enemyCard, MonsterActionModel actionModel)
        {
            if (actionModel == null)
                throw new InvalidOperationException(
                    $"Monster action sequence contains null action. Card: {enemyCard.CardId}.");

            if (actionModel.ExecutionAbility == null)
                throw new InvalidOperationException(
                    $"Monster action has no execution ability. Card: {enemyCard.CardId}, Action: {actionModel.name}.");

            if (_bindings.TryGetHandle(enemyCard.CardId, actionModel, out _))
                return;

            GameplayAbilitySpecHandle handle =
                enemyCard.AbilitySystem.GiveAbility(actionModel.ExecutionAbility);

            _bindings.Bind(enemyCard.CardId, actionModel, handle);
        }
    }
}
