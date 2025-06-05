using System;
using System.Collections.Generic;
using PleaseResync.Session.Backends.Utility;
using PleaseResync.Synchronization;

namespace PleaseResync.Session.Backends
{
    public class ReplaySession : Session
    {
        public enum ReplayState
        {
            NoFile,
            FileLoaded,
            Restarting,
            Running,
            Paused,
            Stepping,
            Jumping,
        }

        private ReplayState _state;
        private int _currentFrame = 0;
        private int _requestedFrame = -1;
        private readonly StateStorage _stateStorage;
        private readonly BroadcastStream _broadcastStream;

        public ReplaySession() : base(0, 0, 0, false, true)
        {
            _state = ReplayState.NoFile;
            _broadcastStream = new BroadcastStream();
            _stateStorage = new StateStorage(0);
        }

        public void LoadFile(string filepath)
        {
            _broadcastStream.LoadReplayFile(filepath);
            
            _state = ReplayState.FileLoaded;

            var initialState = _broadcastStream.GetInitialState();
            _stateStorage.SaveFrame(0, initialState);
        }

        public void Restart()
        {
            if (_state == ReplayState.NoFile) return;

            _state = ReplayState.Restarting;
        }

        public void Pause()
        {
            if (_state == ReplayState.NoFile) return;
            _state = ReplayState.Paused;
        }

        public void Resume()
        {
            if (_state == ReplayState.NoFile) return;
            _state = ReplayState.Running;
        }
        
        public void Step()
        {
            if (_state == ReplayState.NoFile) return;
            _state = ReplayState.Stepping;
            _requestedFrame = _currentFrame + 1;
        }

        public void GoToFrame(int frame)
        {
            if (_state == ReplayState.NoFile) return;
            _state = ReplayState.Jumping;
            _requestedFrame = frame;
        }


        protected internal override Device LocalDevice => throw new System.NotImplementedException();

        protected internal override Device[] AllDevices => throw new System.NotImplementedException();

        public override void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration)
        {
            throw new System.NotImplementedException();
        }

        public override void AddSpectatorDevice(object remoteConfiguration)
        {
            throw new System.NotImplementedException();
        }

        public override List<SessionAction> AdvanceFrame(byte[] localInput = null)
        {
            var actions = new List<SessionAction>();

            switch (_state)
            {
                case ReplayState.NoFile:
                    break;
                case ReplayState.FileLoaded:
                case ReplayState.Restarting:
                    _currentFrame = 0;
                    _broadcastStream.SetCurrentFrame(0);
                    Resume();
                    break;
                case ReplayState.Running:
                    if (_broadcastStream.GetFrameInput(out var frame, out var input))
                    {
                        if (_currentFrame == 0)
                        {
                            actions.Add(new SessionLoadGameAction(0, _stateStorage));
                        }
                        _currentFrame = frame;
                        actions.Add(new SessionAdvanceFrameAction(frame, input));
                    }
                    break;
                case ReplayState.Stepping:
                    if (_currentFrame + 1 == _requestedFrame && 
                        _broadcastStream.GetFrameInput(out var stepFrame, out var stepInput))
                    {
                        _currentFrame = stepFrame;
                        actions.Add(new SessionAdvanceFrameAction(stepFrame, stepInput));
                    }
                    break;
                case ReplayState.Jumping:
                    if (_currentFrame > _requestedFrame)
                    {
                        actions.Add(new SessionLoadGameAction(0, _stateStorage));
                        _currentFrame = 0;
                    }
                    _broadcastStream.SetCurrentFrame(_currentFrame);
                    for (int i = _currentFrame; i <= _requestedFrame; i++)
                    {
                        if (_broadcastStream.GetFrameInput(out var jumpFrame, out var jumpInput))
                        {
                            _currentFrame = jumpFrame;
                            actions.Add(new SessionAdvanceFrameAction(jumpFrame, jumpInput));
                        }
                    }
                    Resume();
                    break;
                case ReplayState.Paused:
                    break;
                default:
                    break;
            }

            return actions;
        }

        public override uint AverageRollbackFrames()
        {
            throw new System.NotImplementedException();
        }

        public override int Frame()
        {
            return _currentFrame;
        }

        public override int FrameAdvantage()
        {
            throw new System.NotImplementedException();
        }

        public override int FrameAdvantageDifference()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsRunning()
        {
            return _state != ReplayState.NoFile;
        }

        public override void Poll()
        {
        }

        public override int RemoteFrame()
        {
            throw new System.NotImplementedException();
        }

        public override int RemoteFrameAdvantage()
        {
            throw new System.NotImplementedException();
        }

        public override uint RollbackFrames()
        {
            throw new System.NotImplementedException();
        }

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay)
        {
            throw new System.NotImplementedException();
        }

        public override int State()
        {
            throw new System.NotImplementedException();
        }

        protected internal override void AddRemoteInput(uint deviceId, DeviceInputMessage message)
        {
            throw new System.NotImplementedException();
        }

        protected internal override uint SendMessageTo(uint deviceId, DeviceMessage message)
        {
            throw new System.NotImplementedException();
        }

        public override void SaveToReplayFile()
        {
            throw new NotImplementedException();
        }
    }
}
