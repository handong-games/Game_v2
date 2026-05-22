using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Card Face/Portrait")]
    public sealed class PortraitCardFaceViewModel : CardFaceViewModel
    {
        [SerializeField]
        private LocalizedString _localizedName;

        [SerializeField]
        private Sprite _portrait;

        public override ECardFaceType FaceType => ECardFaceType.Portrait;
        public LocalizedString LocalizedName => _localizedName;
        public Sprite Portrait => _portrait;

        public static PortraitCardFaceViewModel CreateRuntime(
            LocalizedString localizedName,
            Sprite portrait)
        {
            PortraitCardFaceViewModel viewModel = CreateInstance<PortraitCardFaceViewModel>();
            viewModel._localizedName = localizedName;
            viewModel._portrait = portrait;
            return viewModel;
        }
    }
}
