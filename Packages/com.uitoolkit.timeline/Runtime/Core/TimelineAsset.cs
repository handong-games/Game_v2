using UnityEngine;

namespace UIToolkit.Timeline
{
    [CreateAssetMenu(
        fileName = "TimelineAsset",
        menuName = "UIToolkit Timeline/Timeline Asset")]
    public sealed partial class TimelineAsset : ScriptableObject
    {
        [SerializeField]
        private TimelineRuntimeData _runtime = TimelineRuntimeData.Empty;

        public TimelineRuntimeData RuntimeData => _runtime;
        public bool HasRuntimeData => !_runtime.IsEmpty;
        public int DurationMs => _runtime.DurationMs;
    }
}
