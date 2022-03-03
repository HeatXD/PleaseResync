
using System;
using System.Diagnostics;

namespace PleaseResync
{
    internal class InputQueueV2
    {
        public int FirstBadFrame { get => _firstBadFrame; }
        public uint FrameDelay { get => _frameDelay; set => _frameDelay = value; }

        private const uint QUEUE_SIZE = 128;

        private uint _head, _tail, _length;
        private uint _inputSize;
        private uint _playerCount;
        private uint _frameDelay;
        private bool _firstFrame;
        private int _lastAddedFrame;
        private int _firstBadFrame;
        private int _lastRequestedFrame;
        private GameInput _prediction;
        private GameInput[] _inputs;

        public InputQueueV2(uint inputSize, uint playerCount)
        {
            _head = 0;
            _tail = 0;
            _length = 0;

            _frameDelay = 0;
            _firstFrame = true;
            _inputSize = inputSize;
            _playerCount = playerCount;

            _firstBadFrame = GameInput.NullFrame;
            _lastAddedFrame = GameInput.NullFrame;
            _lastRequestedFrame = GameInput.NullFrame;

            _prediction = EmptyInput();
            _inputs = new GameInput[QUEUE_SIZE];

            for (int i = 0; i < QUEUE_SIZE; i++)
            {
                _inputs[i] = EmptyInput();
            }
        }

        private GameInput EmptyInput(int frame = GameInput.NullFrame)
        {
            return new GameInput(frame, _inputSize, _playerCount);
        }

        public void ResetPrediction()
        {
            _lastRequestedFrame = GameInput.NullFrame;
            _prediction.Frame = GameInput.NullFrame;
            _firstBadFrame = GameInput.NullFrame;
        }

        public GameInput VerifiedInput(uint frame)
        {
            uint frameOffset = frame % QUEUE_SIZE;
            if (_inputs[frameOffset].Frame == frame) return _inputs[frameOffset];
            throw new SessionError($"no verified inputs for frame: {frame}");
        }

        public void RemoveVerifiedFrames(int frame)
        {
            if (_lastRequestedFrame != GameInput.NullFrame)
                frame = Math.Min(frame, _lastRequestedFrame);

            if (frame >= _lastAddedFrame)
            {
                _tail = _head;
                _length = 0;
            }
            else if (!(frame <= _inputs[_tail].Frame))
            {
                uint offset = (uint)(frame - _inputs[_tail].Frame);
                _tail = (_tail + offset) % QUEUE_SIZE;
                _length -= offset;
            }
        }

        public GameInput GetInput(int frame)
        {
            Debug.Assert(_firstBadFrame == GameInput.NullFrame);

            _lastRequestedFrame = frame;

            Debug.Assert(frame >= _inputs[_tail].Frame);

            if (_prediction.Frame == GameInput.NullFrame)
            {
                uint offset = (uint)(frame - _inputs[_tail].Frame);
                if (offset < _length)
                {
                    offset = (offset + _tail) % QUEUE_SIZE;
                    Debug.Assert(_inputs[offset].Frame == frame);
                    return _inputs[offset];
                }
            }

            return null;
        }

        public int AddInput(GameInput input)
        {

            return 0;
        }

        private void AddInputByFrame()
        {

        }

        private int ProgressQueueHead()
        {
            return 0;
        }
        private uint PreviousFrame(uint offset) => (((offset) == 0) ? (QUEUE_SIZE - 1) : ((offset) - 1));
    }
}
