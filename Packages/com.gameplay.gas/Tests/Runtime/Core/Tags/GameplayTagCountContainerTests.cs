using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayTagCountContainerTests
    {
        [Test]
        public void RemoveTag_KeepsTagUntilCountReachesZero()
        {
            GameplayTag tag = GameplayTag.Define("Status.Poison");
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
            GameplayTag tag = GameplayTag.Define("Status.Poison");
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
            container.AddTag(GameplayTag.Define("Status.Poison.Strong"));

            Assert.That(container.HasTag(GameplayTag.Define("Status")), Is.True);
        }

        [Test]
        public void GetCount_ReturnsHierarchicalTagCount()
        {
            GameplayTagCountContainer container = new();
            container.AddTag(GameplayTag.Define("Status.Poison.Strong"));
            container.AddTag(GameplayTag.Define("Status.Poison.Weak"));

            Assert.That(container.GetCount(GameplayTag.Define("Status.Poison")), Is.EqualTo(2));
            Assert.That(container.GetExplicitCount(GameplayTag.Define("Status.Poison")), Is.EqualTo(0));
        }

        [Test]
        public void RemoveTag_KeepsParentCountUntilAllChildrenAreRemoved()
        {
            GameplayTag strongPoison = GameplayTag.Define("Status.Poison.Strong");
            GameplayTag weakPoison = GameplayTag.Define("Status.Poison.Weak");
            GameplayTag poison = GameplayTag.Define("Status.Poison");
            GameplayTagCountContainer container = new();

            container.AddTag(strongPoison);
            container.AddTag(weakPoison);
            container.RemoveTag(strongPoison);

            Assert.That(container.HasTag(poison), Is.True);

            container.RemoveTag(weakPoison);

            Assert.That(container.HasTag(poison), Is.False);
        }

        [Test]
        public void HasAll_ReturnsTrue_WhenAllTagsMatch()
        {
            GameplayTagCountContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("Status.Poison.Strong"));
            ownedTags.AddTag(GameplayTag.Define("State.Blessed"));

            GameplayTagContainer requiredTags = new();
            requiredTags.Add(GameplayTag.Define("Status.Poison"));
            requiredTags.Add(GameplayTag.Define("State.Blessed"));

            Assert.That(ownedTags.HasAll(requiredTags), Is.True);
        }

        [Test]
        public void HasAny_ReturnsTrue_WhenAnyTagMatches()
        {
            GameplayTagCountContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("State.Silenced"));

            GameplayTagContainer blockedTags = new();
            blockedTags.Add(GameplayTag.Define("State.Stunned"));
            blockedTags.Add(GameplayTag.Define("State.Silenced"));

            Assert.That(ownedTags.HasAny(blockedTags), Is.True);
        }

        [Test]
        public void HasAnyExact_ReturnsFalse_ForParentTag()
        {
            GameplayTagCountContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("Status.Poison.Strong"));

            GameplayTagContainer tagsToCheck = new();
            tagsToCheck.AddTag(GameplayTag.Define("Status.Poison"));

            Assert.That(ownedTags.HasAnyExact(tagsToCheck), Is.False);
        }

        [Test]
        public void HasAllExact_ReturnsTrue_WhenExactTagsMatch()
        {
            GameplayTagCountContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("Status.Poison.Strong"));
            ownedTags.AddTag(GameplayTag.Define("State.Blessed"));

            GameplayTagContainer requiredTags = new();
            requiredTags.AddTag(GameplayTag.Define("Status.Poison.Strong"));
            requiredTags.AddTag(GameplayTag.Define("State.Blessed"));

            Assert.That(ownedTags.HasAllExact(requiredTags), Is.True);
        }
    }
}


