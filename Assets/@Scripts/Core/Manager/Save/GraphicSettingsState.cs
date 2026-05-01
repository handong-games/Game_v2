using Game.Core.Define;
using Game.Core.Managers.Dependency;
using UnityEngine;

namespace Game.Core.Managers.Save
{
    [Dependency]
    public sealed class GraphicSettingsState : ISave<GraphicSettingsSave>
    {
        public bool Fullscreen { get; set; } = true;
        public int WindowedWidth { get; set; } = Screen.currentResolution.width;
        public int WindowedHeight { get; set; } = Screen.currentResolution.height;
        public int TargetDisplayIndex { get; set; }
        public int WindowPositionX { get; set; } = -1;
        public int WindowPositionY { get; set; } = -1;
        public EDisplayAspect Aspect { get; set; } = EDisplayAspect.Auto;

        public void LoadFrom(GraphicSettingsSave save)
        {
            Fullscreen = save.Fullscreen;
            WindowedWidth = save.WindowedWidth;
            WindowedHeight = save.WindowedHeight;
            TargetDisplayIndex = Mathf.Max(0, save.TargetDisplayIndex);
            WindowPositionX = save.WindowPositionX;
            WindowPositionY = save.WindowPositionY;
            Aspect = save.Aspect;

            if (WindowedWidth <= 0 || WindowedHeight <= 0)
            {
                WindowedWidth = Screen.currentResolution.width;
                WindowedHeight = Screen.currentResolution.height;
            }

            if (WindowPositionX < 0 || WindowPositionY < 0)
            {
                WindowPositionX = -1;
                WindowPositionY = -1;
            }
        }

        public GraphicSettingsSave ToSave()
        {
            return new GraphicSettingsSave
            {
                Fullscreen = Fullscreen,
                WindowedWidth = WindowedWidth,
                WindowedHeight = WindowedHeight,
                TargetDisplayIndex = TargetDisplayIndex,
                WindowPositionX = WindowPositionX,
                WindowPositionY = WindowPositionY,
                Aspect = Aspect
            };
        }
    }
}
