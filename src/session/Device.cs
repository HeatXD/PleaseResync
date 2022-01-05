using System.Diagnostics;

namespace PleaseResync
{
    public class Device
    {
        public enum DeviceType
        {
            Local,
            Remote,
            Spectator
        }
        public enum DeviceState
        {
            Verifying,
            Verified,
            Disconnected,
        }

        public readonly uint Id;
        public readonly uint PlayerCount;
        public readonly DeviceType Type;
        public readonly DeviceAdapter Adapter;

        public int RemoteFrame;
        public int RemoteFrameAdvantage;
        public DeviceState State;

        public Device(uint deviceId, uint playerCount, DeviceType deviceType, DeviceAdapter deviceAdapter)
        {
            Id = deviceId;
            Type = deviceType;
            Adapter = deviceAdapter;
            PlayerCount = playerCount;

            State = DeviceState.Verifying;
            RemoteFrame = 0;
            RemoteFrameAdvantage = 0;
        }

        public void DoPoll()
        {
            Adapter.Receive();
        }

        public void Verify()
        {
            Debug.Assert(Type == Device.DeviceType.Remote);
            Debug.Assert(Adapter != null);

            var message = new DeviceVerifyMessage { DeviceId = Id, PlayerCount = PlayerCount };

            State = DeviceState.Verifying;
            Adapter.Send(message);
        }
        public void VerifyConfirm(uint deviceId, uint playerCount)
        {
            Debug.Assert(Type == Device.DeviceType.Remote);
            Debug.Assert(Adapter != null);
        }
        public void VerifyConfirmed(uint deviceId, uint playerCount)
        {
            Debug.Assert(Type == Device.DeviceType.Remote);
            Debug.Assert(Adapter != null);

            State = DeviceState.Verified;
        }
    }
}
