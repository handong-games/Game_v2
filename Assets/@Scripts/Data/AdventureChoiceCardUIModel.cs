using UnityEngine;
using UnityEngine.Localization;

namespace Game.Data
{
    // Role:
    // Defines the UI-facing display data for one adventure choice card type.
    [CreateAssetMenu(menuName = "Game/Adventure/Cards/Choice Card UI Model")]
    public sealed class AdventureChoiceCardUIModel : AbstractModel<EChoiceCardType>
    {
        [SerializeField]
        private LocalizedString _displayName;

        [SerializeField]
        private Sprite _icon;

        [SerializeField]
        private string _ussClassName;

        public EChoiceCardType ChoiceType => Id;
        public LocalizedString DisplayName => _displayName;
        public Sprite Icon => _icon;
        public string UssClassName => _ussClassName;
    }
}
