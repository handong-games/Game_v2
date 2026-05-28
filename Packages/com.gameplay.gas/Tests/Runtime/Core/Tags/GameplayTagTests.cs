using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayTagTests
    {
        [Test]
        public void MatchesTag_ReturnsTrue_ForExactTag()
        {
            GameplayTag tag = GameplayTag.Define("Status.Poison");

            Assert.That(tag.MatchesTag(GameplayTag.Define("Status.Poison")), Is.True);
        }

        [Test]
        public void MatchesTag_ReturnsTrue_ForParentTag()
        {
            GameplayTag tag = GameplayTag.Define("Status.Poison.Strong");

            Assert.That(tag.MatchesTag(GameplayTag.Define("Status.Poison")), Is.True);
            Assert.That(tag.MatchesTag(GameplayTag.Define("Status")), Is.True);
        }

        [Test]
        public void MatchesTag_ReturnsTrue_WhenChildTagIsDefinedBeforeParentTag()
        {
            GameplayTag tag = GameplayTag.Define("Regression.Child.Leaf");

            Assert.That(tag.MatchesTag(GameplayTag.Define("Regression.Child")), Is.True);
            Assert.That(tag.MatchesTag(GameplayTag.Define("Regression")), Is.True);
        }

        [Test]
        public void MatchesTag_ReturnsFalse_ForSiblingTag()
        {
            GameplayTag tag = GameplayTag.Define("Status.Poison");

            Assert.That(tag.MatchesTag(GameplayTag.Define("Status.Burn")), Is.False);
        }

        [Test]
        public void Request_ReturnsSameTag_ForSameName()
        {
            GameplayTag first = GameplayTag.Define("Ability.Attack");
            GameplayTag second = GameplayTag.Define("Ability.Attack");

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void GetDirectParent_ReturnsImmediateParentTag()
        {
            GameplayTag tag = GameplayTag.Define("Ability.Attack.Light");

            Assert.That(tag.GetDirectParent(), Is.EqualTo(GameplayTag.Define("Ability.Attack")));
        }

        [Test]
        public void ToString_ReturnsRegisteredName()
        {
            GameplayTag tag = GameplayTag.Define("GameplayEvent.Skill.Confirmed");

            Assert.That(tag.ToString(), Is.EqualTo("GameplayEvent.Skill.Confirmed"));
        }
    }
}



