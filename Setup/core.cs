internal class SetupCore
  {
    protected readonly MineSweeper Parent;
    protected readonly Random rdn = new();
    internal SetupCore(MineSweeper parent)
    {
      this.Parent = parent;
    }


    protected void RandomizeCell((byte, byte) Coords, float BombChance)
    {
      byte y = Coords.Item1;
      byte x = Coords.Item2;

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


    protected void BombNeighborsInRdnRange((byte, byte) Coords, (byte, byte) Range, bool? BombState = null, bool is5x5 = false)
    {
      byte amount = (byte)rdn.Next(Range.Item1, Range.Item2 + 1);

      byte startingPoint = (byte)rdn.Next(0, (is5x5 ? 17 : 9) - Range.Item2);
      byte stoppingPoint = (byte)(startingPoint + amount);

      Parent.IterateNeighbor(Coords, (y, x, n) =>
      {
        if (startingPoint <= n && n <= stoppingPoint)
        {
          Parent[y, x, Cell.isBomb] = BombState ?? rdn.NextSingle() > 0.5f;
        }
      }, is5x5: is5x5);
    }
  }