using System.Collections.Generic;

namespace PleaseResync.session.backends.utility
{
    public class BroadcastStream
    {
        public readonly uint InputSize;
        private readonly int _initialFrameBuffer;
        private int _currentFrame, _availableFrame;
        private readonly List<byte> _frameBuffer;

        public BroadcastStream(int initialBuffer = 30, uint inputSize = 1)
        {
            InputSize = inputSize;
            _initialFrameBuffer = initialBuffer;
            _currentFrame = 0;
            _availableFrame = -1;
            _frameBuffer = new List<byte>((int)(initialBuffer * inputSize));
        }

        public void AddFrameInput(int frame, byte[] input)
        {
            if (frame != _availableFrame + 1) return;

            // Append input to flat buffer
            for (int i = 0; i < InputSize; i++)
            {
                _frameBuffer.Add(input[i]);
            }

            _availableFrame++;
        }

        public bool GetFrameInput(out int frame, out byte[] input)
        {
            frame = 0;
            input = null;

            if (_availableFrame - _currentFrame < _initialFrameBuffer)
            {
                return false;
            }

            if (_currentFrame > _availableFrame)
            {
                return false;
            }

            frame = _currentFrame;
            input = new byte[InputSize];

            int startIndex = (int)(_currentFrame * InputSize);
            for (int i = 0; i < InputSize; i++)
            {
                input[i] = _frameBuffer[startIndex + i];
            }

            _currentFrame++;
            return true;
        }

        public void SaveToFile()
        {
            ReplayFile.SaveToFile(InputSize, _availableFrame, [], _frameBuffer);
        }
    }
}
