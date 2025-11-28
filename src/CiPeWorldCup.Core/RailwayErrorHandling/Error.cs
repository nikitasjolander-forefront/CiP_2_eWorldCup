using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CiPeWorldCup.Core.RailwayErrorHandling;

public readonly record struct Error(string Code, string Message)
{
    public static Error None => new Error();
    public override string ToString() => $"{Code}: {Message}";
}
