using Gameplay.GAS;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Executes skill activation and skill target confirmation.
    // It uses AdventureInputState to separate skill targeting from choice selection.
    public sealed class AdventureSkillFlow
    {
        private readonly AdventureInputState _input;
        private readonly AdventurePlayer _player;
        private readonly AdventureCards _cards;

        public AdventureSkillFlow(
            AdventureInputState input,
            AdventurePlayer player,
            AdventureCards cards)
        {
            _input = input;
            _player = player;
            _cards = cards;
        }

        public bool UseSkill(GameplayAbilitySpecHandle handle)
        {
            CardActor playerCard = _player.PlayerCard;
            if (playerCard == null)
                return false;

            return playerCard.AbilitySystem.TryActivateAbility(handle);
        }

        public void BeginSkillTargeting(GameplayAbilitySpecHandle handle)
        {
            _input.EnterSkillTargetSelection(handle);
        }

        public bool UseSelectedSkillOnTarget(uint targetCardId)
        {
            GameplayAbilitySpecHandle handle = _input.SelectedSkillHandle;
            if (!handle.IsValid)
                return false;

            return UseSkillOnTarget(handle, targetCardId);
        }

        public bool UseSkillOnTarget(GameplayAbilitySpecHandle handle, uint targetCardId)
        {
            CardActor playerCard = _player.PlayerCard;
            if (playerCard == null)
                return false;

            if (!_cards.TryGet(targetCardId, out CardActor targetCard))
                return false;

            GameplayEventData eventData = new(default)
            {
                Instigator = playerCard.AbilitySystem,
                Target = targetCard.AbilitySystem,
            };

            bool result = playerCard.AbilitySystem.TriggerAbilityFromGameplayEvent(handle, eventData);
            _input.Clear();
            return result;
        }
    }
}
