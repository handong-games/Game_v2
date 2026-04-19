using Game.Core.Managers.Audio;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Settings.View
{
    public sealed partial class SettingsView
    {
        private Toggle _muteInBackgroundToggle;
        private Slider _masterSlider;
        private Slider _bgmSlider;
        private Slider _sfxSlider;
        private Label _masterValueLabel;
        private Label _bgmValueLabel;
        private Label _sfxValueLabel;

        private void OnBindAudio()
        {
            _muteInBackgroundToggle = Bind<Toggle, bool>("mute-background-toggle", OnMuteInBackgroundChanged);
            _masterSlider = Bind<Slider, float>("master-slider", OnMasterVolumeChanged);
            _bgmSlider = Bind<Slider, float>("bgm-slider", OnBgmVolumeChanged);
            _sfxSlider = Bind<Slider, float>("sfx-slider", OnSfxVolumeChanged);
            _masterValueLabel = BindElement<Label>("master-value");
            _bgmValueLabel = BindElement<Label>("bgm-value");
            _sfxValueLabel = BindElement<Label>("sfx-value");
            
            _muteInBackgroundToggle.SetValueWithoutNotify(AudioManager.Instance.GetMuteInBackground());
            _masterSlider.SetValueWithoutNotify(AudioManager.Instance.GetVolume(EAudioVolume.Master));
            _bgmSlider.SetValueWithoutNotify(AudioManager.Instance.GetVolume(EAudioVolume.BGM));
            _sfxSlider.SetValueWithoutNotify(AudioManager.Instance.GetVolume(EAudioVolume.SFX));
            _masterValueLabel.text = ToVolumeText(_masterSlider.value);
            _bgmValueLabel.text = ToVolumeText(_bgmSlider.value);
            _sfxValueLabel.text = ToVolumeText(_sfxSlider.value);
        }

        private void OnUnbindAudio()
        {
            Unbind<Toggle, bool>(_muteInBackgroundToggle, OnMuteInBackgroundChanged);
            Unbind<Slider, float>(_masterSlider, OnMasterVolumeChanged);
            Unbind<Slider, float>(_bgmSlider, OnBgmVolumeChanged);
            Unbind<Slider, float>(_sfxSlider, OnSfxVolumeChanged);
        }

        private void OnMasterVolumeChanged(ChangeEvent<float> evt)
        {
            AudioManager.Instance.SetVolume(EAudioVolume.Master, evt.newValue);
            _masterValueLabel.text = ToVolumeText(evt.newValue);
        }

        private void OnBgmVolumeChanged(ChangeEvent<float> evt)
        {
            AudioManager.Instance.SetVolume(EAudioVolume.BGM, evt.newValue);
            _bgmValueLabel.text = ToVolumeText(evt.newValue);
        }

        private void OnSfxVolumeChanged(ChangeEvent<float> evt)
        {
            AudioManager.Instance.SetVolume(EAudioVolume.SFX, evt.newValue);
            _sfxValueLabel.text = ToVolumeText(evt.newValue);
        }

        private void OnMuteInBackgroundChanged(ChangeEvent<bool> evt)
        {
            AudioManager.Instance.SetMuteInBackground(evt.newValue);
        }

        private static string ToVolumeText(float value)
        {
            return Mathf.RoundToInt(value * 100f).ToString();
        }
    }
}
