using System;
using System.Collections.Generic;
using Game.Core.Managers.View;
using Game.Core.Ports;
using UnityEngine;

namespace Game.Core.Navigation
{
    public sealed class SceneViewNavigator : ISceneViewNavigator, IDisposable
    {
        private readonly IViewHost _viewHost;
        private readonly List<BaseView> _views = new();

        public SceneViewNavigator(IViewHost viewHost)
        {
            _viewHost = viewHost ?? throw new ArgumentNullException(nameof(viewHost));
        }

        public BaseView Current => _views.Count > 0 ? _views[_views.Count - 1] : null;

        public void Show(BaseView view)
        {
            if (view == null)
                return;

            if (_views.Contains(view))
            {
                Debug.LogError($"Failed to show view {view.GetType().Name}: view is already in the scene navigator stack.");
                return;
            }

            Current?.Hide();
            _viewHost.Attach(view);
            view.Show();
            _views.Add(view);
        }

        public void Hide(BaseView view)
        {
            if (view == null)
                return;

            int index = _views.IndexOf(view);
            if (index < 0)
                return;

            bool wasCurrent = index == _views.Count - 1;
            _views.RemoveAt(index);
            view.Hide();
            _viewHost.Detach(view);

            if (wasCurrent)
            {
                Current?.Show();
            }
        }

        public void HideCurrent()
        {
            Hide(Current);
        }

        public void HideAll()
        {
            for (int i = _views.Count - 1; i >= 0; i--)
            {
                _views[i].Hide();
                _viewHost.Detach(_views[i]);
            }

            _views.Clear();
        }

        public void Dispose()
        {
            _views.Clear();
        }
    }
}
