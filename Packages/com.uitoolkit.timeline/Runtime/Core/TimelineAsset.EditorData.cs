#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline
{
    public sealed partial class TimelineAsset
    {
        [SerializeField]
        private TimelineAuthoringData _authoring;

        [SerializeField]
        private string[] _validationMessages = Array.Empty<string>();

        public TimelineAuthoringData AuthoringData => _authoring;
        public string[] ValidationMessages => _validationMessages ?? Array.Empty<string>();
        public bool HasValidationMessages => ValidationMessages.Length > 0;
    }

    [Serializable]
    public struct TimelineAuthoringData
    {
        [SerializeField]
        private VisualTreeAsset _previewTree;

        [SerializeField]
        private string _previewTargetName;

        [SerializeField]
        private TimelineTrackData[] _tracks;

        [SerializeField]
        private TimelineNotifyPlacement[] _notifies;

        public VisualTreeAsset PreviewTree => _previewTree;
        public string PreviewTargetName => _previewTargetName;
        public int TrackCount => Tracks.Length;
        public int NotifyCount => Notifies.Length;

        public TimelineTrackData[] Tracks => _tracks ?? Array.Empty<TimelineTrackData>();
        public TimelineNotifyPlacement[] Notifies => _notifies ?? Array.Empty<TimelineNotifyPlacement>();

        public TimelineTrackData GetTrack(int index)
        {
            return Tracks[index];
        }

        public TimelineNotifyPlacement GetNotify(int index)
        {
            return Notifies[index];
        }
    }

    [Serializable]
    public struct TimelineTrackData
    {
        [SerializeField]
        private TimelineValueProperty _property;

        [SerializeField]
        private TimelineValueClipData[] _clips;

        public TimelineValueProperty Property => _property;
        public int ClipCount => Clips.Length;

        public TimelineValueClipData[] Clips => _clips ?? Array.Empty<TimelineValueClipData>();

        public TimelineValueClipData GetClip(int index)
        {
            return Clips[index];
        }
    }

    [Serializable]
    public struct TimelineValueClipData
    {
        [SerializeField]
        private int _startMs;

        [SerializeField]
        private int _durationMs;

        [SerializeField]
        private Vector2 _from;

        [SerializeField]
        private Vector2 _to;

        [SerializeField]
        private TimelineEasing _easing;

        public int StartMs => _startMs;
        public int DurationMs => _durationMs;
        public int EndMs => _startMs + _durationMs;
        public Vector2 From => _from;
        public Vector2 To => _to;
        public TimelineEasing Easing => _easing;
    }

    [Serializable]
    public struct TimelineNotifyPlacement
    {
        [SerializeField]
        private int _timeMs;

        [SerializeField]
        private TimelineNotifyDefinition _definition;

        public int TimeMs => _timeMs;
        public TimelineNotifyDefinition Definition => _definition;
    }
}
#endif
