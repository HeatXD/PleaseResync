using System;
using System.Collections.Generic;

namespace PleaseResync.Session.Backends.Utility
{
    public class BroadcastStream
    {
        private uint _inputSize;
        private readonly int _initialFrameBuffer;
        private int _currentFrame, _availableFrame;
        private List<byte> _frameBuffer;

        private ReplayFile _replay;
        private List<byte> _initialState;

        public BroadcastStream(int initialBuffer = 30, uint inputSize = 1)
        {
            _initialState = [];
            _replay = new ReplayFile();

            _inputSize = inputSize;
            _initialFrameBuffer = initialBuffer;
            _currentFrame = 0;
            _availableFrame = -1;
            _frameBuffer = new List<byte>((int)(initialBuffer * inputSize));
        }

        public void AddFrameInput(int frame, byte[] input)
        {
            if (frame != _availableFrame + 1) return;

            // Append input to flat buffer
            for (var i = 0; i < _inputSize; i++)
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
            input = new byte[_inputSize];

            var startIndex = (int)(_currentFrame * _inputSize);
            for (var i = 0; i < _inputSize; i++)
            {
                input[i] = _frameBuffer[startIndex + i];
            }

            _currentFrame++;
            return true;
        }

        public string SaveReplayFile()
        {
            _replay.Init(_inputSize, _initialState);
            _replay.SetData(_availableFrame, _frameBuffer);
            var path = _replay.Save();
            return path;
        }

        public void LoadReplayFile(string filepath)
        {
            _replay.LoadFromFile(filepath);

            _inputSize = _replay.InputSize;

            _currentFrame = 0;
            _availableFrame = _replay.NumFrames;

            _frameBuffer.InsertRange(0, _replay.InputFrames);

            _initialState.Clear();
            _initialState.AddRange(_replay.InitialState);
        }

        public uint InputSize() => _inputSize;

        public byte[] GetInitialState() => _initialState.ToArray();

        public void SetInitialState(byte[] state)
        {
            _initialState.Clear();
            _initialState.AddRange(state);
        }

        public void SetCurrentFrame(int frame)
        {
            _currentFrame = Math.Clamp(frame, 0, _availableFrame);
        }
    }
}
