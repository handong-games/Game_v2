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
    public sealed class DBManager : BaseManager<DBManager>
    {
        private const string ModelTableLabel = "ModelTable";

        private AsyncOperationHandle<IList<Object>> _tableHandle;

        public CharacterTable Character { get; private set; }
        public CharacterSkillTable CharacterSkill { get; private set; }
        public RegionTable Region { get; private set; }

        protected override void OnInit()
        {
            _tableHandle = Addressables.LoadAssetsAsync<Object>(ModelTableLabel, null);
            IList<Object> assets = _tableHandle.WaitForCompletion();
            
            Character = assets.OfType<CharacterTable>().First();
            CharacterSkill = assets.OfType<CharacterSkillTable>().First();
            Region = assets.OfType<RegionTable>().First();
        }

        protected override void OnDispose()
        {
            Character = null;
            CharacterSkill = null;
            Region = null;

            if (_tableHandle.IsValid())
            {
                Addressables.Release(_tableHandle);
            }

            _tableHandle = default;
        }
    }
}
