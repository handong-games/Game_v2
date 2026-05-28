using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayTagContainerTests
    {
        [Test]
        public void HasTag_ReturnsTrue_ForParentTag()
        {
            GameplayTagContainer container = new();
            container.Add(GameplayTag.Define("Status.Poison.Strong"));

            Assert.That(container.HasTag(GameplayTag.Define("Status.Poison")), Is.True);
        }

        [Test]
        public void AddTag_String_AddsRequestedTag()
        {
            GameplayTagContainer container = new();

            container.AddTag(GameplayTag.Define("Ability.Skill.Warrior.BasicAttack"));

            Assert.That(
                container.HasTagExact(GameplayTag.Define("Ability.Skill.Warrior.BasicAttack")),
                Is.True);
        }

        [Test]
        public void HasAll_ReturnsTrue_WhenAllRequiredTagsMatch()
        {
            GameplayTagContainer ownedTags = new();
            ownedTags.Add(GameplayTag.Define("Ability.Attack.Melee"));
            ownedTags.Add(GameplayTag.Define("Element.Fire"));

            GameplayTagContainer requiredTags = new();
            requiredTags.Add(GameplayTag.Define("Ability.Attack"));
            requiredTags.Add(GameplayTag.Define("Element.Fire"));

            Assert.That(ownedTags.HasAll(requiredTags), Is.True);
        }

        [Test]
        public void HasAny_ReturnsTrue_WhenOneRequiredTagMatches()
        {
            GameplayTagContainer ownedTags = new();
            ownedTags.Add(GameplayTag.Define("State.Silenced"));

            GameplayTagContainer blockedTags = new();
            blockedTags.Add(GameplayTag.Define("State.Stunned"));
            blockedTags.Add(GameplayTag.Define("State.Silenced"));

            Assert.That(ownedTags.HasAny(blockedTags), Is.True);
        }

        [Test]
        public void HasTagExact_ReturnsFalse_ForParentTag()
        {
            GameplayTagContainer container = new();
            container.AddTag(GameplayTag.Define("Status.Poison.Strong"));

            Assert.That(container.HasTagExact(GameplayTag.Define("Status.Poison")), Is.False);
        }

        [Test]
        public void HasAnyExact_ReturnsFalse_ForParentTag()
        {
            GameplayTagContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("Status.Poison.Strong"));

            GameplayTagContainer tagsToCheck = new();
            tagsToCheck.AddTag(GameplayTag.Define("Status.Poison"));

            Assert.That(ownedTags.HasAnyExact(tagsToCheck), Is.False);
        }

        [Test]
        public void HasAll_ReturnsTrue_ForEmptyContainer()
        {
            GameplayTagContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("Status.Poison"));

            GameplayTagContainer emptyTags = new();

            Assert.That(ownedTags.HasAll(emptyTags), Is.True);
            Assert.That(ownedTags.HasAllExact(emptyTags), Is.True);
        }

        [Test]
        public void HasAny_ReturnsFalse_ForEmptyContainer()
        {
            GameplayTagContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("Status.Poison"));

            GameplayTagContainer emptyTags = new();

            Assert.That(ownedTags.HasAny(emptyTags), Is.False);
            Assert.That(ownedTags.HasAnyExact(emptyTags), Is.False);
        }
    }
}


