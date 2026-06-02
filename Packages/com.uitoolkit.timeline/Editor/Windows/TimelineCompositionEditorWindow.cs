using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline.Editor
{
    public sealed class TimelineCompositionEditorWindow : EditorWindow
    {
        private const int SmallJumpMs = 10;
        private const int LargeJumpMs = 50;
        private const float TimelineLabelWidth = 140f;
        private const double PreviewTransitionRepaintSeconds = 0.5;

        [SerializeField]
        private TimelineComposition _composition;

        private readonly List<BindingPreviewRecord> _records = new();
        private readonly List<BindingRowVisual> _rowVisuals = new();
        private VisualElement _previewStage;
        private VisualElement _timelineView;
        private VisualElement _playhead;
        private Label _timeLabel;
        private Label _summaryLabel;
        private Label _debugLabel;
        private SliderInt _timeSlider;
        private Button _playButton;
        private ToolbarMenu _speedMenu;
        private bool _suppressTimeChange;
        private bool _isPlaying;
        private bool _isPreviewRepaintPumpActive;
        private double _previewRepaintUntil;
        private double _playStartedAt;
        private int _timeAtPlayStartMs;
        private float _playbackSpeed = 1f;

        private int CurrentTimeMs { get; set; }
        private int DurationMs => _composition == null ? 0 : _composition.DurationMs;

        [MenuItem("Window/UIToolkit Timeline/Timeline Composition Editor")]
        public static void Open()
        {
            GetWindow<TimelineCompositionEditorWindow>().titleContent =
                new GUIContent("Timeline Composition Editor");
        }

        public static void Open(TimelineComposition composition)
        {
            TimelineCompositionEditorWindow window = GetWindow<TimelineCompositionEditorWindow>();
            window.titleContent = new GUIContent("Timeline Composition Editor");
            window.SetComposition(composition);
            window.Show();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
#pragma warning disable CS0618
            UnityEngine.Object openedObject = EditorUtility.InstanceIDToObject(instanceID);
#pragma warning restore CS0618

            if (openedObject is not TimelineComposition composition)
                return false;

            Open(composition);
            return true;
        }

        public void CreateGUI()
        {
            DisposePlayers();

            VisualElement root = rootVisualElement;
            root.Clear();
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);

            BuildToolbar(root);
            BuildMain(root);
            BuildTimelinePanel(root);

            RefreshAll();
            root.Focus();
        }

        private void OnDisable()
        {
            Pause();
            DisposePlayers();
            StopPreviewRepaintPump();
        }

        private void SetComposition(TimelineComposition composition)
        {
            _composition = composition;

            if (rootVisualElement.childCount > 0)
                RefreshAll();
        }

        private void BuildToolbar(VisualElement root)
        {
            Toolbar toolbar = new();

            ObjectField compositionField = new("Composition")
            {
                objectType = typeof(TimelineComposition),
                value = _composition,
                allowSceneObjects = false
            };
            compositionField.RegisterValueChangedCallback(evt => SetComposition(evt.newValue as TimelineComposition));
            toolbar.Add(compositionField);

            _playButton = new Button(TogglePlay) { text = "Play" };
            toolbar.Add(_playButton);

            _speedMenu = new ToolbarMenu { text = "Speed 1x" };
            AddSpeedMenuItem(0.25f, "0.25x");
            AddSpeedMenuItem(0.5f, "0.5x");
            AddSpeedMenuItem(1f, "1x");
            AddSpeedMenuItem(2f, "2x");
            AddSpeedMenuItem(4f, "4x");
            toolbar.Add(_speedMenu);

            toolbar.Add(new Button(() => SetCompositionTime(0)) { text = "Home" });
            toolbar.Add(new Button(() => SetCompositionTime(CurrentTimeMs + SmallJumpMs)) { text = $"+{SmallJumpMs}ms" });
            toolbar.Add(new Button(() => SetCompositionTime(CurrentTimeMs + LargeJumpMs)) { text = $"+{LargeJumpMs}ms" });

            _timeLabel = new Label("0 ms");
            _timeLabel.style.minWidth = 96f;
            toolbar.Add(_timeLabel);

            root.Add(toolbar);
        }

        private void AddSpeedMenuItem(
            float speed,
            string label)
        {
            _speedMenu.menu.AppendAction(
                label,
                _ => SetPlaybackSpeed(speed, label),
                _ => Mathf.Approximately(_playbackSpeed, speed)
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
        }

        private void SetPlaybackSpeed(
            float speed,
            string label)
        {
            float sanitizedSpeed = Mathf.Max(0.01f, speed);
            if (_isPlaying)
            {
                _timeAtPlayStartMs = CurrentTimeMs;
                _playStartedAt = EditorApplication.timeSinceStartup;
            }

            _playbackSpeed = sanitizedSpeed;

            if (_speedMenu != null)
                _speedMenu.text = $"Speed {label}";
        }

        private void BuildMain(VisualElement root)
        {
            VisualElement main = new();
            main.style.flexDirection = FlexDirection.Row;
            main.style.flexGrow = 1f;
            main.style.minHeight = 280f;
            root.Add(main);

            _previewStage = new VisualElement();
            _previewStage.style.flexGrow = 1f;
            _previewStage.style.marginLeft = 6f;
            _previewStage.style.marginRight = 6f;
            _previewStage.style.paddingLeft = 12f;
            _previewStage.style.paddingRight = 12f;
            _previewStage.style.paddingTop = 12f;
            _previewStage.style.paddingBottom = 12f;
            _previewStage.style.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
            main.Add(_previewStage);

            VisualElement rightPanel = CreatePanel(300f);
            rightPanel.Add(new Label("Composition") { style = { unityFontStyleAndWeight = FontStyle.Bold } });

            _summaryLabel = new Label("No Composition");
            _debugLabel = new Label("Current: -");
            _summaryLabel.style.whiteSpace = WhiteSpace.Normal;
            _debugLabel.style.whiteSpace = WhiteSpace.Normal;
            rightPanel.Add(_summaryLabel);
            rightPanel.Add(_debugLabel);

            main.Add(rightPanel);
        }

        private void BuildTimelinePanel(VisualElement root)
        {
            VisualElement panel = new();
            panel.style.height = 220f;
            panel.style.borderTopWidth = 1f;
            panel.style.borderTopColor = new Color(0.24f, 0.24f, 0.24f);
            panel.style.paddingTop = 4f;
            root.Add(panel);

            _timeSlider = new SliderInt(0, 0);
            _timeSlider.RegisterValueChangedCallback(evt =>
            {
                if (!_suppressTimeChange)
                    SetCompositionTime(evt.newValue);
            });
            panel.Add(_timeSlider);

            _timelineView = new VisualElement();
            _timelineView.style.flexGrow = 1f;
            _timelineView.style.position = Position.Relative;
            _timelineView.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            panel.Add(_timelineView);
        }

        private static VisualElement CreatePanel(float width)
        {
            VisualElement panel = new();
            panel.style.width = width;
            panel.style.paddingLeft = 8f;
            panel.style.paddingRight = 8f;
            panel.style.paddingTop = 8f;
            panel.style.paddingBottom = 8f;
            panel.style.backgroundColor = new Color(0.19f, 0.19f, 0.19f);
            return panel;
        }

        private void RefreshAll()
        {
            Pause();
            RefreshPreview();
            RefreshTimeline();
            SetCompositionTime(0);
            RefreshSummary();
        }

        private void RefreshPreview()
        {
            DisposePlayers();
            _previewStage?.Clear();

            if (_composition == null || _composition.PreviewDocument == null)
                return;

            VisualElement previewRoot = _composition.PreviewDocument.Instantiate();
            _previewStage.Add(previewRoot);

            TimelineCompositionBinding[] bindings = _composition.Bindings;
            for (int i = 0; i < bindings.Length; i++)
            {
                TimelineCompositionBinding binding = bindings[i];
                VisualElement target = FindNamedElement(previewRoot, binding.ElementName);
                TimelinePreviewPlayer player = new();
                if (target != null && binding.Timeline != null)
                {
                    player.Init(target, binding.Timeline.RuntimeData);
                    player.NotifyRaised += OnPreviewNotifyRaised;
                }

                _records.Add(new BindingPreviewRecord(
                    i,
                    binding.ElementName,
                    binding.Timeline,
                    Mathf.Max(0, binding.StartOffsetMs),
                    player,
                    target != null));
            }
        }

        private void RefreshTimeline()
        {
            _timelineView?.Clear();
            _rowVisuals.Clear();

            if (_composition == null)
                return;

            int durationMs = Mathf.Max(1, DurationMs);
            AddTimeRuler(durationMs);
            AddBindingRows(durationMs);
            AddPlayhead();

            _suppressTimeChange = true;
            _timeSlider.highValue = durationMs;
            _timeSlider.value = Mathf.Clamp(CurrentTimeMs, 0, durationMs);
            _suppressTimeChange = false;
        }

        private void RefreshSummary()
        {
            if (_summaryLabel == null)
                return;

            if (_composition == null)
            {
                _summaryLabel.text = "No Composition";
                return;
            }

            _summaryLabel.text =
                $"{_composition.name}\n" +
                $"Bindings: {_composition.Bindings.Length}\n" +
                $"Duration: {_composition.DurationMs} ms\n" +
                $"Preview Document: {(_composition.PreviewDocument == null ? "-" : _composition.PreviewDocument.name)}";
        }

        private void AddTimeRuler(int durationMs)
        {
            VisualElement row = CreateTimelineRow("Time", out VisualElement lane);
            row.style.height = 32f;

            int[] ticks =
            {
                0,
                durationMs / 4,
                durationMs / 2,
                (durationMs * 3) / 4,
                durationMs
            };

            int previousTick = -1;
            for (int i = 0; i < ticks.Length; i++)
            {
                int tick = Mathf.Clamp(ticks[i], 0, durationMs);
                if (tick == previousTick)
                    continue;

                previousTick = tick;

                VisualElement line = new();
                line.style.position = Position.Absolute;
                line.style.left = new Length(ToPercent(tick, durationMs), LengthUnit.Percent);
                line.style.top = 3f;
                line.style.bottom = 3f;
                line.style.width = 1f;
                line.style.backgroundColor = new Color(0.34f, 0.34f, 0.34f);
                lane.Add(line);

                Label label = new($"{tick} ms");
                label.style.position = Position.Absolute;
                label.style.left = new Length(ToPercent(tick, durationMs), LengthUnit.Percent);
                label.style.top = 5f;
                label.style.width = 64f;
                label.style.fontSize = 10f;
                label.style.color = new Color(0.72f, 0.74f, 0.76f);
                lane.Add(label);
            }

            _timelineView.Add(row);
        }

        private void AddBindingRows(int durationMs)
        {
            for (int i = 0; i < _records.Count; i++)
            {
                BindingPreviewRecord record = _records[i];
                string rowLabel = string.IsNullOrWhiteSpace(record.ElementName)
                    ? $"Binding {record.Index}"
                    : record.ElementName;

                VisualElement row = CreateTimelineRow(rowLabel, out VisualElement lane);
                row.style.height = 42f;

                int timelineDurationMs = record.Timeline == null ? 0 : record.Timeline.DurationMs;
                int startMs = record.StartOffsetMs;
                int endMs = startMs + timelineDurationMs;
                string timelineName = record.Timeline == null ? "<Missing Timeline>" : record.Timeline.name;

                Label bar = new($"{timelineName}  +{startMs} ms  {startMs}-{endMs} ms");
                bar.tooltip =
                    $"Binding\nElement: {record.ElementName}\nTimeline: {timelineName}\n" +
                    $"Start Offset: {startMs} ms\nEnd: {endMs} ms\nDuration: {timelineDurationMs} ms";
                bar.style.position = Position.Absolute;
                bar.style.left = new Length(ToPercent(startMs, durationMs), LengthUnit.Percent);
                bar.style.width = new Length(ToPercent(Mathf.Max(1, timelineDurationMs), durationMs), LengthUnit.Percent);
                bar.style.top = 7f;
                bar.style.height = 26f;
                bar.style.paddingLeft = 8f;
                bar.style.paddingRight = 6f;
                bar.style.unityTextAlign = TextAnchor.MiddleLeft;
                bar.style.fontSize = 10f;
                bar.style.color = Color.white;
                bar.style.backgroundColor = new Color(0.35f, 0.50f, 0.80f);
                lane.Add(bar);

                AddOffsetMarker(lane, startMs, durationMs);

                _rowVisuals.Add(new BindingRowVisual(record.Index, bar, startMs, endMs));
                _timelineView.Add(row);
            }
        }

        private static void AddOffsetMarker(
            VisualElement lane,
            int timeMs,
            int durationMs)
        {
            VisualElement marker = new();
            marker.tooltip = $"Start Offset @ {timeMs} ms";
            marker.style.position = Position.Absolute;
            marker.style.left = new Length(ToPercent(timeMs, durationMs), LengthUnit.Percent);
            marker.style.top = 4f;
            marker.style.width = 3f;
            marker.style.height = 32f;
            marker.style.backgroundColor = new Color(0.36f, 0.95f, 0.58f);
            lane.Add(marker);
        }

        private void AddPlayhead()
        {
            VisualElement lane = new();
            lane.style.position = Position.Absolute;
            lane.style.left = TimelineLabelWidth;
            lane.style.right = 8f;
            lane.style.top = 0f;
            lane.style.bottom = 0f;
            lane.pickingMode = PickingMode.Ignore;

            _playhead = new VisualElement();
            _playhead.style.position = Position.Absolute;
            _playhead.style.top = 0f;
            _playhead.style.bottom = 0f;
            _playhead.style.width = 2f;
            _playhead.style.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
            _playhead.pickingMode = PickingMode.Ignore;
            lane.Add(_playhead);
            _timelineView.Add(lane);
            UpdatePlayhead();
        }

        private static VisualElement CreateTimelineRow(
            string label,
            out VisualElement lane)
        {
            VisualElement row = new();
            row.style.height = 28f;
            row.style.position = Position.Relative;
            row.style.borderBottomWidth = 1f;
            row.style.borderBottomColor = new Color(0.22f, 0.22f, 0.22f);

            Label rowLabel = new(label);
            rowLabel.style.position = Position.Absolute;
            rowLabel.style.left = 4f;
            rowLabel.style.top = 5f;
            rowLabel.style.width = TimelineLabelWidth - 8f;
            rowLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            rowLabel.style.fontSize = 11f;
            rowLabel.style.color = new Color(0.78f, 0.80f, 0.82f);
            row.Add(rowLabel);

            lane = new VisualElement();
            lane.style.position = Position.Absolute;
            lane.style.left = TimelineLabelWidth;
            lane.style.right = 8f;
            lane.style.top = 0f;
            lane.style.bottom = 0f;
            row.Add(lane);

            return row;
        }

        private void TogglePlay()
        {
            if (_composition == null)
                return;

            if (_isPlaying)
                Pause();
            else
                Play();

            UpdatePlayButton();
        }

        private void Play()
        {
            if (DurationMs <= 0)
                return;

            if (CurrentTimeMs >= DurationMs)
                SetCompositionTime(0);

            _playStartedAt = EditorApplication.timeSinceStartup;
            _timeAtPlayStartMs = CurrentTimeMs;
            _isPlaying = true;
            EditorApplication.update -= UpdatePlayback;
            EditorApplication.update += UpdatePlayback;
        }

        private void Pause()
        {
            if (!_isPlaying)
                return;

            _isPlaying = false;
            EditorApplication.update -= UpdatePlayback;
            UpdatePlayButton();
        }

        private void UpdatePlayback()
        {
            double elapsedSeconds = EditorApplication.timeSinceStartup - _playStartedAt;
            int nextTimeMs = _timeAtPlayStartMs + Mathf.Max(0, (int)(elapsedSeconds * 1000.0 * _playbackSpeed));
            SetCompositionTime(nextTimeMs);

            if (CurrentTimeMs >= DurationMs)
                Pause();
        }

        private void SetCompositionTime(int timeMs)
        {
            int durationMs = Mathf.Max(0, DurationMs);
            CurrentTimeMs = Mathf.Clamp(timeMs, 0, durationMs);

            for (int i = 0; i < _records.Count; i++)
            {
                BindingPreviewRecord record = _records[i];
                if (!record.CanPreview)
                    continue;

                int localTimeMs = CurrentTimeMs - record.StartOffsetMs;
                int timelineDurationMs = record.Timeline.DurationMs;
                if (localTimeMs < 0)
                    record.Player.SetIdle();
                else
                    record.Player.SetTime(Mathf.Clamp(localTimeMs, 0, timelineDurationMs));
            }

            _suppressTimeChange = true;
            if (_timeSlider != null)
            {
                _timeSlider.highValue = Mathf.Max(1, durationMs);
                _timeSlider.value = Mathf.Clamp(CurrentTimeMs, _timeSlider.lowValue, _timeSlider.highValue);
            }
            _suppressTimeChange = false;

            if (_timeLabel != null)
                _timeLabel.text = $"{CurrentTimeMs} ms";

            UpdatePlayhead();
            UpdateRows();
            UpdateDebug();
            RequestPreviewRepaint();
        }

        private void UpdateRows()
        {
            for (int i = 0; i < _rowVisuals.Count; i++)
            {
                BindingRowVisual row = _rowVisuals[i];
                bool isActive = CurrentTimeMs >= row.StartMs && CurrentTimeMs < row.EndMs;
                row.Bar.style.backgroundColor = isActive
                    ? new Color(0.95f, 0.62f, 0.22f)
                    : new Color(0.35f, 0.50f, 0.80f);
                row.Bar.style.borderTopWidth = isActive ? 2f : 0f;
                row.Bar.style.borderBottomWidth = isActive ? 2f : 0f;
                row.Bar.style.borderLeftWidth = isActive ? 2f : 0f;
                row.Bar.style.borderRightWidth = isActive ? 2f : 0f;
                row.Bar.style.borderTopColor = Color.white;
                row.Bar.style.borderBottomColor = Color.white;
                row.Bar.style.borderLeftColor = Color.white;
                row.Bar.style.borderRightColor = Color.white;
            }
        }

        private void UpdateDebug()
        {
            if (_debugLabel == null)
                return;

            if (_composition == null)
            {
                _debugLabel.text = "Current: -";
                return;
            }

            List<string> lines = new() { $"Current: {CurrentTimeMs} ms" };
            for (int i = 0; i < _records.Count; i++)
            {
                BindingPreviewRecord record = _records[i];
                int localTimeMs = CurrentTimeMs - record.StartOffsetMs;
                string state = localTimeMs < 0
                    ? "Idle"
                    : localTimeMs <= (record.Timeline == null ? 0 : record.Timeline.DurationMs)
                        ? $"Local {Mathf.Max(0, localTimeMs)} ms"
                        : "Complete";

                string resolution = record.CanPreview ? "OK" : "Missing";
                lines.Add($"{record.ElementName}: {state} ({resolution})");
            }

            _debugLabel.text = string.Join("\n", lines);
        }

        private void UpdatePlayhead()
        {
            if (_playhead == null || _composition == null)
                return;

            int durationMs = Mathf.Max(1, DurationMs);
            _playhead.style.left = new Length(ToPercent(CurrentTimeMs, durationMs), LengthUnit.Percent);
        }

        private void UpdatePlayButton()
        {
            if (_playButton == null)
                return;

            _playButton.text = _isPlaying ? "Pause" : "Play";
        }

        private void OnPreviewNotifyRaised(TimelineNotify notify)
        {
            RequestPreviewRepaint();
        }

        private void RequestPreviewRepaint()
        {
            _previewRepaintUntil = EditorApplication.timeSinceStartup + PreviewTransitionRepaintSeconds;

            if (_isPreviewRepaintPumpActive)
                return;

            _isPreviewRepaintPumpActive = true;
            EditorApplication.update += UpdatePreviewRepaintPump;
        }

        private void UpdatePreviewRepaintPump()
        {
            Repaint();

            if (EditorApplication.timeSinceStartup < _previewRepaintUntil)
                return;

            StopPreviewRepaintPump();
        }

        private void StopPreviewRepaintPump()
        {
            if (!_isPreviewRepaintPumpActive)
                return;

            _isPreviewRepaintPumpActive = false;
            EditorApplication.update -= UpdatePreviewRepaintPump;
        }

        private void DisposePlayers()
        {
            for (int i = 0; i < _records.Count; i++)
                _records[i].Player.Dispose();

            _records.Clear();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            switch (evt.keyCode)
            {
                case KeyCode.Space:
                    TogglePlay();
                    evt.StopPropagation();
                    break;

                case KeyCode.Home:
                    SetCompositionTime(0);
                    evt.StopPropagation();
                    break;

                case KeyCode.RightArrow:
                    SetCompositionTime(CurrentTimeMs + (evt.shiftKey ? LargeJumpMs : SmallJumpMs));
                    evt.StopPropagation();
                    break;

                case KeyCode.Period:
                    SetCompositionTime(CurrentTimeMs + (evt.shiftKey ? LargeJumpMs : SmallJumpMs));
                    evt.StopPropagation();
                    break;
            }
        }

        private static VisualElement FindNamedElement(
            VisualElement root,
            string elementName)
        {
            if (string.IsNullOrWhiteSpace(elementName))
                return null;

            VisualElement result = null;
            int count = 0;
            root.Query<VisualElement>().ForEach(element =>
            {
                if (element.name != elementName)
                    return;

                result = element;
                count++;
            });

            return count == 1 ? result : null;
        }

        private static float ToPercent(int valueMs, int durationMs)
        {
            return durationMs <= 0 ? 0f : Mathf.Clamp01(valueMs / (float)durationMs) * 100f;
        }

        private sealed class BindingPreviewRecord
        {
            internal BindingPreviewRecord(
                int index,
                string elementName,
                TimelineAsset timeline,
                int startOffsetMs,
                TimelinePreviewPlayer player,
                bool hasTarget)
            {
                Index = index;
                ElementName = string.IsNullOrWhiteSpace(elementName) ? $"Binding {index}" : elementName;
                Timeline = timeline;
                StartOffsetMs = startOffsetMs;
                Player = player;
                HasTarget = hasTarget;
            }

            internal int Index { get; }
            internal string ElementName { get; }
            internal TimelineAsset Timeline { get; }
            internal int StartOffsetMs { get; }
            internal TimelinePreviewPlayer Player { get; }
            internal bool HasTarget { get; }
            internal bool CanPreview => HasTarget && Timeline != null;
        }

        private readonly struct BindingRowVisual
        {
            internal BindingRowVisual(
                int index,
                VisualElement bar,
                int startMs,
                int endMs)
            {
                Index = index;
                Bar = bar;
                StartMs = startMs;
                EndMs = endMs;
            }

            internal int Index { get; }
            internal VisualElement Bar { get; }
            internal int StartMs { get; }
            internal int EndMs { get; }
        }
    }
}
