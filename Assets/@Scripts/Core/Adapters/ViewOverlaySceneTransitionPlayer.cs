using Game.Core.Managers.View;
using Game.Core.Ports;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Core.Adapters
{
    public sealed class ViewOverlaySceneTransitionPlayer : ISceneTransitionPlayer
    {
        private readonly ViewManager _viewManager;
        private readonly ViewTransitionManager _viewTransitionManager;

        public ViewOverlaySceneTransitionPlayer(
            ViewManager viewManager,
            ViewTransitionManager viewTransitionManager)
        {
            _viewManager = viewManager ?? throw new ArgumentNullException(nameof(viewManager));
            _viewTransitionManager = viewTransitionManager ?? throw new ArgumentNullException(nameof(viewTransitionManager));
        }

        public async Awaitable FadeOut()
        {
            VisualElement overlayLayer = _viewManager.OverlayLayer;
            overlayLayer.pickingMode = PickingMode.Position;

            try
            {
                await _viewTransitionManager.Play(overlayLayer, EViewTransitionType.FadeOut);
            }
            finally
            {
                overlayLayer.pickingMode = PickingMode.Ignore;
            }
        }
    }
}
