using System;
using Game.Core.Define;
using UnityEngine;

namespace Game.Core.Managers.Save
{
    [Serializable]
    public sealed class GraphicSettingsSave : SaveData
    {
        public const int CurrentVersion = 1;

        public bool Fullscreen = true;
        public int WindowedWidth = Screen.currentResolution.width;
        public int WindowedHeight = Screen.currentResolution.height;
        public int TargetDisplayIndex;
        public int WindowPositionX = -1;
        public int WindowPositionY = -1;
        public EDisplayAspect Aspect = EDisplayAspect.Auto;

        public GraphicSettingsSave() : base(CurrentVersion)
        {
        }
    }
}
