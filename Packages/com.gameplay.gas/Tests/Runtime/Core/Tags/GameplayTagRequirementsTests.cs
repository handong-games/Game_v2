using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayTagRequirementsTests
    {
        [Test]
        public void RequirementsMet_ReturnsTrue_WhenRequiredTagsExist()
        {
            GameplayTagRequirements requirements = new();
            requirements.RequiredTags.AddTag(GameplayTag.Define("State.Vulnerable"));

            GameplayTagContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("State.Vulnerable.Strong"));

            Assert.That(requirements.RequirementsMet(ownedTags), Is.True);
        }

        [Test]
        public void RequirementsMet_ReturnsFalse_WhenBlockedTagsExist()
        {
            GameplayTagRequirements requirements = new();
            requirements.BlockedTags.AddTag(GameplayTag.Define("State.Invulnerable"));

            GameplayTagContainer ownedTags = new();
            ownedTags.AddTag(GameplayTag.Define("State.Invulnerable"));

            Assert.That(requirements.RequirementsMet(ownedTags), Is.False);
        }
    }
}


