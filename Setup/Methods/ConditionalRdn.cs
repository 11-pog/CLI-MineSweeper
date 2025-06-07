
using CLI_MineSweeper.Objects;
using CLI_MineSweeper.Utils;

namespace CLI_MineSweeper
{
    public class ConditionalRandom : SetupCore
    {
        General gen = new(new MineSweeper(1, 1));
        internal ConditionalRandom(MineSweeper Field) : base(Field)
        {
            gen = new General(Field);
        }

        internal void ConditionCollection()
        {
            Parent.IterateAllCells(coords =>
            {
                CellData Cell3x3 = coords.GetData(searchSize: 1, searchStyle: NeighborSearchStyle.SquareGrid);
                CellData Cell5x5 = coords.GetData(searchSize: 2, searchStyle: NeighborSearchStyle.SquareGrid);

                bool isEdge = Cell3x3.OutOfBoundNeighbors == 3;
                bool isCorner = Cell3x3.OutOfBoundNeighbors == 5;


                if (Parent[coords, Cell.isBomb] && Cell3x3.BombNeighbors + Cell3x3.OutOfBoundNeighbors == 8)
                    Parent[coords, Cell.isBomb] = false;


                if ((isEdge && rdn.NextSingle() <= 0.2f)
                || (isCorner && rdn.NextSingle() <= 0.4f))
                    Parent[coords, Cell.isBomb] = false;


                if ((Cell3x3.BombNeighbors == 7 && rdn.NextSingle() <= 0.4f)
                || (Cell3x3.BombNeighbors == 8 && rdn.NextSingle() <= 0.9f))
                    BombNeighborsInRdnRange(coords, new NumRange<int>(1, 5), false);


                if (Cell5x5.BombNeighbors == 0 && rdn.NextSingle() <= 0.21f)
                {
                    BombNeighborsInRdnRange(coords, new NumRange<int>(2, 7));

                    if (rdn.NextSingle() <= 0.38f)
                        BombNeighborsInRdnRange(coords, new NumRange<int>(5, 12), searchSize: 2);
                }


                if (Cell5x5.BombNeighbors >= 14 && rdn.NextSingle() <= 0.26f)
                    Parent.IterateNeighbor(coords, (neighborCoords, n) =>
                    {
                        if (rdn.Next(1, 17) == n)
                            BombNeighborsInRdnRange(neighborCoords, new NumRange<int>(2, 8));
                    }, searchSize: 2);
            });
        }


        internal void SetField(byte chance)
        {
            gen.Randomize(chance);

            for (byte i = 0; i < 3; i++)
            {
                ConditionCollection();
            }
        }
    }
}