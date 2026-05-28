using System;

namespace Gameplay.GAS
{
    public readonly struct GameplayTagFieldDefinition
    {
        public GameplayTagFieldDefinition(Type providerType, string fieldName, GameplayTag tag)
        {
            ProviderType = providerType;
            FieldName = fieldName;
            Tag = tag;
        }

        public Type ProviderType { get; }
        public string FieldName { get; }
        public GameplayTag Tag { get; }
    }
}
