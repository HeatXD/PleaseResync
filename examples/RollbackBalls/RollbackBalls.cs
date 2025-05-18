using PleaseResync.session;
using PleaseResync.session.adapters;
using PleaseResync.session.backends;
using Raylib_cs;

namespace RollbackBalls
{
    public class Game
    {
        public int[,] Position;

        public Game()
        {
            Position = new int[2, 2] { { 20000, 30000 }, { 40000, 30000 } };
        }

        public void Draw(Session session)
        {
            Raylib.DrawFPS(500,0);
            Raylib.DrawText($"Frame: {session.Frame()}", 0, 550, 24, Color.White);

            int players = Position.GetLength(0);
            for (int i = 0; i < players; i++)
            {
                Raylib.DrawCircle(
                    Position[i, 0] / 100,
                    Position[i, 1] / 100,
                    25, i == 0 ? Color.Orange : Color.Lime);

                Raylib.DrawText(
                    $"{Position[i, 0]},{Position[i, 1]}",
                    0, i * 24, 24,
                    Color.White);
            }
        }

        public void Update(byte[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                // player movement
                if ((input[i] & (1 << 0)) != 0) Position[i, 0] -= 250;
                if ((input[i] & (1 << 1)) != 0) Position[i, 0] += 250;
                if ((input[i] & (1 << 2)) != 0) Position[i, 1] -= 250;
                if ((input[i] & (1 << 3)) != 0) Position[i, 1] += 250;

                // screen wrap
                if (Position[i, 0] < 0) Position[i, 0] = 60000;
                if (Position[i, 1] < 0) Position[i, 1] = 60000;
                if (Position[i, 0] > 60000) Position[i, 0] = 0;
                if (Position[i, 1] > 60000) Position[i, 1] = 0;
            }
        }

        public void Load(SessionLoadGameAction lGAction)
        {
            var state = lGAction.Load();
            // load positions from state
            for (var i = 0; i < 4; i++)
            {
                int val = BitConverter.ToInt32(state, i * 4);
                Position[i / 2, i % 2] = val;
            }
        }

        public void Save(SessionSaveGameAction sGAction)
        {
            // save positions
            var state = new[] 
            {
                Position[0, 0], Position[0, 1],
                Position[1, 0], Position[1, 1]
            }.SelectMany(BitConverter.GetBytes).ToArray();

            sGAction.Save(state);
        }
    }

    internal class RollbackBalls
    {
        private static uint LocalId = 0;
        private static bool Spectating = false;
        private static readonly string IpAddr = "127.0.0.1";
        private static readonly ushort[] PlayerPorts = { 7001, 7002 };
        private static readonly ushort[] SpectatorPorts = { 8001, 8002 };
        private static LiteNetLibSessionAdapter? NetAdapter;
        private static Session? Session;
        public static void Init(uint localId, bool spectating)
        {
            LocalId = localId;
            Spectating = spectating;

            if (!Spectating)
            {
                NetAdapter = new LiteNetLibSessionAdapter(IpAddr, PlayerPorts[LocalId]);
                Session = new Peer2PeerSession(sizeof(byte), 2, 2, false, NetAdapter);
                Session.SetLocalDevice(LocalId, 1, 0);

                for (uint i = 0; i < 2; i++)
                {
                    if (LocalId != i)
                    { 
                        // add remote players
                        Session.AddRemoteDevice(
                            i, 1, LiteNetLibSessionAdapter.CreateRemoteConfig(IpAddr, PlayerPorts[i]));
                    } 
                }

                if (LocalId == 0)
                {
                    // add spectators
                    for (uint i = 0; i < 2; i++)
                    {
                        Session.AddSpectatorDevice(
                            LiteNetLibSessionAdapter.CreateRemoteConfig(IpAddr, SpectatorPorts[i]));
                    }
                }

            }
            else
            {
                // spectate
                NetAdapter = new LiteNetLibSessionAdapter(IpAddr, SpectatorPorts[LocalId]);
                Session = new SpectatorSession(sizeof(byte), 2, NetAdapter);
                Session.AddRemoteDevice(0, 2, LiteNetLibSessionAdapter.CreateRemoteConfig(IpAddr, PlayerPorts[0]));
            }

            var game = new Game();

            Raylib.SetTargetFPS(60);
            Raylib.InitWindow(600, 600, $"PleaseResync: RollbackBalls {(spectating ? "Spectator" : "Player")}");

            while (!Raylib.WindowShouldClose())
            {
                Session.Poll();

                if (Session.IsRunning())
                {
                    byte localInput = GetLocalInput();

                    var actions = Session.AdvanceFrame([localInput]);

                    foreach (var action in actions)
                    {
                        switch (action)
                        {
                            case SessionAdvanceFrameAction AFAction:
                                game.Update(AFAction.Inputs);
                                break;
                            case SessionLoadGameAction LGAction:
                                game.Load(LGAction);
                                break;
                            case SessionSaveGameAction SGAction:
                                game.Save(SGAction);
                                break;
                        }
                    }
                }

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                game.Draw(Session);

                Raylib.EndDrawing();
            }

            NetAdapter.Close();
            Raylib.CloseWindow();
        }

        private static byte GetLocalInput()
        {
            byte input = 0;

            if (Raylib.IsKeyDown(KeyboardKey.Left)) input |= 1 << 0;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) input |= 1 << 1;
            if (Raylib.IsKeyDown(KeyboardKey.Up)) input |= 1 << 2;
            if (Raylib.IsKeyDown(KeyboardKey.Down)) input |= 1 << 3;

            return input;
        }
    }
}
