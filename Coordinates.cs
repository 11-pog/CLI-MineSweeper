using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI_MineSweeper
{
    public struct Coordinates(int x, int y) : IEquatable<Coordinates>
    {
        public int X { get; private set; } = x;
        public int Y { get; private set; } = y;

        public Coordinates Offset(int dx, int dy) => new(X + dx, Y + dy);

        public readonly bool Equals(Coordinates other) => other.X == X && other.Y == Y;
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Coordinates other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);
        public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);
        public static Coordinates operator +(Coordinates left, Coordinates right) => new(left.X + right.X, left.Y + right.Y);
    }
}
