using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Serialization;

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Card Face/Choice")]
    public sealed class ChoiceCardFaceModel : CardFaceModel
    {
        [FormerlySerializedAs("_choiceType")]
        [SerializeField]
        private EChoiceCardType _styleType;

        [SerializeField]
        private LocalizedString _label;

        [SerializeField]
        private Sprite _icon;

        public EChoiceCardType StyleType => _styleType;
        public LocalizedString Label => _label;
        public Sprite Icon => _icon;
    }
}
