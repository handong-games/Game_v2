using System;
using Game.Core.Managers.Garphic;
using UnityEngine;

namespace Domains.Settings
{
    [Serializable]
    public class GraphicSaveData
    {
        public bool fullscreen = true;
        public int windowedWidth = Screen.currentResolution.width;
        public int windowedHeight = Screen.currentResolution.height;
        public int targetDisplayIndex = 0;
        public int windowPositionX = -1;
        public int windowPositionY = -1;
        public EDisplayAspectPreset aspectPreset = EDisplayAspectPreset.Auto;
    }
    
    public sealed partial class SettingsSaveData
    {
        [SerializeField]
        private GraphicSaveData _graphicSaveData = new GraphicSaveData();
        public GraphicSaveData GraphicSaveData => _graphicSaveData;

        private void NormalizeDisplay()
        {
            if (_graphicSaveData.windowedWidth <= 0 || _graphicSaveData.windowedHeight <= 0)
            {
                _graphicSaveData.windowedWidth = Screen.currentResolution.width;
                _graphicSaveData.windowedHeight = Screen.currentResolution.height;
            }

            if (_graphicSaveData.targetDisplayIndex < 0)
                _graphicSaveData.targetDisplayIndex = 0;

            if (_graphicSaveData.windowPositionX < 0 || _graphicSaveData.windowPositionY < 0)
            {
                _graphicSaveData.windowPositionX = -1;
                _graphicSaveData.windowPositionY = -1;
            }
        }
    }
}
