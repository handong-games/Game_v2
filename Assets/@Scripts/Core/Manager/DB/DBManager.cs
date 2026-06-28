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
        public MonsterTable Monster { get; private set; }
        public AdventureChoiceCardUITable ChoiceCardUI { get; private set; }

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
            Monster = assets.OfType<MonsterTable>().First();
            ChoiceCardUI = assets.OfType<AdventureChoiceCardUITable>().FirstOrDefault();
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
            Monster?.ReleaseLoadedAssets();
            ChoiceCardUI?.ReleaseLoadedAssets();

            Character = null;
            CharacterSkill = null;
            Adventure = null;
            Card = null;
            Monster = null;
            ChoiceCardUI = null;

            if (_tableHandle.IsValid())
            {
                Addressables.Release(_tableHandle);
            }

            _tableHandle = default;
        }
    }
}
