using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CLI_MineSweeper.Objects
{
    public readonly struct NumRange<T>(T min, T max) where T : INumber<T>
    {
        public T Start { get; } = min;
        public T End { get; } = max;

        public readonly T Length => End - Start;
    }
}
