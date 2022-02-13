using System.Diagnostics;
using System.Collections.Generic;
using System;

namespace PleaseResync
{
    internal class Sync
    {
        private readonly uint _inputSize;
        private readonly Device[] _devices;

        private TimeSync _timeSync;
        private InputQueue[] _deviceInputs;
        private StateStorage _stateStorage;
        private int _lastSavedFrame;

        public Sync(Device[] devices, uint inputSize)
        {
            _devices = devices;
            _inputSize = inputSize;
            _timeSync = new TimeSync();
            _stateStorage = new StateStorage(TimeSync.MaxRollbackFrames);
            _deviceInputs = new InputQueue[_devices.Length];
            _lastSavedFrame = GameInput.NullFrame;
        }

        public void AddRemoteInput(uint deviceId, int frame, byte[] deviceInput)
        {
            // only allow adding input to the local device
            Debug.Assert(_devices[deviceId].Type == Device.DeviceType.Remote);
            // update device variables if needed
            if (_devices[deviceId].RemoteFrame < frame)
            {
                AckRemoteInput(deviceId, frame);
            }
            AddDeviceInput(frame, deviceId, deviceInput);
        }

        private void AckRemoteInput(uint deviceId, int frame)
        {
            _devices[deviceId].RemoteFrame = frame;
            _devices[deviceId].RemoteFrameAdvantage = _timeSync.LocalFrame - frame;
            // let them know u recieved the packet
            _devices[deviceId].SendMessage(new DeviceInputAckMessage { Frame = (uint)frame });
        }

        public void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            _deviceInputs[deviceId] = new InputQueue(_inputSize, playerCount, frameDelay);
        }

        public void AddRemoteDevice(uint deviceId, uint playerCount)
        {
            _deviceInputs[deviceId] = new InputQueue(_inputSize, playerCount);
        }

        public void IncrementFrame() => _timeSync.LocalFrame++;

        public int CurrentFrame() => _timeSync.LocalFrame;

        public int SyncFrame() => _timeSync.SyncFrame;

        public int RemoteFrame() => _timeSync.RemoteFrame;

        public void SendLocalInputs(uint localDeviceId)
        {
            if (CurrentFrame() != GameInput.NullFrame)
            {
                foreach (var device in _devices)
                {
                    if (device.Type == Device.DeviceType.Remote)
                    {
                        uint finalFrame = (uint)(_timeSync.LocalFrame + _deviceInputs[localDeviceId].GetFrameDelay());

                        var combinedInput = new List<byte>();

                        for (uint i = device.LastAckedInputFrame; i <= finalFrame; i++)
                        {
                            combinedInput.AddRange(GetDeviceInput((int)i, localDeviceId).Inputs);
                        }

                        device.SendMessage(new DeviceInputMessage
                        {
                            StartFrame = device.LastAckedInputFrame,
                            EndFrame = finalFrame,
                            Input = combinedInput.ToArray()
                        });
                    }
                }
            }
        }

        public void UpdateSyncFrame()
        {
            int finalFrame = _timeSync.RemoteFrame;
            if (_timeSync.RemoteFrame > _timeSync.LocalFrame)
            {
                finalFrame = _timeSync.LocalFrame;
            }
            bool foundMistake = false;
            int foundFrame = finalFrame;
            for (int i = _timeSync.SyncFrame + 1; i <= finalFrame; i++)
            {
                foreach (var input in _deviceInputs)
                {
                    var predInput = input.GetPredictedInput(i);
                    if (predInput.Frame == i &&
                        input.GetInput(i, false).Frame == i)
                    {
                        // Incorrect Prediction
                        if (!predInput.Equal(input.GetInput(i, false), true))
                        {
                            foundFrame = i - 1;
                            foundMistake = true;
                        }
                        // remove prediction form queue
                        input.ResetPrediction(i);
                    }
                }
                if (foundMistake) break;
            }
            _timeSync.SyncFrame = foundFrame;
        }

        public void UpdateTimeSync() => _timeSync.UpdateTimeSync(_devices);

        public SessionAction SaveCurrentFrame()
        {
            _lastSavedFrame = CurrentFrame();
            return new SessionSaveGameAction(CurrentFrame(), _stateStorage);
        }

        public SessionAction LoadFrame(int frame)
        {
            Debug.Assert(
                frame != GameInput.NullFrame &&
                frame <= CurrentFrame() &&
                frame >= CurrentFrame() - TimeSync.MaxRollbackFrames,
                "Requested frame is probably too far in the past or future");

            var action = new SessionLoadGameAction(frame, _stateStorage);

            _timeSync.LocalFrame = frame;

            return action;
        }
        public int AddLocalInput(uint deviceId, byte[] deviceInput)
        {
            // only allow adding input to the local device
            Debug.Assert(_devices[deviceId].Type == Device.DeviceType.Local);
            // check if the predictition threshold has been reached. if it has reached the predictition threshold drop the input.
            // otherwise return the frame where the input has been added with input delay
            if (_timeSync.PredictionLimitReached())
            {
                System.Console.WriteLine("Prediction Limit Reached!");
                return GameInput.NullFrame;
            }
            else
            {
                return AddDeviceInput(_timeSync.LocalFrame, deviceId, deviceInput);
            }
        }

        private int AddDeviceInput(int frame, uint deviceId, byte[] deviceInput)
        {
            Debug.Assert(deviceInput.Length == _devices[deviceId].PlayerCount * _inputSize,
             "the length of the given deviceInput isnt correct!");

            var input = new GameInput(frame, _inputSize, _devices[deviceId].PlayerCount);
            input.SetInputs(0, _devices[deviceId].PlayerCount, deviceInput);

            return _deviceInputs[deviceId].AddInput(frame, input);
        }

        private GameInput GetDeviceInput(int frame, uint deviceId, bool predict = true)
        {
            return _deviceInputs[deviceId].GetInput(frame, predict);
        }

        public GameInput GetFrameInput(int frame, bool predict = true)
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
                var tmpInput = GetDeviceInput(frame, i, predict);
                input.SetInputs(playerOffset, _devices[i].PlayerCount, tmpInput.Inputs);
                // advance player offset to the position of the next device
                playerOffset += _devices[i].PlayerCount;
            }
            return input;
        }
    }
}
