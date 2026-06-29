using UnityEngine;
using Game.Core.Managers.View;

namespace Domains.View.Widgets
{
    // Role:
    // Exposes health presentation for board card widgets that render a HealthWidget.
    // Choice card widgets intentionally do not implement this.
    public interface IAdventureHealthCardWidget
    {
        void SetHealth(int currentHealth, int maxHealth);
        Awaitable ShowHealthAsync(ViewTransitionManager transitionManager);
    }
}
