using System;
using System.Collections.Generic;
using Domains.Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.Adventure
{
    public sealed class CoinEffectPlayer
    {
        private const float CoinSize = 28f;
        private const float HeadsCenterX = -56f;
        private const float TailsCenterX = 56f;
        private const float SpreadSpacing = 24f;
        private const float SpreadOuterDropY = 12f;
        private const float BurstY = -112f;
        private const float StartScale = 0.45f;
        private const float AbsorbScale = 0.22f;
        private const float CoinStaggerSeconds = 0.045f;
        private const float CoinMotionSeconds = 1.06f;
        private const float SpreadPhaseRatio = 0.34f;
        private const float HoldPhaseRatio = 0.18f;
        private const float FadeInPhaseRatio = 0.16f;
        private const float FadeOutStartRatio = 0.9f;
        private const float ArriveCallbackRatio = 0.96f;
        private const float ArcHeight = 18f;

        private const string RootClass = "coin-effect";
        private const string VisualClass = "coin-effect__visual";
        private const string HeadsClass = "coin-effect__visual--heads";
        private const string TailsClass = "coin-effect__visual--tails";

        private readonly List<CoinEffect> _pool = new();
        private readonly List<CoinEffect> _active = new();

        private VisualElement _effectLayer;

        public void Bind(VisualElement effectLayer)
        {
            _effectLayer = effectLayer;
        }

        public async Awaitable Play(
            CoinFlipDto coinFlip,
            VisualElement source,
            VisualElement headsTarget,
            VisualElement tailsTarget,
            Action<ECoinFace> onArrived)
        {
            if (!CanPlay(coinFlip, source, headsTarget, tailsTarget))
                return;

            EnsurePool(coinFlip.Count);

            CoinEffectCompletion completion = new(coinFlip.Count);
            CoinFaceLayoutCounter layoutCounter = new(coinFlip);

            for (int i = 0; i < coinFlip.Count; i++)
            {
                ECoinFace face = coinFlip.Faces[i];
                int faceIndex = layoutCounter.Next(face);
                int faceCount = layoutCounter.GetCount(face);
                VisualElement target = GetTarget(face, headsTarget, tailsTarget);
                CoinEffect coin = Rent(face);

                _ = PlayOne(coin, face, faceIndex, faceCount, source, target, onArrived, completion);

                await Awaitable.WaitForSecondsAsync(CoinStaggerSeconds);
            }

            await completion.Awaitable;
        }

        public void Clear()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                Return(_active[i]);
            }
        }

        private bool CanPlay(
            CoinFlipDto coinFlip,
            VisualElement source,
            VisualElement headsTarget,
            VisualElement tailsTarget)
        {
            return _effectLayer != null &&
                   source != null &&
                   headsTarget != null &&
                   tailsTarget != null &&
                   coinFlip != null &&
                   coinFlip.Count > 0;
        }

        private async Awaitable PlayOne(
            CoinEffect coin,
            ECoinFace face,
            int faceIndex,
            int faceCount,
            VisualElement source,
            VisualElement targetElement,
            Action<ECoinFace> onArrived,
            CoinEffectCompletion completion)
        {
            CoinEffectPath path = GetPath(face, faceIndex, faceCount, source, targetElement);

            coin.Reset(path.Start);

            await Awaitable.NextFrameAsync();
            await PlayMotion(coin, face, path, onArrived);
            Return(coin);
            completion.Complete();
        }

        private async Awaitable PlayMotion(
            CoinEffect coin,
            ECoinFace face,
            CoinEffectPath path,
            Action<ECoinFace> onArrived)
        {
            bool arrived = false;

            await Animate(CoinMotionSeconds, t =>
            {
                CoinMotionFrame frame = GetMotionFrame(path, t);
                coin.SetOffset(frame.Position.x, 0f);
                coin.SetVisual(frame.Opacity, frame.Scale, frame.Position.y);

                if (!arrived && t >= ArriveCallbackRatio)
                {
                    arrived = true;
                    onArrived?.Invoke(face);
                }
            });

            if (!arrived)
                onArrived?.Invoke(face);
        }

        private CoinEffect Rent(ECoinFace face)
        {
            CoinEffect coin = TakeFromPool();
            coin.SetFace(face);
            _effectLayer.Add(coin.Root);
            _active.Add(coin);
            return coin;
        }

        private CoinEffect TakeFromPool()
        {
            if (_pool.Count == 0)
                return new CoinEffect();

            int lastIndex = _pool.Count - 1;
            CoinEffect coin = _pool[lastIndex];
            _pool.RemoveAt(lastIndex);
            return coin;
        }

        private void Return(CoinEffect coin)
        {
            coin.Clear();
            coin.Root.RemoveFromHierarchy();
            _active.Remove(coin);
            _pool.Add(coin);
        }

        private void EnsurePool(int count)
        {
            int availableCount = _pool.Count + _active.Count;

            while (availableCount < count)
            {
                _pool.Add(new CoinEffect());
                availableCount++;
            }
        }

        private CoinEffectPath GetPath(
            ECoinFace face,
            int faceIndex,
            int faceCount,
            VisualElement source,
            VisualElement target)
        {
            Vector2 start = GetLocalCenter(source);
            Vector2 spread = GetSpread(face, faceIndex, faceCount);
            Vector2 targetOffset = GetLocalCenter(target) - start;
            return new CoinEffectPath(start, spread, targetOffset);
        }

        private Vector2 GetLocalCenter(VisualElement element)
        {
            Vector2 layerPosition = _effectLayer.worldBound.position;
            return element.worldBound.center - layerPosition;
        }

        private static Vector2 GetSpread(ECoinFace face, int index, int count)
        {
            float centerX = face == ECoinFace.Heads ? HeadsCenterX : TailsCenterX;
            float offsetIndex = index - (count - 1) * 0.5f;
            float x = centerX + offsetIndex * SpreadSpacing;
            float y = BurstY + Mathf.Abs(offsetIndex) * SpreadOuterDropY;
            return new Vector2(x, y);
        }

        private static VisualElement GetTarget(
            ECoinFace face,
            VisualElement headsTarget,
            VisualElement tailsTarget)
        {
            return face == ECoinFace.Heads
                ? headsTarget
                : tailsTarget;
        }

        private static async Awaitable Animate(float durationSeconds, Action<float> onUpdate)
        {
            float elapsedSeconds = 0f;

            while (elapsedSeconds < durationSeconds)
            {
                elapsedSeconds += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedSeconds / durationSeconds);
                onUpdate(t);

                await Awaitable.NextFrameAsync();
            }

            onUpdate(1f);
        }

        private static CoinMotionFrame GetMotionFrame(CoinEffectPath path, float progress)
        {
            float spreadEnd = SpreadPhaseRatio;
            float holdEnd = SpreadPhaseRatio + HoldPhaseRatio;
            Vector2 position;
            float scale;

            if (progress < spreadEnd)
            {
                float spreadT = progress / spreadEnd;
                float eased = EaseOutCubic(spreadT);
                float arcLift = Mathf.Sin(spreadT * Mathf.PI) * ArcHeight;
                position = new Vector2(
                    Mathf.LerpUnclamped(0f, path.Spread.x, eased),
                    Mathf.LerpUnclamped(0f, path.Spread.y, eased) - arcLift);
                scale = Mathf.Lerp(StartScale, 1f, eased);
            }
            else if (progress < holdEnd)
            {
                position = path.Spread;
                scale = 1f;
            }
            else
            {
                float absorbT = (progress - holdEnd) / Mathf.Max(0.0001f, 1f - holdEnd);
                float eased = EaseInOutSine(absorbT);
                position = Vector2.LerpUnclamped(path.Spread, path.Target, eased);
                scale = Mathf.Lerp(1f, AbsorbScale, eased);
            }

            float opacity = Mathf.Lerp(0f, 1f, Mathf.Clamp01(progress / FadeInPhaseRatio));
            opacity = Mathf.Lerp(
                opacity,
                0f,
                Mathf.Clamp01((progress - FadeOutStartRatio) / Mathf.Max(0.0001f, 1f - FadeOutStartRatio)));

            if (progress >= spreadEnd && progress < holdEnd)
                opacity = 1f;
            else if (progress >= holdEnd)
                opacity = Mathf.Max(opacity, 0.3f);

            return new CoinMotionFrame(position, opacity, scale);
        }

        private static float EaseOutCubic(float t)
        {
            float inverseT = 1f - t;
            return 1f - inverseT * inverseT * inverseT;
        }

        private static float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
        }

        private readonly struct CoinEffectPath
        {
            public CoinEffectPath(Vector2 start, Vector2 spread, Vector2 target)
            {
                Start = start;
                Spread = spread;
                Target = target;
            }

            public Vector2 Start { get; }
            public Vector2 Spread { get; }
            public Vector2 Target { get; }
        }

        private readonly struct CoinMotionFrame
        {
            public CoinMotionFrame(Vector2 position, float opacity, float scale)
            {
                Position = position;
                Opacity = opacity;
                Scale = scale;
            }

            public Vector2 Position { get; }
            public float Opacity { get; }
            public float Scale { get; }
        }

        private sealed class CoinEffect
        {
            public CoinEffect()
            {
                Root = new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                };

                Visual = new VisualElement
                {
                    pickingMode = PickingMode.Ignore,
                };

                Root.AddToClassList(RootClass);
                Visual.AddToClassList(VisualClass);
                Root.Add(Visual);
            }

            public VisualElement Root { get; }
            public VisualElement Visual { get; }
            public Vector2 Position { get; private set; }

            public void Reset(Vector2 center)
            {
                Clear();
                Root.style.left = center.x - CoinSize * 0.5f;
                Root.style.top = center.y - CoinSize * 0.5f;
                Root.style.opacity = 1f;
                SetOffset(0f, 0f);
                SetVisual(0f, StartScale, 0f);
            }

            public void SetFace(ECoinFace face)
            {
                Visual.RemoveFromClassList(HeadsClass);
                Visual.RemoveFromClassList(TailsClass);
                Visual.AddToClassList(face == ECoinFace.Heads ? HeadsClass : TailsClass);
            }

            public void SetOffset(float x, float y)
            {
                Position = new Vector2(x, y);
                Root.style.translate = new Translate(
                    new Length(x, LengthUnit.Pixel),
                    new Length(y, LengthUnit.Pixel));
            }

            public void SetVisual(float opacity, float scale, float y)
            {
                Visual.style.opacity = opacity;
                Visual.style.scale = new Scale(new Vector2(scale, scale));
                Visual.style.translate = new Translate(
                    new Length(0f, LengthUnit.Pixel),
                    new Length(y, LengthUnit.Pixel));
            }

            public void Clear()
            {
                Position = Vector2.zero;
                Root.style.opacity = StyleKeyword.Null;
                Root.style.translate = StyleKeyword.Null;
                Visual.style.opacity = StyleKeyword.Null;
                Visual.style.scale = StyleKeyword.Null;
                Visual.style.translate = StyleKeyword.Null;
            }
        }

        private sealed class CoinEffectCompletion
        {
            private readonly AwaitableCompletionSource _completionSource = new();
            private readonly int _totalCount;
            private int _completedCount;

            public CoinEffectCompletion(int totalCount)
            {
                _totalCount = totalCount;
            }

            public Awaitable Awaitable => _completionSource.Awaitable;

            public void Complete()
            {
                _completedCount++;

                if (_completedCount >= _totalCount)
                    _completionSource.SetResult();
            }
        }

        private sealed class CoinFaceLayoutCounter
        {
            private readonly int _headsCount;
            private readonly int _tailsCount;
            private int _headsIndex;
            private int _tailsIndex;

            public CoinFaceLayoutCounter(CoinFlipDto coinFlip)
            {
                _headsCount = coinFlip.HeadsCount;
                _tailsCount = coinFlip.TailsCount;
            }

            public int Next(ECoinFace face)
            {
                if (face == ECoinFace.Heads)
                    return _headsIndex++;

                return _tailsIndex++;
            }

            public int GetCount(ECoinFace face)
            {
                return face == ECoinFace.Heads
                    ? _headsCount
                    : _tailsCount;
            }
        }
    }
}
