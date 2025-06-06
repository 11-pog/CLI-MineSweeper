using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI_MineSweeper
{
    public struct Coordinates(int x, int y, MineSweeper? source = null) : IEquatable<Coordinates>
    {
        public int X { get; private set; } = x;
        public int Y { get; private set; } = y;
        public readonly MineSweeper? source = source;

        public readonly Coordinates Offset(int dx, int dy) => new(X + dx, Y + dy);

        public readonly bool Equals(Coordinates other) => other.X == X && other.Y == Y;
        public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Coordinates other && Equals(other);

        public override readonly int GetHashCode() => HashCode.Combine(X, Y);

        public static bool operator ==(Coordinates left, Coordinates right) => left.Equals(right);
        public static bool operator !=(Coordinates left, Coordinates right) => !left.Equals(right);
        public static Coordinates operator +(Coordinates left, Coordinates right) => new(left.X + right.X, left.Y + right.Y);
    }

    public static class CoordinateExtension
    {
        public static CellData GetData(this Coordinates src, int searchSize = 1, NeighborSearchStyle searchStyle = NeighborSearchStyle.SquareGrid)
        {
            byte OutOfBoundNeighbors = 0;
            byte RevealedNeighbors = 0;
            byte BombNeighbors = 0;

            if (src.source is null) throw new InvalidOperationException("Coordinates object has no associated source.");
            
            src.source.IterateNeighbor(src, (coords, _) =>
            {
                if (src.source.IsInBounds(coords))
                {
                    if (src.source[coords, Cell.isRevealed])
                    {
                        RevealedNeighbors++;
                    }

                    if (src.source[coords, Cell.isBomb])
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

            return new CellData(BombNeighbors, OutOfBoundNeighbors, RevealedNeighbors);
        }
    }

}
