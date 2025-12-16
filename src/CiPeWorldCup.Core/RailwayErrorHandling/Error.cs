namespace CiPeWorldCup.Core.RailwayErrorHandling;

public readonly record struct Error(string Code, string Message)
{
    public static Error None => new Error();
    public override string ToString() => $"{Code}: {Message}";
}
