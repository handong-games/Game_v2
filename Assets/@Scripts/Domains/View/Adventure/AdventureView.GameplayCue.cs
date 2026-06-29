using System.Collections.Generic;

namespace Domains.Adventure
{
    // Role:
    // Bridges gameplay cue receivers to the current Adventure screen widgets.
    public sealed partial class AdventureView
    {
        public async void HandleCoinFlipCue(CoinFlipCueData data)
        {
            try
            {
                int screenLifetimeVersion = _screenLifetimeVersion;
                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                await _screenUIFlow.PlayCoinFlip(
                    data,
                    _screenWidgets.Pouch,
                    _screenWidgets.CoinStatus,
                    _screenWidgets.SkillSlots,
                    _screenWidgets.EndTurn,
                    () => IsCurrentScreenLifetime(screenLifetimeVersion));
            }
            catch (System.Exception exception)
            {
                LogAsyncException(exception);
            }
        }

        public async void HandleCoinChangeCue(CoinChangeCueData data)
        {
            try
            {
                int screenLifetimeVersion = _screenLifetimeVersion;
                if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                    return;

                await _screenUIFlow.PlayCoinChange(
                    data,
                    _screenWidgets.CoinStatus,
                    () => IsCurrentScreenLifetime(screenLifetimeVersion));
            }
            catch (System.Exception exception)
            {
                LogAsyncException(exception);
            }
        }

        private void BindGameplayCueReceivers(IReadOnlyList<AdventureCardViewModel> runtimeCards)
        {
            IReadOnlyList<AdventureCardAvatarBinding> bindings =
                _gameplayCueAvatarBinder.CreateBindings(
                    runtimeCards,
                    this,
                    _boardUIFlow);

            _controller.BindCardAvatars(bindings);
        }

        private void UnbindGameplayCueReceivers()
        {
            _controller.UnbindCardAvatars();
        }
    }
}
