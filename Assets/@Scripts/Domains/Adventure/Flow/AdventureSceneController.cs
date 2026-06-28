using Domains.Combat;
using Domains.View.Widgets;
using Game.Scenes.Adventure.Events;
using Gameplay.GAS;

namespace Domains.Adventure
{
    // Role:
    // Application controller for AdventureScene UI commands.
    // It coordinates flows and presentation but does not own runtime data directly.
    public sealed class AdventureSceneController
    {
        private readonly AdventureStageFlow _stageFlow;
        private readonly AdventureChoiceFlow _choiceFlow;
        private readonly AdventureEncounterFlow _encounterFlow;
        private readonly AdventureTurnFlow _turnFlow;
        private readonly AdventureCoinFlow _coinFlow;
        private readonly AdventureSkillFlow _skillFlow;
        private readonly AdventurePresenter _presenter;
        private readonly AdventureGameEvents _events;

        public AdventureSceneController(
            AdventureStageFlow stageFlow,
            AdventureChoiceFlow choiceFlow,
            AdventureEncounterFlow encounterFlow,
            AdventureTurnFlow turnFlow,
            AdventureCoinFlow coinFlow,
            AdventureSkillFlow skillFlow,
            AdventurePresenter presenter,
            AdventureGameEvents events)
        {
            _stageFlow = stageFlow;
            _choiceFlow = choiceFlow;
            _encounterFlow = encounterFlow;
            _turnFlow = turnFlow;
            _coinFlow = coinFlow;
            _skillFlow = skillFlow;
            _presenter = presenter;
            _events = events;
        }

        public void StartInitialStage()
        {
            _stageFlow.StartCurrentStage();
        }

        public AdventureEntryPresentationViewModel GetEntryPresentation()
        {
            return _presenter.CreateEntryPresentation();
        }

        public System.Collections.Generic.IReadOnlyList<AdventureSkillSlotViewModel> GetSkillSlots()
        {
            return _presenter.CreateSkillSlots();
        }

        public CombatTurnViewModel GetCombatTurnViewModel()
        {
            return _presenter.CreateCombatTurnViewModel();
        }

        public System.Collections.Generic.IReadOnlyList<AdventureBoardCardViewModel> GetBoardCards()
        {
            return _presenter.CreateBoardDisplayCards();
        }

        public System.Collections.Generic.IReadOnlyList<AdventureCardViewModel> GetRuntimeBoardCards()
        {
            return _presenter.CreateBoardCards();
        }

        public void OnInitialBoardShown()
        {
            if (!_choiceFlow.TryCommitImmediateEncounter(out AdventureChoiceCommitResult committed))
                return;

            StartEncounter(committed, refreshBoard: false);
        }

        public void OnCardClicked(uint cardId)
        {
            AdventureChoiceSelectionResult selection = _choiceFlow.Select(cardId);
            if (!selection.Selected)
                return;

            AdventureChoiceCommitResult committed = _choiceFlow.CommitSelection();
            StartEncounter(committed, refreshBoard: true);
        }

        private void StartEncounter(
            AdventureChoiceCommitResult committed,
            bool refreshBoard)
        {
            AdventureEncounterStartResult encounter = _encounterFlow.StartEncounter(committed);

            if (refreshBoard)
            {
                _events.Board.RefreshRequested?.Invoke(_presenter.CreateBoardCards());
            }

            if (encounter.EncounterType is AdventureEncounterType.Combat or AdventureEncounterType.Boss)
            {
                _turnFlow.StartInitialPlayerTurn();
            }
        }

        public void OnPouchClicked()
        {
            _coinFlow.FlipCoin();
        }

        public void OnEndTurnClicked()
        {
            _turnFlow.EndPlayerTurn();
        }

        public void OnEnemyTurnBannerCompleted()
        {
            _turnFlow.StartEnemyTurn();
        }

        public void OnIntentRevealCompleted()
        {
        }

        public void OnEnemyTurnCompleted()
        {
            _turnFlow.CompleteEnemyTurn();
        }

        public bool UseSkill(GameplayAbilitySpecHandle handle)
        {
            return _skillFlow.UseSkill(handle);
        }

        public bool UseSkillOnTarget(GameplayAbilitySpecHandle handle, uint targetCardId)
        {
            return _skillFlow.UseSkillOnTarget(handle, targetCardId);
        }
    }
}
