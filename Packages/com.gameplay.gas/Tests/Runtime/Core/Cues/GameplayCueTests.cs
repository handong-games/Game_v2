using System.Collections.Generic;
using NUnit.Framework;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayCueTests
    {
        [Test]
        public void InstantEffect_InvokesExecutedCue()
        {
            GameplayActor actor = new();
            GameplayTag cueTag = GameplayTag.Request("GameplayCue.Damage");
            GameplayEffect effect = new();
            effect.AddGameplayCue(CreateCue(cueTag));

            GameplayCueEventData received = null;
            actor.AbilitySystem.GameplayCueReceived += cue => received = cue;

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(received, Is.Not.Null);
            Assert.That(received.Target, Is.SameAs(actor.AbilitySystem));
            Assert.That(received.CueTag, Is.EqualTo(cueTag));
            Assert.That(received.EventType, Is.EqualTo(GameplayCueEvent.Executed));
        }

        [Test]
        public void DurationEffect_InvokesOnActiveAndWhileActiveCue()
        {
            GameplayActor actor = new();
            GameplayTag cueTag = GameplayTag.Request("GameplayCue.Buff");
            GameplayEffect effect = new();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Duration;
            effect.DurationSeconds = 3f;
            effect.AddGameplayCue(CreateCue(cueTag));

            List<GameplayCueEvent> receivedEvents = new();
            actor.AbilitySystem.GameplayCueReceived += cue => receivedEvents.Add(cue.EventType);

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));

            Assert.That(receivedEvents, Is.EqualTo(new[]
            {
                GameplayCueEvent.OnActive,
                GameplayCueEvent.WhileActive
            }));
        }

        [Test]
        public void RemoveActiveGameplayEffect_InvokesRemovedCue()
        {
            GameplayActor actor = new();
            GameplayTag cueTag = GameplayTag.Request("GameplayCue.Buff");
            GameplayEffect effect = new();
            effect.DurationPolicy = GameplayEffectDurationPolicy.Infinite;
            effect.AddGameplayCue(CreateCue(cueTag));

            List<GameplayCueEvent> receivedEvents = new();
            actor.AbilitySystem.GameplayCueReceived += cue => receivedEvents.Add(cue.EventType);

            ActiveGameplayEffect activeEffect =
                actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect));
            receivedEvents.Clear();

            actor.AbilitySystem.RemoveActiveGameplayEffect(activeEffect.Handle);

            Assert.That(receivedEvents, Is.EqualTo(new[] { GameplayCueEvent.Removed }));
        }

        [Test]
        public void ExecuteGameplayCue_InvokesExecutedCueWithoutEffect()
        {
            GameplayActor actor = new();
            GameplayTag cueTag = GameplayTag.Request("GameplayCue.Click");

            GameplayCueEventData received = null;
            actor.AbilitySystem.GameplayCueReceived += cue => received = cue;

            actor.AbilitySystem.ExecuteGameplayCue(cueTag);

            Assert.That(received, Is.Not.Null);
            Assert.That(received.CueTag, Is.EqualTo(cueTag));
            Assert.That(received.EventType, Is.EqualTo(GameplayCueEvent.Executed));
        }

        [Test]
        public void GameplayEffectCue_NormalizesLevel()
        {
            GameplayActor actor = new();
            GameplayTag cueTag = GameplayTag.Request("GameplayCue.Level");
            GameplayEffect effect = new();
            effect.AddGameplayCue(CreateCue(cueTag, 1f, 5f));

            GameplayCueParameters parameters = null;
            actor.AbilitySystem.GameplayCueReceived += cue => parameters = cue.Parameters;

            actor.AbilitySystem.ApplyGameplayEffectSpecToSelf(new GameplayEffectSpec(effect, level: 3));

            Assert.That(parameters, Is.Not.Null);
            Assert.That(parameters.NormalizedMagnitude, Is.EqualTo(0.5f));
            Assert.That(parameters.RawMagnitude, Is.EqualTo(3f));
            Assert.That(parameters.GameplayEffectLevel, Is.EqualTo(3));
        }

        private static GameplayEffectCue CreateCue(GameplayTag tag, float minLevel = 0f, float maxLevel = 0f)
        {
            GameplayEffectCue cue = new(minLevel, maxLevel);
            cue.GameplayCueTags.Add(tag);
            return cue;
        }
    }
}
