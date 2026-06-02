#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline
{
    public sealed partial class TimelineAsset
    {
        private void OnValidate()
        {
            CompileRuntimeData();
        }

        public void CompileRuntimeData()
        {
            List<string> messages = new();
            try
            {
                if (TryCompileRuntimeData(_authoring, messages, out TimelineRuntimeData runtime))
                {
                    _runtime = runtime;
                    _validationMessages = Array.Empty<string>();
                    return;
                }
            }
            catch (Exception exception)
            {
                messages.Add(exception.Message);
            }

            if (messages.Count > 0)
            {
                _runtime = TimelineRuntimeData.Empty;
                _validationMessages = messages.ToArray();
            }
        }

        private static bool TryCompileRuntimeData(
            TimelineAuthoringData authoring,
            List<string> messages,
            out TimelineRuntimeData runtime)
        {
            runtime = TimelineRuntimeData.Empty;

            ValidatePreview(authoring, messages);
            ValidateClips(authoring, messages);
            ValidateNotifies(authoring, messages);
            ValidateValueOverlap(authoring, messages);

            if (messages.Count > 0)
                return false;

            Dictionary<TimelineNotifyDefinition, ushort> notifyIndexes = new();
            List<TimelineValueClip> valueClips = new();
            List<TimelineNotifyDefinition> notifyTable = new();
            List<CompilationEvent> compilationEvents = new();

            int sequence = 0;
            TimelineTrackData[] tracks = authoring.Tracks;
            for (int trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
            {
                TimelineValueClipData[] clips = tracks[trackIndex].Clips;
                for (int clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                {
                    TimelineValueClipData clip = clips[clipIndex];
                    valueClips.Add(new TimelineValueClip(
                        tracks[trackIndex].Property,
                        clip.StartMs,
                        clip.DurationMs,
                        clip.From,
                        clip.To,
                        clip.Easing));
                }
            }

            TimelineNotifyPlacement[] notifies = authoring.Notifies;
            for (int notifyIndex = 0; notifyIndex < notifies.Length; notifyIndex++)
            {
                TimelineNotifyPlacement notify = notifies[notifyIndex];
                ushort compiledNotifyIndex = GetOrAddNotifyIndex(
                    notify.Definition,
                    notifyIndexes,
                    notifyTable);

                compilationEvents.Add(new CompilationEvent(
                    new TimelineEvent(notify.TimeMs, TimelineOp.Notify, compiledNotifyIndex),
                    sequence));

                sequence++;
            }

            compilationEvents.Sort(CompareCompilationEvents);
            valueClips.Sort(CompareValueClips);

            TimelineEvent[] events = new TimelineEvent[compilationEvents.Count];
            for (int i = 0; i < compilationEvents.Count; i++)
                events[i] = compilationEvents[i].Event;

            runtime = new TimelineRuntimeData(
                valueClips.ToArray(),
                notifyTable.ToArray(),
                events);

            return true;
        }

        private static void ValidatePreview(
            TimelineAuthoringData authoring,
            List<string> messages)
        {
            if (authoring.PreviewTree == null)
            {
                messages.Add("Preview Tree is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(authoring.PreviewTargetName))
            {
                messages.Add("Preview Target Name is required.");
                return;
            }

            VisualElement root = authoring.PreviewTree.Instantiate();
            int matchCount = CountElementNameMatches(root, authoring.PreviewTargetName);
            if (matchCount != 1)
            {
                messages.Add(
                    $"Preview Target Name '{authoring.PreviewTargetName}' must match exactly one element, but found {matchCount}.");
            }
        }

        private static int CountElementNameMatches(
            VisualElement root,
            string elementName)
        {
            int count = 0;
            root.Query<VisualElement>().ForEach(element =>
            {
                if (element.name == elementName)
                    count++;
            });

            return count;
        }

        private static void ValidateClips(
            TimelineAuthoringData authoring,
            List<string> messages)
        {
            TimelineTrackData[] tracks = authoring.Tracks;
            for (int trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
            {
                TimelineValueClipData[] clips = tracks[trackIndex].Clips;
                for (int clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                {
                    TimelineValueClipData clip = clips[clipIndex];
                    string path = $"Track {trackIndex}, Clip {clipIndex}";

                    if (clip.StartMs < 0)
                        messages.Add($"{path}: StartMs must be greater than or equal to 0.");

                    if (clip.DurationMs <= 0)
                    {
                        messages.Add($"{path}: DurationMs must be greater than 0.");
                        continue;
                    }

                    long endMs = (long)clip.StartMs + clip.DurationMs;
                    if (endMs > int.MaxValue)
                        messages.Add($"{path}: EndMs exceeds Int32.MaxValue.");
                }
            }
        }

        private static void ValidateNotifies(
            TimelineAuthoringData authoring,
            List<string> messages)
        {
            TimelineNotifyPlacement[] notifies = authoring.Notifies;
            for (int notifyIndex = 0; notifyIndex < notifies.Length; notifyIndex++)
            {
                TimelineNotifyPlacement notify = notifies[notifyIndex];
                string path = $"Notify {notifyIndex}";

                if (notify.Definition == null)
                    messages.Add($"{path}: Definition is required.");

                if (notify.TimeMs < 0)
                    messages.Add($"{path}: TimeMs must be greater than or equal to 0.");
            }
        }

        private static void ValidateValueOverlap(
            TimelineAuthoringData authoring,
            List<string> messages)
        {
            Dictionary<TimelineValueProperty, List<ClipInterval>> intervalsByProperty = new();
            TimelineTrackData[] tracks = authoring.Tracks;

            for (int trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
            {
                TimelineValueClipData[] clips = tracks[trackIndex].Clips;
                for (int clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                {
                    TimelineValueClipData clip = clips[clipIndex];
                    long endMs = (long)clip.StartMs + clip.DurationMs;
                    if (clip.StartMs < 0 ||
                        clip.DurationMs <= 0 ||
                        endMs > int.MaxValue)
                    {
                        continue;
                    }

                    TimelineValueProperty property = tracks[trackIndex].Property;
                    if (!intervalsByProperty.TryGetValue(property, out List<ClipInterval> intervals))
                    {
                        intervals = new List<ClipInterval>();
                        intervalsByProperty.Add(property, intervals);
                    }

                    intervals.Add(new ClipInterval(
                        clip.StartMs,
                        (int)endMs,
                        trackIndex,
                        clipIndex));
                }
            }

            foreach (KeyValuePair<TimelineValueProperty, List<ClipInterval>> pair in intervalsByProperty)
            {
                List<ClipInterval> intervals = pair.Value;
                intervals.Sort((left, right) => left.StartMs.CompareTo(right.StartMs));

                for (int i = 1; i < intervals.Count; i++)
                {
                    ClipInterval previous = intervals[i - 1];
                    ClipInterval current = intervals[i];
                    if (current.StartMs < previous.EndMs)
                    {
                        messages.Add(
                            $"{pair.Key} clips overlap: " +
                            $"Track {previous.TrackIndex}, Clip {previous.ClipIndex} and " +
                            $"Track {current.TrackIndex}, Clip {current.ClipIndex}.");
                    }
                }
            }
        }

        private static ushort GetOrAddNotifyIndex(
            TimelineNotifyDefinition definition,
            Dictionary<TimelineNotifyDefinition, ushort> indexes,
            List<TimelineNotifyDefinition> table)
        {
            if (indexes.TryGetValue(definition, out ushort index))
                return index;

            index = checked((ushort)table.Count);
            indexes.Add(definition, index);
            table.Add(definition);
            return index;
        }

        private static int CompareCompilationEvents(
            CompilationEvent left,
            CompilationEvent right)
        {
            int timeComparison = left.Event.TimeMs.CompareTo(right.Event.TimeMs);
            if (timeComparison != 0)
                return timeComparison;

            int priorityComparison = GetOpPriority(left.Event.Op).CompareTo(GetOpPriority(right.Event.Op));
            if (priorityComparison != 0)
                return priorityComparison;

            return left.Sequence.CompareTo(right.Sequence);
        }

        private static int CompareValueClips(
            TimelineValueClip left,
            TimelineValueClip right)
        {
            int propertyComparison = left.Property.CompareTo(right.Property);
            if (propertyComparison != 0)
                return propertyComparison;

            return left.StartMs.CompareTo(right.StartMs);
        }

        private static int GetOpPriority(TimelineOp op)
        {
            return op switch
            {
                TimelineOp.Notify => 0,
                _ => 3
            };
        }

        private readonly struct CompilationEvent
        {
            internal CompilationEvent(
                TimelineEvent timelineEvent,
                int sequence)
            {
                Event = timelineEvent;
                Sequence = sequence;
            }

            internal TimelineEvent Event { get; }
            internal int Sequence { get; }
        }

        private readonly struct ClipInterval
        {
            internal ClipInterval(
                int startMs,
                int endMs,
                int trackIndex,
                int clipIndex)
            {
                StartMs = startMs;
                EndMs = endMs;
                TrackIndex = trackIndex;
                ClipIndex = clipIndex;
            }

            internal int StartMs { get; }
            internal int EndMs { get; }
            internal int TrackIndex { get; }
            internal int ClipIndex { get; }
        }
    }
}
#endif
