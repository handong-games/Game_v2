using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline
{
    [Serializable]
    public struct TimelineRuntimeData
    {
        [SerializeField]
        private TimelineValueClip[] _valueClips;

        [SerializeField]
        private TimelineNotifyDefinition[] _notifyTable;

        [SerializeField]
        private TimelineEvent[] _events;

        public TimelineRuntimeData(
            TimelineValueClip[] valueClips,
            TimelineNotifyDefinition[] notifyTable,
            TimelineEvent[] events)
        {
            _valueClips = valueClips ?? Array.Empty<TimelineValueClip>();
            _notifyTable = notifyTable ?? Array.Empty<TimelineNotifyDefinition>();
            _events = events ?? Array.Empty<TimelineEvent>();
        }

        public static TimelineRuntimeData Empty => new(
            Array.Empty<TimelineValueClip>(),
            Array.Empty<TimelineNotifyDefinition>(),
            Array.Empty<TimelineEvent>());

        public int ValueClipCount => ValueClips.Length;
        public int NotifyCount => NotifyTable.Length;
        public int EventCount => Events.Length;
        public bool IsEmpty => ValueClipCount == 0 && EventCount == 0;

        public int DurationMs
        {
            get
            {
                int durationMs = 0;

                TimelineValueClip[] valueClips = ValueClips;
                for (int i = 0; i < valueClips.Length; i++)
                    durationMs = Mathf.Max(durationMs, valueClips[i].EndMs);

                TimelineEvent[] events = Events;
                if (events.Length > 0)
                    durationMs = Mathf.Max(durationMs, events[events.Length - 1].TimeMs);

                return durationMs;
            }
        }

        internal TimelineValueClip[] ValueClips => _valueClips ?? Array.Empty<TimelineValueClip>();
        internal TimelineNotifyDefinition[] NotifyTable => _notifyTable ?? Array.Empty<TimelineNotifyDefinition>();
        internal TimelineEvent[] Events => _events ?? Array.Empty<TimelineEvent>();

        public TimelineValueClip GetValueClip(int index)
        {
            return ValueClips[index];
        }

        public TimelineNotifyDefinition GetNotifyDefinition(int index)
        {
            return NotifyTable[index];
        }

        public TimelineEvent GetEvent(int index)
        {
            return Events[index];
        }
    }

    [Serializable]
    public struct TimelineValueClip
    {
        [SerializeField]
        private TimelineValueProperty _property;

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

        public TimelineValueClip(
            TimelineValueProperty property,
            int startMs,
            int durationMs,
            Vector2 from,
            Vector2 to,
            TimelineEasing easing)
        {
            _property = property;
            _startMs = startMs;
            _durationMs = durationMs;
            _from = from;
            _to = to;
            _easing = easing;
        }

        public TimelineValueProperty Property => _property;
        public int StartMs => _startMs;
        public int DurationMs => _durationMs;
        public int EndMs => _startMs + _durationMs;
        public Vector2 From => _from;
        public Vector2 To => _to;
        public TimelineEasing Easing => _easing;
    }

    public static class TimelineValueEvaluator
    {
        public static void Apply(
            VisualElement target,
            TimelineRuntimeData runtime,
            int timeMs)
        {
            bool hasTranslate = false;
            bool hasScale = false;
            bool hasRotate = false;
            Vector2 translate = Vector2.zero;
            Vector2 scale = Vector2.one;
            float rotate = 0f;

            for (int i = 0; i < runtime.ValueClipCount; i++)
            {
                TimelineValueClip clip = runtime.GetValueClip(i);
                if (timeMs < clip.StartMs)
                    continue;

                if (timeMs > clip.EndMs)
                {
                    SetValue(clip.Property, clip.To, ref hasTranslate, ref translate, ref hasScale, ref scale, ref hasRotate, ref rotate);
                    continue;
                }

                float normalizedTime = clip.DurationMs <= 0
                    ? 1f
                    : Mathf.Clamp01((timeMs - clip.StartMs) / (float)clip.DurationMs);
                Vector2 value = Vector2.LerpUnclamped(clip.From, clip.To, Ease(normalizedTime, clip.Easing));
                SetValue(clip.Property, value, ref hasTranslate, ref translate, ref hasScale, ref scale, ref hasRotate, ref rotate);
            }

            if (hasTranslate)
                target.style.translate = new Translate(translate.x, translate.y);

            if (hasScale)
                target.style.scale = new Scale(new Vector3(scale.x, scale.y, 1f));

            if (hasRotate)
                target.style.rotate = new Rotate(Angle.Degrees(rotate));
        }

        public static void Clear(VisualElement target)
        {
            target.style.translate = new Translate(0f, 0f);
            target.style.scale = new Scale(Vector3.one);
            target.style.rotate = new Rotate(Angle.Degrees(0f));
        }

        private static void SetValue(
            TimelineValueProperty property,
            Vector2 value,
            ref bool hasTranslate,
            ref Vector2 translate,
            ref bool hasScale,
            ref Vector2 scale,
            ref bool hasRotate,
            ref float rotate)
        {
            switch (property)
            {
                case TimelineValueProperty.Translate:
                    hasTranslate = true;
                    translate = value;
                    break;

                case TimelineValueProperty.Scale:
                    hasScale = true;
                    scale = value;
                    break;

                case TimelineValueProperty.Rotate:
                    hasRotate = true;
                    rotate = value.x;
                    break;
            }
        }

        private static float Ease(float normalizedTime, TimelineEasing easing)
        {
            return easing switch
            {
                TimelineEasing.EaseIn => normalizedTime * normalizedTime,
                TimelineEasing.EaseOut => 1f - (1f - normalizedTime) * (1f - normalizedTime),
                TimelineEasing.EaseInOut => Mathf.SmoothStep(0f, 1f, normalizedTime),
                _ => normalizedTime
            };
        }
    }
}
