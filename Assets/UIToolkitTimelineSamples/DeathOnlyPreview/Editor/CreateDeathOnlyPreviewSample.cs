using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UIToolkit.Timeline;

namespace UIToolkitTimelineSamples.DeathOnlyPreview.Editor
{
    public static class CreateDeathOnlyPreviewSample
    {
        private const string SampleRoot = "Assets/UIToolkitTimelineSamples/DeathOnlyPreview";
        private const string GeneratedRoot = SampleRoot + "/Generated";
        private const string PreviewTreePath = SampleRoot + "/CardDeathPreview.uxml";

        [MenuItem("UIToolkit Timeline/Samples/Create Death Only Preview Sample")]
        public static void Create()
        {
            EnsureFolder(GeneratedRoot);

            TimelineAsset softSink = CreateSoftSinkTimeline(
                GeneratedRoot + "/SoftSink_Timeline.asset");
            TimelineAsset snapSink = CreateSnapSinkTimeline(
                GeneratedRoot + "/SnapSink_Timeline.asset");
            TimelineAsset tiltSink = CreateTiltSinkTimeline(
                GeneratedRoot + "/TiltSink_Timeline.asset");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.objects = new Object[]
            {
                softSink,
                snapSink,
                tiltSink
            };

            if (!Application.isBatchMode)
                UIToolkit.Timeline.Editor.TimelineEditorWindow.Open(softSink);
        }

        private static TimelineAsset CreateSoftSinkTimeline(string path)
        {
            TimelineAsset timeline = CreateBaseTimeline(path, 2, 2, 2, 1);
            SerializedObject serializedObject = new(timeline);
            SerializedProperty tracks = serializedObject.FindProperty("_authoring")
                .FindPropertyRelative("_tracks");

            SetClip(
                tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                120,
                Vector2.zero,
                new Vector2(10f, 0f),
                TimelineEasing.EaseInOut);
            SetClip(
                tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                120,
                240,
                new Vector2(10f, 0f),
                new Vector2(28f, 0f),
                TimelineEasing.EaseOut);

            SetClip(
                tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                140,
                Vector2.one,
                new Vector2(0.98f, 0.98f),
                TimelineEasing.EaseInOut);
            SetClip(
                tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                140,
                220,
                new Vector2(0.98f, 0.98f),
                new Vector2(0.93f, 0.93f),
                TimelineEasing.EaseOut);

            SetClip(
                tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                140,
                Vector2.zero,
                new Vector2(-3f, 0f),
                TimelineEasing.EaseInOut);
            SetClip(
                tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                140,
                220,
                new Vector2(-3f, 0f),
                new Vector2(-5f, 0f),
                TimelineEasing.EaseOut);

            SetClip(
                tracks.GetArrayElementAtIndex(3).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                140,
                220,
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                TimelineEasing.EaseOut);

            ApplyTimeline(serializedObject, timeline);
            return timeline;
        }

        private static TimelineAsset CreateSnapSinkTimeline(string path)
        {
            TimelineAsset timeline = CreateBaseTimeline(path, 3, 3, 2, 1);
            SerializedObject serializedObject = new(timeline);
            SerializedProperty tracks = serializedObject.FindProperty("_authoring")
                .FindPropertyRelative("_tracks");

            SetClip(
                tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                40,
                Vector2.zero,
                new Vector2(-2f, 0f),
                TimelineEasing.EaseOut);
            SetClip(
                tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                40,
                100,
                new Vector2(-2f, 0f),
                new Vector2(22f, 0f),
                TimelineEasing.EaseIn);
            SetClip(
                tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(2),
                140,
                100,
                new Vector2(22f, 0f),
                new Vector2(44f, 0f),
                TimelineEasing.EaseOut);

            SetClip(
                tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                40,
                Vector2.one,
                new Vector2(1.02f, 1.02f),
                TimelineEasing.EaseOut);
            SetClip(
                tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                40,
                100,
                new Vector2(1.02f, 1.02f),
                new Vector2(0.96f, 0.96f),
                TimelineEasing.EaseIn);
            SetClip(
                tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(2),
                140,
                100,
                new Vector2(0.96f, 0.96f),
                new Vector2(0.86f, 0.86f),
                TimelineEasing.EaseOut);

            SetClip(
                tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                90,
                Vector2.zero,
                new Vector2(2f, 0f),
                TimelineEasing.EaseOut);
            SetClip(
                tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                90,
                150,
                new Vector2(2f, 0f),
                new Vector2(6f, 0f),
                TimelineEasing.EaseIn);

            SetClip(
                tracks.GetArrayElementAtIndex(3).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                90,
                150,
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                TimelineEasing.EaseOut);

            ApplyTimeline(serializedObject, timeline);
            return timeline;
        }

