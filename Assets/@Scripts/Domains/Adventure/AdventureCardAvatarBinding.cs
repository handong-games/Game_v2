using System;

namespace Domains.Adventure
{
    // Role:
    // Carries the UI avatar object that gameplay systems should bind to one Adventure card.
    // The object is created by the screen, but AbilitySystem mutation is handled by game flow.
    public sealed class AdventureCardAvatarBinding
    {
        public AdventureCardAvatarBinding(
            uint cardId,
            object avatar)
        {
            CardId = cardId;
            Avatar = avatar ?? throw new ArgumentNullException(nameof(avatar));
        }

        public uint CardId { get; }
        public object Avatar { get; }
    }
}
