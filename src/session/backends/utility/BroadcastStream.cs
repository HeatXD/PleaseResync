using System.Collections.Generic;

namespace PleaseResync.session.backends.utility
{
    public class BroadcastStream
    {
        public readonly uint InputSize;
        private readonly int _initialFrameBuffer;
        private int _currentFrame, _availableFrame;
        private readonly List<byte> _frameInputs;

        public BroadcastStream(int initialBuffer = 30, uint inputSize = 1)
        {
            InputSize = inputSize;
            _initialFrameBuffer = initialBuffer;
            _currentFrame = 0;
            _availableFrame = -1;
            _frameInputs = new List<byte>((int)(_initialFrameBuffer * inputSize));
        }

        public void AddFrameInput(int frame, byte[] input)
        {
            // we only accept the inputs for the next frame in order.
            if (frame != _availableFrame + 1)
            {
                return;
            }
            // add the input to the buffer
            _frameInputs.AddRange(input);
            // be ready to accept the following frames.
            _availableFrame++;
        }

        public bool GetFrameInput(out int frame, out byte[] input)
        {
            frame = 0;
            input = null;

            // return false if we haven't buffered enough frames yet for smooth playback
            // or if we've already processed all available frames.
            if (_availableFrame <= _initialFrameBuffer || _currentFrame > _availableFrame)
            {
                return false;
            }

            var inputSize = (int)InputSize;
            frame = _currentFrame;

            input = new byte[inputSize];
            _frameInputs.CopyTo(inputSize * _currentFrame, input, 0, inputSize);

            _currentFrame++;
            return true;
        }
    }
}
