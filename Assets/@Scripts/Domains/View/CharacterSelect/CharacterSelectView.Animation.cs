using UnityEngine;

namespace Domains.CharacterSelect
{
    public partial class CharacterSelectView
    {
        private const string BlockerActiveClass = "character-select__blocker--active";
        
        private async Awaitable PlayIntroAnimation()
        {
            await Awaitable.NextFrameAsync();
            
            _blockerBackground?.AddToClassList(BlockerActiveClass);
        }
    }
}
