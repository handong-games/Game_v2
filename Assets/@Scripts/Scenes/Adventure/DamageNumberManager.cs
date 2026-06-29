using System.Collections.Generic;
using Domains.View.Widgets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Scenes.Adventure
{
    // Role:
    // Owns creation, active lifetime, and reuse of floating damage number widgets.
    public sealed class DamageNumberManager
    {
        private readonly List<DamageNumberWidget> _pool = new();
        private readonly List<DamageNumberWidget> _active = new();

        public async Awaitable Play(
            VisualElement effectLayer,
            int amount,
            Vector2 position)
        {
            if (effectLayer == null || amount <= 0)
                return;

            DamageNumberWidget widget = Rent(effectLayer);
            widget.SetAmount(amount);
            widget.SetCenterPosition(position);

            try
            {
                await widget.Play();
            }
            finally
            {
                Return(widget);
            }
        }

        public void Clear()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Return(_active[i]);
            }
        }

        private DamageNumberWidget Rent(VisualElement effectLayer)
        {
            DamageNumberWidget widget = TakeFromPool();
            widget.Reset();
            effectLayer.Add(widget);
            _active.Add(widget);
            return widget;
        }

        private DamageNumberWidget TakeFromPool()
        {
            if (_pool.Count == 0)
                return new DamageNumberWidget();

            int lastIndex = _pool.Count - 1;
            DamageNumberWidget widget = _pool[lastIndex];
            _pool.RemoveAt(lastIndex);
            return widget;
        }

        private void Return(DamageNumberWidget widget)
        {
            if (widget == null || !_active.Remove(widget))
                return;

            widget.Reset();
            widget.RemoveFromHierarchy();
            _pool.Add(widget);
        }
    }
}
