using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class AttributeSetTests
    {
        [Test]
        public void AddAttribute_StoresBaseAndCurrentValue()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            TestAttributeSet attributes = new();

            attributes.AddAttribute(health, 50f);

            GameplayAttributeData data = attributes.GetAttributeData(health);
            Assert.That(data.BaseValue, Is.EqualTo(50f));
            Assert.That(data.CurrentValue, Is.EqualTo(50f));
        }

        [Test]
        public void SetCurrentValue_RaisesCurrentValueChanged()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            GameplayAttributeData data = attributes.GetAttributeData(health);

            float previous = 0f;
            float next = 0f;
            data.CurrentValueChanged += (previousValue, nextValue) =>
            {
                previous = previousValue;
                next = nextValue;
            };

            data.SetCurrentValue(30f);

            Assert.That(previous, Is.EqualTo(50f));
            Assert.That(next, Is.EqualTo(30f));
        }
    }
}



