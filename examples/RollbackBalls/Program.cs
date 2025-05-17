uint id = 0;
bool spectate = false;
bool idFound = false;

foreach (var arg in args)
{
    if (arg.StartsWith("id=", StringComparison.OrdinalIgnoreCase) && arg.Length > 3)
    {
        char c = arg[3];
        if (char.IsDigit(c))
            id = (uint)(c - '0');
        else
            id = c;
        idFound = true;
    }
    else if (arg.Equals("/s", StringComparison.OrdinalIgnoreCase))
    {
        spectate = true;
    }
}

if (!idFound)
{
    Console.WriteLine("Error: id argument is mandatory. Use id=X where X is a single character.");
    return;
}

Console.WriteLine($"ID:{id}, spec:{spectate}");
RollbackBalls.RollbackBalls.Init(id, spectate);
