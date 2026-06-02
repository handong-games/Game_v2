using UnityEngine;
using UIToolkit.Timeline;

namespace UIToolkitTimelineSamples.BasicAttackPreview
{
    [CreateAssetMenu(
        fileName = "SampleTimelineNotifyDefinition",
        menuName = "UIToolkit Timeline Samples/Notify Definition")]
    public sealed class SampleTimelineNotifyDefinition : TimelineNotifyDefinition
    {
        [SerializeField]
        private string _label;

        public string Label => _label;
    }
}
