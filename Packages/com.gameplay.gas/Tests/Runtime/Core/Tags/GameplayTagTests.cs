using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayTagTests
    {
        [Test]
        public void MatchesTag_ReturnsTrue_ForExactTag()
        {
            GameplayTag tag = GameplayTag.Request("Status.Poison");

            Assert.That(tag.MatchesTag(GameplayTag.Request("Status.Poison")), Is.True);
        }

        [Test]
        public void MatchesTag_ReturnsTrue_ForParentTag()
        {
            GameplayTag tag = GameplayTag.Request("Status.Poison.Strong");

            Assert.That(tag.MatchesTag(GameplayTag.Request("Status.Poison")), Is.True);
            Assert.That(tag.MatchesTag(GameplayTag.Request("Status")), Is.True);
        }

        [Test]
        public void MatchesTag_ReturnsFalse_ForSiblingTag()
        {
            GameplayTag tag = GameplayTag.Request("Status.Poison");

            Assert.That(tag.MatchesTag(GameplayTag.Request("Status.Burn")), Is.False);
        }

        [Test]
        public void Request_ReturnsSameTag_ForSameName()
        {
            GameplayTag first = GameplayTag.Request("Ability.Attack");
            GameplayTag second = GameplayTag.Request("Ability.Attack");

            Assert.That(first, Is.EqualTo(second));
        }

        [Test]
        public void RequestDirectParent_ReturnsImmediateParentTag()
        {
            GameplayTag tag = GameplayTag.Request("Ability.Attack.Light");

            Assert.That(tag.RequestDirectParent(), Is.EqualTo(GameplayTag.Request("Ability.Attack")));
        }

        [Test]
        public void ToString_ReturnsRegisteredName()
        {
            GameplayTag tag = GameplayTag.Request("GameplayEvent.Skill.Confirmed");

            Assert.That(tag.ToString(), Is.EqualTo("GameplayEvent.Skill.Confirmed"));
        }
    }
}
