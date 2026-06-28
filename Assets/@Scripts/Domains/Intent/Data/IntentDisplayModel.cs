using UnityEngine;
using UnityEngine.Localization;

namespace Domains.Intent.Data
{
    // Role:
    // Reusable authored presentation data for one intent item.
    [CreateAssetMenu(menuName = "Game/Intent/Display")]
    public sealed class IntentDisplayModel : ScriptableObject
    {
        [SerializeField]
        private Sprite _icon;

        [SerializeField]
        private LocalizedString _description;

        [SerializeField]
        private bool _requiresNumber;

        public Sprite Icon => _icon;
        public LocalizedString Description => _description;
        public bool RequiresNumber => _requiresNumber;
    }
}
