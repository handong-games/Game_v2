using System;
using UnityEngine;

namespace UIToolkit.Timeline
{
    [Serializable]
    public struct TimelineEvent
    {
        [SerializeField]
        private int _timeMs;

        [SerializeField]
        private TimelineOp _op;

        [SerializeField]
        private ushort _operand;

        public TimelineEvent(int timeMs, TimelineOp op, ushort operand)
        {
            _timeMs = timeMs;
            _op = op;
            _operand = operand;
        }

        public int TimeMs => _timeMs;
        public TimelineOp Op => _op;
        public ushort Operand => _operand;
    }
}
