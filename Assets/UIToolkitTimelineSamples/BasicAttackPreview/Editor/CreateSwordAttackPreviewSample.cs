using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UIToolkit.Timeline;

namespace UIToolkitTimelineSamples.BasicAttackPreview.Editor
{
    public static class CreateSwordAttackPreviewSample
    {
        private const string SampleRoot = "Assets/UIToolkitTimelineSamples/BasicAttackPreview";
        private const string GeneratedRoot = SampleRoot + "/Generated";
        private const string PreviewTreePath = SampleRoot + "/SwordCardWidget.uxml";
        private const string CompositionPreviewTreePath = SampleRoot + "/SwordDualCardWidget.uxml";

        [MenuItem("UIToolkit Timeline/Samples/Create Warrior Sword Attack Preview Sample")]
        public static void Create()
        {
            EnsureFolder(GeneratedRoot);

            SampleTimelineNotifyDefinition impact = CreateNotifyDefinition(
                GeneratedRoot + "/Notify_SwordImpact.asset",
                "sword-impact");

            TimelineAsset attackTimeline = CreateSwordAttackTimeline(
                GeneratedRoot + "/SwordAttack_Timeline.asset",
                impact);

            TimelineAsset hitTimeline = CreateSwordHitReactionTimeline(
                GeneratedRoot + "/SwordHitReaction_Timeline.asset");

            UIToolkit.Timeline.Editor.TimelineComposition composition = CreateComposition(
                GeneratedRoot + "/SwordAttack_Composition.asset",
                attackTimeline,
                hitTimeline);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = composition;

            if (!Application.isBatchMode)
                UIToolkit.Timeline.Editor.TimelineCompositionEditorWindow.Open(composition);
        }

