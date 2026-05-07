using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Domains.View.Targeting
{
    public enum ESkillTargetingState
    {
        None,
        Holding,
        Aiming,
    }

    public sealed class SkillTargetingManipulator : PointerManipulator
    {
        private readonly int _slotIndex;
        private readonly Action<SkillTargetingManipulator, int, Vector2, Vector2> _started;
        private readonly Action<Vector2> _moved;
        private readonly Action<Vector2> _released;
        private readonly Action _canceled;

        private int _capturedPointerId = -1;

        public ESkillTargetingState State { get; private set; }
        public bool IsActive => State != ESkillTargetingState.None;

        public SkillTargetingManipulator(
            int slotIndex,
            Action<SkillTargetingManipulator, int, Vector2, Vector2> started,
            Action<Vector2> moved,
            Action<Vector2> released,
            Action canceled)
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });

            _slotIndex = slotIndex;
            _started = started;
            _moved = moved;
            _released = released;
            _canceled = canceled;
        }

        public void Cancel()
        {
            ReleasePointer();
            State = ESkillTargetingState.None;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
            target.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            target.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (IsRightMouseButton(evt) && IsActive)
            {
                Cancel();
                _canceled?.Invoke();
                evt.StopPropagation();
                return;
            }

            if (!CanStartManipulation(evt))
                return;

            target.CapturePointer(evt.pointerId);
            _capturedPointerId = evt.pointerId;
            State = ESkillTargetingState.Holding;
            _started?.Invoke(this, _slotIndex, GetArrowOrigin(target), evt.position);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!HasCapturedPointer(evt.pointerId))
                return;

            _moved?.Invoke(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!HasCapturedPointer(evt.pointerId))
                return;

            if (!CanStopManipulation(evt))
                return;

            ReleasePointer();

            State = ESkillTargetingState.Aiming;
            _released?.Invoke(evt.position);
            evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (_capturedPointerId != evt.pointerId)
                return;

            ReleasePointer();

            State = ESkillTargetingState.None;
            _canceled?.Invoke();
            evt.StopPropagation();
        }

        private void ReleasePointer()
        {
            if (_capturedPointerId < 0)
                return;

            if (target != null && target.HasPointerCapture(_capturedPointerId))
                target.ReleasePointer(_capturedPointerId);

            _capturedPointerId = -1;
        }

        private bool HasCapturedPointer(int pointerId)
        {
            return _capturedPointerId == pointerId
                && target != null
                && target.HasPointerCapture(pointerId);
        }

        private static bool IsRightMouseButton(PointerDownEvent evt)
        {
            return evt.button == (int)MouseButton.RightMouse;
        }

        private static Vector2 GetArrowOrigin(VisualElement element)
        {
            Rect bounds = element.worldBound;

            return new Vector2(
                bounds.center.x,
                bounds.yMin + bounds.height * 0.25f);
        }
    }
}
