using System;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Data;
using Game.Generated;

namespace Domains.Adventure
{
    [Dependency]
    public sealed class AdventureService : IDisposable
    {
        public AdventureSession CurrentAdventure { get; private set; }

        public void StartNew(ECharacter character)
        {
            CharacterModel characterModel = DBManager.Instance.Character.Get(character);
            
            CurrentAdventure = new AdventureSession(character);
        }

        public void Dispose()
        {
            CurrentAdventure = null;
        }
    }
}
