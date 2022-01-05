using System;
using System.Diagnostics;
using System.Linq;

namespace PleaseResync
{
    // contains all the input for a given frame from all devices.
    public class GameInput
    {
        public const int NullFrame = -1;
        public int Frame;
        public byte[] Inputs;
        public GameInput(int frame, int inputSize, int totalPlayerCount)
        {
            Frame = frame;
            Inputs = new byte[inputSize * totalPlayerCount];
        }

        public bool Equal(GameInput other, bool inputsOnly)
        {
            if (!inputsOnly && Frame != other.Frame)
            {
                Console.WriteLine("frames don't match: {0}, {1}", Frame, other.Frame);
            }
            if (Inputs.Length != other.Inputs.Length)
            {
               Console.WriteLine("sizes don't match: {0}, {1}", Inputs.Length, other.Inputs.Length);
            }
            if (!Inputs.SequenceEqual(other.Inputs))
            {
                Console.WriteLine("Inputs don't match\n");
            }
            Debug.Assert(Inputs.Length > 0 && other.Inputs.Length  > 0);
            return (inputsOnly || Frame == other.Frame) &&
                   Inputs.Length == other.Inputs.Length  &&
                   Inputs.SequenceEqual(other.Inputs);
        }
    }

}
