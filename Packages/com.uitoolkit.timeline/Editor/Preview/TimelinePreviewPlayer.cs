using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline.Editor
{
    internal sealed class TimelinePreviewPlayer : IDisposable
    {
        private VisualElement _target;
        private TimelineRuntimeData _runtime;
        private bool _isInitialized;
        private bool _isPlaying;
        private bool _isPreparingPlay;
        private bool _isEditorUpdateRegistered;
        private bool _hasPendingAdvance;
        private double _playStartedAt;
        private int _timeAtPlayStartMs;
        private int _pendingPreviousTimeMs;
        private int _pendingCurrentTimeMs;
        private bool _pendingEmitNotifies;
        private bool _includeZeroEventsOnNextAdvance = true;
        private float _playbackSpeed = 1f;

        internal event Action<TimelineNotify> NotifyRaised;
        internal event Action<int> TimeChanged;

        internal int CurrentTimeMs { get; private set; }
        internal int DurationMs => _runtime.DurationMs;
        internal bool IsPlaying => _isPlaying;
        internal float PlaybackSpeed => _playbackSpeed;

        internal void Init(
            VisualElement target,
            TimelineRuntimeData runtime)
        {
            ResetUpdateState();

            _target = target ?? throw new ArgumentNullException(nameof(target));
            _runtime = runtime;
            _isInitialized = true;
            CurrentTimeMs = 0;
            _includeZeroEventsOnNextAdvance = true;

            TimelineValueEvaluator.Clear(_target);
            TimeChanged?.Invoke(CurrentTimeMs);
        }

        internal void Play()
        {
            if (!_isInitialized || _runtime.IsEmpty)
                return;

            if (CurrentTimeMs >= DurationMs)
                SetTime(0);

            if (_isPlaying)
                return;

            _isPlaying = true;
            _isPreparingPlay = true;
            EnsureEditorUpdate();
        }

        internal void SetPlaybackSpeed(float playbackSpeed)
        {
            float sanitizedSpeed = Mathf.Max(0.01f, playbackSpeed);
            if (Mathf.Approximately(_playbackSpeed, sanitizedSpeed))
                return;

            if (_isPlaying && !_isPreparingPlay)
            {
                _timeAtPlayStartMs = CurrentTimeMs;
                _playStartedAt = EditorApplication.timeSinceStartup;
            }

            _playbackSpeed = sanitizedSpeed;
        }

        internal void Pause()
        {
            if (!_isPlaying)
                return;

            _isPlaying = false;
            _isPreparingPlay = false;
            ReleaseEditorUpdateIfIdle();
        }

        internal void SetTime(int timeMs)
        {
            if (!_isInitialized)
                return;

            int clampedTimeMs = Mathf.Clamp(timeMs, 0, DurationMs);
            int previousTimeMs = CurrentTimeMs;

            if (clampedTimeMs == previousTimeMs)
            {
                TimeChanged?.Invoke(CurrentTimeMs);
                return;
            }

            CurrentTimeMs = clampedTimeMs;

            CancelPendingAdvance();
            TimelineValueEvaluator.Apply(_target, _runtime, clampedTimeMs);

            if (clampedTimeMs >= previousTimeMs)
                EmitCrossedNotifies(previousTimeMs, clampedTimeMs, true);
            else
                _includeZeroEventsOnNextAdvance = clampedTimeMs == 0;

            TimeChanged?.Invoke(CurrentTimeMs);
        }

        public void Dispose()
        {
            ResetUpdateState();
        }

        private void Update()
        {
            if (_hasPendingAdvance)
            {
                AdvanceForward(_pendingPreviousTimeMs, _pendingCurrentTimeMs, _pendingEmitNotifies);
                _hasPendingAdvance = false;
                TimeChanged?.Invoke(CurrentTimeMs);
                ReleaseEditorUpdateIfIdle();
                return;
            }

            if (_isPreparingPlay)
            {
                _isPreparingPlay = false;
                _playStartedAt = EditorApplication.timeSinceStartup;
                _timeAtPlayStartMs = CurrentTimeMs;
                TimelineValueEvaluator.Apply(_target, _runtime, CurrentTimeMs);
                EmitCrossedNotifies(CurrentTimeMs, CurrentTimeMs, true);
                TimeChanged?.Invoke(CurrentTimeMs);
                return;
            }

            double elapsedSeconds = EditorApplication.timeSinceStartup - _playStartedAt;
            int nextTimeMs = _timeAtPlayStartMs + Mathf.Max(0, (int)(elapsedSeconds * 1000.0 * _playbackSpeed));

            SetTime(nextTimeMs);

            if (CurrentTimeMs >= DurationMs)
                Pause();
        }

        private void QueuePendingAdvance(
            int previousTimeMs,
            int currentTimeMs,
            bool emitNotifies)
        {
            _pendingPreviousTimeMs = previousTimeMs;
            _pendingCurrentTimeMs = currentTimeMs;
            _pendingEmitNotifies = emitNotifies;
            _hasPendingAdvance = true;
            EnsureEditorUpdate();
        }

        private void CancelPendingAdvance()
        {
            if (!_hasPendingAdvance)
                return;

            _hasPendingAdvance = false;
            ReleaseEditorUpdateIfIdle();
        }

        private void EnsureEditorUpdate()
        {
            if (_isEditorUpdateRegistered)
                return;

            _isEditorUpdateRegistered = true;
            EditorApplication.update += Update;
        }

        private void ReleaseEditorUpdateIfIdle()
        {
            if (_isPlaying || _hasPendingAdvance || !_isEditorUpdateRegistered)
                return;

            _isEditorUpdateRegistered = false;
            EditorApplication.update -= Update;
        }

        private void ResetUpdateState()
        {
            _isPlaying = false;
            _isPreparingPlay = false;
            _hasPendingAdvance = false;

            if (!_isEditorUpdateRegistered)
                return;

            _isEditorUpdateRegistered = false;
            EditorApplication.update -= Update;
        }

        private void AdvanceForward(
            int previousTimeMs,
            int currentTimeMs,
            bool emitNotifies)
        {
            TimelineValueEvaluator.Apply(_target, _runtime, currentTimeMs);
            EmitCrossedNotifies(previousTimeMs, currentTimeMs, emitNotifies);
        }

        private void EmitCrossedNotifies(
            int previousTimeMs,
            int currentTimeMs,
            bool emitNotifies)
        {
            int lowerBoundMs = _includeZeroEventsOnNextAdvance ? -1 : previousTimeMs;
            _includeZeroEventsOnNextAdvance = false;

            if (!emitNotifies || currentTimeMs <= lowerBoundMs)
                return;

            for (int i = 0; i < _runtime.EventCount; i++)
            {
                TimelineEvent timelineEvent = _runtime.GetEvent(i);
                if (timelineEvent.TimeMs > currentTimeMs)
                    break;

                if (timelineEvent.TimeMs <= lowerBoundMs)
                    continue;

                switch (timelineEvent.Op)
                {
                    case TimelineOp.Notify:
                        TimelineNotifyDefinition definition = _runtime.GetNotifyDefinition(timelineEvent.Operand);
                        if (definition != null)
                            NotifyRaised?.Invoke(new TimelineNotify(TimelinePlayback.Invalid, definition));
                        break;
                }
            }
        }

        internal void SetIdle()
        {
            if (!_isInitialized)
                return;

            TimelineValueEvaluator.Clear(_target);
        }
    }
}