        private static SampleTimelineNotifyDefinition CreateNotifyDefinition(
            string path,
            string label)
        {
            SampleTimelineNotifyDefinition definition = LoadOrCreate<SampleTimelineNotifyDefinition>(path);
            SerializedObject serializedObject = new(definition);
            serializedObject.FindProperty("_label").stringValue = label;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static TimelineAsset CreateSwordAttackTimeline(
            string path,
            TimelineNotifyDefinition impact)
        {
            TimelineAsset timeline = LoadOrCreate<TimelineAsset>(path);
            VisualTreeAsset previewTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PreviewTreePath);

            SerializedObject serializedObject = new(timeline);
            SerializedProperty authoring = serializedObject.FindProperty("_authoring");

            authoring.FindPropertyRelative("_previewTree").objectReferenceValue = previewTree;
            authoring.FindPropertyRelative("_previewTargetName").stringValue = "attack-card";

            SerializedProperty tracks = authoring.FindPropertyRelative("_tracks");
            tracks.arraySize = 3;
            SetTrack(tracks.GetArrayElementAtIndex(0), TimelineValueProperty.Translate, 4);
            SetTrack(tracks.GetArrayElementAtIndex(1), TimelineValueProperty.Scale, 4);
            SetTrack(tracks.GetArrayElementAtIndex(2), TimelineValueProperty.Rotate, 4);

            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(0), 0, 100, Vector2.zero, new Vector2(-12f, -2f), TimelineEasing.EaseOut);
            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(1), 100, 38, new Vector2(-12f, -2f), new Vector2(46f, -10f), TimelineEasing.EaseIn);
            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(2), 138, 30, new Vector2(46f, -10f), new Vector2(46f, -10f), TimelineEasing.Linear);
            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(3), 168, 132, new Vector2(46f, -10f), Vector2.zero, TimelineEasing.EaseOut);

            SetClip(tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(0), 0, 100, Vector2.one, new Vector2(0.97f, 1.03f), TimelineEasing.EaseOut);
            SetClip(tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(1), 100, 38, new Vector2(0.97f, 1.03f), new Vector2(1.08f, 0.93f), TimelineEasing.EaseIn);
            SetClip(tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(2), 138, 30, new Vector2(1.08f, 0.93f), new Vector2(1.08f, 0.93f), TimelineEasing.Linear);
            SetClip(tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(3), 168, 132, new Vector2(1.08f, 0.93f), Vector2.one, TimelineEasing.EaseOut);

            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(0), 0, 100, Vector2.zero, new Vector2(-5f, 0f), TimelineEasing.EaseOut);
            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(1), 100, 38, new Vector2(-5f, 0f), new Vector2(10f, 0f), TimelineEasing.EaseIn);
            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(2), 138, 30, new Vector2(10f, 0f), new Vector2(10f, 0f), TimelineEasing.Linear);
            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(3), 168, 132, new Vector2(10f, 0f), Vector2.zero, TimelineEasing.EaseOut);

            SerializedProperty notifies = authoring.FindPropertyRelative("_notifies");
            notifies.arraySize = 1;
            SetNotify(notifies.GetArrayElementAtIndex(0), impact, 138);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            timeline.CompileRuntimeData();
            EditorUtility.SetDirty(timeline);

            return timeline;
        }

        private static TimelineAsset CreateSwordHitReactionTimeline(string path)
        {
            TimelineAsset timeline = LoadOrCreate<TimelineAsset>(path);
            VisualTreeAsset previewTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PreviewTreePath);

            SerializedObject serializedObject = new(timeline);
            SerializedProperty authoring = serializedObject.FindProperty("_authoring");

            authoring.FindPropertyRelative("_previewTree").objectReferenceValue = previewTree;
            authoring.FindPropertyRelative("_previewTargetName").stringValue = "attack-card";

            SerializedProperty tracks = authoring.FindPropertyRelative("_tracks");
            tracks.arraySize = 3;
            SetTrack(tracks.GetArrayElementAtIndex(0), TimelineValueProperty.Translate, 5);
            SetTrack(tracks.GetArrayElementAtIndex(1), TimelineValueProperty.Scale, 3);
            SetTrack(tracks.GetArrayElementAtIndex(2), TimelineValueProperty.Rotate, 5);

            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(0), 0, 28, Vector2.zero, new Vector2(30f, -4f), TimelineEasing.EaseOut);
            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(1), 28, 26, new Vector2(30f, -4f), new Vector2(18f, 3f), TimelineEasing.Linear);
            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(2), 54, 32, new Vector2(18f, 3f), new Vector2(24f, -2f), TimelineEasing.Linear);
            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(3), 86, 44, new Vector2(24f, -2f), new Vector2(10f, 1f), TimelineEasing.EaseOut);
            SetClip(tracks.GetArrayElementAtIndex(0).FindPropertyRelative("_clips").GetArrayElementAtIndex(4), 130, 110, new Vector2(10f, 1f), Vector2.zero, TimelineEasing.EaseOut);

            SetClip(tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(0), 0, 28, Vector2.one, new Vector2(0.94f, 1.06f), TimelineEasing.EaseOut);
            SetClip(tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(1), 28, 52, new Vector2(0.94f, 1.06f), new Vector2(1.04f, 0.96f), TimelineEasing.EaseInOut);
            SetClip(tracks.GetArrayElementAtIndex(1).FindPropertyRelative("_clips").GetArrayElementAtIndex(2), 80, 160, new Vector2(1.04f, 0.96f), Vector2.one, TimelineEasing.EaseOut);

            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(0), 0, 28, Vector2.zero, new Vector2(5f, 0f), TimelineEasing.EaseOut);
            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(1), 28, 26, new Vector2(5f, 0f), new Vector2(-3f, 0f), TimelineEasing.Linear);
            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(2), 54, 32, new Vector2(-3f, 0f), new Vector2(2f, 0f), TimelineEasing.Linear);
            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(3), 86, 44, new Vector2(2f, 0f), new Vector2(-1f, 0f), TimelineEasing.EaseOut);
            SetClip(tracks.GetArrayElementAtIndex(2).FindPropertyRelative("_clips").GetArrayElementAtIndex(4), 130, 110, new Vector2(-1f, 0f), Vector2.zero, TimelineEasing.EaseOut);

            SerializedProperty notifies = authoring.FindPropertyRelative("_notifies");
            notifies.arraySize = 0;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            timeline.CompileRuntimeData();
            EditorUtility.SetDirty(timeline);

            return timeline;
        }

        private static UIToolkit.Timeline.Editor.TimelineComposition CreateComposition(
            string path,
            TimelineAsset attackTimeline,
            TimelineAsset hitTimeline)
        {
            UIToolkit.Timeline.Editor.TimelineComposition composition =
                LoadOrCreate<UIToolkit.Timeline.Editor.TimelineComposition>(path);
            VisualTreeAsset previewTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CompositionPreviewTreePath);

            SerializedObject serializedObject = new(composition);
            serializedObject.FindProperty("_previewDocument").objectReferenceValue = previewTree;

            SerializedProperty bindings = serializedObject.FindProperty("_bindings");
            bindings.arraySize = 2;
            SetCompositionBinding(bindings.GetArrayElementAtIndex(0), "attacker-card", attackTimeline, 0);
            SetCompositionBinding(bindings.GetArrayElementAtIndex(1), "target-card", hitTimeline, 138);

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(composition);

            return composition;
        }

        private static void SetTrack(
            SerializedProperty track,
            TimelineValueProperty property,
            int clipCount)
        {
            track.FindPropertyRelative("_property").enumValueIndex = (int)property;

            SerializedProperty clips = track.FindPropertyRelative("_clips");
            clips.arraySize = clipCount;
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

        private static void SetNotify(
            SerializedProperty notify,
            TimelineNotifyDefinition definition,
            int timeMs)
        {
            notify.FindPropertyRelative("_definition").objectReferenceValue = definition;
            notify.FindPropertyRelative("_timeMs").intValue = timeMs;
        }

        private static void SetCompositionBinding(
            SerializedProperty binding,
            string elementName,
            TimelineAsset timeline,
            int startOffsetMs)
        {
            binding.FindPropertyRelative("_elementName").stringValue = elementName;
            binding.FindPropertyRelative("_timeline").objectReferenceValue = timeline;
            binding.FindPropertyRelative("_startOffsetMs").intValue = startOffsetMs;
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
