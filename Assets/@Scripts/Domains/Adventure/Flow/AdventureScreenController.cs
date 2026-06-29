using Domains.View.Widgets;
using Game.Scenes.Adventure.Events;
using Game.Scenes.Adventure.Events.Flow;
using Gameplay.GAS;
using System;
using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Application controller for AdventureScreen commands.
    // It coordinates gameplay flows and presentation events but does not own runtime data directly.
    public sealed class AdventureScreenController
    {
        private readonly AdventureStageFlow _stageFlow;
        private readonly AdventureProgress _progress;
        private readonly AdventureEncounterStartFlow _encounterStartFlow;
        private readonly AdventureTurnFlow _turnFlow;
        private readonly AdventureCoinFlow _coinFlow;
        private readonly AdventureSkillFlow _skillFlow;
        private readonly AdventureCardAvatarBindingFlow _avatarBindingFlow;
        private readonly AdventurePresenter _presenter;
        private readonly AdventureGameEvents _events;
        private bool _initialized;

        public AdventureScreenController(
            AdventureStageFlow stageFlow,
            AdventureProgress progress,
            AdventureEncounterStartFlow encounterStartFlow,
            AdventureTurnFlow turnFlow,
            AdventureCoinFlow coinFlow,
            AdventureSkillFlow skillFlow,
            AdventureCardAvatarBindingFlow avatarBindingFlow,
            AdventurePresenter presenter,
            AdventureGameEvents events)
        {
            _stageFlow = stageFlow ?? throw new ArgumentNullException(nameof(stageFlow));
            _progress = progress ?? throw new ArgumentNullException(nameof(progress));
            _encounterStartFlow = encounterStartFlow ?? throw new ArgumentNullException(nameof(encounterStartFlow));
            _turnFlow = turnFlow ?? throw new ArgumentNullException(nameof(turnFlow));
            _coinFlow = coinFlow ?? throw new ArgumentNullException(nameof(coinFlow));
            _skillFlow = skillFlow ?? throw new ArgumentNullException(nameof(skillFlow));
            _avatarBindingFlow = avatarBindingFlow ?? throw new ArgumentNullException(nameof(avatarBindingFlow));
            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        public void InitializeAdventure()
        {
            _stageFlow.StartCurrentStage();
            _initialized = true;
        }

        public async Awaitable StartAdventure()
        {
            EnsureAdventureInitialized();

            bool presentationCompleted = await RequestInitialPresentation();

            if (!presentationCompleted)
                return;

            await ContinueAfterInitialPresentation();
        }

        private void EnsureAdventureInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    $"{nameof(InitializeAdventure)} must be called before {nameof(StartAdventure)}.");
        }

        private async Awaitable<bool> RequestInitialPresentation()
        {
            if (_events.Screen.InitialPresentationPrepared == null)
                throw new InvalidOperationException(
                    $"{nameof(AdventureScreenEvents.InitialPresentationPrepared)} is not bound.");

            return await _events.Screen.InitialPresentationPrepared.Invoke(
                _presenter.CreateInitialPresentation());
        }

        private async Awaitable ContinueAfterInitialPresentation()
        {
            _progress.MarkIntroCompleted();
            _stageFlow.OpenChoiceSelection();
            await _encounterStartFlow.TryStartImmediateEncounter();
        }

        public async Awaitable OnCardClicked(uint cardId)
        {
            await _encounterStartFlow.StartSelectedChoice(cardId);
        }

        public bool CanClickCard(uint cardId)
        {
            return _encounterStartFlow.CanStartSelectedChoice(cardId);
        }

        public void OnPouchClicked()
        {
            _coinFlow.FlipCoin();
        }

        public bool CanClickPouch()
        {
            return _coinFlow.CanFlipCoin();
        }

        public Awaitable OnEndTurnClicked()
        {
            return _turnFlow.EndPlayerTurnAndStartEnemyTurn();
        }

        public bool CanClickEndTurn()
        {
            return _turnFlow.CanEndPlayerTurn();
        }

        public Awaitable<bool> UseSkill(GameplayAbilitySpecHandle handle)
        {
            return _skillFlow.UseSkill(handle);
        }

        public bool CanUseSkill(GameplayAbilitySpecHandle handle)
        {
            return _skillFlow.CanUseSkill(handle);
        }

        public Awaitable<bool> UseSkillOnTarget(GameplayAbilitySpecHandle handle, uint targetCardId)
        {
            return _skillFlow.UseSkillOnTarget(handle, targetCardId);
        }

        public bool CanUseSkillOnTarget(GameplayAbilitySpecHandle handle, uint targetCardId)
        {
            return _skillFlow.CanUseSkillOnTarget(handle, targetCardId);
        }

        public void BindCardAvatars(
            System.Collections.Generic.IReadOnlyList<AdventureCardAvatarBinding> bindings)
        {
            _avatarBindingFlow.Bind(bindings);
        }

        public void UnbindCardAvatars()
        {
            _avatarBindingFlow.Unbind();
        }
    }
}
