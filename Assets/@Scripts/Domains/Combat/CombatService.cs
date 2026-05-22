using System;
using System.Collections.Generic;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Data;
using Game.Generated;

namespace Domains.Combat
{
    [Dependency]
    public sealed class CombatService : IDisposable
    {
        private readonly Dictionary<uint, CreatureState> _creaturesByCardId = new();
        private readonly List<CreatureState> _playerCreatures = new();
        private readonly List<CreatureState> _enemyCreatures = new();

        public IReadOnlyList<CreatureState> PlayerCreatures => _playerCreatures;
        public IReadOnlyList<CreatureState> EnemyCreatures => _enemyCreatures;
        public ECombatSide CurrentSide { get; private set; }
        public int TurnNumber { get; private set; }

        public void Initialize()
        {
            CreatureState.ResetId();
            _creaturesByCardId.Clear();
            _playerCreatures.Clear();
            _enemyCreatures.Clear();
            CurrentSide = ECombatSide.Player;
            TurnNumber = 1;
        }

        public CreatureState RegisterPlayer(uint cardId, CharacterModel character)
        {
            CreatureState creature = new(
                ECombatSide.Player,
                ECreatureKind.Player,
                character.MaxHp);

            RegisterCreature(cardId, creature);
            _playerCreatures.Add(creature);
            return creature;
        }

        public CreatureState RegisterMonster(uint cardId, MonsterModel monster)
        {
            CreatureState creature = new(
                ECombatSide.Enemy,
                ECreatureKind.Monster,
                monster.MaxHp);

            RegisterCreature(cardId, creature);
            _enemyCreatures.Add(creature);
            return creature;
        }

        public bool TryGetCreature(uint cardId, out CreatureState creature)
        {
            return _creaturesByCardId.TryGetValue(cardId, out creature);
        }

        public bool ApplySkill(ECharacterSkill skillId, uint targetCardId)
        {
            CharacterSkillModel skill = DBManager.Instance.CharacterSkill.Get(skillId);
            return ApplySkill(skill, targetCardId);
        }

        public bool ApplySkill(CharacterSkillModel skill, uint targetCardId)
        {
            if (skill == null || !TryGetCreature(targetCardId, out CreatureState target))
                return false;

            SkillEffect[] effects = skill.Effects;
            if (effects == null)
                return false;

            for (int i = 0; i < effects.Length; i++)
            {
                target.ApplyDamage(effects[i].Amount);
            }

            return true;
        }

        public EndTurnResultDto EndPlayerTurn()
        {
            CurrentSide = ECombatSide.Enemy;
            CurrentSide = ECombatSide.Player;
            TurnNumber++;

            return new EndTurnResultDto(TurnNumber);
        }

        public void Dispose()
        {
            _creaturesByCardId.Clear();
            _playerCreatures.Clear();
            _enemyCreatures.Clear();
            CurrentSide = ECombatSide.Player;
            TurnNumber = 0;
        }

        private void RegisterCreature(uint cardId, CreatureState creature)
        {
            _creaturesByCardId[cardId] = creature;
        }
    }
}
