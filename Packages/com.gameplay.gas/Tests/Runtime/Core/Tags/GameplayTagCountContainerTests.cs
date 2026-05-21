using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayTagCountContainerTests
    {
        [Test]
        public void RemoveTag_KeepsTagUntilCountReachesZero()
        {
            GameplayTag tag = GameplayTag.Request("Status.Poison");
            GameplayTagCountContainer container = new();

            container.AddTag(tag);
            container.AddTag(tag);
            container.RemoveTag(tag);

            Assert.That(container.GetCount(tag), Is.EqualTo(1));
            Assert.That(container.HasTag(tag), Is.True);
        }

        [Test]
        public void RemoveTag_RemovesTag_WhenCountReachesZero()
        {
            GameplayTag tag = GameplayTag.Request("Status.Poison");
            GameplayTagCountContainer container = new();

            container.AddTag(tag);
            container.RemoveTag(tag);

            Assert.That(container.GetCount(tag), Is.EqualTo(0));
            Assert.That(container.HasTag(tag), Is.False);
        }

        [Test]
        public void HasTag_ReturnsTrue_ForParentTag()
        {
            GameplayTagCountContainer container = new();
            container.AddTag(GameplayTag.Request("Status.Poison.Strong"));

            Assert.That(container.HasTag(GameplayTag.Request("Status")), Is.True);
        }

        [Test]
        public void HasAll_ReturnsTrue_WhenAllTagsMatch()
        {
            GameplayTagCountContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Request("Status.Poison.Strong"));
            ownedTags.AddTag(GameplayTag.Request("State.Blessed"));

            GameplayTagContainer requiredTags = new();
            requiredTags.Add(GameplayTag.Request("Status.Poison"));
            requiredTags.Add(GameplayTag.Request("State.Blessed"));

            Assert.That(ownedTags.HasAll(requiredTags), Is.True);
        }

        [Test]
        public void HasAny_ReturnsTrue_WhenAnyTagMatches()
        {
            GameplayTagCountContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Request("State.Silenced"));

            GameplayTagContainer blockedTags = new();
            blockedTags.Add(GameplayTag.Request("State.Stunned"));
            blockedTags.Add(GameplayTag.Request("State.Silenced"));

            Assert.That(ownedTags.HasAny(blockedTags), Is.True);
        }

        [Test]
        public void HasAnyExact_ReturnsFalse_ForParentTag()
        {
            GameplayTagCountContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Request("Status.Poison.Strong"));

            GameplayTagContainer tagsToCheck = new();
            tagsToCheck.AddTag(GameplayTag.Request("Status.Poison"));

            Assert.That(ownedTags.HasAnyExact(tagsToCheck), Is.False);
        }

        [Test]
        public void HasAllExact_ReturnsTrue_WhenExactTagsMatch()
        {
            GameplayTagCountContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Request("Status.Poison.Strong"));
            ownedTags.AddTag(GameplayTag.Request("State.Blessed"));

            GameplayTagContainer requiredTags = new();
            requiredTags.AddTag(GameplayTag.Request("Status.Poison.Strong"));
            requiredTags.AddTag(GameplayTag.Request("State.Blessed"));

            Assert.That(ownedTags.HasAllExact(requiredTags), Is.True);
        }
    }
}
