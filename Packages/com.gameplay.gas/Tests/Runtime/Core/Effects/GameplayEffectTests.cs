using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayEffectTests
    {
        [Test]
        public void ApplyGameplayEffectSpecToSelf_AddsModifierMagnitude()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -10f));

            ActiveGameplayEffect activeEffect =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(activeEffect.Handle.IsValid, Is.True);
            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(40f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(40f));
            Assert.That(actor.AbilitySystem.ActiveEffects, Is.Empty);
        }

        [Test]
        public void ApplyGameplayEffectSpecToSelf_OverridesAttributeValue()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Override, 1f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(1f));
        }

        [Test]
        public void ApplyGameplayEffectSpecToSelf_InfiniteEffectAddsGrantedTags()
        {
            GameplayActor actor = new();
            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            GameplayTag stun = GameplayTag.Define("Status.Stun");
            effect.GrantedTags.Add(stun);

            ActiveGameplayEffect activeEffect =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(actor.AbilitySystem.OwnedTags.HasTagExact(stun), Is.True);
            Assert.That(actor.AbilitySystem.ActiveEffects, Has.Count.EqualTo(1));
            Assert.That(actor.AbilitySystem.ActiveEffects[0], Is.SameAs(activeEffect));
        }

        [Test]
        public void RemoveActiveGameplayEffect_RemovesGrantedTags()
        {
            GameplayActor actor = new();
            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            GameplayTag stun = GameplayTag.Define("Status.Stun");
            effect.GrantedTags.Add(stun);

            ActiveGameplayEffect activeEffect =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            bool removed = actor.AbilitySystem.RemoveActiveGameplayEffect(activeEffect.Handle);

            Assert.That(removed, Is.True);
            Assert.That(actor.AbilitySystem.OwnedTags.HasTagExact(stun), Is.False);
            Assert.That(actor.AbilitySystem.ActiveEffects, Is.Empty);
        }

        [Test]
        public void TickActiveGameplayEffects_RemovesExpiredDurationEffect()
        {
            GameplayActor actor = new();
            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 2f;
            GameplayTag stun = GameplayTag.Define("Status.Stun");
            effect.GrantedTags.Add(stun);

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(1f);

            Assert.That(actor.AbilitySystem.OwnedTags.HasTagExact(stun), Is.True);
            Assert.That(actor.AbilitySystem.ActiveEffects, Has.Count.EqualTo(1));

            actor.AbilitySystem.TickActiveGameplayEffects(1f);

            Assert.That(actor.AbilitySystem.OwnedTags.HasTagExact(stun), Is.False);
            Assert.That(actor.AbilitySystem.ActiveEffects, Is.Empty);
        }

        [Test]
        public void DurationModifier_RecalculatesCurrentValueWithoutChangingBaseValue()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 2f;
            effect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(attributes.GetAttributeData(attack).BaseValue, Is.EqualTo(10f));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(15f));
        }

        [Test]
        public void RemoveActiveGameplayEffect_RecalculatesModifiedAttributes()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            effect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));

            ActiveGameplayEffect activeEffect =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.RemoveActiveGameplayEffect(activeEffect.Handle);

            Assert.That(attributes.GetAttributeData(attack).BaseValue, Is.EqualTo(10f));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(10f));
        }

        [Test]
        public void ActiveModifiers_AreAggregatedInAddMultiplyOverrideOrder()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect addEffect = GameplayEffect.Create();
            addEffect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            addEffect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));

            GameplayEffect multiplyEffect = GameplayEffect.Create();
            multiplyEffect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            multiplyEffect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Multiply, 2f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(addEffect));
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(multiplyEffect));

            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(30f));

            GameplayEffect overrideEffect = GameplayEffect.Create();
            overrideEffect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            overrideEffect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Override, 1f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(overrideEffect));

            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(1f));
        }

        [Test]
        public void InstantModifier_ChangesBaseValueThenRecalculatesActiveModifiers()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect buff = GameplayEffect.Create();
            buff.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            buff.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(buff));

            GameplayEffect instant = GameplayEffect.Create();
            instant.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 3f));
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(instant));

            Assert.That(attributes.GetAttributeData(attack).BaseValue, Is.EqualTo(13f));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(18f));
        }

        [Test]
        public void ApplyGameplayEffectSpecToTarget_ChangesTargetAttribute()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet targetAttributes = new();
            targetAttributes.AddAttribute(health, 50f);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -15f));

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            ActiveGameplayEffect activeEffect =
                source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(targetAttributes.GetAttributeData(health).CurrentValue, Is.EqualTo(35f));
            Assert.That(activeEffect.Context.Source, Is.SameAs(source.AbilitySystem));
            Assert.That(activeEffect.Context.Target, Is.SameAs(target.AbilitySystem));
        }

        [Test]
        public void Execution_CanUseSourceAttributeToModifyTargetAttribute()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet sourceAttributes = new();
            TestAttributeSet targetAttributes = new();
            sourceAttributes.AddAttribute(attack, 12f);
            targetAttributes.AddAttribute(health, 50f);
            source.AbilitySystem.AddAttributeSet(sourceAttributes);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddExecution(new SourceAttackDamageExecution(attack, health));

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(targetAttributes.GetAttributeData(health).BaseValue, Is.EqualTo(38f));
            Assert.That(targetAttributes.GetAttributeData(health).CurrentValue, Is.EqualTo(38f));
        }

        [Test]
        public void Execution_CanUseSetByCallerMagnitude()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayTag damageTag = GameplayTag.Define("Data.Damage");
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddExecution(new SetByCallerDamageExecution(health, damageTag));

            GameplayEffectSpec spec = new(effect);
            spec.SetSetByCallerMagnitude(damageTag, 8f);
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(spec);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(42f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(42f));
        }

        [Test]
        public void Execution_CanUseTargetAttributeWhenOutgoingSpecIsAppliedToSelf()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddExecution(new TargetPercentDamageExecution(health, 0.1f));

            GameplayEffectSpec spec = actor.AbilitySystem.MakeOutgoingSpec(effect);
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(spec);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(45f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(45f));
        }

        [Test]
        public void Execution_SourceSnapshotCapture_UsesValueFromSpecCreation()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet sourceAttributes = new();
            TestAttributeSet targetAttributes = new();
            sourceAttributes.AddAttribute(attack, 12f);
            targetAttributes.AddAttribute(health, 50f);
            source.AbilitySystem.AddAttributeSet(sourceAttributes);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddExecution(new CapturedSourceAttackDamageExecution(attack, health, true));

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            sourceAttributes.GetAttributeData(attack).SetCurrentValue(20f);
            source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(targetAttributes.GetAttributeData(health).BaseValue, Is.EqualTo(38f));
        }

        [Test]
        public void Execution_SourceLiveCapture_UsesValueFromExecutionTime()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet sourceAttributes = new();
            TestAttributeSet targetAttributes = new();
            sourceAttributes.AddAttribute(attack, 12f);
            targetAttributes.AddAttribute(health, 50f);
            source.AbilitySystem.AddAttributeSet(sourceAttributes);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddExecution(new CapturedSourceAttackDamageExecution(attack, health, false));

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            sourceAttributes.GetAttributeData(attack).SetCurrentValue(20f);
            source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(targetAttributes.GetAttributeData(health).BaseValue, Is.EqualTo(30f));
        }

        [Test]
        public void SetByCallerModifier_UsesMagnitudeFromSpec()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayTag damageTag = GameplayTag.Define("Data.Damage");
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, damageTag));

            GameplayEffectSpec spec = new(effect);
            spec.SetSetByCallerMagnitude(damageTag, -12f);

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(spec);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(38f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(38f));
        }

        [Test]
        public void AttributeBasedModifier_SourceSnapshot_UsesValueFromSpecCreation()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet sourceAttributes = new();
            TestAttributeSet targetAttributes = new();
            sourceAttributes.AddAttribute(attack, 12f);
            targetAttributes.AddAttribute(health, 50f);
            source.AbilitySystem.AddAttributeSet(sourceAttributes);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffectAttributeCaptureDefinition attackDefinition = new(
                attack,
                GameplayEffectAttributeCaptureSource.Source,
                true);
            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(
                health,
                GameplayModifierOperation.Add,
                new GameplayAttributeBasedFloat(attackDefinition, -1f)));

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            sourceAttributes.GetAttributeData(attack).SetCurrentValue(20f);
            source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(targetAttributes.GetAttributeData(health).BaseValue, Is.EqualTo(38f));
        }

        [Test]
        public void AttributeBasedModifier_SourceLive_UsesValueFromApplicationTime()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet sourceAttributes = new();
            TestAttributeSet targetAttributes = new();
            sourceAttributes.AddAttribute(attack, 12f);
            targetAttributes.AddAttribute(health, 50f);
            source.AbilitySystem.AddAttributeSet(sourceAttributes);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffectAttributeCaptureDefinition attackDefinition = new(
                attack,
                GameplayEffectAttributeCaptureSource.Source,
                false);
            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(
                health,
                GameplayModifierOperation.Add,
                new GameplayAttributeBasedFloat(attackDefinition, -1f)));

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            sourceAttributes.GetAttributeData(attack).SetCurrentValue(20f);
            source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(targetAttributes.GetAttributeData(health).BaseValue, Is.EqualTo(30f));
        }

        [Test]
        public void AttributeBasedModifier_CanUsePreAndPostAdditiveValues()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet sourceAttributes = new();
            TestAttributeSet targetAttributes = new();
            sourceAttributes.AddAttribute(attack, 10f);
            targetAttributes.AddAttribute(health, 50f);
            source.AbilitySystem.AddAttributeSet(sourceAttributes);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffectAttributeCaptureDefinition attackDefinition = new(
                attack,
                GameplayEffectAttributeCaptureSource.Source,
                false);
            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(
                health,
                GameplayModifierOperation.Add,
                new GameplayAttributeBasedFloat(
                    attackDefinition,
                    coefficient: -2f,
                    preMultiplyAdditiveValue: 3f,
                    postMultiplyAdditiveValue: 1f)));

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(targetAttributes.GetAttributeData(health).BaseValue, Is.EqualTo(25f));
        }

        [Test]
        public void SetByCallerModifier_DefaultsToZeroWhenMagnitudeIsMissing()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(
                health,
                GameplayModifierOperation.Add,
                GameplayTag.Define("Data.Damage")));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(50f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(50f));
        }

        [Test]
        public void ApplyGameplayEffectSpecToTarget_PreservesSetByCallerMagnitude()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayTag damageTag = GameplayTag.Define("Data.Damage");
            GameplayActor source = new();
            GameplayActor target = new();
            TestAttributeSet targetAttributes = new();
            targetAttributes.AddAttribute(health, 50f);
            target.AbilitySystem.AddAttributeSet(targetAttributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, damageTag));

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            spec.SetSetByCallerMagnitude(damageTag, -9f);

            source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(targetAttributes.GetAttributeData(health).BaseValue, Is.EqualTo(41f));
        }

        [Test]
        public void DurationSetByCallerModifier_UsesMagnitudeFromSpecInAggregator()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayTag buffTag = GameplayTag.Define("Data.AttackBonus");
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            effect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, buffTag));

            GameplayEffectSpec spec = new(effect);
            spec.SetSetByCallerMagnitude(buffTag, 7f);

            ActiveGameplayEffect activeEffect = actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(spec);

            Assert.That(activeEffect.Handle.IsValid, Is.True);
            Assert.That(attributes.GetAttributeData(attack).BaseValue, Is.EqualTo(10f));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(17f));
        }

        [Test]
        public void ApplyGameplayEffectSpecToTarget_AddsGrantedTagsToTarget()
        {
            GameplayActor source = new();
            GameplayActor target = new();
            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            GameplayTag stun = GameplayTag.Define("Status.Stun");
            effect.GrantedTags.Add(stun);

            GameplayEffectSpec spec = source.AbilitySystem.MakeOutgoingSpec(effect);
            source.AbilitySystem.ApplyGameplayEffectSpecToTarget(spec, target.AbilitySystem);

            Assert.That(target.AbilitySystem.OwnedTags.HasTagExact(stun), Is.True);
            Assert.That(source.AbilitySystem.OwnedTags.HasTagExact(stun), Is.False);
        }

        [Test]
        public void ApplicationTagRequirements_BlocksEffectWhenRequiredTagIsMissing()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.ApplicationTagRequirements.RequiredTags.Add(GameplayTag.Define("State.Vulnerable"));
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -10f));

            ActiveGameplayEffect activeEffect =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(activeEffect.Handle.IsValid, Is.False);
            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(50f));
        }

        [Test]
        public void ApplicationTagRequirements_AllowsEffectWhenRequiredTagExists()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);
            actor.AbilitySystem.OwnedTags.AddTag(GameplayTag.Define("State.Vulnerable"));

            GameplayEffect effect = GameplayEffect.Create();
            effect.ApplicationTagRequirements.RequiredTags.Add(GameplayTag.Define("State.Vulnerable"));
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -10f));

            ActiveGameplayEffect activeEffect =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(activeEffect.Handle.IsValid, Is.True);
            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(40f));
        }

        [Test]
        public void ApplicationTagRequirements_BlocksEffectWhenBlockedTagExists()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);
            actor.AbilitySystem.OwnedTags.AddTag(GameplayTag.Define("State.Invulnerable"));

            GameplayEffect effect = GameplayEffect.Create();
            effect.ApplicationTagRequirements.BlockedTags.Add(GameplayTag.Define("State.Invulnerable"));
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -10f));

            ActiveGameplayEffect activeEffect =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(activeEffect.Handle.IsValid, Is.False);
            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(50f));
        }

        [Test]
        public void PeriodicEffect_DoesNotExecuteBeforePeriodElapses()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 3f;
            effect.PeriodSeconds = 1f;
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(0.5f);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(50f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(50f));
        }

        [Test]
        public void PeriodicEffect_ExecutesWhenPeriodElapses()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 3f;
            effect.PeriodSeconds = 1f;
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(1f);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(45f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(45f));
        }

        [Test]
        public void PeriodicEffect_CanExecuteOnApplication()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 3f;
            effect.PeriodSeconds = 1f;
            effect.ExecutePeriodicEffectOnApplication = true;
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(45f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(45f));
        }

        [Test]
        public void PeriodicEffect_ExecutesMultipleTicksForLargeDelta()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 5f;
            effect.PeriodSeconds = 1f;
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(2.5f);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(40f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(40f));
        }

        [Test]
        public void PeriodicEffect_DoesNotUseDurationModifierAggregation()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 3f;
            effect.PeriodSeconds = 1f;
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(50f));
            Assert.That(attributes.GetAttributeData(health).CurrentValue, Is.EqualTo(50f));
        }

        [Test]
        public void PeriodicEffect_DoesNotExecuteBeyondDuration()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 1f;
            effect.PeriodSeconds = 0.25f;
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(10f);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(30f));
            Assert.That(actor.AbilitySystem.ActiveEffects, Is.Empty);
        }

        [Test]
        public void StackingTypeNone_CreatesSeparateActiveEffects()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            effect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(actor.AbilitySystem.ActiveEffects, Has.Count.EqualTo(2));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(20f));
        }

        [Test]
        public void AggregateByTarget_ReusesActiveEffectAndIncreasesStackCount()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            effect.StackingType = GameplayEffectStackingType.AggregateByTarget;
            effect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));

            ActiveGameplayEffect first =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            ActiveGameplayEffect second =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(actor.AbilitySystem.ActiveEffects, Has.Count.EqualTo(1));
            Assert.That(second, Is.SameAs(first));
            Assert.That(first.StackCount, Is.EqualTo(2));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(20f));
        }

        [Test]
        public void AggregateBySource_KeepsSeparateStacksPerSource()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor sourceA = new();
            GameplayActor sourceB = new();
            GameplayActor target = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            target.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            effect.StackingType = GameplayEffectStackingType.AggregateBySource;
            effect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));

            GameplayEffectSpec sourceASpec = sourceA.AbilitySystem.MakeOutgoingSpec(effect);
            GameplayEffectSpec sourceBSpec = sourceB.AbilitySystem.MakeOutgoingSpec(effect);

            sourceA.AbilitySystem.ApplyGameplayEffectSpecToTarget(sourceASpec, target.AbilitySystem);
            sourceA.AbilitySystem.ApplyGameplayEffectSpecToTarget(sourceASpec, target.AbilitySystem);
            sourceB.AbilitySystem.ApplyGameplayEffectSpecToTarget(sourceBSpec, target.AbilitySystem);

            Assert.That(target.AbilitySystem.ActiveEffects, Has.Count.EqualTo(2));
            Assert.That(target.AbilitySystem.ActiveEffects[0].StackCount, Is.EqualTo(2));
            Assert.That(target.AbilitySystem.ActiveEffects[1].StackCount, Is.EqualTo(1));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(25f));
        }

        [Test]
        public void StackLimitCount_CapsStackCount()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            effect.StackingType = GameplayEffectStackingType.AggregateByTarget;
            effect.StackLimitCount = 2;
            effect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(actor.AbilitySystem.ActiveEffects, Has.Count.EqualTo(1));
            Assert.That(actor.AbilitySystem.ActiveEffects[0].StackCount, Is.EqualTo(2));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(20f));
        }

        [Test]
        public void StackDurationRefreshPolicy_CanRefreshDurationOnSuccessfulApplication()
        {
            GameplayActor actor = new();
            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 2f;
            effect.StackingType = GameplayEffectStackingType.AggregateByTarget;
            effect.StackDurationRefreshPolicy =
                GameplayEffectStackingDurationPolicy.RefreshOnSuccessfulApplication;

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(1.5f);
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(0.6f);

            Assert.That(actor.AbilitySystem.ActiveEffects, Has.Count.EqualTo(1));
            Assert.That(actor.AbilitySystem.ActiveEffects[0].StackCount, Is.EqualTo(2));
        }

        [Test]
        public void StackDurationRefreshPolicy_CanKeepExistingDuration()
        {
            GameplayActor actor = new();
            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 2f;
            effect.StackingType = GameplayEffectStackingType.AggregateByTarget;
            effect.StackDurationRefreshPolicy = GameplayEffectStackingDurationPolicy.NeverRefresh;

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(1.5f);
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(0.6f);

            Assert.That(actor.AbilitySystem.ActiveEffects, Is.Empty);
        }

        [Test]
        public void StackExpirationPolicy_CanRemoveSingleStackAndRefreshDuration()
        {
            GameplayAttribute attack = TestAttributeSet.AttackAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(attack, 10f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 1f;
            effect.StackingType = GameplayEffectStackingType.AggregateByTarget;
            effect.StackExpirationPolicy =
                GameplayEffectStackingExpirationPolicy.RemoveSingleStackAndRefreshDuration;
            effect.AddModifier(new GameplayModifier(attack, GameplayModifierOperation.Add, 5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(1f);

            Assert.That(actor.AbilitySystem.ActiveEffects, Has.Count.EqualTo(1));
            Assert.That(actor.AbilitySystem.ActiveEffects[0].StackCount, Is.EqualTo(1));
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(15f));

            actor.AbilitySystem.TickActiveGameplayEffects(1f);

            Assert.That(actor.AbilitySystem.ActiveEffects, Is.Empty);
            Assert.That(attributes.GetAttributeData(attack).CurrentValue, Is.EqualTo(10f));
        }

        [Test]
        public void StackPeriodResetPolicy_CanResetProgressTowardNextTick()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 3f;
            effect.PeriodSeconds = 1f;
            effect.StackingType = GameplayEffectStackingType.AggregateByTarget;
            effect.StackPeriodResetPolicy =
                GameplayEffectStackingPeriodPolicy.ResetOnSuccessfulApplication;
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(0.5f);
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(0.6f);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(50f));
        }

        [Test]
        public void StackPeriodResetPolicy_CanKeepProgressTowardNextTick()
        {
            GameplayAttribute health = TestAttributeSet.HealthAttribute;
            GameplayActor actor = new();
            TestAttributeSet attributes = new();
            attributes.AddAttribute(health, 50f);
            actor.AbilitySystem.AddAttributeSet(attributes);

            GameplayEffect effect = GameplayEffect.Create();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 3f;
            effect.PeriodSeconds = 1f;
            effect.StackingType = GameplayEffectStackingType.AggregateByTarget;
            effect.StackPeriodResetPolicy = GameplayEffectStackingPeriodPolicy.NeverReset;
            effect.AddModifier(new GameplayModifier(health, GameplayModifierOperation.Add, -5f));

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(0.5f);
            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            actor.AbilitySystem.TickActiveGameplayEffects(0.6f);

            Assert.That(attributes.GetAttributeData(health).BaseValue, Is.EqualTo(40f));
        }

        private sealed class SourceAttackDamageExecution : GameplayEffectExecution
        {
            private readonly GameplayAttribute _attackAttribute;
            private readonly GameplayAttribute _healthAttribute;

            public SourceAttackDamageExecution(
                GameplayAttribute attackAttribute,
                GameplayAttribute healthAttribute)
            {
                _attackAttribute = attackAttribute;
                _healthAttribute = healthAttribute;
            }

            public override void Execute(
                GameplayEffectExecutionParameters parameters,
                GameplayEffectExecutionOutput output)
            {
                GameplayEffectAttributeCaptureDefinition definition = new(
                    _attackAttribute,
                    GameplayEffectAttributeCaptureSource.Source,
                    false);

                if (!parameters.AttemptCalculateCapturedAttributeMagnitude(definition, out float attack))
                    return;

                output.AddOutputModifier(new GameplayModifierEvaluatedData(
                    _healthAttribute,
                    GameplayModifierOperation.Add,
                    -attack));
            }
        }

        private sealed class CapturedSourceAttackDamageExecution : GameplayEffectExecution
        {
            private readonly GameplayEffectAttributeCaptureDefinition _attackDefinition;
            private readonly GameplayAttribute _healthAttribute;

            public CapturedSourceAttackDamageExecution(
                GameplayAttribute attackAttribute,
                GameplayAttribute healthAttribute,
                bool snapshot)
            {
                _attackDefinition = new GameplayEffectAttributeCaptureDefinition(
                    attackAttribute,
                    GameplayEffectAttributeCaptureSource.Source,
                    snapshot);
                _healthAttribute = healthAttribute;
            }

            public override void GetAttributeCaptureDefinitions(
                System.Collections.Generic.List<GameplayEffectAttributeCaptureDefinition> definitions)
            {
                definitions.Add(_attackDefinition);
            }

            public override void Execute(
                GameplayEffectExecutionParameters parameters,
                GameplayEffectExecutionOutput output)
            {
                if (!parameters.AttemptCalculateCapturedAttributeMagnitude(_attackDefinition, out float attack))
                    return;

                output.AddOutputModifier(new GameplayModifierEvaluatedData(
                    _healthAttribute,
                    GameplayModifierOperation.Add,
                    -attack));
            }
        }

        private sealed class SetByCallerDamageExecution : GameplayEffectExecution
        {
            private readonly GameplayAttribute _healthAttribute;
            private readonly GameplayTag _damageTag;

            public SetByCallerDamageExecution(GameplayAttribute healthAttribute, GameplayTag damageTag)
            {
                _healthAttribute = healthAttribute;
                _damageTag = damageTag;
            }

            public override void Execute(
                GameplayEffectExecutionParameters parameters,
                GameplayEffectExecutionOutput output)
            {
                float damage = parameters.GetSetByCallerMagnitude(_damageTag);
                output.AddOutputModifier(new GameplayModifierEvaluatedData(
                    _healthAttribute,
                    GameplayModifierOperation.Add,
                    -damage));
            }
        }

        private sealed class TargetPercentDamageExecution : GameplayEffectExecution
        {
            private readonly GameplayAttribute _healthAttribute;
            private readonly float _ratio;

            public TargetPercentDamageExecution(GameplayAttribute healthAttribute, float ratio)
            {
                _healthAttribute = healthAttribute;
                _ratio = ratio;
            }

            public override void Execute(
                GameplayEffectExecutionParameters parameters,
                GameplayEffectExecutionOutput output)
            {
                if (!parameters.TryGetTargetAttribute(_healthAttribute, out GameplayAttributeData health))
                    return;

                output.AddOutputModifier(new GameplayModifierEvaluatedData(
                    _healthAttribute,
                    GameplayModifierOperation.Add,
                    -health.CurrentValue * _ratio));
            }
        }
    }
}




