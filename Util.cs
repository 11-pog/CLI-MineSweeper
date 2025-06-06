using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLI_MineSweeper
{
    static class Util
    {
        public static Coordinates[] GetOffsetMap(NeighborSearchStyle searchStyle, int searchSize)
        {
            List<Coordinates> OffsetMap = [];

            for (int i = -searchSize; i <= searchSize; i++)
                for (int j = -searchSize; j <= searchSize; j++)
                {
                    if (i == 0 && j == 0) continue;

                    bool include = searchStyle switch
                    {
                        NeighborSearchStyle.SquareGrid => true,
                        NeighborSearchStyle.DiamondGrid => Math.Abs(i) + Math.Abs(j) <= searchSize,
                        NeighborSearchStyle.Radial => Math.Sqrt(i * i + j * j) <= searchSize + .5,
                        _ => false
                    };

                    if (include) OffsetMap.Add(new Coordinates(i, j));
                }
        ;

            return [.. OffsetMap];
        }
    }
}
