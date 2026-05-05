using System;
using Domains.Event;
using Domains.Scene;
using Game.Core.Managers.Dependency;

namespace Domains.Adventure
{
    [Dependency(nameof(AdventureScene))]
    public sealed class AdventureDirector : IDisposable
    {
        [Inject]
        private AdventureService _adventureService;

        public void StartAdventure()
        {
            RegisterEvents();

            AdventureEvents.AdventureStarted?.Invoke();
        }

        public void Dispose()
        {
            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            AdventureEvents.IntroCompleted += OnIntroCompleted;
            AdventureEvents.CardDealCompleted += OnCardDealCompleted;
        }

        private void UnregisterEvents()
        {
            AdventureEvents.IntroCompleted -= OnIntroCompleted;
            AdventureEvents.CardDealCompleted -= OnCardDealCompleted;
        }

        private void OnIntroCompleted()
        {
            _adventureService.StartFirstStage();
        }

        private void OnCardDealCompleted()
        {
            AdventureEvents.TurnBannerRequested?.Invoke();
        }
    }
}
