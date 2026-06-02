using System;
using Gameplay.GAS;
using UIToolkit.Timeline;

namespace Game.AbilitySystem.Tasks
{
    public sealed class AbilityTask_PlayTimeline : GameplayAbilityTask
    {
        private UIToolkitTimelineComponent _component;
        private TimelineAsset _timeline;
        private TimelinePlayback _playback;
        private bool _cancelWhenAbilityEnds;
        private bool _hasFinished;

        public event Action<TimelineNotify> NotifyRaised;
        public event Action Completed;
        public event Action Cancelled;

        public static AbilityTask_PlayTimeline PlayTimeline(
            GameplayAbility ability,
            GameplayAbilitySpecHandle handle,
            GameplayAbilityActorInfo actorInfo,
            GameplayAbilityActivationInfo activationInfo,
            UIToolkitTimelineComponent component,
            TimelineAsset timeline,
            bool cancelWhenAbilityEnds = true)
        {
            AbilityTask_PlayTimeline task =
                ability.CreateTask<AbilityTask_PlayTimeline>(handle, actorInfo, activationInfo);

            task._component = component;
            task._timeline = timeline;
            task._cancelWhenAbilityEnds = cancelWhenAbilityEnds;
            return task;
        }

        public override void Activate()
        {
            if (_component == null ||
                _timeline == null ||
                !_component.CanPlay(_timeline))
            {
                Cancel();
                return;
            }

            _component.NotifyRaised += OnNotifyRaised;
            _component.PlaybackCompleted += OnPlaybackCompleted;
            _component.PlaybackCancelled += OnPlaybackCancelled;

            _playback = _component.Play(_timeline);
            if (!_playback.IsValid)
                Cancel();
        }

        protected override void OnDestroy(bool abilityEnding)
        {
            if (_component != null)
            {
                _component.NotifyRaised -= OnNotifyRaised;
                _component.PlaybackCompleted -= OnPlaybackCompleted;
                _component.PlaybackCancelled -= OnPlaybackCancelled;

                if (abilityEnding &&
                    _cancelWhenAbilityEnds &&
                    !_hasFinished &&
                    _playback.IsValid)
                {
                    _component.Cancel(_playback);
                }
            }

            NotifyRaised = null;
            Completed = null;
            Cancelled = null;
            _component = null;
            _timeline = null;
        }

        private void OnNotifyRaised(TimelineNotify notify)
        {
            if (notify.Playback.Id != _playback.Id)
                return;

            if (ShouldBroadcastAbilityTaskDelegates())
                NotifyRaised?.Invoke(notify);
        }

        private void OnPlaybackCompleted(TimelinePlayback playback)
        {
            if (playback.Id == _playback.Id)
                Complete();
        }

        private void OnPlaybackCancelled(TimelinePlayback playback)
        {
            if (playback.Id == _playback.Id)
                Cancel();
        }

        private void Complete()
        {
            if (_hasFinished)
                return;

            _hasFinished = true;

            if (ShouldBroadcastAbilityTaskDelegates())
                Completed?.Invoke();

            EndTask();
        }

        private void Cancel()
        {
            if (_hasFinished)
                return;

            _hasFinished = true;

            if (ShouldBroadcastAbilityTaskDelegates())
                Cancelled?.Invoke();

            EndTask();
        }
    }
}
