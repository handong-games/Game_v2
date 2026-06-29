using UnityEngine;

namespace Domains.Adventure
{
    // Role:
    // Handles Adventure skill selection and target confirmation for AdventureView.
    public sealed partial class AdventureView
    {
        private async Awaitable ConfirmSkillTarget()
        {
            int screenLifetimeVersion = _screenLifetimeVersion;
            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return;

            if (!_skillUIFlow.TryGetHoveredCard(out UnityEngine.UIElements.VisualElement hoveredCard))
                return;

            if (_boardUIFlow.TryGetCardId(hoveredCard, out uint targetCardId))
            {
                if (!_controller.CanUseSkillOnTarget(_skillUIFlow.ActiveSkillHandle, targetCardId))
                    return;

                await _controller.UseSkillOnTarget(_skillUIFlow.ActiveSkillHandle, targetCardId);
            }

            if (!IsCurrentScreenLifetime(screenLifetimeVersion))
                return;

            _skillUIFlow.Clear();
        }
    }
}
