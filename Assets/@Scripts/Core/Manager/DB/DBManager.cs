using System.Collections.Generic;
using System.Linq;
using Game.Core.Managers;
using Game.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace Game.Core.Managers.DB
{
    public sealed class DBManager : System.IDisposable
    {
        private const string ModelTableLabel = "ModelTable";

        private AsyncOperationHandle<IList<Object>> _tableHandle;
        private bool _initialized;

        public CharacterTable Character { get; private set; }
        public CharacterSkillTable CharacterSkill { get; private set; }
        public AdventureTable Adventure { get; private set; }
        public CardTable Card { get; private set; }
        public CardDeckTable CardDeck { get; private set; }
        public MonsterTable Monster { get; private set; }
        public CombatCardAbilityTable CombatCardAbility { get; private set; }

        public void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;
            _tableHandle = Addressables.LoadAssetsAsync<Object>(ModelTableLabel, null);
            IList<Object> assets = _tableHandle.WaitForCompletion();
            
            Character = assets.OfType<CharacterTable>().First();
            CharacterSkill = assets.OfType<CharacterSkillTable>().First();
            Adventure = assets.OfType<AdventureTable>().First();
            Card = assets.OfType<CardTable>().First();
            CardDeck = assets.OfType<CardDeckTable>().First();
            Monster = assets.OfType<MonsterTable>().First();
            CombatCardAbility = assets.OfType<CombatCardAbilityTable>().First();
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            _initialized = false;
            Character?.ReleaseLoadedAssets();
            CharacterSkill?.ReleaseLoadedAssets();
            Adventure?.ReleaseLoadedAssets();
            Card?.ReleaseLoadedAssets();
            CardDeck?.ReleaseLoadedAssets();
            Monster?.ReleaseLoadedAssets();

            Character = null;
            CharacterSkill = null;
            Adventure = null;
            Card = null;
            CardDeck = null;
            Monster = null;
            CombatCardAbility = null;

            if (_tableHandle.IsValid())
            {
                Addressables.Release(_tableHandle);
            }

            _tableHandle = default;
        }
    }
}
