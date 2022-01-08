using System;
using System.Diagnostics;
using MessagePack;

namespace PleaseResync
{
    [Union(0, typeof(DeviceSyncMessage))]
    [Union(1, typeof(DeviceSyncConfirmMessage))]
    [Union(2, typeof(DeviceInputMessage))]
    [MessagePackObject]
    public abstract class DeviceMessage
    {
        [Key(0)]
        public uint SequenceNumber;
    }

    [MessagePackObject]
    public class DeviceSyncMessage : DeviceMessage
    {
        [Key(1)]
        public uint DeviceId;
        [Key(2)]
        public uint PlayerCount;
        [Key(3)]
        public uint RandomRequest;

        public override string ToString() { return $"{typeof(DeviceSyncMessage)}: {new { DeviceId, PlayerCount, RandomRequest }}"; }
    }

    [MessagePackObject]
    public class DeviceSyncConfirmMessage : DeviceMessage
    {
        [Key(1)]
        public uint DeviceId;
        [Key(2)]
        public uint PlayerCount;
        [Key(3)]
        public uint RandomResponse;

        public override string ToString() { return $"{typeof(DeviceSyncConfirmMessage)}: {new { DeviceId, PlayerCount, RandomResponse }}"; }
    }

    [MessagePackObject]
    public class DeviceInputMessage : DeviceMessage
    {
        [Key(1)]
        public uint Frame;
        [Key(2)]
        public byte[] Input;
    }
}
