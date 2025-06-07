using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CLI_MineSweeper.Objects
{
    public struct CellData(byte outOfBoundNeighbor, byte revealedNeighbors, byte bombNeighbors)
    {
        public Coordinates cell { get; private set; }

        public byte OutOfBoundNeighbors { get; private set; } = outOfBoundNeighbor;
        public byte RevealedNeighbors { get; private set; } = revealedNeighbors;
        public byte BombNeighbors { get; private set; } = bombNeighbors;
    }
}
