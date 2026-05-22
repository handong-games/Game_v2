using Domains.Adventure;
using Domains.Card;
using Game.Data;
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

        public void Bind(CardViewModel viewModel)
        {
            ResolveSlots();
            Unbind();
            ClearFaces();
            AddFace(_frontSlot, viewModel.Front);
            AddFace(_backSlot, viewModel.Back);
            SetFace(viewModel.Face);
        }

        public void Unbind()
        {
            ResolveSlots();
            UnbindChildren(_frontSlot);
            UnbindChildren(_backSlot);
        }

        public void SetFace(ECardFace face)
        {
            EnableInClassList(FrontClass, face == ECardFace.Front);
            EnableInClassList(BackClass, face == ECardFace.Back);
        }

        private void ResolveSlots()
        {
            _frontSlot ??= this.Q<VisualElement>(FrontSlotName);
            _backSlot ??= this.Q<VisualElement>(BackSlotName);
        }

        private void ClearFaces()
        {
            _frontSlot.Clear();
            _backSlot.Clear();
        }

        private static void AddFace(VisualElement slot, CardFaceViewModel viewModel)
        {
            VisualElement face = CardFaceWidgetFactory.Create(viewModel);
            if (face != null)
            {
                slot.Add(face);
            }
        }

        private static void UnbindChildren(VisualElement slot)
        {
            foreach (VisualElement child in slot.Children())
            {
                if (child is ICardFaceWidget faceWidget)
                {
                    faceWidget.Unbind();
                }
            }
        }
    }
}
