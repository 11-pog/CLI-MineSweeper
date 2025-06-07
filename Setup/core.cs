using CLI_MineSweeper.Objects;
using CLI_MineSweeper.Utils;

namespace CLI_MineSweeper
{
    public abstract class SetupCore
    {
        protected readonly MineSweeper Parent;
        protected readonly Random rdn = new();
        internal SetupCore(MineSweeper parent)
        {
            this.Parent = parent;
        }

        // public abstract void SetField();

        protected void RandomizeCell(Coordinates Coords, float BombChance)
        {
            int y = Coords.Y;
            int x = Coords.X;

            Parent[y, x, Cell.isRevealed] = false;
            Parent[y, x, Cell.isFlagged] = false;

            if (rdn.NextSingle() <= BombChance)
            {
                Parent[y, x, Cell.isBomb] = true;
            }
            else
            {
                Parent[y, x, Cell.isBomb] = false;
            }
        }


        protected void BombNeighborsInRdnRange(Coordinates Coords, NumRange<int> range, bool? BombState = null, NeighborSearchStyle 
            searchStyle = NeighborSearchStyle.SquareGrid, int searchSize = 1)
        {
            byte amount = (byte)rdn.Next(range.Start, range.End + 1);
            int SearchPatternSize = Matrix2Utils.GetSearchPatternSize(searchStyle, searchSize);

            byte startingPoint = (byte)rdn.Next(0, SearchPatternSize - range.End);
            byte stoppingPoint = (byte)(startingPoint + amount);

            Parent.IterateNeighbor(Coords, (coords, n) =>
            {
                if (startingPoint <= n && n <= stoppingPoint)
                {
                    Parent[coords, Cell.isBomb] = BombState ?? rdn.NextSingle() > 0.5f;
                }
            }, searchStyle, searchSize);
        }
    }
}