using Domains.Adventure;
using Domains.Card;
using Game.Data;
using System;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class CardWidget : VisualElement
    {
        private const string FrontSlotName = "card-v2-front-slot";
        private const string BackSlotName = "card-v2-back-slot";
        private const string FrontClass = "card-v2--front";
        private const string BackClass = "card-v2--back";

        private VisualElement _frontSlot;
        private VisualElement _backSlot;
        private CardViewModel _pendingViewModel;
        private CardFaceWidgetTemplates _faceTemplates;

        public CardWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        public void Bind(
            CardViewModel viewModel,
            CardFaceWidgetTemplates faceTemplates)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            if (faceTemplates == null)
                throw new ArgumentNullException(nameof(faceTemplates));

            _pendingViewModel = viewModel;
            _faceTemplates = faceTemplates;
            EnsureSlots();
            ApplyBinding();
        }

        public void Unbind()
        {
            UnbindChildren(_frontSlot);
            UnbindChildren(_backSlot);
            ClearFaces();
        }

        public void SetFace(ECardFace face)
        {
            EnableInClassList(FrontClass, face == ECardFace.Front);
            EnableInClassList(BackClass, face == ECardFace.Back);
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            EnsureSlots();
            ApplyBinding();
        }

        private void ApplyBinding()
        {
            if (_pendingViewModel == null)
                return;

            Unbind();
            AddFace(_frontSlot, _pendingViewModel.Front, _faceTemplates);
            AddFace(_backSlot, _pendingViewModel.Back, _faceTemplates);
            SetFace(_pendingViewModel.Face);
        }

        private void ClearFaces()
        {
            if (_frontSlot == null || _backSlot == null)
                return;

            _frontSlot.Clear();
            _backSlot.Clear();
        }

        private static void AddFace(
            VisualElement slot,
            CardFaceViewModel viewModel,
            CardFaceWidgetTemplates faceTemplates)
        {
            if (viewModel == null)
                return;

            VisualElement face = CardFaceWidgetFactory.Create(viewModel, faceTemplates);
            slot.Add(face);

            if (face is ICardFaceWidget faceWidget)
            {
                faceWidget.Bind(viewModel);
            }
        }

        private static void UnbindChildren(VisualElement slot)
        {
            if (slot == null)
                return;

            foreach (VisualElement child in slot.Children())
            {
                if (child is ICardFaceWidget faceWidget)
                {
                    faceWidget.Unbind();
                }
            }
        }

        private void InitializeSlots()
        {
            _frontSlot ??= this.Q<VisualElement>(FrontSlotName);
            _backSlot ??= this.Q<VisualElement>(BackSlotName);
        }

        private void EnsureSlots()
        {
            InitializeSlots();

            if (_frontSlot == null)
                throw new InvalidOperationException($"Required element missing: {FrontSlotName}");

            if (_backSlot == null)
                throw new InvalidOperationException($"Required element missing: {BackSlotName}");
        }
    }
}
