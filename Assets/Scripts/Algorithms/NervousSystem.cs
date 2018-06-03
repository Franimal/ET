using System;

namespace Assets.Scripts.Algorithms
{
    public class NervousSystem
    {
        public bool _move { get; set; }
        public bool _turnLeft { get; set; }
        public bool _turnRight { get; set; }
        public bool _turnUp { get; set; }
        public bool _turnDown { get; set; }
        public bool _tryBuy { get; set; }
        public bool _trySell { get; set; }
        public bool _moveBackwards { get; set; }

        internal bool ShouldTurnLeft()
        {
            return _turnLeft;
        }

        internal bool ShouldTurnRight()
        {
            return _turnRight;
        }

        internal bool ShouldMove()
        {
            return _move;
        }

        internal bool ShouldMoveBackwards()
        {
            return _moveBackwards;
        }

        internal bool ShouldTryBuy()
        {
            return _tryBuy;
        }

        internal bool ShouldTrySell()
        {
            return _trySell;
        }

        internal bool ShouldTurnUp()
        {
            return _turnUp;
        }

        internal bool ShouldTurnDown()
        {
            return _turnDown;
        }
    }
}