using Game.Core.Define;
using UnityEngine;

namespace Game.Core.Managers.View
{
    public partial class ViewManager
    {
        private const float AutoNarrowThreshold = 1.3333334f;
        private const float AutoUltraWideThreshold = 2.3888888f;
        private EDisplayAspect _currentAspect = EDisplayAspect.Auto;

        private void OnViewAspectChanged(EDisplayAspect aspect)
        {
            _currentAspect = aspect;
            ApplyAspectFrame(Screen.width, Screen.height);
        }

        public void OnViewportSizeChanged(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            ApplyAspectFrame(width, height);
        }

        private void ApplyAspectFrame(int viewWidth, int viewHeight)
        {
            Vector2Int targetSize = ResolveTargetSize((float)viewWidth / viewHeight);

            float scale = Mathf.Min(
                viewWidth / (float)targetSize.x,
                viewHeight / (float)targetSize.y);

            float frameWidth = targetSize.x * scale;
            float frameHeight = targetSize.y * scale;
            float left = (viewWidth - frameWidth) * 0.5f;
            float top = (viewHeight - frameHeight) * 0.5f;

            _viewLayer.style.left = left;
            _viewLayer.style.top = top;
            _viewLayer.style.width = frameWidth;
            _viewLayer.style.height = frameHeight;
        }

        private Vector2Int ResolveTargetSize(float aspectRatio)
        {
            switch (_currentAspect)
            {
                case EDisplayAspect.Ratio4x3:
                    return new Vector2Int(1680, 1260);
                case EDisplayAspect.Ratio16x10:
                    return new Vector2Int(1920, 1200);
                case EDisplayAspect.Ratio16x9:
                    return new Vector2Int(1920, 1080);
                case EDisplayAspect.Ratio21x9:
                    return new Vector2Int(2580, 1080);
                case EDisplayAspect.Auto:
                    if (aspectRatio > AutoUltraWideThreshold)
                    {
                        return new Vector2Int(2580, 1080);
                    }

                    if (aspectRatio < AutoNarrowThreshold)
                    {
                        return new Vector2Int(1680, 1260);
                    }

                    return new Vector2Int(1680, 1080);
                default:
                    return new Vector2Int(1920, 1080);
            }
        }
    }
}
