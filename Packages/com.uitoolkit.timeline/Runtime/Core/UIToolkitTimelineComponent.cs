using System;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline
{
    public sealed class UIToolkitTimelineComponent
    {
        private readonly UIToolkitTimelinePlayer _player = new();
        private VisualElement _target;

        public event Action<TimelineNotify> NotifyRaised
        {
            add => _player.NotifyRaised += value;
            remove => _player.NotifyRaised -= value;
        }

        public event Action<TimelinePlayback> PlaybackCompleted
        {
            add => _player.PlaybackCompleted += value;
            remove => _player.PlaybackCompleted -= value;
        }

        public event Action<TimelinePlayback> PlaybackCancelled
        {
            add => _player.PlaybackCancelled += value;
            remove => _player.PlaybackCancelled -= value;
        }

        public bool IsBound => _target != null;
        public bool IsInitialized => IsBound;

        public void Init(VisualElement target)
        {
            Bind(target);
        }

        public void Bind(VisualElement target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (_target != null)
                throw new InvalidOperationException("UIToolkitTimelineComponent is already bound.");

            _target = target;
            _player.Bind(target);
        }

        public void Release(VisualElement target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (_target == null)
                throw new InvalidOperationException("UIToolkitTimelineComponent is not bound.");

            if (!ReferenceEquals(_target, target))
                throw new InvalidOperationException("UIToolkitTimelineComponent release target does not match the bound target.");

            _player.Release();
            _target = null;
        }

        public bool CanPlay(TimelineAsset timeline)
        {
            return timeline != null &&
                   _player.CanPlay(timeline.RuntimeData);
        }

        public TimelinePlayback Play(TimelineAsset timeline)
        {
            return CanPlay(timeline)
                ? _player.Play(timeline.RuntimeData)
                : TimelinePlayback.Invalid;
        }

        public void Cancel(TimelinePlayback playback)
        {
            _player.Cancel(playback);
        }

        public bool TryGetPlaybackState(
            TimelinePlayback playback,
            out TimelinePlaybackState state)
        {
            return _player.TryGetPlaybackState(playback, out state);
        }
    }
}
