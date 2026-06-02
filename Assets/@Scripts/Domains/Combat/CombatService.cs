using System;
using System.Collections.Generic;
using Game.AbilitySystem;
using Game.AbilitySystem.Attributes;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Data;
using Gameplay.GAS;
using Domains.Event;

namespace Domains.Combat
{
    [Dependency]
    public sealed class CombatService : IDisposable
    {
        private readonly List<CombatCard>[] _cardsBySide =
            new List<CombatCard>[(int)ECombatSide.Count];

        public CombatService()
        {
            for (int i = 0; i < _cardsBySide.Length; i++)
            {
                _cardsBySide[i] = new List<CombatCard>();
            }
        }

        private ECombatSide _currentSide;
        private bool _enemyTurnCompletionRequested;

        public int RoundNumber { get; private set; }
        public ECombatSide CurrentSide => _currentSide;
        public IReadOnlyList<CombatCard> PlayerCards => _cardsBySide[(int)ECombatSide.Player];
        public IReadOnlyList<CombatCard> EnemyCards => _cardsBySide[(int)ECombatSide.Enemy];

        public void ReadyCombat(IReadOnlyList<CombatCard> combatCards)
        {
            if (combatCards == null)
                throw new ArgumentNullException(nameof(combatCards));

            ClearCards();
            
            for (int i = 0; i < combatCards.Count; i++)
            {
                CombatCard combatCard = combatCards[i];
                if (combatCard == null)
                    continue;

                _cardsBySide[(int)combatCard.Side].Add(combatCard);
            }
            
            int playerCount = _cardsBySide[(int)ECombatSide.Player].Count;
            if (playerCount != 1)
            {
                throw new InvalidOperationException(
                    $"ReadyCombat expects exactly one player card, but found {playerCount}.");
            }

            ApplyCombatEntries();

            _currentSide = ECombatSide.Player;
            RoundNumber = 1;
        }

        public void NextTurn()
        {
            EndCurrentTurn();
            _currentSide = GetNextSide();

            switch (_currentSide)
            {
                case ECombatSide.Player:
                    StartPlayerTurn();
                    break;
                case ECombatSide.Enemy:
                    StartEnemyTurn();
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported combat side: {_currentSide}.");
            }
        }

        public IReadOnlyList<CombatCard> GetCards(ECombatSide side)
        {
            return _cardsBySide[(int)side];
        }

        public void Dispose()
        {
            ClearCards();
            _currentSide = ECombatSide.Enemy;
            RoundNumber = 0;
        }

        private ECombatSide GetNextSide()
        {
            return _currentSide == ECombatSide.Player
                ? ECombatSide.Enemy
                : ECombatSide.Player;
        }

        private void StartPlayerTurn()
        {
            RoundNumber++;
        }

        private void EndCurrentTurn()
        {
            switch (_currentSide)
            {
                case ECombatSide.Player:
                    EndPlayerTurn();
                    break;
                case ECombatSide.Enemy:
                    EndEnemyTurn();
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported combat side: {_currentSide}.");
            }
        }

        private void EndPlayerTurn()
        {
            AbilitySystemComponent abilitySystem = PlayerCards[0].AbilitySystem;
            GameplayEventData eventData = new(AbilityGameplayTags.EventCombatTurnEnded)
            {
                Instigator = abilitySystem,
            };

            abilitySystem.HandleGameplayEvent(eventData);
        }

        private void EndEnemyTurn()
        {
        }

        private void StartEnemyTurn()
        {
            IReadOnlyList<CombatCard> enemyCards = GetCards(ECombatSide.Enemy);
            AbilitySystemComponent playerAbilitySystem = PlayerCards[0].AbilitySystem;
            _enemyTurnCompletionRequested = false;

            for (int i = 0; i < enemyCards.Count; i++)
            {
                CombatCard combatCard = enemyCards[i];
                if (combatCard == null)
                    continue;

                GameplayEventData eventData = new(AbilityGameplayTags.EventTurnStarted)
                {
                    Instigator = combatCard.AbilitySystem,
                    Target = playerAbilitySystem,
                    OptionalObject = new CombatTurnActionContext(OnEnemyTurnActionCompleted),
                };

                combatCard.AbilitySystem.HandleGameplayEvent(eventData);
            }
        }

        private void OnEnemyTurnActionCompleted()
        {
            if (_currentSide != ECombatSide.Enemy)
                return;

            if (_enemyTurnCompletionRequested)
                return;

            _enemyTurnCompletionRequested = true;
            AdventureEvents.EnemyTurnBannerRequested?.Invoke();
        }

        private void ApplyCombatEntries()
        {
            for (int sideIndex = 0; sideIndex < _cardsBySide.Length; sideIndex++)
            {
                List<CombatCard> sideCards = _cardsBySide[sideIndex];

                for (int i = 0; i < sideCards.Count; i++)
                {
                    ApplyCombatEntry(sideCards[i]);
                }
            }
        }

        private void ApplyCombatEntry(CombatCard combatCard)
        {
            CombatCardAbilityTable table = DBManager.Instance.CombatCardAbility;

            if (!table.TryGet(combatCard.Card.Model, out CombatCardAbilityEntry entry))
            {
                if (combatCard.Side == ECombatSide.Enemy)
                {
                    throw new InvalidOperationException(
                        $"Enemy combat ability entry not found. CardModel: {combatCard.Card.Model.Name}.");
                }

                return;
            }

            ApplyCombatAttributeSets(combatCard, entry);
            GrantCombatAbilities(combatCard, entry);
        }

        private void ApplyCombatAttributeSets(
            CombatCard combatCard,
            CombatCardAbilityEntry entry)
        {
            IReadOnlyList<AttributeSetDefaultsDefinition> definitions = entry.AttributeSetDefaults;
            HashSet<Type> createdSetTypes = new();

            for (int i = 0; i < definitions.Count; i++)
            {
                AttributeSetDefaultsDefinition definition = definitions[i];
                Type setType = definition.GetAttributeSetType();
                if (!createdSetTypes.Add(setType))
                    continue;

                AttributeSet attributeSet = Activator.CreateInstance(setType) as AttributeSet;
                definition.ApplyTo(attributeSet);
                combatCard.AbilitySystem.AddAttributeSet(attributeSet);
            }
        }

        private void GrantCombatAbilities(
            CombatCard combatCard,
            CombatCardAbilityEntry entry)
        {
            IReadOnlyList<GameplayAbility> abilities = entry.GrantedAbilities;
            for (int i = 0; i < abilities.Count; i++)
            {
                GameplayAbility ability = abilities[i];
                combatCard.AbilitySystem.GiveAbility(ability, 1);
            }
        }

        private void ClearCards()
        {
            for (int i = 0; i < _cardsBySide.Length; i++)
            {
                _cardsBySide[i].Clear();
            }
        }

    }
}