        private static TimelineAsset CreateTiltSinkTimeline(string path)
        {
            TimelineAsset timeline = CreateBaseTimeline(path, 2, 2, 2, 1);
            SerializedObject serializedObject = new(timeline);
            SerializedProperty tracks = serializedObject.FindProperty("_authoring")
                .FindPropertyRelative("_tracks");

            SetClip(
                tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                100,
                Vector2.zero,
                new Vector2(10f, 0f),
                TimelineEasing.EaseInOut);
            SetClip(
                tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                100,
                220,
                new Vector2(10f, 0f),
                new Vector2(30f, 0f),
                TimelineEasing.EaseOut);

            SetClip(
                tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                120,
                Vector2.one,
                new Vector2(0.99f, 0.99f),
                TimelineEasing.EaseInOut);
            SetClip(
                tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                120,
                200,
                new Vector2(0.99f, 0.99f),
                new Vector2(0.90f, 0.90f),
                TimelineEasing.EaseOut);

            SetClip(
                tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                0,
                120,
                Vector2.zero,
                new Vector2(-6f, 0f),
                TimelineEasing.EaseInOut);
            SetClip(
                tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(1),
                120,
                200,
                new Vector2(-6f, 0f),
                new Vector2(-11f, 0f),
                TimelineEasing.EaseOut);

            SetClip(
                tracks.GetArrayElementAtIndex(3).FindPropertyRelative("_clips").GetArrayElementAtIndex(0),
                150,
                170,
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                TimelineEasing.EaseOut);

            ApplyTimeline(serializedObject, timeline);
            return timeline;
        }

        private static TimelineAsset CreateBaseTimeline(
            string path,
            int translateClipCount,
            int scaleClipCount,
            int rotateClipCount,
            int opacityClipCount)
        {
            TimelineAsset timeline = LoadOrCreate<TimelineAsset>(path);
            VisualTreeAsset previewTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PreviewTreePath);

            SerializedObject serializedObject = new(timeline);
            SerializedProperty authoring = serializedObject.FindProperty("_authoring");
            authoring.FindPropertyRelative("_previewTree").objectReferenceValue = previewTree;
            authoring.FindPropertyRelative("_previewTargetName").stringValue = "death-card";

            SerializedProperty tracks = authoring.FindPropertyRelative("_tracks");
            tracks.arraySize = 4;
            SetTrack(tracks.GetArrayElementAtIndex(0), TimelineValueProperty.Translate, translateClipCount);
            SetTrack(tracks.GetArrayElementAtIndex(1), TimelineValueProperty.Scale, scaleClipCount);
            SetTrack(tracks.GetArrayElementAtIndex(2), TimelineValueProperty.Rotate, rotateClipCount);
            SetTrack(tracks.GetArrayElementAtIndex(3), TimelineValueProperty.Opacity, opacityClipCount);

            SerializedProperty notifies = authoring.FindPropertyRelative("_notifies");
            notifies.arraySize = 0;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            timeline.CompileRuntimeData();
            EditorUtility.SetDirty(timeline);
            return timeline;
        }

        private static void ApplyTimeline(
            SerializedObject serializedObject,
            TimelineAsset timeline)
        {
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            timeline.CompileRuntimeData();
            EditorUtility.SetDirty(timeline);
        }

        private static void SetTrack(
            SerializedProperty track,
            TimelineValueProperty property,
            int clipCount)
        {
            track.FindPropertyRelative("_property").enumValueIndex = (int)property;
            track.FindPropertyRelative("_clips").arraySize = clipCount;
        }

        private static void SetClip(
            SerializedProperty clip,
            int startMs,
            int durationMs,
            Vector2 from,
            Vector2 to,
            TimelineEasing easing)
        {
            clip.FindPropertyRelative("_startMs").intValue = startMs;
            clip.FindPropertyRelative("_durationMs").intValue = durationMs;
            clip.FindPropertyRelative("_from").vector2Value = from;
            clip.FindPropertyRelative("_to").vector2Value = to;
            clip.FindPropertyRelative("_easing").enumValueIndex = (int)easing;
        }

        private static T LoadOrCreate<T>(string path)
            where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
                return;

            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            string name = Path.GetFileName(folder);

            if (!string.IsNullOrEmpty(parent) &&
                !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolder(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
