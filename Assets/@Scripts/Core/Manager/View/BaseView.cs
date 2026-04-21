using System;
using UnityEngine.UIElements;

namespace Game.Core.Managers.View
{
    public abstract class BaseView : IDisposable
    {
        private VisualElement _root;
        
        public VisualElement Root => _root;

        public void Bind(VisualElement root)
        {
            _root = root;
            _root.RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            _root.RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
            OnBind(root);
        }

        public virtual void SetVisible(bool visible)
        {
            if (_root == null)
                return;

            _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected abstract void OnBind(VisualElement root);

        protected virtual void OnAttachedToPanel(AttachToPanelEvent evt)
        {
        }

        protected virtual void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
        }

        protected TField BindElement<TField>(string name)
            where TField : VisualElement
        {
            return Root.Q<TField>(name);
        }

        protected TField Bind<TField, TValue>(string name, EventCallback<ChangeEvent<TValue>> handler)
            where TField : BaseField<TValue>
        {
            TField field = BindElement<TField>(name);
            field?.RegisterValueChangedCallback(handler);
            return field;
        }

        protected void Unbind<TField, TValue>(TField field, EventCallback<ChangeEvent<TValue>> handler)
            where TField : BaseField<TValue>
        {
            field?.UnregisterValueChangedCallback(handler);
        }

        public virtual void Dispose()
        {
            if (_root == null)
                return;

            _root.UnregisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            _root.UnregisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }
    }
}
