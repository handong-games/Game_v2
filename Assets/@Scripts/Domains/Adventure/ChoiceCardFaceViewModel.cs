using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Card Face/Choice")]
    public sealed class ChoiceCardFaceViewModel : CardFaceViewModel
    {
        [SerializeField]
        private EChoiceCardType _styleType;

        [SerializeField]
        private LocalizedString _label;

        [SerializeField]
        private Sprite _icon;

        public override ECardFaceType FaceType => ECardFaceType.Choice;
        public EChoiceCardType StyleType => _styleType;
        public LocalizedString Label => _label;
        public Sprite Icon => _icon;

        public static ChoiceCardFaceViewModel CreateRuntime(
            EChoiceCardType styleType,
            Sprite icon,
            LocalizedString label)
        {
            ChoiceCardFaceViewModel viewModel = CreateInstance<ChoiceCardFaceViewModel>();
            viewModel._styleType = styleType;
            viewModel._icon = icon;
            viewModel._label = label;
            return viewModel;
        }
    }
}
