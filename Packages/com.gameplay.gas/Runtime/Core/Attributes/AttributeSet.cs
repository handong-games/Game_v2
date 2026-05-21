using System.Collections.Generic;

namespace Gameplay.GAS
{
    public class AttributeSet
    {
        private readonly Dictionary<GameplayAttribute, GameplayAttributeData> _attributes = new();

        public void AddAttribute(GameplayAttribute attribute, float baseValue)
        {
            if (attribute.IsValid)
                _attributes[attribute] = new GameplayAttributeData(baseValue);
        }

        public bool HasAttribute(GameplayAttribute attribute)
        {
            return _attributes.ContainsKey(attribute);
        }

        public GameplayAttributeData GetAttributeData(GameplayAttribute attribute)
        {
            return _attributes[attribute];
        }

        public bool TryGetAttributeData(GameplayAttribute attribute, out GameplayAttributeData data)
        {
            return _attributes.TryGetValue(attribute, out data);
        }
    }
}
