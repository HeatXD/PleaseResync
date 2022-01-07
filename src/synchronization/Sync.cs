using System.Collections.Generic;
using System.Diagnostics;

namespace PleaseResync
{
    public class Sync
    {
        private TimeSync _timeSync;
        private readonly Device[] _devices;
        private InputQueue[] _deviceInputs;
        private readonly int _inputSize;
        public Sync(Device[] devices, int inputSize)
        {
            _devices = devices;
            _inputSize = inputSize;
            _timeSync = new TimeSync();
            _deviceInputs = new InputQueue[_devices.Length];

            for (int i = 0; i < _deviceInputs.Length; i++)
            {
                _deviceInputs[i] = new InputQueue(_inputSize, _devices[i].PlayerCount);
            }
        }
        // should be called after polling the remote devices
        public List<SessionAction> AdvanceSync()
        {
            UpdateSyncFrame();

            var actions = new List<SessionAction>();

            if (_timeSync.ShouldRollback())
            {
                actions.Add(new SessionLoadGameAction());
                for (int i = _timeSync.SyncFrame + 1; i <= _timeSync.LocalFrame; i++)
                {
                    var inputs = GetFrameInput(_timeSync.LocalFrame).Inputs;
                    actions.Add(new SessionAdvanceFrameAction(inputs));
                }
                actions.Add(new SessionSaveGameAction());
            }

            if (_timeSync.IsTimeSynced(_devices))
            {
                _timeSync.LocalFrame++;
                var inputs = GetFrameInput(_timeSync.LocalFrame).Inputs;
                actions.Add(new SessionAdvanceFrameAction(inputs));
                actions.Add(new SessionSaveGameAction());
            }

            return actions;
        }
        public void UpdateSyncFrame()
        {
            int finalFrame = _timeSync.RemoteFrame;
            if (_timeSync.RemoteFrame > _timeSync.LocalFrame)
            {
                finalFrame = _timeSync.LocalFrame;
            }
            int foundFrame = finalFrame;
            for (int i = _timeSync.SyncFrame + 1; i <= finalFrame; i++)
            {
                // find the first frame where the predicted and remote inputs don't match
                // we assume the last frame is still correct
                // foundFrame =  i - 1;
                // break;
            }
            _timeSync.SyncFrame = foundFrame;
        }
        public void AddDeviceInput(int frame, uint deviceId, byte[] deviceInput)
        {
            Debug.Assert(deviceInput.Length == _devices[deviceId].PlayerCount * _inputSize);

            var input = new GameInput(frame, _inputSize, _devices[deviceId].PlayerCount);
            input.SetInputs(0, _devices[deviceId].PlayerCount, deviceInput);

            _deviceInputs[deviceId].AddInput(frame, input);
        }
        public GameInput GetDeviceInput(int frame, uint deviceId)
        {
            return _deviceInputs[deviceId].GetInput(frame);
        }
        public GameInput GetFrameInput(int frame)
        {
            uint playerCount = 0;
            foreach (var device in _devices)
            {
                playerCount += device.PlayerCount;
            }
            // add all device inputs into a single GameInput
            var input = new GameInput(frame, _inputSize, playerCount);
            // offset is needed to put the players input in the correct position
            uint playerOffset = 0;
            for (uint i = 0; i < _devices.Length; i++)
            {
                // get the input of the device and add it to the rest of the inputs
                var tmpInput = GetDeviceInput(frame, i);
                input.SetInputs(playerOffset, _devices[i].PlayerCount, tmpInput.Inputs);
                // advance player offset to the position of the next device
                playerOffset += _devices[i].PlayerCount;
            }
            return input;
        }
        public void SetFrameDelay(uint delay, uint deviceId)
        {
            // only allow setting frame delay of the local device
            if (_devices[deviceId].Type == Device.DeviceType.Local)
            {
                _deviceInputs[deviceId].SetFrameDelay(delay);
            }
        }
    }
}
