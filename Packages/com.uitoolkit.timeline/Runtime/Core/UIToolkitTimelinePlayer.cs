using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline
{
    internal sealed class UIToolkitTimelinePlayer
    {
        private readonly Dictionary<int, PlaybackRecord> _recordsById = new();
        private readonly List<int> _recordsToRemove = new();
        private VisualElement _target;
        private int _nextPlaybackId;
        private int _activePlaybackId;

        internal event Action<TimelineNotify> NotifyRaised;
        internal event Action<TimelinePlayback> PlaybackCompleted;
        internal event Action<TimelinePlayback> PlaybackCancelled;

        internal void Bind(VisualElement target)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
        }

        internal void Release()
        {
            CancelAll();
            _target = null;
        }

        internal bool CanPlay(TimelineRuntimeData runtime)
        {
            return _target != null &&
                   _target.panel != null &&
                   !runtime.IsEmpty;
        }

        internal TimelinePlayback Play(TimelineRuntimeData runtime)
        {
            if (!CanPlay(runtime))
                return TimelinePlayback.Invalid;

            if (_activePlaybackId != 0)
                Cancel(new TimelinePlayback(_activePlaybackId));

            PruneFinishedRecords();

            int id = ++_nextPlaybackId;
            TimelinePlayback playback = new(id);
            PlaybackRecord record = new(_target, playback, runtime);

            _recordsById[id] = record;
            _activePlaybackId = id;

            TimelineValueEvaluator.Clear(record.Target);

            record.ScheduledItem = _target.schedule.Execute(() => RunScheduled(record));
            record.ScheduledItem.Every(1);
            record.ScheduledItem.ExecuteLater(1);

            return playback;
        }

        internal void Cancel(TimelinePlayback playback)
        {
            if (!playback.IsValid ||
                !_recordsById.TryGetValue(playback.Id, out PlaybackRecord record))
            {
                return;
            }

            if (record.State == TimelinePlaybackState.Cancelled ||
                record.State == TimelinePlaybackState.Completed)
            {
                return;
            }

            CancelRecord(record, notify: true);
        }

        internal bool TryGetPlaybackState(
            TimelinePlayback playback,
            out TimelinePlaybackState state)
        {
            if (playback.IsValid &&
                _recordsById.TryGetValue(playback.Id, out PlaybackRecord record))
            {
                state = record.State;
                return true;
            }

            state = TimelinePlaybackState.None;
            return false;
        }

        private void RunScheduled(PlaybackRecord record)
        {
            if (_activePlaybackId != record.Playback.Id)
                return;

            if (record.State == TimelinePlaybackState.Preparing)
            {
                record.StartTimeMs = CurrentTimeMs;
                record.State = TimelinePlaybackState.Playing;
            }

            RunFrame(record);
        }

        private void RunFrame(PlaybackRecord record)
        {
            int elapsedMs = (int)Math.Max(0L, CurrentTimeMs - record.StartTimeMs);
            TimelineValueEvaluator.Apply(record.Target, record.Runtime, elapsedMs);

            while (record.Cursor < record.Runtime.EventCount)
            {
                TimelineEvent timelineEvent = record.Runtime.GetEvent(record.Cursor);
                if (timelineEvent.TimeMs > elapsedMs)
                    break;

                ExecuteEvent(record, timelineEvent);
                record.Cursor++;
            }

            if (elapsedMs >= record.Runtime.DurationMs)
            {
                TimelineValueEvaluator.Apply(record.Target, record.Runtime, record.Runtime.DurationMs);
                record.ScheduledItem?.Pause();
                record.State = TimelinePlaybackState.Completed;

                if (_activePlaybackId == record.Playback.Id)
                    _activePlaybackId = 0;

                PlaybackCompleted?.Invoke(record.Playback);
                return;
            }
        }

        private void CancelAll()
        {
            _recordsToRemove.Clear();

            foreach (KeyValuePair<int, PlaybackRecord> pair in _recordsById)
                _recordsToRemove.Add(pair.Key);

            for (int i = 0; i < _recordsToRemove.Count; i++)
            {
                if (_recordsById.TryGetValue(_recordsToRemove[i], out PlaybackRecord record))
                    CancelRecord(record, notify: true);
            }

            _recordsToRemove.Clear();
        }

        private void CancelRecord(PlaybackRecord record, bool notify)
        {
            if (record.State == TimelinePlaybackState.Cancelled ||
                record.State == TimelinePlaybackState.Completed)
            {
                return;
            }

            record.ScheduledItem?.Pause();
            TimelineValueEvaluator.Clear(record.Target);
            record.State = TimelinePlaybackState.Cancelled;

            if (_activePlaybackId == record.Playback.Id)
                _activePlaybackId = 0;

            if (notify)
                PlaybackCancelled?.Invoke(record.Playback);
        }

        private void PruneFinishedRecords()
        {
            _recordsToRemove.Clear();

            foreach (KeyValuePair<int, PlaybackRecord> pair in _recordsById)
            {
                if (pair.Value.State == TimelinePlaybackState.Completed ||
                    pair.Value.State == TimelinePlaybackState.Cancelled)
                {
                    _recordsToRemove.Add(pair.Key);
                }
            }

            for (int i = 0; i < _recordsToRemove.Count; i++)
                _recordsById.Remove(_recordsToRemove[i]);

            _recordsToRemove.Clear();
        }

        private void ExecuteEvent(PlaybackRecord record, TimelineEvent timelineEvent)
        {
            switch (timelineEvent.Op)
            {
                case TimelineOp.Notify:
                    TimelineNotifyDefinition definition = record.Runtime.GetNotifyDefinition(timelineEvent.Operand);
                    if (definition != null)
                        NotifyRaised?.Invoke(new TimelineNotify(record.Playback, definition));
                    break;
            }
        }

        private static long CurrentTimeMs => (long)(Time.realtimeSinceStartup * 1000f);

        private sealed class PlaybackRecord
        {
            internal PlaybackRecord(
                VisualElement target,
                TimelinePlayback playback,
                TimelineRuntimeData runtime)
            {
                Target = target;
                Playback = playback;
                Runtime = runtime;
                State = TimelinePlaybackState.Preparing;
            }

            internal VisualElement Target { get; }
            internal TimelinePlayback Playback { get; }
            internal TimelineRuntimeData Runtime { get; }
            internal TimelinePlaybackState State { get; set; }
            internal IVisualElementScheduledItem ScheduledItem { get; set; }
            internal int Cursor { get; set; }
            internal long StartTimeMs { get; set; }
        }
    }
}
