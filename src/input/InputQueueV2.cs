
namespace PleaseResync
{
    internal class InputQueueV2
    {
        public int FirstBadEntry { get => _firstBadEntry; }
        public uint FrameDelay { get => _frameDelay; set => _frameDelay = value; }
        private const uint QUEUE_SIZE = 128;
        private uint _head, _tail, _length;
        private uint _inputSize;
        private uint _playerCount;
        private uint _frameDelay;
        private bool _firstEntry;
        private int _lastAddedEntry;
        private int _firstBadEntry;
        private int _lastRequestedEntry;
        private GameInput _prediction;
        private GameInput[] _inputs;

        public InputQueueV2(uint inputSize, uint playerCount)
        {
            _head = 0;
            _tail = 0;
            _length = 0;

            _frameDelay = 0;
            _firstEntry = true;
            _inputSize = inputSize;
            _playerCount = playerCount;

            _firstBadEntry = GameInput.NullFrame;
            _lastAddedEntry = GameInput.NullFrame;
            _lastRequestedEntry = GameInput.NullFrame;

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
            _lastRequestedEntry = GameInput.NullFrame;
            _prediction.Frame = GameInput.NullFrame;
            _firstBadEntry = GameInput.NullFrame;
        }

        public GameInput VerifiedInput(uint frame)
        {
            uint frameOffset = frame % QUEUE_SIZE;
            if (_inputs[frameOffset].Frame == frame) return _inputs[frameOffset];
            throw new SessionError("No verified inputs for frame: " + frame);
        }
    }
}
