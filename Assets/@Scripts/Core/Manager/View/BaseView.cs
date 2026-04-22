using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Core.Managers.View
{
    public readonly struct LogicalCanvasResult
    {
        public readonly float Width;
        public readonly float Height;
        public readonly float Scale;
        public readonly float OffsetX;
        public readonly float OffsetY;

        public LogicalCanvasResult(float width, float height, float scale, float offsetX, float offsetY)
        {
            Width = width;
            Height = height;
            Scale = scale;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }

    public abstract class BaseView : IDisposable
    {
        private VisualElement _root;
        private VisualElement _logicalRoot;
        
        public VisualElement Root => _root;
        public VisualElement LogicalRoot => _logicalRoot;

        public void Bind(VisualElement root)
        {
            Bind(root, root);
        }

        public void Bind(VisualElement root, VisualElement logicalRoot)
        {
            _root = root;
            _logicalRoot = logicalRoot;
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

        public virtual void ApplyResponsiveLayout(LogicalCanvasResult logicalCanvas)
        {
            if (_logicalRoot == null)
                return;

            _logicalRoot.style.left = logicalCanvas.OffsetX;
            _logicalRoot.style.top = logicalCanvas.OffsetY;
            _logicalRoot.style.width = logicalCanvas.Width;
            _logicalRoot.style.height = logicalCanvas.Height;
            _logicalRoot.style.transformOrigin = new TransformOrigin(new Length(0, LengthUnit.Pixel), new Length(0, LengthUnit.Pixel), 0f);
            _logicalRoot.style.scale = new Scale(new Vector2(logicalCanvas.Scale, logicalCanvas.Scale));
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
