using System;
using System.Collections.Generic;
using CardActor = Domains.Card.Card;

namespace Domains.Adventure
{
    // Role:
    // Applies screen-provided avatar objects to Adventure card AbilitySystems.
    // It owns the SetAvatar/ClearAvatar and avatar registry lifetime for visible board cards.
    public sealed class AdventureCardAvatarBindingFlow : IDisposable
    {
        private readonly AdventureCards _cards;
        private readonly AdventureCardAvatarRegistry _avatarRegistry;
        private readonly List<BoundAvatar> _boundAvatars = new();

        public AdventureCardAvatarBindingFlow(
            AdventureCards cards,
            AdventureCardAvatarRegistry avatarRegistry)
        {
            _cards = cards ?? throw new ArgumentNullException(nameof(cards));
            _avatarRegistry = avatarRegistry ?? throw new ArgumentNullException(nameof(avatarRegistry));
        }

        public void Bind(IReadOnlyList<AdventureCardAvatarBinding> bindings)
        {
            Unbind();

            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            for (int i = 0; i < bindings.Count; i++)
            {
                AdventureCardAvatarBinding binding = bindings[i];
                if (binding == null)
                    throw new InvalidOperationException("Adventure card avatar binding is missing.");

                if (!_cards.TryGet(binding.CardId, out CardActor card))
                    throw new InvalidOperationException(
                        $"Adventure card runtime object is missing: {binding.CardId}");

                if (card.AbilitySystem == null)
                    throw new InvalidOperationException(
                        $"Adventure card ability system is missing: {binding.CardId}");

                card.AbilitySystem.SetAvatar(binding.Avatar);
                _avatarRegistry.Register(binding.CardId, binding.Avatar);
                _boundAvatars.Add(new BoundAvatar(card, binding.Avatar));
            }
        }

        public void Unbind()
        {
            for (int i = _boundAvatars.Count - 1; i >= 0; i--)
            {
                BoundAvatar bound = _boundAvatars[i];

                if (ReferenceEquals(
                        bound.Card.AbilitySystem?.ActorInfo.Avatar,
                        bound.Avatar))
                {
                    bound.Card.AbilitySystem.ClearAvatar();
                }

                _avatarRegistry.Unregister(bound.Avatar);
            }

            _boundAvatars.Clear();
        }

        public void Dispose()
        {
            Unbind();
        }

        private readonly struct BoundAvatar
        {
            public BoundAvatar(
                CardActor card,
                object avatar)
            {
                Card = card;
                Avatar = avatar;
            }

            public CardActor Card { get; }
            public object Avatar { get; }
        }
    }
}
