using System;
using System.Collections.Generic;
using Domains.Adventure;
using Game.AbilitySystem;
using Game.AbilitySystem.Attributes;
using Game.Core.Managers.DB;
using Game.Data;
using Game.Messages;
using Gameplay.GAS;

namespace Domains.Combat
{
    public sealed class CombatService : IDisposable
    {
        private readonly List<CombatCard>[] _cardsBySide = new List<CombatCard>[(int)ECombatSide.Count];
        private readonly Dictionary<object, CombatCard> _combatCardByAvatar = new();
        private readonly HashSet<object> _resolvedDeaths = new();
        private readonly AdventureCombatEvents _events;
        private readonly DBManager _dbManager;
        private readonly GameplayMessageManager _gameplayMessageManager;
        private IDisposable _combatDeathSubscription;
        private ECombatSide _currentSide;
        private bool _enemyTurnCompletionRequested;
        private bool _combatEnded;

        public int RoundNumber { get; private set; }
        public ECombatSide CurrentSide => _currentSide;
        public IReadOnlyList<CombatCard> PlayerCards => _cardsBySide[(int)ECombatSide.Player];
        public IReadOnlyList<CombatCard> EnemyCards => _cardsBySide[(int)ECombatSide.Enemy];

        public CombatService(
            AdventureCombatEvents events,
            DBManager dbManager,
            GameplayMessageManager gameplayMessageManager)
        {
            _events = events;
            _dbManager = dbManager;
            _gameplayMessageManager = gameplayMessageManager;

            for (int i = 0; i < _cardsBySide.Length; i++)
            {
                _cardsBySide[i] = new List<CombatCard>();
            }
        }
        
        public void ReadyCombat(IReadOnlyList<CombatCard> combatCards)
        {
            if (combatCards == null)
                throw new ArgumentNullException(nameof(combatCards));

            EnsureDeathSubscription();
            ClearCards();
            _combatCardByAvatar.Clear();
            _resolvedDeaths.Clear();
            _combatEnded = false;
            _enemyTurnCompletionRequested = false;
            
            for (int i = 0; i < combatCards.Count; i++)
            {
                CombatCard combatCard = combatCards[i];
                if (combatCard == null)
                    continue;

                combatCard.AbilitySystem.SetAvatar(combatCard);
                _cardsBySide[(int)combatCard.Side].Add(combatCard);
                _combatCardByAvatar[combatCard] = combatCard;
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
            if (_combatEnded)
                return;

            EndCurrentTurn();
            if (_combatEnded)
                return;

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
            _combatDeathSubscription?.Dispose();
            _combatDeathSubscription = null;
            _combatCardByAvatar.Clear();
            _resolvedDeaths.Clear();
            ClearCards();
            _currentSide = ECombatSide.Enemy;
            RoundNumber = 0;
            _combatEnded = false;
        }

        private ECombatSide GetNextSide()
        {
            return _currentSide == ECombatSide.Player
                ? ECombatSide.Enemy
                : ECombatSide.Player;
        }

        private void StartPlayerTurn()
        {
            if (_combatEnded)
                return;

            RoundNumber++;
        }

        private void EndCurrentTurn()
        {
            if (_combatEnded)
                return;

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
            if (_combatEnded)
                return;

            IReadOnlyList<CombatCard> enemyCards = GetCards(ECombatSide.Enemy);
            AbilitySystemComponent playerAbilitySystem = PlayerCards[0].AbilitySystem;
            _enemyTurnCompletionRequested = false;

            for (int i = 0; i < enemyCards.Count; i++)
            {
                if (_combatEnded)
                    return;

                CombatCard combatCard = enemyCards[i];
                if (!IsAlive(combatCard))
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
            if (_combatEnded)
                return;

            if (_currentSide != ECombatSide.Enemy)
                return;

            if (_enemyTurnCompletionRequested)
                return;

            _enemyTurnCompletionRequested = true;
            _events.EnemyTurnBannerRequested?.Invoke();
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
            CombatCardAbilityTable table = _dbManager.CombatCardAbility;

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

        private void EnsureDeathSubscription()
        {
            _combatDeathSubscription ??=
                _gameplayMessageManager.Subscribe<GameplayDeathMessage>(
                    GameplayMessageTags.CombatDeath,
                    OnCombatDeathMessage);
        }

        private void OnCombatDeathMessage(GameplayDeathMessage message)
        {
            if (_combatEnded)
                return;

            if (message.Avatar == null)
                return;

            if (!_combatCardByAvatar.TryGetValue(message.Avatar, out CombatCard combatCard))
                return;

            if (!_resolvedDeaths.Add(message.Avatar))
                return;

            CancelPendingActionResolution();
            ClearPendingExecutionQueue();

            if (combatCard.Side == ECombatSide.Player)
            {
                CompleteCombat(ECombatEndResult.Defeat);
                return;
            }

            if (CountAliveEnemies() == 0)
            {
                CompleteCombat(ECombatEndResult.Victory);
            }
        }

        private void CompleteCombat(ECombatEndResult result)
        {
            if (_combatEnded)
                return;

            _combatEnded = true;
            _events.ResultRequested?.Invoke(result);
        }

        private bool IsAlive(CombatCard combatCard)
        {
            if (combatCard == null)
                return false;

            return !combatCard.AbilitySystem.OwnedTags.HasTagExact(StateGameplayTags.Dead);
        }

        private int CountAliveEnemies()
        {
            int aliveCount = 0;
            IReadOnlyList<CombatCard> enemyCards = EnemyCards;

            for (int i = 0; i < enemyCards.Count; i++)
            {
                if (IsAlive(enemyCards[i]))
                    aliveCount++;
            }

            return aliveCount;
        }

        private void CancelPendingActionResolution()
        {
        }

        private void ClearPendingExecutionQueue()
        {
        }

        private void ClearCards()
        {
            for (int i = 0; i < _cardsBySide.Length; i++)
            {
                for (int j = 0; j < _cardsBySide[i].Count; j++)
                {
                    _cardsBySide[i][j]?.AbilitySystem.ClearAvatar();
                    _cardsBySide[i][j]?.Intent.ClearIntent();
                }

                _cardsBySide[i].Clear();
            }
        }

    }
}
