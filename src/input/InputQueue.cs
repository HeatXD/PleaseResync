using System.Collections.Generic;
using System.Diagnostics;

namespace PleaseResync
{
    public class InputQueue
    {
        public const int QueueSize = 128;
        private Queue<GameInput> _lastPredictedInputs;
        private GameInput[] _inputs;
        private int _frameDelay;
        public InputQueue(int inputSize, uint playerCount)
        {
            _lastPredictedInputs = new Queue<GameInput>();
            _inputs = new GameInput[QueueSize];
            _frameDelay = 0;

            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = new GameInput(GameInput.NullFrame, inputSize, playerCount);
            }
        }
        public void AddInput(int frame, GameInput input)
        {
            Debug.Assert(frame >= 0);

            frame += _frameDelay;

            _inputs[frame % QueueSize].Frame = frame;
            _inputs[frame % QueueSize].InputSize = input.InputSize;
            _inputs[frame % QueueSize].SetInputs(0, input.Inputs.Length / input.InputSize, input.Inputs);
        }
        public void SetFrameDelay(int frameDelay)
        {
            Debug.Assert(frameDelay >= 0);

            _frameDelay = frameDelay;
        }
        public GameInput GetInput(int frame)
        {
            Debug.Assert(frame >= 0);

            GameInput resultInput;

            int frameOffset = frame % QueueSize;
            // if the frame is a NullFrame or the frames dont match predict the next frame based off the previous frame.
            if (_inputs[frameOffset].Frame == GameInput.NullFrame ||
                _inputs[frameOffset].Frame != frame)
            {
                // predict current frame based off previous frame.
                var prevFrame = _inputs[PreviousFrame(frameOffset)];
                _inputs[frameOffset] = new GameInput(prevFrame);
                _inputs[frameOffset].Frame = frame;
                // add predicted frame to the queue. when later is proved that the input was right it will be removed.
                _lastPredictedInputs.Enqueue(new GameInput(_inputs[frameOffset]));
            }

            resultInput = new GameInput(_inputs[frameOffset]);
            return resultInput;
        }
        protected int PreviousFrame(int offset) => (((offset) == 0) ? (QueueSize - 1) : ((offset) - 1));
    }
}