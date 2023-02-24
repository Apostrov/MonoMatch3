using System;
using MonoMatch3.Code;

internal static class Program
{
    [STAThread]
    internal static void Main(string[] args)
    {
        using var game = new Match3Game();
        game.Run();
    }
}