using UnityEngine.Localization;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class ResourceStatusBar : VisualElement
    {
        private const string RegionNameLabelName = "resource-status-bar-region-name";

        private Label _regionNameLabel;
        private string _regionName = string.Empty;

        public ResourceStatusBar()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        public void SetRegion(LocalizedString regionName)
        {
            SetRegion(GetLocalizedText(regionName));
        }

        public void SetRegion(string regionName)
        {
            _regionName = regionName ?? string.Empty;
            Refresh();
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _regionNameLabel = this.Q<Label>(RegionNameLabelName);
            Refresh();
        }

        private void Refresh()
        {
            if (_regionNameLabel != null)
                _regionNameLabel.text = _regionName;
        }

        private static string GetLocalizedText(LocalizedString localizedString)
        {
            return localizedString == null || localizedString.IsEmpty
                ? string.Empty
                : localizedString.GetLocalizedString();
        }
    }
}
