using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Gameplay.GAS.Tests
{
    public sealed class AbilitySystemComponentTests
    {
        [Test]
        public void GameplayActor_CreatesAbilitySystemComponent()
        {
            GameplayActor actor = new();

            Assert.That(actor.AbilitySystem, Is.Not.Null);
            Assert.That(actor.AbilitySystem.Owner, Is.SameAs(actor));
        }

        [Test]
        public void TryActivateAbility_ActivatesGrantedAbility()
        {
            GameplayActor actor = new();
            TestAbility ability = ScriptableObject.CreateInstance<TestAbility>();

            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);
            bool activated = actor.AbilitySystem.TryActivateAbility(handle);

            Assert.That(activated, Is.True);
            Assert.That(ability.WasActivated, Is.True);
        }

        [Test]
        public void TryActivateAbility_ReturnsFalse_WhenCanActivateFails()
        {
            GameplayActor actor = new();
            TestAbility ability = ScriptableObject.CreateInstance<TestAbility>();
            ability.CanActivateValue = false;

            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);
            bool activated = actor.AbilitySystem.TryActivateAbility(handle);

            Assert.That(activated, Is.False);
            Assert.That(ability.WasActivated, Is.False);
        }

        [Test]
        public void TryActivateAbility_CanApplyGameplayEffectToTarget()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet targetAttributes = new();
            targetAttributes.AddAttribute(health, 50f);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffect damageEffect = GameplayEffect.Create();
            damageEffect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -12f));

            EffectTargetAbility ability = ScriptableObject.CreateInstance<EffectTargetAbility>();
            ability.Effect = damageEffect;
            ability.Target = target.AbilitySystem;
            GameplayAbilitySpecHandle handle = source.AbilitySystem.GiveAbility(ability, 3);

            bool activated = source.AbilitySystem.TryActivateAbility(handle);

            Assert.That(activated, Is.True);
            Assert.That(targetAttributes.GetAttributeData(health).CurrentValue, Is.EqualTo(38f));
            Assert.That(ability.AppliedEffect.Spec.Level, Is.EqualTo(3));
            Assert.That(ability.AppliedEffect.Context.Source, Is.SameAs(source.AbilitySystem));
            Assert.That(ability.AppliedEffect.Context.Target, Is.SameAs(target.AbilitySystem));
        }

        [Test]
        public void TryActivateAbility_CommitsCostAndCooldownBeforeActivation()
        {
            GameplayAttribute energy = TestAttributeSet.EnergyAttribute;
            GameplayTag cooldown = GameplayTag.Request("Cooldown.Fireball");
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(energy, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect cost = GameplayEffect.Create();
            cost.AddModifier(new GameplayModifier(energy, GameplayModifierOperation.Add, -3f));

            GameplayEffect cooldownEffect = GameplayEffect.Create();
            cooldownEffect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            cooldownEffect.DurationSeconds = 2f;
            cooldownEffect.GrantedTags.Add(cooldown);

            TestAbility ability = ScriptableObject.CreateInstance<TestAbility>();
            ability.CostGameplayEffect = cost;
            ability.CooldownGameplayEffect = cooldownEffect;
            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);

            bool activated = actor.AbilitySystem.TryActivateAbility(handle);

            Assert.That(activated, Is.True);
            Assert.That(ability.WasActivated, Is.True);
            Assert.That(attributes.GetAttributeData(energy).CurrentValue, Is.EqualTo(7f));
            Assert.That(actor.AbilitySystem.OwnedTags.HasTagExact(cooldown), Is.True);
        }

        [Test]
        public void TryActivateAbility_ReturnsFalse_WhenCostCannotBePaid()
        {
            GameplayAttribute energy = TestAttributeSet.EnergyAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(energy, 2f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect cost = GameplayEffect.Create();
            cost.AddModifier(new GameplayModifier(energy, GameplayModifierOperation.Add, -3f));

            TestAbility ability = ScriptableObject.CreateInstance<TestAbility>();
            ability.CostGameplayEffect = cost;
            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);

            bool activated = actor.AbilitySystem.TryActivateAbility(handle);

            Assert.That(activated, Is.False);
            Assert.That(ability.WasActivated, Is.False);
            Assert.That(attributes.GetAttributeData(energy).CurrentValue, Is.EqualTo(2f));
        }

        [Test]
        public void TryActivateAbility_ReturnsFalse_WhenCooldownTagIsOwned()
        {
            GameplayTag cooldown = GameplayTag.Request("Cooldown.Fireball");
            GameplayActor actor = new();
            GameplayEffect cooldownEffect = GameplayEffect.Create();
            cooldownEffect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            cooldownEffect.DurationSeconds = 2f;
            cooldownEffect.GrantedTags.Add(cooldown);

            TestAbility ability = ScriptableObject.CreateInstance<TestAbility>();
            ability.CooldownGameplayEffect = cooldownEffect;
            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);

            Assert.That(actor.AbilitySystem.TryActivateAbility(handle), Is.True);
            ability.Reset();

            bool activated = actor.AbilitySystem.TryActivateAbility(handle);

            Assert.That(activated, Is.False);
            Assert.That(ability.WasActivated, Is.False);
        }

        [Test]
        public void HandleGameplayEvent_ActivatesAbilityWithMatchingTrigger()
        {
            GameplayTag eventTag = GameplayTag.Request("Event.Skill.Confirmed");
            GameplayActor actor = new();
            EventTriggeredAbility ability = ScriptableObject.CreateInstance<EventTriggeredAbility>();
            ability.AddGameplayEventTrigger(eventTag);
            actor.AbilitySystem.GiveAbility(ability);

            int triggeredCount = actor.AbilitySystem.HandleGameplayEvent(new GameplayEventData(eventTag));

            Assert.That(triggeredCount, Is.EqualTo(1));
            Assert.That(ability.WasActivated, Is.True);
        }

        [Test]
        public void HandleGameplayEvent_ActivatesParentTriggerForChildEvent()
        {
            GameplayTag parentTag = GameplayTag.Request("Event.Skill");
            GameplayTag childTag = GameplayTag.Request("Event.Skill.Confirmed");
            GameplayActor actor = new();
            EventTriggeredAbility ability = ScriptableObject.CreateInstance<EventTriggeredAbility>();
            ability.AddGameplayEventTrigger(parentTag);
            actor.AbilitySystem.GiveAbility(ability);

            int triggeredCount = actor.AbilitySystem.HandleGameplayEvent(new GameplayEventData(childTag));

            Assert.That(triggeredCount, Is.EqualTo(1));
            Assert.That(ability.WasActivated, Is.True);
            Assert.That(ability.ReceivedEventData.EventTag, Is.EqualTo(childTag));
        }

        [Test]
        public void HandleGameplayEvent_PassesPayloadToActivatedAbility()
        {
            GameplayTag eventTag = GameplayTag.Request("Event.Damage");
            GameplayActor instigator = new();
            GameplayActor target = new();
            EventTriggeredAbility ability = ScriptableObject.CreateInstance<EventTriggeredAbility>();
            ability.AddGameplayEventTrigger(eventTag);
            target.AbilitySystem.GiveAbility(ability);

            GameplayEventData eventData = new(eventTag)
            {
                Instigator = instigator.AbilitySystem,
                Target = target.AbilitySystem,
                EventMagnitude = 12f
            };

            target.AbilitySystem.HandleGameplayEvent(eventData);

            Assert.That(ability.ReceivedEventData, Is.SameAs(eventData));
            Assert.That(ability.ReceivedEventData.Instigator, Is.SameAs(instigator.AbilitySystem));
            Assert.That(ability.ReceivedEventData.Target, Is.SameAs(target.AbilitySystem));
            Assert.That(ability.ReceivedEventData.EventMagnitude, Is.EqualTo(12f));
        }

        [Test]
        public void HandleGameplayEvent_DoesNotActivate_WhenShouldRespondReturnsFalse()
        {
            GameplayTag eventTag = GameplayTag.Request("Event.Skill.Confirmed");
            GameplayActor actor = new();
            EventTriggeredAbility ability = ScriptableObject.CreateInstance<EventTriggeredAbility>();
            ability.ShouldRespondValue = false;
            ability.AddGameplayEventTrigger(eventTag);
            actor.AbilitySystem.GiveAbility(ability);

            int triggeredCount = actor.AbilitySystem.HandleGameplayEvent(new GameplayEventData(eventTag));

            Assert.That(triggeredCount, Is.EqualTo(0));
            Assert.That(ability.WasActivated, Is.False);
        }

        [Test]
        public void FindAllAbilitiesWithTags_ReturnsMatchingAbilityHandles()
        {
            GameplayActor actor = new();
            GameplayTagContainer tags = new();
            tags.AddTag(GameplayTag.Request("Ability.Skill"));

            TaggedAbility matchingAbility = ScriptableObject.CreateInstance<TaggedAbility>();
            matchingAbility.AbilityTags.AddTag(GameplayTag.Request("Ability.Skill.Attack"));

            TaggedAbility nonMatchingAbility = ScriptableObject.CreateInstance<TaggedAbility>();
            nonMatchingAbility.AbilityTags.AddTag(GameplayTag.Request("Ability.Passive"));

            GameplayAbilitySpecHandle matchingHandle = actor.AbilitySystem.GiveAbility(matchingAbility);
            actor.AbilitySystem.GiveAbility(nonMatchingAbility);

            List<GameplayAbilitySpecHandle> handles = new();
            actor.AbilitySystem.FindAllAbilitiesWithTags(handles, tags, exactMatch: false);

            Assert.That(handles.Count, Is.EqualTo(1));
            Assert.That(handles[0], Is.EqualTo(matchingHandle));
        }

        private sealed class TestAbility : GameplayAbility
        {
            public bool CanActivateValue { get; set; } = true;

            public bool WasActivated { get; private set; }

            public override bool CanActivateAbility(
                GameplayAbilitySpecHandle handle,
                GameplayAbilityActorInfo actorInfo)
            {
                return CanActivateValue;
            }

            public override void ActivateAbility(
                GameplayAbilitySpecHandle handle,
                GameplayAbilityActorInfo actorInfo,
                GameplayAbilityActivationInfo activationInfo,
                GameplayEventData triggerEventData)
            {
                WasActivated = true;
            }

            public void Reset()
            {
                WasActivated = false;
            }
        }

        private sealed class EventTriggeredAbility : GameplayAbility
        {
            public bool ShouldRespondValue { get; set; } = true;

            public bool WasActivated { get; private set; }
            public GameplayEventData ReceivedEventData { get; private set; }

            public override bool ShouldAbilityRespondToEvent(
                GameplayAbilityActorInfo actorInfo,
                GameplayEventData eventData)
            {
                return ShouldRespondValue;
            }

            public override void ActivateAbility(
                GameplayAbilitySpecHandle handle,
                GameplayAbilityActorInfo actorInfo,
                GameplayAbilityActivationInfo activationInfo,
                GameplayEventData triggerEventData)
            {
                WasActivated = true;
                ReceivedEventData = triggerEventData;
            }
        }

        private sealed class EffectTargetAbility : GameplayAbility
        {
            public GameplayEffect Effect { get; set; }
            public AbilitySystemComponent Target { get; set; }

            public ActiveGameplayEffect AppliedEffect { get; private set; }

            public override void ActivateAbility(
                GameplayAbilitySpecHandle handle,
                GameplayAbilityActorInfo actorInfo,
                GameplayAbilityActivationInfo activationInfo,
                GameplayEventData triggerEventData)
            {
                AppliedEffect = ApplyGameplayEffectToTarget(actorInfo, handle, Effect, Target);
            }
        }

        private sealed class TaggedAbility : GameplayAbility
        {
            public override void ActivateAbility(
                GameplayAbilitySpecHandle handle,
                GameplayAbilityActorInfo actorInfo,
                GameplayAbilityActivationInfo activationInfo,
                GameplayEventData triggerEventData)
            {
            }
        }
    }
}


