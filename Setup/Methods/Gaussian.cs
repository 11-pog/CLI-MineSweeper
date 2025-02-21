internal class Gaussian : SetupCore
    {
      internal Gaussian(MineSweeper Field) : base(Field) {}

      private Queue<(byte, byte)> GetRandomPoints((byte, byte) PointsAmountRdnRange)
      {
        Queue<(byte, byte)> values = new();
        byte pointsAmount = (byte)rdn.Next(PointsAmountRdnRange.Item1, PointsAmountRdnRange.Item2);

        for (byte r = 0; r <= pointsAmount; r++)
        {
          values.Enqueue(((byte, byte))(rdn.Next(0, parent.height), rdn.Next(0, parent.width)));
        }

        return values;
      }


      private static void TestDisplay(float ToDisplay, ref byte? reference, byte y)
      {
        if (reference != y)
        {
          Console.Write("\n");
          reference = y;
          if (y < 9)
          {
            Console.Write(" ");
          }
          Console.Write(y + 1 + " -> ");
        }

        Console.Write($"{ToDisplay:F2} ");
      }


      private static float GetGaussian((byte, byte) Coords, (byte, byte) Center, float mod)
      {
        byte y = Coords.Item1;
        byte x = Coords.Item2;

        byte CenterY = Center.Item1;
        byte CenterX = Center.Item2;

        return (float)Math.Exp(-(Math.Pow(x - CenterX, 2) / (2 * mod * mod) + Math.Pow(y - CenterY, 2) / (2 * mod * mod)));
      }


      internal void SetField((float, float)? mod = null)
      {
        mod ??= (1.2f, 1.6f);
        byte? CurrentY = null;

        byte PointsAmount = (byte)((parent.height + parent.width) / 2);
        Queue<(byte, byte)> Points = GetRandomPoints(((byte, byte))(PointsAmount, PointsAmount + 5));

        float[,] AccumulatedConcentrations = new float[parent.height, parent.width];

        foreach (var Point in Points)
        {
          float RandomMod = (float)((rdn.NextDouble() * (mod.Value.Item2 - mod.Value.Item1)) + mod.Value.Item1);

          parent.IterateAllCells((y, x) =>
          {
            float Concentration = GetGaussian((y, x), Point, RandomMod);

            AccumulatedConcentrations[y, x] += Concentration;
            TestDisplay(Concentration, ref CurrentY, y);
          });

          Console.Write('\n');
        }

        CurrentY = null;

        parent.IterateAllCells((y, x) =>
        {
          AccumulatedConcentrations[y, x] = Math.Clamp(AccumulatedConcentrations[y, x] / 2, 0, 1);
          TestDisplay(AccumulatedConcentrations[y, x], ref CurrentY, y);
          RandomizeCell((y, x), AccumulatedConcentrations[y, x]);
        });
      }
    }
