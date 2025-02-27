internal class ConditionalRandom : SetupCore
  {
    General gen = new(new MineSweeper(1, 1));
    internal ConditionalRandom(MineSweeper Field) : base(Field)
    {
      gen = new General(Field);
    }

    internal void ConditionCollection()
    {
      Parent.IterateAllCells((y, x) =>
      {
        (byte, byte, byte) CellData3x3 = Parent.GetCellData((y, x));
        (byte, byte, byte) CellData5x5 = Parent.GetDataAllIn5x5((y, x));

        byte NeighborBombs3x3 = CellData3x3.Item1;
        byte OutOfBoundNeighbors3x3 = CellData3x3.Item2;

        byte NeighborBombs5x5 = CellData5x5.Item1;
        byte OutOfBoundNeighbors5x5 = CellData5x5.Item2;

        bool isEdge = OutOfBoundNeighbors3x3 == 3;
        bool isCorner = OutOfBoundNeighbors3x3 == 5;


        if (Parent[y, x, Cell.isBomb] && (NeighborBombs3x3 == 8
              || (NeighborBombs3x3 == 5 && isEdge)
              || (NeighborBombs3x3 == 3 && isCorner)))
        {
          Parent[y, x, Cell.isBomb] = false;
        }


        if ((isEdge && rdn.NextSingle() <= 0.2f)
          || 
           (isCorner && rdn.NextSingle() <= 0.4f))
        {
          Parent[y, x, Cell.isBomb] = false;
        }


        if ((NeighborBombs3x3 == 7 && rdn.NextSingle() <= 0.4f) || (NeighborBombs3x3 == 8 && rdn.NextSingle() <= 0.9f))
        {
          BombNeighborsInRdnRange((y, x), (1, 5), false);
        }


        if (NeighborBombs5x5 == 0 && rdn.NextSingle() <= 0.21f)
        {
          BombNeighborsInRdnRange((y, x), (2, 7), is5x5: false);

          if (rdn.NextSingle() <= 0.38f)
          {
            BombNeighborsInRdnRange((y, x), (5, 12), is5x5: true);
          }
        }


        if (NeighborBombs5x5 >= 14 && rdn.NextSingle() <= 0.26f)
        {
          Parent.IterateNeighbor((y, x), (y, x, n) =>
          {
            if (rdn.Next(1, 17) == n)
            {
              BombNeighborsInRdnRange((y, x), (2, 8), is5x5: false);
            }
          }, is5x5: true);
        }
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