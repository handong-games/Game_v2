using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Card Face/Portrait")]
    public sealed class PortraitCardFaceModel : CardFaceModel
    {
        [SerializeField]
        private LocalizedString _localizedName;

        [SerializeField]
        private Sprite _portrait;

        public LocalizedString LocalizedName => _localizedName;
        public Sprite Portrait => _portrait;
    }
}
