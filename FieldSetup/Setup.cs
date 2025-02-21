internal class SetupMethods
  {
    protected readonly MineSweeper parent;
    internal readonly Random rdn = new();
    internal SetupMethods(MineSweeper parent)
    {
      this.parent = parent;
    }


    internal void SingleRandom((byte, byte) Coords, float chance)
    {
      byte y = Coords.Item1;
      byte x = Coords.Item2;

      parent[y, x, MineSweeper.isRevealed] = true;
      parent[y, x, MineSweeper.isFlagged] = false;

      if (rdn.NextSingle() <= chance)
      {
        parent[y, x, MineSweeper.isBomb] = true;
      }
      else
      {
        parent[y, x, MineSweeper.isBomb] = false;
      }
    }


    internal void Random(byte chance)
    {
      float fChance = 1 / chance;

      parent.IterateAllCells((y, x) =>
      {
        SingleRandom((y, x), fChance);
      });
    }


    private void BombNeighborsInRdnRange((byte, byte) Coords, (byte, byte) Range, bool? BombState = null, bool is5x5 = false)
    {
      byte amount = (byte)rdn.Next(Range.Item1, Range.Item2 + 1);

      byte startingPoint = (byte)rdn.Next(0, (is5x5 ? 17 : 9) - Range.Item2);
      byte stoppingPoint = (byte)(startingPoint + amount);

      parent.IterateNeighbor(Coords, (y, x, n) =>
      {
        if (startingPoint <= n && n <= stoppingPoint)
        {
          parent[y, x, MineSweeper.isBomb] = BombState ?? rdn.NextSingle() > 0.5f;
        }
      }, is5x5: is5x5);
    }

    internal void ConditionCollection()
    {
      parent.IterateAllCells((y, x) =>
      {
        (byte, byte, byte) CellData3x3 = parent.GetCellData((y, x));
        (byte, byte, byte) CellData5x5 = parent.GetDataAllIn5x5((y, x));

        byte NeighborBombs3x3 = CellData3x3.Item1;
        byte OutOfBoundNeighbors3x3 = CellData3x3.Item2;

        byte NeighborBombs5x5 = CellData5x5.Item1;
        byte OutOfBoundNeighbors5x5 = CellData5x5.Item2;

        bool isEdge = OutOfBoundNeighbors3x3 == 3;
        bool isCorner = OutOfBoundNeighbors3x3 == 5;


        if (parent[y, x, MineSweeper.isBomb] && (NeighborBombs3x3 == 8
              || (NeighborBombs3x3 == 5 && isEdge)
              || (NeighborBombs3x3 == 3 && isCorner)))
        {
          parent[y, x, MineSweeper.isBomb] = false;
        }


        if ((isEdge && rdn.NextSingle() <= 0.2f)
              || (isCorner && rdn.NextSingle() <= 0.4f))
        {
          parent[y, x, MineSweeper.isBomb] = false;
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
          parent.IterateNeighbor((y, x), (y, x, n) =>
          {
            if (rdn.Next(1, 17) == n)
            {
              BombNeighborsInRdnRange((y, x), (2, 8), is5x5: false);
            }
          }, is5x5: true);
        }
      });
    }


    internal void Conditional(byte chance)
    {
      Random(chance);

      for (byte i = 0; i < 3; i++)
      {
        ConditionCollection();
      }
    }


    internal void NoGuessing(float Mod = 1.0f)
    {
      //PlaceHolder
      //Literalmente a mais complicada de todas
    }

    //Necessito de ideias pra setups diferentes
    internal ConcentrationBased Concentration => new(this, parent);
  }