using Domains.Combat;
using Game.Core.Managers.View;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class HealthWidget : VisualElement
    {
        private const string FillClipName = "health-widget-fill-clip";
        private const string TextName = "health-widget-text";
        private const string DamagePreviewName = "health-widget-damage-preview";
        private const string DamagePreviewTextName = "health-widget-damage-preview-text";
        private const string HiddenClass = "ui-transition--hidden";
        private const string UiHiddenClass = "ui-hidden";
        private const string FromBottomClass = "ui-transition--from-bottom";
        private const string EnterClass = "ui-transition--enter";

        private VisualElement _fillClip;
        private VisualElement _damagePreview;
        private Label _text;
        private Label _damagePreviewText;
        private int _currentHealth = 34;
        private int _maxHealth = 50;
        private float _ratio = 0.68f;
        private bool _isShown;

        public HealthWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
        }

        public void SetRatio(float ratio)
        {
            ResolveElements();
            _ratio = Mathf.Clamp01(ratio);
            Refresh();
        }

        public void SetHealth(int currentHealth, int maxHealth)
        {
            ResolveElements();
            _currentHealth = Mathf.Max(0, currentHealth);
            _maxHealth = Mathf.Max(1, maxHealth);
            _ratio = GetHealthRatio(_currentHealth, _maxHealth);
            HidePreview();
            Refresh();
        }

        public void ShowPreview(HealthPreviewDto preview)
        {
            ResolveElements();
            float currentRatio = GetHealthRatio(preview.CurrentHealth, _maxHealth);
            float previewRatio = GetHealthRatio(preview.PreviewHealth, _maxHealth);
            float damageRatio = Mathf.Max(0f, currentRatio - previewRatio);

            _damagePreview.style.left = Length.Percent(previewRatio * 100f);
            _damagePreview.style.width = Length.Percent(damageRatio * 100f);
            _damagePreviewText.text = $"-{preview.Amount}";
            _text.text = $"{preview.PreviewHealth} / {_maxHealth}";
            _damagePreview.RemoveFromClassList(UiHiddenClass);
            _damagePreviewText.RemoveFromClassList(UiHiddenClass);
        }

        public void HidePreview()
        {
            ResolveElements();
            _damagePreview.AddToClassList(UiHiddenClass);
            _damagePreviewText.AddToClassList(UiHiddenClass);
            _text.text = $"{_currentHealth} / {_maxHealth}";
        }

        public async Awaitable Show()
        {
            if (_isShown)
                return;

            _isShown = true;

            await ViewTransitionManager.Instance.Play(this, EnterClass);
        }

        public void Hide()
        {
            ResolveElements();
            _isShown = false;
            HidePreview();
            RemoveFromClassList(EnterClass);
            AddToClassList(HiddenClass);
            AddToClassList(FromBottomClass);
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            ResolveElements();
            Refresh();
        }

        private void Refresh()
        {
            ResolveElements();
            _fillClip.style.width = Length.Percent(_ratio * 100f);
            _text.text = $"{_currentHealth} / {_maxHealth}";
        }

        private void ResolveElements()
        {
            _fillClip ??= this.Q<VisualElement>(FillClipName);
            _text ??= this.Q<Label>(TextName);
            _damagePreview ??= this.Q<VisualElement>(DamagePreviewName);
            _damagePreviewText ??= this.Q<Label>(DamagePreviewTextName);
        }

        private static float GetHealthRatio(int currentHealth, int maxHealth)
        {
            return Mathf.Clamp01((float)Mathf.Max(0, currentHealth) / Mathf.Max(1, maxHealth));
        }
    }
}
