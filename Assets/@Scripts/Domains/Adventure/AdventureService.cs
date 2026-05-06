using System;
using System.Collections.Generic;
using Domains.Event;
using Game.Core.Managers.DB;
using Game.Core.Managers.Dependency;
using Game.Core.Utility;
using Game.Data;
using Game.Generated;

namespace Domains.Adventure
{
    [Dependency]
    public sealed class AdventureService : IDisposable
    {
        [Inject]
        private CardDeckService _cardDeckService;

        public AdventureSession CurrentAdventure { get; private set; }

        public AdventureSession StartNew(ECharacter character)
        {
            DBManager.Instance.Character.Get(character);

            AdventureModel adventure = DBManager.Instance.Adventure.Get(EAdventure.Default);
            CurrentAdventure = new AdventureSession(
                character,
                adventure.Id,
                adventure.CardDeckId,
                RandomUtility.CreateSeed());

            return CurrentAdventure;
        }

        public void StartFirstStage()
        {
            AdventureModel adventure = DBManager.Instance.Adventure.Get(CurrentAdventure.AdventureId);
            IReadOnlyList<CardState> cards = _cardDeckService.DrawCard(adventure.StartDrawCount);

            AdventureEvents.CardsDrawn?.Invoke(cards);
        }

        public void Dispose()
        {
            CurrentAdventure = null;
        }
    }
}
