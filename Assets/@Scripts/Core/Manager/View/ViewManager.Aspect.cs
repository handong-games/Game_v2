using Game.Core.Define;
using UnityEngine;

namespace Game.Core.Managers.View
{
    public partial class ViewManager
    {
        private const float AutoNarrowThreshold = 1.3333334f;
        private const float AutoUltraWideThreshold = 2.3888888f;
        private const string NarrowBucketClass = "app-view-layer--narrow";
        private const string StandardBucketClass = "app-view-layer--standard";
        private const string UltraWideBucketClass = "app-view-layer--ultrawide";
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
            float aspectRatio = (float)viewWidth / viewHeight;
            Vector2Int targetSize = ResolveTargetSize(aspectRatio);
            float frameWidth;
            float frameHeight;
            float left;
            float top;

            if (_currentAspect == EDisplayAspect.Auto)
            {
                if (aspectRatio > AutoUltraWideThreshold)
                {
                    // STS2 KeepWidth: logical width is fixed, physical content keeps full height.
                    frameHeight = viewHeight;
                    frameWidth = viewHeight * (targetSize.x / (float)targetSize.y);
                    left = (viewWidth - frameWidth) * 0.5f;
                    top = 0f;
                }
                else if (aspectRatio < AutoNarrowThreshold)
                {
                    // STS2 KeepHeight: logical height is fixed, physical content keeps full width.
                    frameWidth = viewWidth;
                    frameHeight = viewWidth * (targetSize.y / (float)targetSize.x);
                    left = 0f;
                    top = (viewHeight - frameHeight) * 0.5f;
                }
                else
                {
                    // STS2 Expand: content fills the whole window.
                    frameWidth = viewWidth;
                    frameHeight = viewHeight;
                    left = 0f;
                    top = 0f;
                }
            }
            else
            {
                float fixedFrameScale = Mathf.Min(
                    viewWidth / (float)targetSize.x,
                    viewHeight / (float)targetSize.y);

                frameWidth = targetSize.x * fixedFrameScale;
                frameHeight = targetSize.y * fixedFrameScale;
                left = (viewWidth - frameWidth) * 0.5f;
                top = (viewHeight - frameHeight) * 0.5f;
            }

            _viewLayer.style.left = left;
            _viewLayer.style.top = top;
            _viewLayer.style.width = frameWidth;
            _viewLayer.style.height = frameHeight;

            ApplyAspectBucketClass(aspectRatio);
            float responsiveScale = Mathf.Min(frameWidth / targetSize.x, frameHeight / targetSize.y);
            ApplyResponsiveLayoutToViews(responsiveScale, frameWidth, frameHeight, targetSize);
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

        private void ApplyAspectBucketClass(float aspectRatio)
        {
            _viewLayer.EnableInClassList(NarrowBucketClass, false);
            _viewLayer.EnableInClassList(StandardBucketClass, false);
            _viewLayer.EnableInClassList(UltraWideBucketClass, false);

            if (_currentAspect == EDisplayAspect.Auto)
            {
                if (aspectRatio > AutoUltraWideThreshold)
                {
                    _viewLayer.AddToClassList(UltraWideBucketClass);
                    return;
                }

                if (aspectRatio < AutoNarrowThreshold)
                {
                    _viewLayer.AddToClassList(NarrowBucketClass);
                    return;
                }
            }

            _viewLayer.AddToClassList(StandardBucketClass);
        }

        private void ApplyResponsiveLayoutToViews(float frameScale, float frameWidth, float frameHeight, Vector2Int targetSize)
        {
            foreach (BaseView view in _views)
            {
                view?.ApplyResponsiveLayout(frameScale, frameWidth, frameHeight, targetSize);
            }
        }
    }
}
