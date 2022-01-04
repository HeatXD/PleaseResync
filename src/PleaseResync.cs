namespace PleaseResync
{
    public static class PleaseResync
    {
        public static Peer2PeerSession CreatePeer2PeerSession(uint inputSize, uint deviceCount, uint totalPlayerCount)
        {
            return new Peer2PeerSession(inputSize, deviceCount, totalPlayerCount);
        }
    }
}
