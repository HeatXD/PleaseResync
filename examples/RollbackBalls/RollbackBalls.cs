using Raylib_cs;

namespace RollbackBalls
{
    internal class Game
    {
        public int[,] Position;

        public Game()
        {
            Position = new int[2, 2] { { 20000, 30000 }, { 40000, 30000 } };
        }

        public void Draw()
        {
            int players = Position.GetLength(0);
            for (int i = 0; i < players; i++)
            {
                Raylib.DrawCircle(
                    Position[i, 0] / 100,
                    Position[i, 1] / 100,
                    50, i == 0 ? Color.Orange : Color.Lime);
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
    }

    internal class RollbackBalls
    {
        public static void Init()
        {
            var game = new Game();

            Raylib.SetTargetFPS(60);
            Raylib.InitWindow(600, 600, "PleaseResync: RollbackBalls");

            while (!Raylib.WindowShouldClose())
            {
                game.Update([2, 8]);

                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                game.Draw();

                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
