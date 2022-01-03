using System.Diagnostics;

namespace PleaseResync
{
    /// <summary>
    /// Session is responsible for managing a pool of devices wanting to play your game together.
    /// Each device must create a Session on their end and make sure all devices IDs, numbers of players for each device, are the same on every device.
    /// <br />
    /// <listheader>Terminology</listheader>
    /// <list type="bullet">
    ///     <item>Frame: a single update of your game logic, to simulate correctly a frame of your game, you will need all player inputs for that frame.</item>
    ///     <item>Device: a device where your game runs (computer, xbox, ...) where one or more players can play your game.</item>
    ///     <item>Player: a human (or AI) on a device producing inputs to change the state of the game.</item>
    ///     <item>DeviceID: an unique number used to uniquely identify a device, starting at zero (eg. 0 = John's Xbox, 1 = Tom's Switch)</item>
    ///     <item>Rollback: while the Session is waiting for remote device inputs, it may use placeholder inputs for that your game can continue, but if the remote player inputs are different, we need to resimulate the game from where we left with the correct inputs: that's a rollback</item>
    ///     <item>PlayerInput: a piece of information describing how a player would like to change the state of the game for a given frame. (eg. left = false, right = true, jumping = true)</item>
    ///     <item>PlayerCount: a number of players on a given device, usually one for most of the games, but can be greater for a game with multiple local players (eg. splitscreen).</item>
    /// </list>
    /// </summary>
    public abstract class Session
    {
        public const int LIMIT_DEVICE_COUNT = 4;
        public const int LIMIT_TOTAL_PLAYER_COUNT = 16;

        /// <summary>
        /// DeviceCount is the number of devices taking part in this session.
        /// </summary>
        protected readonly uint DeviceCount;
        /// <summary>
        /// TotalPlayerCount is the total number of players accross all devices taking part in this session.
        /// </summary>
        protected readonly uint TotalPlayerCount;

        /// <param name="deviceCount">The number of devices taking part in this session.</param>
        /// <param name="totalPlayerCount">The total number of players accross all devices taking part in this session.</param>
        public Session(uint deviceCount, uint totalPlayerCount)
        {
            Debug.Assert(deviceCount >= 2);
            Debug.Assert(deviceCount <= LIMIT_DEVICE_COUNT);
            Debug.Assert(totalPlayerCount >= 2);
            Debug.Assert(totalPlayerCount <= LIMIT_TOTAL_PLAYER_COUNT);

            DeviceCount = deviceCount;
            TotalPlayerCount = totalPlayerCount;
        }

        /// <summary>
        /// AddLocalDevice tells that the given device is local to this Session and hosts the given player count.
        /// </summary>
        /// <param name="deviceId">Unique number used to identify this local device. this number must be exactly the same in every Sessions for that particular device</param>
        /// <param name="playerCount">Number of players playing on this device. this number must be exactly the same in every Sessions for that particular device</param>
        /// <param name="frameDelay">Number of frames to skip before registering local input, used to avoid rollbacking every frame.</param>
        public abstract void AddLocalDevice(int deviceId, uint playerCount, uint frameDelay);
        /// <summary>
        /// AddRemoteDevice tells that the given device is remote to this Session and hosts the given player count.
        /// </summary>
        /// <param name="deviceId">Unique number used to identify this local device. this number must be exactly the same in every Sessions for that particular device</param>
        /// <param name="playerCount">Number of players playing on this device. this number must be exactly the same in every Sessions for that particular device</param>
        /// <param name="networkAdapter">As the given device is not local to the Session, we must provide a way to communicate with that given device</param>
        public abstract void AddRemoteDevice(int deviceId, uint playerCount, object networkAdapter);

        /// <summary>
        /// SetFrameInputs sets this local device inputs for the current frame + frameDelay.
        /// This must be called before calling GetFrameInputs()
        /// </summary>
        /// <param name="inputs">inputs gathered from this device for all of its local players</param>
        public abstract void SetFrameInputs(byte[] input);
        /// <summary>
        /// GetFrameInputs returns all inputs from all devices and players for the current frame.
        /// Sometimes, the inputs of one or more remote devices won't be available for this frame, but this is not a problem since
        /// the session will synchronize them later on and trigger a rollback to correct your game state with the newly arrived inputs.
        /// </summary>
        /// <returns>inputs ordered by deviceID asc</returns>
        public abstract byte[] GetFrameInputs();
        /// <summary>
        /// AdvanceFrame will tell the session to increment the current frame by one and that you are ready to work on the next frame.
        /// This must be called after you set your local inputs for this frame and simulated your game frame with the inputs provided by the session.
        /// </summary>
        /// <returns>an array of actions to perform in order before calling AdvanceFrame again</returns>
        public abstract SessionAction[] AdvanceFrame();
    }
}
