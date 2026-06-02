using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline.Editor
{
    [CreateAssetMenu(
        fileName = "TimelineComposition",
        menuName = "UIToolkit Timeline/Timeline Composition")]
    public sealed class TimelineComposition : ScriptableObject
    {
        [SerializeField]
        private VisualTreeAsset _previewDocument;

        [SerializeField]
        private TimelineCompositionBinding[] _bindings = Array.Empty<TimelineCompositionBinding>();

        public VisualTreeAsset PreviewDocument => _previewDocument;
        public TimelineCompositionBinding[] Bindings => _bindings ?? Array.Empty<TimelineCompositionBinding>();

        public int DurationMs
        {
            get
            {
                int durationMs = 0;
                TimelineCompositionBinding[] bindings = Bindings;
                for (int i = 0; i < bindings.Length; i++)
                {
                    TimelineAsset timeline = bindings[i].Timeline;
                    if (timeline == null)
                        continue;

                    durationMs = Mathf.Max(
                        durationMs,
                        Mathf.Max(0, bindings[i].StartOffsetMs) + timeline.DurationMs);
                }

                return durationMs;
            }
        }
    }

    [Serializable]
    public struct TimelineCompositionBinding
    {
        [SerializeField]
        private string _elementName;

        [SerializeField]
        private TimelineAsset _timeline;

        [SerializeField]
        private int _startOffsetMs;

        public string ElementName => _elementName;
        public TimelineAsset Timeline => _timeline;
        public int StartOffsetMs => _startOffsetMs;
    }
}
