using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLI_MineSweeper.Objects;

namespace CLI_MineSweeper.Utils
{

    static class Util
    {
        

        public static void Clear()
        {
            try
            {
                Console.Clear();
            }
            catch
            {
                for (int i = 0; i < 50; i++)
                {
                    Console.WriteLine('\n');
                }
            }
        } 
    }

    public static class StringUtils
    {
        public static int FindCodeSwitchIndex(string code, CharType? firstChar = null)
        {
            firstChar ??= code[0].GetCharType();

            for (int i = 0; i < code.Length; i++)
            {
                bool changed = firstChar.Value switch
                {
                    CharType.Number => char.IsLetter(code[i]),
                    CharType.Letter => char.IsNumber(code[i]),
                    _ => false
                };

                if (changed) return i;
            }

            return -1;
        }

        public static int GetNumberFromLetters(string letters)
        {
            letters = letters.Trim().ToLower();

            int length = letters.Length - 1;
            int sum = 0;

            for (int i = 0; i <= length; i++)
            {
                sum += (letters[length - i] - 97) * (int)Math.Pow(26, i);
            }

            return sum;
        }
    }

    public static class Extensions
    {
        public static bool IsAllDigit(this string src) => src.All(letter => char.IsDigit(letter));
        public static bool IsAllLetter(this string src) => src.All(letter => char.IsLetter(letter));
        public static CharType GetCharType(this char src)
        {
            if (char.IsLetter(src)) return CharType.Letter;
            else if (char.IsNumber(src)) return CharType.Number;
            else return CharType.Other;
        }
    }

    public static class Matrix2Utils
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
                        NeighborSearchStyle.SquareEdge => i == -searchSize || i == searchSize || j == -searchSize || j == searchSize,
                        NeighborSearchStyle.DiamondGrid => Math.Abs(i) + Math.Abs(j) <= searchSize,
                        NeighborSearchStyle.DiamondEdge => Math.Abs(i) + Math.Abs(j) == searchSize,
                        NeighborSearchStyle.InverseDiamondGrid => Math.Abs(i) + Math.Abs(j) >= searchSize,
                        NeighborSearchStyle.Radial => Math.Sqrt(i * i + j * j) <= searchSize + .5,
                        NeighborSearchStyle.InverseRadial => Math.Sqrt(i * i + j * j) >= searchSize + .5,
                        _ => false
                    };

                    if (include) OffsetMap.Add(new Coordinates(i, j));
                }
        ;

            return [.. OffsetMap];
        }

        public static int GetSearchPatternSize(NeighborSearchStyle searchStyle, int searchSize)
        {
            Coordinates[] map = GetOffsetMap(searchStyle, searchSize);
            return map.Length;
        }
    }

    public enum NeighborSearchStyle
    {
        SquareGrid,
        SquareEdge,
        DiamondGrid,
        DiamondEdge,
        InverseDiamondGrid,
        Radial,
        InverseRadial,
    }

    public enum CharType
    {
        Number,
        Letter,
        Other
    }
}
