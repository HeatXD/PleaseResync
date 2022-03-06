
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("PleaseResync.Tests")]
namespace PleaseResync
{
    internal class InputQueue
    {
        public int FirstBadFrame { get => _firstBadFrame; }
        public uint FrameDelay { get => _frameDelay; set => _frameDelay = value; }
        public int LastAddedFrame { get => _lastAddedFrame; }
        public uint Length { get => _length; }

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

        public InputQueue(uint inputSize, uint playerCount, uint frameDelay = 0)
        {
            _head = 0;
            _tail = 0;
            _length = 0;

            _firstFrame = true;
            _inputSize = inputSize;
            _frameDelay = frameDelay;
            _playerCount = playerCount;

            _firstBadFrame = GameInput.NullFrame;
            _lastAddedFrame = GameInput.NullFrame;
            _lastRequestedFrame = GameInput.NullFrame;

            _prediction = EmptyInput();
            _inputs = new GameInput[QUEUE_SIZE];

            for (int i = 0; i < QUEUE_SIZE; i++)
                _inputs[i] = EmptyInput();
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
            Platform.Assert(_firstBadFrame == GameInput.NullFrame);

            _lastRequestedFrame = frame;

            Platform.Assert(frame >= _inputs[_tail].Frame);

            if (_prediction.Frame == GameInput.NullFrame)
            {
                uint offset = (uint)(frame - _inputs[_tail].Frame);
                if (offset < _length)
                {
                    offset = (offset + _tail) % QUEUE_SIZE;
                    Platform.Assert(_inputs[offset].Frame == frame);
                    return new GameInput(_inputs[offset]);
                }

                if (frame == 0 || _lastAddedFrame == GameInput.NullFrame)
                {
                    _prediction = EmptyInput(frame);
                }
                else
                {
                    uint prev = PreviousFrame(_head);
                    _prediction = new GameInput(_inputs[prev]);
                }
                _prediction.Frame++;
            }

            Platform.Assert(_prediction.Frame != GameInput.NullFrame);

            var pred = new GameInput(_prediction);
            pred.Frame = frame;

            return pred;
        }

        public int AddInput(GameInput input)
        {
            // check whether inputs have passed in in the right order
            Platform.Assert(_lastAddedFrame == GameInput.NullFrame ||
                input.Frame + _frameDelay == _lastAddedFrame + 1);

            int frame = ProgressQueueHead(input.Frame);

            if (frame != GameInput.NullFrame)
                AddInputByFrame(input, frame);

            return frame;
        }

        private void AddInputByFrame(GameInput input, int frame)
        {
            Platform.Assert(_lastAddedFrame == GameInput.NullFrame || frame == _lastAddedFrame + 1);
            Platform.Assert(frame == 0 || _inputs[PreviousFrame(_head)].Frame == frame - 1);
            // put input at the end of the queue
            _inputs[_head] = new GameInput(input);
            _inputs[_head].Frame = frame;
            _head = (_head + 1) % QUEUE_SIZE;
            _length++;
            // check bounds
            Platform.Assert(_length <= QUEUE_SIZE);

            _firstFrame = false;
            _lastAddedFrame = frame;
            // correct prediction if needed 
            if (_prediction.Frame != GameInput.NullFrame)
            {
                Platform.Assert(frame == _prediction.Frame);
                // report first bad frame
                if (_firstBadFrame == GameInput.NullFrame && !_prediction.Equal(input, true))
                    _firstBadFrame = frame;
                // either advance in prediction mode or leave it.
                if (_prediction.Frame == _lastRequestedFrame && _firstBadFrame == GameInput.NullFrame)
                    _prediction.Frame = GameInput.NullFrame;
                else
                    _prediction.Frame++;
            }
        }

        private int ProgressQueueHead(int frame)
        {
            int expFrame = _firstFrame ? 0 : _inputs[PreviousFrame(_head)].Frame + 1;
            // account for frame delay
            frame += (int)_frameDelay;
            // this can happen when the frame delay has decreased since the last time
            if (expFrame > frame) return GameInput.NullFrame;
            // this can happen when the frame delay has increased since the last time
            // be sure to fill the empty space
            while (expFrame < frame)
            {
                var copyInput = new GameInput(_inputs[PreviousFrame(_head)]);
                AddInputByFrame(copyInput, expFrame);
                expFrame++;
            }

            Platform.Assert(frame == 0 || frame == _inputs[PreviousFrame(_head)].Frame + 1);
            return frame;
        }

        private uint PreviousFrame(uint offset) => (((offset) == 0) ? (QUEUE_SIZE - 1) : ((offset) - 1));
    }
}
