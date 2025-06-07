

using CLI_MineSweeper.Objects;

namespace CLI_MineSweeper
{

    public class Gaussian : SetupCore
    {
        internal Gaussian(MineSweeper Field) : base(Field) { }

        private Queue<Coordinates> GetRandomPoints(NumRange<int> PointsAmountRdnRange)
        {
            Queue<Coordinates> values = new();
            byte pointsAmount = (byte)rdn.Next(PointsAmountRdnRange.Start, PointsAmountRdnRange.End);

            for (byte r = 0; r <= pointsAmount; r++)
            {
                values.Enqueue(new Coordinates(rdn.Next(0, Parent.Width), rdn.Next(0, Parent.Height)));
            }

            return values;
        }


        private static void TestDisplay(float ToDisplay, ref int? reference, int y)
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


        private static float GetGaussian(Coordinates coords, Coordinates Center, float mod)
        {
            int y = coords.Y;
            int x = coords.X;

            int CenterY = Center.Y;
            int CenterX = Center.X;

            return (float)Math.Exp(-(Math.Pow(x - CenterX, 2)
                / (2 * mod * mod) + Math.Pow(y - CenterY, 2)
                / (2 * mod * mod)));
        }


        internal void SetField(NumRange<float>? mod = null)
        {
            mod ??= new NumRange<float>(1.2f, 1.6f);
            int? CurrentY = null;

            int PointsAmount = ((Parent.Height + Parent.Width) / 2);
            Queue<Coordinates> Points = GetRandomPoints(new NumRange<int>(PointsAmount, PointsAmount + 5));

            float[,] AccumulatedConcentrations = new float[Parent.Height, Parent.Width];

            foreach (Coordinates Point in Points)
            {
                float RandomMod = (float)((rdn.NextDouble() * (mod.Value.End - mod.Value.Start)) + mod.Value.Start);

                Parent.IterateAllCells(coords =>
                {
                    float Concentration = GetGaussian(coords, Point, RandomMod);

                    AccumulatedConcentrations[coords.Y, coords.X] += Concentration;
                    TestDisplay(Concentration, ref CurrentY, coords.Y);
                });

                Console.Write('\n');
            }

            CurrentY = null;

            Parent.IterateAllCells((coords) =>
            {
                int y = coords.Y;
                int x = coords.X;

                AccumulatedConcentrations[y, x] = Math.Clamp(AccumulatedConcentrations[y, x] / 2, 0, 1);
                TestDisplay(AccumulatedConcentrations[y, x], ref CurrentY, y);
                RandomizeCell(coords, AccumulatedConcentrations[y, x]);
            });
        }
    }
}