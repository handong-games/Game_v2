using System;
using Domains.Combat;

namespace Domains.Adventure
{
    public sealed class CardHealthBinding
    {
        private readonly CreatureState _creature;

        public CardHealthBinding(CreatureState creature)
        {
            _creature = creature;
        }

        public bool IsValid => _creature != null;
        public int CurrentHealth => _creature?.CurrentHealth ?? 0;
        public int MaxHealth => _creature?.MaxHealth ?? 1;

        public event Action<int, int> HealthChanged
        {
            add
            {
                if (_creature != null)
                {
                    _creature.HealthChanged += value;
                }
            }
            remove
            {
                if (_creature != null)
                {
                    _creature.HealthChanged -= value;
                }
            }
        }

        public event Action<HealthPreviewDto> HealthPreviewChanged
        {
            add
            {
                if (_creature != null)
                {
                    _creature.HealthPreviewChanged += value;
                }
            }
            remove
            {
                if (_creature != null)
                {
                    _creature.HealthPreviewChanged -= value;
                }
            }
        }
    }
}
