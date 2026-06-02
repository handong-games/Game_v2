using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UIToolkit.Timeline.Editor
{
    public sealed class TimelineAssetEditorWindow : EditorWindow
    {
        private const int SmallJumpMs = 10;
        private const int LargeJumpMs = 50;
        private const float TimelineLabelWidth = 108f;
        private const double PreviewTransitionRepaintSeconds = 0.5;

        [SerializeField]
        private TimelineAsset _timeline;

        private SerializedObject _serializedTimeline;
        private TimelinePreviewPlayer _previewPlayer;
        private VisualElement _previewStage;
        private VisualElement _timelineView;
        private VisualElement _playheadLane;
        private VisualElement _playhead;
        private Label _titleLabel;
        private Label _timeLabel;
        private Label _currentStateLabel;
        private Label _diagnosticsLabel;
        private Label _notifyLogLabel;
        private Label _selectionDetailLabel;
        private Label _timelineSummaryLabel;
        private SliderInt _timeSlider;
        private Button _playButton;
        private ToolbarMenu _speedMenu;
        private readonly List<TimelineClipVisual> _clipVisuals = new();
        private readonly List<TimelineNotifyVisual> _notifyVisuals = new();
        private string _selectionDetail = "Selection: -";
        private bool _suppressTimeChange;
        private bool _isPreviewRepaintPumpActive;
        private double _previewRepaintUntil;

        [MenuItem("Window/UIToolkit Timeline/Timeline Asset Editor")]
        public static void Open()
        {
            GetWindow<TimelineAssetEditorWindow>().titleContent = new GUIContent("Timeline Asset Editor");
        }

        public static void Open(TimelineAsset timeline)
        {
            TimelineAssetEditorWindow window = GetWindow<TimelineAssetEditorWindow>();
            window.titleContent = new GUIContent("Timeline Asset Editor");
            window.SetTimeline(timeline);
            window.Show();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
#pragma warning disable CS0618
            Object openedObject = EditorUtility.InstanceIDToObject(instanceID);
#pragma warning restore CS0618

            if (openedObject is not TimelineAsset timeline)
                return false;

            Open(timeline);
            return true;
        }

        public void CreateGUI()
        {
            _previewPlayer?.Dispose();
            _previewPlayer = new TimelinePreviewPlayer();
            _previewPlayer.TimeChanged += OnPreviewTimeChanged;
            _previewPlayer.NotifyRaised += OnPreviewNotifyRaised;

            VisualElement root = rootVisualElement;
            root.Clear();
            root.focusable = true;
            root.RegisterCallback<KeyDownEvent>(OnKeyDown);

            BuildToolbar(root);
            BuildMainLayout(root);
            BuildTimelinePanel(root);

            RefreshAll();
            root.Focus();
        }

        private void OnDisable()
        {
            _previewPlayer?.Dispose();
            StopPreviewRepaintPump();
        }

        private void SetTimeline(TimelineAsset timeline)
        {
            _timeline = timeline;
            _serializedTimeline = timeline == null ? null : new SerializedObject(timeline);

            if (rootVisualElement.childCount > 0)
                RefreshAll();
        }

        private void BuildToolbar(VisualElement root)
        {
            Toolbar toolbar = new();

            ObjectField assetField = new("Timeline")
            {
                objectType = typeof(TimelineAsset),
                value = _timeline,
                allowSceneObjects = false
            };
            assetField.RegisterValueChangedCallback(evt => SetTimeline(evt.newValue as TimelineAsset));
            toolbar.Add(assetField);

            _playButton = new Button(TogglePlay) { text = "Play" };
            toolbar.Add(_playButton);

            _speedMenu = new ToolbarMenu { text = "Speed 1x" };
            AddSpeedMenuItem(0.25f, "0.25x");
            AddSpeedMenuItem(0.5f, "0.5x");
            AddSpeedMenuItem(1f, "1x");
            AddSpeedMenuItem(2f, "2x");
            AddSpeedMenuItem(4f, "4x");
            toolbar.Add(_speedMenu);

            Button resetButton = new(() => SetPreviewTime(0)) { text = "Home" };
            toolbar.Add(resetButton);

            Button stepButton = new(() => SetPreviewTime(CurrentPreviewTimeMs + SmallJumpMs)) { text = $"+{SmallJumpMs}ms" };
            toolbar.Add(stepButton);

            Button largeStepButton = new(() => SetPreviewTime(CurrentPreviewTimeMs + LargeJumpMs)) { text = $"+{LargeJumpMs}ms" };
            toolbar.Add(largeStepButton);

            Button notifyButton = new(JumpToNextNotify) { text = "Next Notify" };
            toolbar.Add(notifyButton);

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
                _ => Mathf.Approximately(_previewPlayer?.PlaybackSpeed ?? 1f, speed)
                    ? DropdownMenuAction.Status.Checked
                    : DropdownMenuAction.Status.Normal);
        }

        private void SetPlaybackSpeed(
            float speed,
            string label)
        {
            _previewPlayer?.SetPlaybackSpeed(speed);

            if (_speedMenu != null)
                _speedMenu.text = $"Speed {label}";
        }

        private void BuildMainLayout(VisualElement root)
        {
            VisualElement main = new();
            main.style.flexDirection = FlexDirection.Row;
            main.style.flexGrow = 1f;
            main.style.minHeight = 260f;
            root.Add(main);

            VisualElement leftPanel = CreatePanel(260f);
            leftPanel.Add(new Label("Authoring") { style = { unityFontStyleAndWeight = FontStyle.Bold } });
            leftPanel.Add(new IMGUIContainer(DrawAuthoringInspector));
            main.Add(leftPanel);

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

            VisualElement rightPanel = CreatePanel(260f);
            _titleLabel = new Label("No Timeline") { style = { unityFontStyleAndWeight = FontStyle.Bold } };
            _currentStateLabel = new Label("Current: -");
            _diagnosticsLabel = new Label();
            _notifyLogLabel = new Label("Notify: -");
            _selectionDetailLabel = new Label(_selectionDetail);
            _currentStateLabel.style.whiteSpace = WhiteSpace.Normal;
            _diagnosticsLabel.style.whiteSpace = WhiteSpace.Normal;
            _notifyLogLabel.style.whiteSpace = WhiteSpace.Normal;
            _selectionDetailLabel.style.whiteSpace = WhiteSpace.Normal;
            rightPanel.Add(_titleLabel);
            rightPanel.Add(_currentStateLabel);
            rightPanel.Add(_diagnosticsLabel);
            rightPanel.Add(_notifyLogLabel);
            rightPanel.Add(_selectionDetailLabel);
            main.Add(rightPanel);
        }

        private void BuildTimelinePanel(VisualElement root)
        {
            VisualElement timelinePanel = new();
            timelinePanel.style.height = 190f;
            timelinePanel.style.borderTopWidth = 1f;
            timelinePanel.style.borderTopColor = new Color(0.24f, 0.24f, 0.24f);
            timelinePanel.style.paddingTop = 4f;
            root.Add(timelinePanel);

            _timeSlider = new SliderInt(0, 0);
            _timeSlider.RegisterValueChangedCallback(evt =>
            {
                if (!_suppressTimeChange)
                    SetPreviewTime(evt.newValue);
            });
            timelinePanel.Add(_timeSlider);

            _timelineSummaryLabel = new Label("Timeline: -");
            _timelineSummaryLabel.style.height = 18f;
            _timelineSummaryLabel.style.marginLeft = 6f;
            _timelineSummaryLabel.style.marginRight = 6f;
            _timelineSummaryLabel.style.whiteSpace = WhiteSpace.Normal;
            timelinePanel.Add(_timelineSummaryLabel);

            _timelineView = new VisualElement();
            _timelineView.style.flexGrow = 1f;
            _timelineView.style.position = Position.Relative;
            _timelineView.style.backgroundColor = new Color(0.12f, 0.12f, 0.12f);
            timelinePanel.Add(_timelineView);
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

        private void DrawAuthoringInspector()
        {
            if (_timeline == null)
            {
                EditorGUILayout.HelpBox("Select a Timeline Asset.", MessageType.Info);
                return;
            }

            _serializedTimeline ??= new SerializedObject(_timeline);
            _serializedTimeline.Update();

            EditorGUI.BeginChangeCheck();
            SerializedProperty authoring = _serializedTimeline.FindProperty("_authoring");
            if (authoring != null)
                EditorGUILayout.PropertyField(authoring, true);

            if (EditorGUI.EndChangeCheck())
            {
                _serializedTimeline.ApplyModifiedProperties();
                _timeline.CompileRuntimeData();
                EditorUtility.SetDirty(_timeline);
                RefreshAll();
            }
            else
            {
                _serializedTimeline.ApplyModifiedProperties();
            }
        }

        private void RefreshAll()
        {
            RefreshPreview();
            RefreshTimeline();
            RefreshDiagnostics();
            OnPreviewTimeChanged(0);
        }

        private void RefreshPreview()
        {
            _previewStage?.Clear();

            if (_timeline == null)
                return;

            TimelineAuthoringData authoring = _timeline.AuthoringData;
            if (authoring.PreviewTree == null)
                return;

            VisualElement previewRoot = authoring.PreviewTree.Instantiate();
            _previewStage.Add(previewRoot);

            VisualElement target = FindNamedElement(previewRoot, authoring.PreviewTargetName);
            if (target == null)
                return;

            _previewPlayer.Init(target, _timeline.RuntimeData);
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

        private void RefreshTimeline()
        {
            _timelineView?.Clear();
            _clipVisuals.Clear();
            _notifyVisuals.Clear();

            if (_timeline == null)
            {
                if (_timelineSummaryLabel != null)
                    _timelineSummaryLabel.text = "Timeline: -";

                return;
            }

            int durationMs = Mathf.Max(1, _timeline.DurationMs);

            UpdateTimelineSummary();
            AddTimeRuler(durationMs);
            AddNotifyLane(durationMs);
            AddTrackRows(durationMs);
            AddPlayhead();
            UpdateTimelineState();

            _suppressTimeChange = true;
            _timeSlider.highValue = durationMs;
            _timeSlider.value = Mathf.Clamp(CurrentPreviewTimeMs, 0, durationMs);
            _suppressTimeChange = false;
        }

        private void UpdateTimelineSummary()
        {
            if (_timelineSummaryLabel == null || _timeline == null)
                return;

            TimelineAuthoringData authoring = _timeline.AuthoringData;
            _timelineSummaryLabel.text =
                $"Timeline: {authoring.TrackCount} tracks, {CountClips(authoring)} clips, " +
                $"{authoring.NotifyCount} notifies, {_timeline.DurationMs} ms";
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

        private void AddNotifyLane(int durationMs)
        {
            VisualElement row = CreateTimelineRow("Notify", out VisualElement lane);
            row.style.height = 34f;

            TimelineNotifyPlacement[] notifies = _timeline.AuthoringData.Notifies;
            for (int i = 0; i < notifies.Length; i++)
            {
                TimelineNotifyPlacement notify = notifies[i];
                string notifyName = GetNotifyName(notify);
                string labelText = $"{notifyName} @ {notify.TimeMs} ms";

                Label marker = new(labelText);
                marker.tooltip = $"Notify\nName: {notifyName}\nTime: {notify.TimeMs} ms";
                marker.style.position = Position.Absolute;
                marker.style.left = new Length(ToPercent(notify.TimeMs, durationMs), LengthUnit.Percent);
                marker.style.top = 5f;
                marker.style.height = 22f;
                marker.style.minWidth = 84f;
                marker.style.paddingLeft = 5f;
                marker.style.paddingRight = 5f;
                marker.style.unityTextAlign = TextAnchor.MiddleLeft;
                marker.style.fontSize = 10f;
                marker.style.color = Color.black;
                marker.style.backgroundColor = new Color(0.95f, 0.76f, 0.24f);
                marker.RegisterCallback<ClickEvent>(_ => SelectTimelineItem(
                    $"Selection: Notify\nName: {notifyName}\nTime: {notify.TimeMs} ms"));
                lane.Add(marker);
                _notifyVisuals.Add(new TimelineNotifyVisual(marker, notify.TimeMs));
            }

            _timelineView.Add(row);
        }

        private void AddTrackRows(int durationMs)
        {
            TimelineTrackData[] tracks = _timeline.AuthoringData.Tracks;
            for (int trackIndex = 0; trackIndex < tracks.Length; trackIndex++)
            {
                TimelineValueClipData[] clips = tracks[trackIndex].Clips;
                string propertyName = tracks[trackIndex].Property.ToString();
                VisualElement row = CreateTimelineRow($"{propertyName} ({clips.Length})", out VisualElement lane);
                row.style.height = 42f;

                for (int clipIndex = 0; clipIndex < clips.Length; clipIndex++)
                {
                    TimelineValueClipData clip = clips[clipIndex];
                    string clipText = $"{propertyName}  {clip.StartMs}-{clip.EndMs} ms  ({clip.DurationMs} ms)";

                    Label bar = new(clipText);
                    bar.tooltip =
                        $"Value Clip\nProperty: {propertyName}\nStart: {clip.StartMs} ms\n" +
                        $"End: {clip.EndMs} ms\nDuration: {clip.DurationMs} ms\n" +
                        $"From: {clip.From}\nTo: {clip.To}\nEasing: {clip.Easing}";
                    bar.style.position = Position.Absolute;
                    bar.style.left = new Length(ToPercent(clip.StartMs, durationMs), LengthUnit.Percent);
                    bar.style.width = new Length(ToPercent(clip.DurationMs, durationMs), LengthUnit.Percent);
                    bar.style.top = 7f;
                    bar.style.height = 26f;
                    bar.style.paddingLeft = 8f;
                    bar.style.paddingRight = 6f;
                    bar.style.unityTextAlign = TextAnchor.MiddleLeft;
                    bar.style.fontSize = 10f;
                    bar.style.color = Color.white;
                    bar.style.backgroundColor = new Color(0.30f, 0.56f, 0.82f);
                    bar.RegisterCallback<ClickEvent>(_ => SelectTimelineItem(
                        $"Selection: Value Clip\nTrack: {trackIndex}\nClip: {clipIndex}\nProperty: {propertyName}\n" +
                        $"Start: {clip.StartMs} ms\nEnd: {clip.EndMs} ms\nDuration: {clip.DurationMs} ms\n" +
                        $"From: {clip.From}\nTo: {clip.To}\nEasing: {clip.Easing}"));
                    lane.Add(bar);

                    AddEventMarker(lane, clip.StartMs, durationMs, "Start", new Color(0.36f, 0.95f, 0.58f));
                    AddEventMarker(lane, clip.EndMs, durationMs, "End", new Color(1.0f, 0.42f, 0.36f));

                    _clipVisuals.Add(new TimelineClipVisual(
                        bar,
                        propertyName,
                        clip.StartMs,
                        clip.EndMs));
                }

                _timelineView.Add(row);
            }
        }

        private void AddEventMarker(
            VisualElement lane,
            int timeMs,
            int durationMs,
            string eventName,
            Color color)
        {
            VisualElement marker = new();
            marker.tooltip = $"{eventName} @ {timeMs} ms";
            marker.style.position = Position.Absolute;
            marker.style.left = new Length(ToPercent(timeMs, durationMs), LengthUnit.Percent);
            marker.style.top = 4f;
            marker.style.width = 3f;
            marker.style.height = 32f;
            marker.style.backgroundColor = color;
            marker.RegisterCallback<ClickEvent>(_ => SelectTimelineItem(
                $"Selection: Event\nType: {eventName}\nTime: {timeMs} ms"));
            lane.Add(marker);
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

        private void AddPlayhead()
        {
            _playheadLane = new VisualElement();
            _playheadLane.style.position = Position.Absolute;
            _playheadLane.style.left = TimelineLabelWidth;
            _playheadLane.style.right = 8f;
            _playheadLane.style.top = 0f;
            _playheadLane.style.bottom = 0f;
            _playheadLane.pickingMode = PickingMode.Ignore;

            _playhead = new VisualElement();
            _playhead.style.position = Position.Absolute;
            _playhead.style.top = 0f;
            _playhead.style.bottom = 0f;
            _playhead.style.width = 2f;
            _playhead.style.backgroundColor = new Color(0.95f, 0.95f, 0.95f);
            _playhead.pickingMode = PickingMode.Ignore;
            _playheadLane.Add(_playhead);
            _timelineView.Add(_playheadLane);
            UpdatePlayhead();
        }

        private void RefreshDiagnostics()
        {
            if (_timeline == null)
            {
                _titleLabel.text = "No Timeline";
                _diagnosticsLabel.text = string.Empty;
                return;
            }

            TimelineRuntimeData runtime = _timeline.RuntimeData;
            _titleLabel.text = _timeline.name;
            _diagnosticsLabel.text =
                $"Value Clips: {runtime.ValueClipCount}\n" +
                $"Notifies: {runtime.NotifyCount}\n" +
                $"Events: {runtime.EventCount}\n" +
                $"Duration: {runtime.DurationMs} ms\n" +
                BuildValidationText(_timeline.ValidationMessages);
        }

        private static string BuildValidationText(string[] messages)
        {
            if (messages == null || messages.Length == 0)
                return "Validation: OK";

            return "Validation:\n" + string.Join("\n", messages);
        }

        private void TogglePlay()
        {
            if (_previewPlayer == null)
                return;

            if (_previewPlayer.IsPlaying)
                _previewPlayer.Pause();
            else
                _previewPlayer.Play();

            UpdatePlayButton();
        }

        private void SetPreviewTime(int timeMs)
        {
            _previewPlayer?.SetTime(timeMs);
            UpdatePlayButton();
        }

        private void JumpToNextNotify()
        {
            if (_timeline == null)
                return;

            TimelineRuntimeData runtime = _timeline.RuntimeData;
            for (int i = 0; i < runtime.EventCount; i++)
            {
                TimelineEvent timelineEvent = runtime.GetEvent(i);
                if (timelineEvent.Op == TimelineOp.Notify &&
                    timelineEvent.TimeMs > CurrentPreviewTimeMs)
                {
                    SetPreviewTime(timelineEvent.TimeMs);
                    return;
                }
            }
        }

        private void OnPreviewTimeChanged(int timeMs)
        {
            _suppressTimeChange = true;
            if (_timeSlider != null)
                _timeSlider.value = Mathf.Clamp(timeMs, _timeSlider.lowValue, _timeSlider.highValue);
            _suppressTimeChange = false;

            if (_timeLabel != null)
                _timeLabel.text = $"{timeMs} ms";

            UpdatePlayhead();
            UpdatePlayButton();
            UpdateTimelineState();
            RequestPreviewRepaint();
        }

        private void OnPreviewNotifyRaised(TimelineNotify notify)
        {
            _notifyLogLabel.text = notify.Definition == null
                ? "Notify: -"
                : $"Notify: {notify.Definition.name}";
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

        private void UpdatePlayhead()
        {
            if (_playhead == null || _timeline == null)
                return;

            int durationMs = Mathf.Max(1, _timeline.DurationMs);
            _playhead.style.left = new Length(ToPercent(CurrentPreviewTimeMs, durationMs), LengthUnit.Percent);
        }

        private void UpdateTimelineState()
        {
            if (_timeline == null)
            {
                if (_currentStateLabel != null)
                    _currentStateLabel.text = "Current: -";

                return;
            }

            int currentTimeMs = CurrentPreviewTimeMs;
            List<string> activeValues = new();

            for (int i = 0; i < _clipVisuals.Count; i++)
            {
                TimelineClipVisual clip = _clipVisuals[i];
                bool isActive = currentTimeMs >= clip.StartMs && currentTimeMs < clip.EndMs;
                clip.Bar.style.backgroundColor = isActive
                    ? new Color(0.95f, 0.62f, 0.22f)
                    : new Color(0.30f, 0.56f, 0.82f);
                clip.Bar.style.borderTopWidth = isActive ? 2f : 0f;
                clip.Bar.style.borderBottomWidth = isActive ? 2f : 0f;
                clip.Bar.style.borderLeftWidth = isActive ? 2f : 0f;
                clip.Bar.style.borderRightWidth = isActive ? 2f : 0f;
                clip.Bar.style.borderTopColor = Color.white;
                clip.Bar.style.borderBottomColor = Color.white;
                clip.Bar.style.borderLeftColor = Color.white;
                clip.Bar.style.borderRightColor = Color.white;

                if (isActive)
                    activeValues.Add(clip.ClassName);
            }

            for (int i = 0; i < _notifyVisuals.Count; i++)
            {
                TimelineNotifyVisual notify = _notifyVisuals[i];
                bool hasPassed = currentTimeMs >= notify.TimeMs;
                notify.Marker.style.opacity = hasPassed ? 1f : 0.62f;
            }

            if (_currentStateLabel == null)
                return;

            string activeText = activeValues.Count == 0
                ? "-"
                : string.Join(", ", activeValues);
            _currentStateLabel.text =
                $"Current: {currentTimeMs} ms\n" +
                $"Active Values: {activeText}\n" +
                $"Due Events: {BuildDueEventText(currentTimeMs)}";
        }

        private string BuildDueEventText(int currentTimeMs)
        {
            if (_timeline == null)
                return "-";

            TimelineRuntimeData runtime = _timeline.RuntimeData;
            List<string> dueEvents = new();
            for (int i = 0; i < runtime.EventCount; i++)
            {
                TimelineEvent timelineEvent = runtime.GetEvent(i);
                if (timelineEvent.TimeMs > currentTimeMs)
                    break;

                dueEvents.Add(FormatRuntimeEvent(runtime, timelineEvent));
            }

            return dueEvents.Count == 0 ? "-" : string.Join(", ", dueEvents);
        }

        private static string FormatRuntimeEvent(
            TimelineRuntimeData runtime,
            TimelineEvent timelineEvent)
        {
            return timelineEvent.Op switch
            {
                TimelineOp.Notify => $"!{GetNotifyDefinitionName(runtime.GetNotifyDefinition(timelineEvent.Operand))} @{timelineEvent.TimeMs}",
                _ => $"{timelineEvent.Op} @{timelineEvent.TimeMs}"
            };
        }

        private void SelectTimelineItem(string detail)
        {
            _selectionDetail = detail;

            if (_selectionDetailLabel != null)
                _selectionDetailLabel.text = _selectionDetail;

            RequestPreviewRepaint();
        }

        private void UpdatePlayButton()
        {
            if (_playButton == null || _previewPlayer == null)
                return;

            _playButton.text = _previewPlayer.IsPlaying ? "Pause" : "Play";
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
                    SetPreviewTime(0);
                    evt.StopPropagation();
                    break;

                case KeyCode.RightArrow:
                    SetPreviewTime(CurrentPreviewTimeMs + (evt.shiftKey ? LargeJumpMs : SmallJumpMs));
                    evt.StopPropagation();
                    break;

                case KeyCode.Period:
                    SetPreviewTime(CurrentPreviewTimeMs + (evt.shiftKey ? LargeJumpMs : SmallJumpMs));
                    evt.StopPropagation();
                    break;

                case KeyCode.N:
                    JumpToNextNotify();
                    evt.StopPropagation();
                    break;
            }
        }

        private static float ToPercent(int valueMs, int durationMs)
        {
            return durationMs <= 0 ? 0f : Mathf.Clamp01(valueMs / (float)durationMs) * 100f;
        }

        private static int CountClips(TimelineAuthoringData authoring)
        {
            int count = 0;
            TimelineTrackData[] tracks = authoring.Tracks;
            for (int i = 0; i < tracks.Length; i++)
                count += tracks[i].ClipCount;

            return count;
        }

        private static string GetNotifyName(TimelineNotifyPlacement notify)
        {
            return GetNotifyDefinitionName(notify.Definition);
        }

        private static string GetNotifyDefinitionName(TimelineNotifyDefinition definition)
        {
            return definition == null ? "<Missing Notify>" : definition.name;
        }

        private int CurrentPreviewTimeMs => _previewPlayer?.CurrentTimeMs ?? 0;

        private readonly struct TimelineClipVisual
        {
            internal TimelineClipVisual(
                VisualElement bar,
                string className,
                int startMs,
                int endMs)
            {
                Bar = bar;
                ClassName = className;
                StartMs = startMs;
                EndMs = endMs;
            }

            internal VisualElement Bar { get; }
            internal string ClassName { get; }
            internal int StartMs { get; }
            internal int EndMs { get; }
        }

        private readonly struct TimelineNotifyVisual
        {
            internal TimelineNotifyVisual(
                VisualElement marker,
                int timeMs)
            {
                Marker = marker;
                TimeMs = timeMs;
            }

            internal VisualElement Marker { get; }
            internal int TimeMs { get; }
        }
    }

    public static class TimelineEditorWindow
    {
        public static void Open()
        {
            TimelineAssetEditorWindow.Open();
        }

        public static void Open(TimelineAsset timeline)
        {
            TimelineAssetEditorWindow.Open(timeline);
        }
    }
}
