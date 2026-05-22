using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Widgets
{
    [UxmlElement]
    public sealed partial class ArrowWidget : VisualElement
    {
        private const string HeadResourcePath = "UI/Arrow/SketchArrowHead";
        private const string BodyResourcePath = "UI/Arrow/SketchArrowBody";
        private const int MaxBodySegments = 18;
        private const int CurveSampleCount = 96;
        private const float BodyContentHeightNormalized = 571f / 573f;
        private const float HeadTipOffsetNormalized = 0.32794118f;
        private const float SegmentSpacingStartFactor = 0.82f;
        private const float SegmentSpacingEndFactor = 0.50f;
        private const float HeadExtraBackOverlapNormalized = 0.05f;

        private static bool _resourcesLoaded;
        private static Texture2D _headTexture;
        private static Texture2D _bodyTexture;

        private readonly List<VisualElement> _bodySegments = new();
        private readonly Vector2[] _curvePoints = new Vector2[CurveSampleCount + 1];
        private readonly float[] _curveLengths = new float[CurveSampleCount + 1];
        private readonly float[] _rawCenters = new float[MaxBodySegments];
        private readonly float[] _bodyWidths = new float[MaxBodySegments];
        private readonly float[] _bodyHeights = new float[MaxBodySegments];
        private readonly float[] _visibleLengths = new float[MaxBodySegments];
        private readonly bool[] _bodySegmentVisible = new bool[MaxBodySegments];
        private readonly VisualElement _head;

        private Vector2 _origin;
        private bool _isShown;
        private bool _headVisible;

        private readonly int _segmentCount = 8;
        private readonly float _curveHeight = 0.35f;
        private readonly float _minCurveHeight = 60f;
        private readonly float _bodyWidthStart = 24f;
        private readonly float _bodyWidthEnd = 66f;
        private readonly float _bodyAspect = 1.18f;
        private readonly float _headWidth = 126f;
        private readonly float _headHeight = 126f;

        public ArrowWidget()
        {
            pickingMode = PickingMode.Ignore;
            usageHints = UsageHints.GroupTransform;

            style.position = Position.Absolute;
            style.left = 0f;
            style.top = 0f;
            style.right = 0f;
            style.bottom = 0f;
            style.display = DisplayStyle.None;

            EnsureResourcesLoaded();

            for (int i = 0; i < MaxBodySegments; i++)
            {
                VisualElement segment = CreateImageElement($"arrow-widget-body-{i}", _bodyTexture);
                segment.AddToClassList("arrow-widget__body");
                _bodySegments.Add(segment);
                Add(segment);
            }

            _head = CreateImageElement("arrow-widget-head", _headTexture);
            _head.AddToClassList("arrow-widget__head");
            Add(_head);

            InitializeGeometry();
            SetArrowVisible(false);
        }

        public void Show(Vector2 origin)
        {
            _origin = this.WorldToLocal(origin);
            _isShown = true;
            style.display = DisplayStyle.Flex;
            SetArrowVisible(false);
        }

        public void Update(Vector2 target)
        {
            if (!_isShown)
                return;

            Draw(_origin, this.WorldToLocal(target));
        }

        public void Hide()
        {
            _isShown = false;
            style.display = DisplayStyle.None;
            SetArrowVisible(false);
        }

        private static void EnsureResourcesLoaded()
        {
            if (_resourcesLoaded)
                return;

            _resourcesLoaded = true;
            _headTexture = Resources.Load<Texture2D>(HeadResourcePath);
            _bodyTexture = Resources.Load<Texture2D>(BodyResourcePath);

            if (_headTexture == null)
                Debug.LogWarning($"{nameof(ArrowWidget)} missing resource: {HeadResourcePath}");

            if (_bodyTexture == null)
                Debug.LogWarning($"{nameof(ArrowWidget)} missing resource: {BodyResourcePath}");
        }

        private static VisualElement CreateImageElement(string name, Texture2D texture)
        {
            VisualElement element = new()
            {
                name = name,
                pickingMode = PickingMode.Ignore,
                usageHints = UsageHints.DynamicTransform,
            };

            element.style.position = Position.Absolute;
            element.style.transformOrigin = new TransformOrigin(Length.Percent(50), Length.Percent(50), 0f);
            element.style.display = DisplayStyle.None;

            if (texture != null)
                element.style.backgroundImage = new StyleBackground(texture);

            return element;
        }

        private void Draw(Vector2 origin, Vector2 target)
        {
            EnsureResourcesLoaded();

            float distance = Vector2.Distance(origin, target);
            if (distance < 1f || _headTexture == null || _bodyTexture == null)
            {
                SetArrowVisible(false);
                return;
            }

            float curveMagnitude = Mathf.Max(distance * _curveHeight, _minCurveHeight);
            Vector2 mid = (origin + target) * 0.5f;
            Vector2 controlPoint = mid + new Vector2(0f, -curveMagnitude);

            int activeSegments = Mathf.Clamp(_segmentCount, 4, MaxBodySegments);
            float bodyStartT = 0.04f;
            float bodyEndT = 0.88f;

            BuildCurveSamples(origin, controlPoint, controlPoint, target, _curvePoints, _curveLengths);

            float totalCurveLength = _curveLengths[CurveSampleCount];
            float bodyStartDistance = totalCurveLength * bodyStartT;
            float bodyEndDistance = totalCurveLength * bodyEndT;
            float availableBodyLength = Mathf.Max(1f, bodyEndDistance - bodyStartDistance);

            float rawMin = _rawCenters[0] - _visibleLengths[0] * 0.5f;
            float rawMax = _rawCenters[activeSegments - 1] + _visibleLengths[activeSegments - 1] * 0.5f;
            float rawSpan = Mathf.Max(1f, rawMax - rawMin);
            float fitScale = availableBodyLength / rawSpan;

            for (int i = 0; i < MaxBodySegments; i++)
            {
                if (i >= activeSegments)
                {
                    _bodySegments[i].style.display = DisplayStyle.None;
                    continue;
                }

                SetBodySegmentVisible(i, true);

                float centerDistance = bodyStartDistance + (_rawCenters[i] - rawMin) * fitScale;
                Vector2 position = EvaluateCurvePointAtDistance(_curvePoints, _curveLengths, centerDistance);
                Vector2 tangent = EvaluateCurveTangentAtDistance(_curvePoints, _curveLengths, centerDistance).normalized;

                PlaceElement(
                    _bodySegments[i],
                    position,
                    Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg - 180f);
            }

            Vector2 headTangent = EvaluateCurveTangentAtDistance(_curvePoints, _curveLengths, totalCurveLength).normalized;
            Vector2 headAnchor = target - headTangent * (_headHeight * (HeadTipOffsetNormalized + HeadExtraBackOverlapNormalized));

            SetHeadVisible(true);

            PlaceElement(
                _head,
                headAnchor,
                Mathf.Atan2(headTangent.y, headTangent.x) * Mathf.Rad2Deg + 90f);
        }

        private void InitializeGeometry()
        {
            int activeSegments = Mathf.Clamp(_segmentCount, 4, MaxBodySegments);

            for (int i = 0; i < activeSegments; i++)
            {
                float ratio = activeSegments == 1 ? 1f : i / (activeSegments - 1f);
                float curvedRatio = Mathf.Pow(ratio, 1.08f);
                float sizeBoost = i == activeSegments - 2 ? 1.14f : i == activeSegments - 1 ? 1.22f : 1f;

                _bodyWidths[i] = Mathf.Lerp(_bodyWidthStart, _bodyWidthEnd, curvedRatio) * sizeBoost;
                _bodyHeights[i] = _bodyWidths[i] * _bodyAspect;
                _visibleLengths[i] = _bodyHeights[i] * BodyContentHeightNormalized;

                if (i == 0)
                {
                    _rawCenters[i] = _visibleLengths[i] * 0.5f;
                }
                else
                {
                    float spacingRatio = activeSegments <= 1 ? 1f : i / (activeSegments - 1f);
                    float spacingFactor = Mathf.Lerp(SegmentSpacingStartFactor, SegmentSpacingEndFactor, spacingRatio);
                    float spacing = (_visibleLengths[i - 1] + _visibleLengths[i]) * 0.5f * spacingFactor;
                    _rawCenters[i] = _rawCenters[i - 1] + spacing;
                }

                SetElementGeometry(_bodySegments[i], _bodyWidths[i], _bodyHeights[i]);
            }

            SetElementGeometry(_head, _headWidth, _headHeight);
        }

        private void SetArrowVisible(bool visible)
        {
            for (int i = 0; i < MaxBodySegments; i++)
            {
                SetBodySegmentVisible(i, visible);
            }

            SetHeadVisible(visible);
        }

        private void SetBodySegmentVisible(int index, bool visible)
        {
            if (_bodySegmentVisible[index] == visible)
                return;

            _bodySegmentVisible[index] = visible;
            _bodySegments[index].style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetHeadVisible(bool visible)
        {
            if (_headVisible == visible)
                return;

            _headVisible = visible;
            _head.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void SetElementGeometry(VisualElement element, float width, float height)
        {
            element.style.width = width;
            element.style.height = height;
            element.style.left = -width * 0.5f;
            element.style.top = -height * 0.5f;
        }

        private static void PlaceElement(
            VisualElement element,
            Vector2 center,
            float angleDegrees)
        {
            element.style.translate = new Translate(
                new Length(center.x, LengthUnit.Pixel),
                new Length(center.y, LengthUnit.Pixel));
            element.style.rotate = new StyleRotate(new Rotate(angleDegrees));
        }

        private static Vector2 EvaluateCubicBezier(
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            float t)
        {
            float u = 1f - t;
            float u2 = u * u;
            float u3 = u2 * u;
            float t2 = t * t;
            float t3 = t2 * t;

            return u3 * p0
                 + 3f * u2 * t * p1
                 + 3f * u * t2 * p2
                 + t3 * p3;
        }

        private static void BuildCurveSamples(
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            Vector2 p3,
            Vector2[] curvePoints,
            float[] curveLengths)
        {
            curvePoints[0] = p0;
            curveLengths[0] = 0f;

            for (int i = 1; i <= CurveSampleCount; i++)
            {
                float t = i / (float)CurveSampleCount;
                curvePoints[i] = EvaluateCubicBezier(p0, p1, p2, p3, t);
                curveLengths[i] = curveLengths[i - 1] + Vector2.Distance(curvePoints[i - 1], curvePoints[i]);
            }
        }

        private static Vector2 EvaluateCurvePointAtDistance(
            Vector2[] curvePoints,
            float[] curveLengths,
            float distance)
        {
            if (distance <= 0f)
                return curvePoints[0];

            float totalLength = curveLengths[CurveSampleCount];
            if (distance >= totalLength)
                return curvePoints[CurveSampleCount];

            for (int i = 1; i <= CurveSampleCount; i++)
            {
                if (distance > curveLengths[i])
                    continue;

                float segmentLength = curveLengths[i] - curveLengths[i - 1];
                if (segmentLength <= 0.0001f)
                    return curvePoints[i];

                float localT = (distance - curveLengths[i - 1]) / segmentLength;
                return Vector2.Lerp(curvePoints[i - 1], curvePoints[i], localT);
            }

            return curvePoints[CurveSampleCount];
        }

        private static Vector2 EvaluateCurveTangentAtDistance(
            Vector2[] curvePoints,
            float[] curveLengths,
            float distance)
        {
            if (distance <= 0f)
                return curvePoints[1] - curvePoints[0];

            float totalLength = curveLengths[CurveSampleCount];
            if (distance >= totalLength)
                return curvePoints[CurveSampleCount] - curvePoints[CurveSampleCount - 1];

            for (int i = 1; i <= CurveSampleCount; i++)
            {
                if (distance <= curveLengths[i])
                    return curvePoints[i] - curvePoints[i - 1];
            }

            return curvePoints[CurveSampleCount] - curvePoints[CurveSampleCount - 1];
        }
    }
}
