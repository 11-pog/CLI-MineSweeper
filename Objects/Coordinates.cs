using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLI_MineSweeper.Utils;

namespace CLI_MineSweeper.Objects
{
    public struct Coordinates(int x, int y, MineSweeper? source = null) : IEquatable<Coordinates>
    {
        public int X { get; private set; } = x;
        public int Y { get; private set; } = y;
        public MineSweeper? Source { get; private set; } = source;

        public readonly Coordinates Offset(int dx, int dy) => new(X + dx, Y + dy);
        public void SetParent(MineSweeper parent) => Source ??= parent;

        public readonly bool Equals(Coordinates other) => other.X == X && other.Y == Y;
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Coordinates other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);
        public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);
        public static Coordinates operator +(Coordinates left, Coordinates right) => new(left.X + right.X, left.Y + right.Y, left.Source);

        public override readonly string ToString() => $"({X + 1}, {Y + 1})";

    }

    public static class CoordinateExtension
    {
        public static CellData GetData(this Coordinates src, int searchSize = 1, NeighborSearchStyle searchStyle = NeighborSearchStyle.SquareGrid)
        {
            if (src.Source is null) throw new InvalidOperationException("Coordinates object has no associated source.");

            byte OutOfBoundNeighbors = 0;
            byte RevealedNeighbors = 0;
            byte BombNeighbors = 0;

            src.Source.IterateNeighbor(src, (coords, _) =>
            {
                if (src.Source.IsInBounds(coords))
                {
                    if (src.Source[coords, Cell.isRevealed])
                    {
                        RevealedNeighbors++;
                    }

                    if (src.Source[coords, Cell.isBomb])
                    {
                        BombNeighbors++;
                    }
                }
                else
                {
                    OutOfBoundNeighbors++;
                }
            }, searchStyle: searchStyle, searchSize: searchSize,
            includeCenterCell: false, excludeOutOfBounds: false);

            return new CellData(OutOfBoundNeighbors, RevealedNeighbors, BombNeighbors);
        }
    }

}
