using Game.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;

namespace Domains.Combat.Intent.Data
{
    [CreateAssetMenu(menuName = "Game/Combat/Intent/Display")]
    public sealed class IntentDisplayModel : AbstractModel<EIntentDisplay>
    {
        [SerializeField]
        private AssetReferenceSprite _icon;

        [SerializeField]
        private LocalizedString _description;

        [SerializeField]
        private bool _requiresNumber;

        public AssetReferenceSprite Icon => _icon;
        public LocalizedString Description => _description;
        public bool RequiresNumber => _requiresNumber;
    }
}
