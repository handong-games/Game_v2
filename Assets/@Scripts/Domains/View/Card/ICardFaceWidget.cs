using Game.Data;

namespace Domains.View.Widgets
{
    public interface ICardFaceWidget
    {
        void Bind(CardFaceViewModel viewModel);
        void Unbind();
    }
}
