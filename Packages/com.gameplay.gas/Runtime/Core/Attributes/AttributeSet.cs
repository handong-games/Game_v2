using System;
using System.Reflection;

namespace Gameplay.GAS
{
    public class AttributeSet
    {
        public void AddAttribute(GameplayAttribute attribute, float baseValue)
        {
            if (!TryGetAttributeData(attribute, out GameplayAttributeData data))
                throw new InvalidOperationException($"Attribute '{attribute}' is not defined on {GetType().Name}.");

            data.SetBaseValue(baseValue);
            data.SetCurrentValue(baseValue);
        }

        public bool HasAttribute(GameplayAttribute attribute)
        {
            return TryGetAttributeData(attribute, out _);
        }

        public GameplayAttributeData GetAttributeData(GameplayAttribute attribute)
        {
            if (TryGetAttributeData(attribute, out GameplayAttributeData data))
                return data;

            throw new InvalidOperationException($"Attribute '{attribute}' is not defined on {GetType().Name}.");
        }

        public bool TryGetAttributeData(GameplayAttribute attribute, out GameplayAttributeData data)
        {
            if (!attribute.IsValid)
            {
                data = null;
                return false;
            }

            if (attribute.AttributeSetType != null &&
                !attribute.AttributeSetType.IsAssignableFrom(GetType()))
            {
                data = null;
                return false;
            }

            FieldInfo fieldInfo = attribute.FieldInfo ??
                                  GetType().GetField(
                                      attribute.FieldName,
                                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (fieldInfo == null)
            {
                data = null;
                return false;
            }

            data = fieldInfo.GetValue(this) as GameplayAttributeData;
            return data != null;
        }

        public virtual void PreAttributeBaseChange(
            GameplayAttribute attribute,
            ref float newValue)
        {
        }

        public virtual void PostAttributeBaseChange(
            GameplayAttribute attribute,
            float oldValue,
            float newValue)
        {
        }

        public virtual void PreAttributeChange(
            GameplayAttribute attribute,
            ref float newValue)
        {
        }

        public virtual void PostAttributeChange(
            GameplayAttribute attribute,
            float oldValue,
            float newValue)
        {
        }

        public virtual void PostGameplayEffectExecute(GameplayEffectModCallbackData data)
        {
        }
    }
}
