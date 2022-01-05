using MessagePack;

namespace PleaseResync
{
    [Union(0, typeof(DeviceVerifyMessage))]
    [Union(1, typeof(DeviceVerifyConfirmMessage))]
    [MessagePackObject]
    public abstract class DeviceMessage
    {
        [Key(0)]
        public uint SequenceNumber;
    }

    [MessagePackObject]
    public class DeviceVerifyMessage : DeviceMessage
    {
        [Key(1)]
        public uint DeviceId;
        [Key(2)]
        public uint PlayerCount;

        public override string ToString() { return $"DeviceVerifyMessage(DeviceId: {DeviceId}, PlayerCount: {PlayerCount})"; }
    }

    [MessagePackObject]
    public class DeviceVerifyConfirmMessage : DeviceMessage
    {
        [Key(1)]
        public uint DeviceId;
        [Key(2)]
        public uint PlayerCount;

        public override string ToString() { return $"DeviceVerifyConfirmMessage(DeviceId: {DeviceId}, PlayerCount: {PlayerCount})"; }
    }
}
