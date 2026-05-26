using Game.AbilitySystem.Attributes;
using Game.Core.Managers.View;
using Gameplay.GAS;
using Unity.Mathematics;
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
        private VitalAttributeSet _vitalAttributeSet;
        private int _currentHealth = 34;
        private int _maxHealth = 50;
        private int? _previewHealth;
        private float _ratio = 0.68f;
        private bool _isShown;
        private bool _isElementReady;

        public HealthWidget()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }
        
        public void Bind(AbilitySystemComponent abilitySystem)
        {
            Unbind();

            if (abilitySystem == null)
                return;

            _vitalAttributeSet = abilitySystem.GetSet<VitalAttributeSet>();
            if (_vitalAttributeSet == null)
                return;
            
            ApplyHealthState(
                Mathf.RoundToInt(_vitalAttributeSet.Health.CurrentValue),
                Mathf.RoundToInt(_vitalAttributeSet.MaxHealth.CurrentValue));

            _vitalAttributeSet.Health.CurrentValueChanged += OnHealthValueChanged;
            _vitalAttributeSet.MaxHealth.CurrentValueChanged += OnMaxHealthValueChanged;
        }

        public void Unbind()
        {
            if (_vitalAttributeSet != null)
            {
                _vitalAttributeSet.Health.CurrentValueChanged -= OnHealthValueChanged;
                _vitalAttributeSet.MaxHealth.CurrentValueChanged -= OnMaxHealthValueChanged;
            }

            _vitalAttributeSet = null;
            Hide();
        }

        public void SetRatio(float ratio)
        {
            _ratio = Mathf.Clamp01(ratio);
            Refresh();
        }

        public void SetHealth(int currentHealth)
        {
            _currentHealth = Mathf.Max(0, currentHealth);
            _ratio = GetHealthRatio(_currentHealth, _maxHealth);
            RefreshState();
        }

        public void SetMaxHealth(int maxHealth)
        {
            _maxHealth = Mathf.Max(1, maxHealth);
            _ratio = GetHealthRatio(_currentHealth, _maxHealth);
            RefreshState();
        }

        public void SetPreviewHealth(int previewHealth)
        {
            _previewHealth = Mathf.Clamp(previewHealth, 0, _maxHealth);
            RefreshState();
        }

        public void HidePreview()
        {
            _previewHealth = null;
            if (!_isElementReady)
                return;

            _damagePreview.AddToClassList(UiHiddenClass);
            _damagePreviewText.AddToClassList(UiHiddenClass);
            _text.text = $"{_currentHealth} / {_maxHealth}";
        }

        private void RefreshPreview()
        {
            if (!_previewHealth.HasValue)
            {
                HidePreview();
                return;
            }

            int previewHealth = _previewHealth.Value;
            if (previewHealth >= _currentHealth)
            {
                HidePreview();
                return;
            }

            float currentRatio = GetHealthRatio(_currentHealth, _maxHealth);
            float previewRatio = GetHealthRatio(previewHealth, _maxHealth);
            float damageRatio = Mathf.Max(0f, currentRatio - previewRatio);
            int damageAmount = Mathf.Max(0, _currentHealth - previewHealth);

            _damagePreview.style.left = Length.Percent(previewRatio * 100f);
            _damagePreview.style.width = Length.Percent(damageRatio * 100f);
            _damagePreviewText.text = $"-{damageAmount}";
            _text.text = $"{previewHealth} / {_maxHealth}";
            _damagePreview.RemoveFromClassList(UiHiddenClass);
            _damagePreviewText.RemoveFromClassList(UiHiddenClass);
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
            _isShown = false;
            HidePreview();
            RemoveFromClassList(EnterClass);
            AddToClassList(HiddenClass);
            AddToClassList(FromBottomClass);
        }

        private void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            _fillClip = this.Q<VisualElement>(FillClipName);
            _text = this.Q<Label>(TextName);
            _damagePreview = this.Q<VisualElement>(DamagePreviewName);
            _damagePreviewText = this.Q<Label>(DamagePreviewTextName);
            _isElementReady = true;
        }

        private void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            _isElementReady = false;
            Unbind();
        }

        private void Refresh()
        {
            if (!_isElementReady)
                return;

            _fillClip.style.width = Length.Percent(_ratio * 100f);
            _text.text = $"{_currentHealth} / {_maxHealth}";
        }

        private void RefreshState()
        {
            Refresh();
            if (_previewHealth.HasValue)
            {
                RefreshPreview();
                return;
            }

            HidePreview();
        }

        private void ApplyHealthState(int currentHealth, int maxHealth)
        {
            _maxHealth = Mathf.Max(1, maxHealth);
            _currentHealth = Mathf.Max(0, currentHealth);
            _ratio = GetHealthRatio(_currentHealth, _maxHealth);
            _previewHealth = null;
            RefreshState();
        }

        private void OnHealthValueChanged(float oldValue, float newValue)
        {
            if (panel == null)
                return;

            SetHealth(Mathf.RoundToInt(newValue));
        }

        private void OnMaxHealthValueChanged(float oldValue, float newValue)
        {
            if (panel == null)
                return;

            SetMaxHealth(Mathf.RoundToInt(newValue));
        }

        private static float GetHealthRatio(int currentHealth, int maxHealth)
        {
            return Mathf.Clamp01((float)Mathf.Max(0, currentHealth) / Mathf.Max(1, maxHealth));
        }
    }
}
