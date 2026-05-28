using NUnit.Framework;
using UnityEngine;

namespace Gameplay.GAS.Tests
{
    public sealed class GameplayAbilityTaskTests
    {
        [Test]
        public void WaitGameplayEvent_BroadcastsMatchingEvent()
        {
            GameplayTag eventTag = GameplayTag.Define("Event.Attack");
            GameplayActor actor = new();
            EventWaitAbility ability = ScriptableObject.CreateInstance<EventWaitAbility>();
            ability.EventTag = eventTag;

            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);
            actor.AbilitySystem.TryActivateAbility(handle);
            actor.AbilitySystem.HandleGameplayEvent(new GameplayEventData(eventTag));

            Assert.That(ability.EventCount, Is.EqualTo(1));
            Assert.That(ability.Task.IsActive, Is.True);
            Assert.That(ability.ActiveTasks.Count, Is.EqualTo(1));
        }

        [Test]
        public void WaitGameplayEvent_OnlyTriggerOnceEndsTask()
        {
            GameplayTag eventTag = GameplayTag.Define("Event.Attack");
            GameplayActor actor = new();
            EventWaitAbility ability = ScriptableObject.CreateInstance<EventWaitAbility>();
            ability.EventTag = eventTag;
            ability.OnlyTriggerOnce = true;

            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);
            actor.AbilitySystem.TryActivateAbility(handle);

            actor.AbilitySystem.HandleGameplayEvent(new GameplayEventData(eventTag));
            actor.AbilitySystem.HandleGameplayEvent(new GameplayEventData(eventTag));

            Assert.That(ability.EventCount, Is.EqualTo(1));
            Assert.That(ability.Task.IsEnded, Is.True);
            Assert.That(ability.ActiveTasks, Is.Empty);
        }

        [Test]
        public void WaitGameplayEvent_NonExactMatchesChildTag()
        {
            GameplayTag parentTag = GameplayTag.Define("Event.Attack");
            GameplayTag childTag = GameplayTag.Define("Event.Attack.Light");
            GameplayActor actor = new();
            EventWaitAbility ability = ScriptableObject.CreateInstance<EventWaitAbility>();
            ability.EventTag = parentTag;
            ability.OnlyMatchExact = false;

            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);
            actor.AbilitySystem.TryActivateAbility(handle);
            actor.AbilitySystem.HandleGameplayEvent(new GameplayEventData(childTag));

            Assert.That(ability.EventCount, Is.EqualTo(1));
        }

        [Test]
        public void EndAbility_EndsActiveTasks()
        {
            GameplayTag eventTag = GameplayTag.Define("Event.Attack");
            GameplayActor actor = new();
            EventWaitAbility ability = ScriptableObject.CreateInstance<EventWaitAbility>();
            ability.EventTag = eventTag;

            GameplayAbilitySpecHandle handle = actor.AbilitySystem.GiveAbility(ability);
            actor.AbilitySystem.TryActivateAbility(handle);
            ability.EndAbility(
                ability.AbilityHandle,
                ability.ActorInfo,
                ability.ActivationInfo,
                false);
            actor.AbilitySystem.HandleGameplayEvent(new GameplayEventData(eventTag));

            Assert.That(ability.EventCount, Is.EqualTo(0));
            Assert.That(ability.Task.IsEnded, Is.True);
            Assert.That(ability.ActiveTasks, Is.Empty);
        }

        private sealed class EventWaitAbility : GameplayAbility
        {
            public GameplayTag EventTag { get; set; }
            public bool OnlyTriggerOnce { get; set; }
            public bool OnlyMatchExact { get; set; } = true;

            public AbilityTaskWaitGameplayEvent Task { get; private set; }
            public GameplayAbilitySpecHandle AbilityHandle { get; private set; }
            public GameplayAbilityActorInfo ActorInfo { get; private set; }
            public GameplayAbilityActivationInfo ActivationInfo { get; private set; }
            public int EventCount { get; private set; }

            public override void ActivateAbility(
                GameplayAbilitySpecHandle handle,
                GameplayAbilityActorInfo actorInfo,
                GameplayAbilityActivationInfo activationInfo,
                GameplayEventData triggerEventData)
            {
                AbilityHandle = handle;
                ActorInfo = actorInfo;
                ActivationInfo = activationInfo;
                Task = AbilityTaskWaitGameplayEvent.WaitGameplayEvent(
                    this,
                    handle,
                    actorInfo,
                    activationInfo,
                    EventTag,
                    OnlyTriggerOnce,
                    OnlyMatchExact);
                Task.EventReceived += OnEventReceived;
                Task.Activate();
            }

            private void OnEventReceived(GameplayEventData eventData)
            {
                EventCount++;
            }
        }
    }
}


