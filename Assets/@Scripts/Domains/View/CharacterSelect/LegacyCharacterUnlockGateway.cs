using Game.Core.Managers.Save;
using Game.Generated;

namespace Domains.CharacterSelect
{
    public sealed class LegacyCharacterUnlockGateway : ICharacterUnlockGateway
    {
        private readonly SaveManager _saveManager;

        public LegacyCharacterUnlockGateway(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        public bool IsUnlocked(ECharacter characterId)
        {
            ProgressState progress = _saveManager.GetState<ProgressState>();
            return progress.IsUnlocked(characterId);
        }
    }
}
