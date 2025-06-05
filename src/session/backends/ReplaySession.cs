using System;
using System.Collections.Generic;
using PleaseResync.Session.Backends.Utility;
using PleaseResync.Synchronization;

namespace PleaseResync.Session.Backends
{
    public sealed class ReplaySession : Session
    {
        private enum PlaybackState
        {
            NoFile,
            FileLoaded,
            Running,
            Paused
        }

        private readonly Queue<Action<List<SessionAction>>> _commandQueue = new();
        private readonly BroadcastStream _broadcastStream;
        private readonly StateStorage _stateStorage;

        private PlaybackState _currentState = PlaybackState.NoFile;
        private int _currentFrame;
        private int _targetFrame;

        public ReplaySession(): base(0, 0, 0, false, true)
        {
            _broadcastStream = new BroadcastStream();
            _stateStorage = new StateStorage(0);
            _currentFrame = 0;
            _targetFrame = -1;
        }

        public void LoadFile(string filePath)
        {
            Enqueue(actions =>
            {
                _broadcastStream.LoadReplayFile(filePath);
                var initialState = _broadcastStream.GetInitialState();
                _stateStorage.SaveFrame(0, initialState);

                _currentFrame = 0;
                _targetFrame = -1;
                _currentState = PlaybackState.FileLoaded;

                // Immediately emit a load-game action so the engine knows to load state 0
                actions.Add(new SessionLoadGameAction(0, _stateStorage));
            });
        }

        public void Restart()
        {
            Enqueue(actions =>
            {
                if (_currentState == PlaybackState.NoFile) return;

                _currentFrame = 0;
                _broadcastStream.SetCurrentFrame(0);
                _currentState = PlaybackState.Running;

                actions.Add(new SessionLoadGameAction(0, _stateStorage));
            });
        }

        public void Pause()
        {
            Enqueue(actions =>
            {
                if (_currentState != PlaybackState.NoFile)
                    _currentState = PlaybackState.Paused;
            });
        }

        public void Resume()
        {
            Enqueue(actions =>
            {
                if (_currentState == PlaybackState.NoFile) return;
                    _currentState = PlaybackState.Running;
            });
        }

        public void Step()
        {
            Enqueue(actions =>
            {
                if (_currentState == PlaybackState.NoFile) return;

                if (_broadcastStream.GetFrameInput(out var frame, out var input))
                {
                    _currentFrame = frame;
                    actions.Add(new SessionAdvanceFrameAction(frame, input));
                }

                _currentState = PlaybackState.Paused;
            });
        }

        public void GoToFrame(int frame)
        {
            Enqueue(actions =>
            {
                if (_currentState == PlaybackState.NoFile) return;

                _targetFrame = frame;
                if (_currentFrame > _targetFrame)
                {
                    actions.Add(new SessionLoadGameAction(0, _stateStorage));
                    _currentFrame = 0;
                }

                _broadcastStream.SetCurrentFrame(_currentFrame);

                while (_currentFrame < _targetFrame)
                {
                    if (!_broadcastStream.GetFrameInput(out var nextFrame, out var nextInput))
                        break;

                    _currentFrame = nextFrame;
                    actions.Add(new SessionAdvanceFrameAction(nextFrame, nextInput));
                }

                _currentState = PlaybackState.Paused;
                _targetFrame = -1;
            });
        }

        private void Enqueue(Action<List<SessionAction>> command) =>
            _commandQueue.Enqueue(command);

        public override List<SessionAction> AdvanceFrame(byte[] localInput = null)
        {
            var actions = new List<SessionAction>();

            while (_commandQueue.Count > 0)
            {
                var cmd = _commandQueue.Dequeue();
                cmd.Invoke(actions);
            }

            if (_currentState == PlaybackState.Running)
            {
                if (_broadcastStream.GetFrameInput(out var frame, out var input))
                {
                    _currentFrame = frame;
                    actions.Add(new SessionAdvanceFrameAction(frame, input));
                }
            }

            return actions;
        }

        protected internal override Device LocalDevice =>
            throw new NotImplementedException();

        protected internal override Device[] AllDevices =>
            throw new NotImplementedException();

        public override void AddRemoteDevice(uint deviceId, uint playerCount, object remoteConfiguration) =>
            throw new NotImplementedException();

        public override void AddSpectatorDevice(object remoteConfiguration) =>
            throw new NotImplementedException();

        public override uint AverageRollbackFrames() =>
            throw new NotImplementedException();

        public override int Frame() => _currentFrame;

        public override int FrameAdvantage() =>
            throw new NotImplementedException();

        public override int FrameAdvantageDifference() =>
            throw new NotImplementedException();

        public override bool IsRunning() =>
            _currentState != PlaybackState.NoFile;

        public override void Poll()
        {
            // No-op for replay
        }

        public override int RemoteFrame() =>
            throw new NotImplementedException();

        public override int RemoteFrameAdvantage() =>
            throw new NotImplementedException();

        public override uint RollbackFrames() =>
            throw new NotImplementedException();

        public override void SetLocalDevice(uint deviceId, uint playerCount, uint frameDelay) =>
            throw new NotImplementedException();

        public override int State() =>
            throw new NotImplementedException();

        protected internal override void AddRemoteInput(uint deviceId, DeviceInputMessage message) =>
            throw new NotImplementedException();

        protected internal override uint SendMessageTo(uint deviceId, DeviceMessage message) =>
            throw new NotImplementedException();

        public override void SaveToReplayFile() =>
            throw new NotImplementedException();
    }
}
