using System;
using System.Diagnostics;
using System.Linq;

namespace PleaseResync
{
    public class GameInput
    {
        public const int NullFrame = -1;
        public int Frame;
        public byte[] Inputs;
        public int InputSize;
        public GameInput(int frame, int inputSize, uint playerCount)
        {
            Frame = frame;
            InputSize = inputSize;
            Inputs = new byte[inputSize * playerCount];
        }
        public GameInput(GameInput gameInput)
        {
            Debug.Assert(gameInput.Inputs != null);

            Frame = gameInput.Frame;
            InputSize = gameInput.InputSize;
            Array.Copy(gameInput.Inputs, Inputs, gameInput.Inputs.Length);
        }
        public void SetInputs(int offset, int playerCount, byte[] deviceInputs)
        {
            Debug.Assert(deviceInputs != null);
            Debug.Assert(offset + (playerCount * InputSize) < Inputs.Length);
            Debug.Assert(deviceInputs.Length == playerCount * InputSize);

            Array.Copy(deviceInputs, 0, Inputs, offset, deviceInputs.Length);
        }
        public bool Equal(GameInput other, bool inputsOnly)
        {
            if (!inputsOnly && Frame != other.Frame)
            {
                Console.WriteLine("frames don't match: {0}, {1}", Frame, other.Frame);
            }
            if (InputSize != other.InputSize)
            {
                Console.WriteLine("inputsize for a single player doesn't match: {0}, {1}", InputSize, other.InputSize);
            }
            if (Inputs.Length != other.Inputs.Length)
            {
                Console.WriteLine("inputs array length don't match: {0}, {1}", Inputs.Length, other.Inputs.Length);
            }
            if (!Inputs.SequenceEqual(other.Inputs))
            {
                Console.WriteLine("inputs don't match\n");
            }
            Debug.Assert(Inputs.Length > 0 && other.Inputs.Length > 0);
            return (inputsOnly || Frame == other.Frame) &&
                   Inputs.Length == other.Inputs.Length &&
                   InputSize == other.InputSize &&
                   Inputs.SequenceEqual(other.Inputs);
        }
    }
}
