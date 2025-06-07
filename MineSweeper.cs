using System.Diagnostics;

namespace CLI_MineSweeper
{
    public class MineSweeper
    {
        public int Height { get; private set; }
        public int Width { get; private set; }

        private bool[,,] field;
        internal bool GameEnded = false;

        internal MineSweeper(int height, int width)
        {
            field = new bool[height, width, 3];
            //bombas[y,x,0] = Revelado ou não
            //bombas[y,x,1] = Flagado ou não
            //bombas[y,x,2] = Bomba ou não

            this.Height = height;
            this.Width = width;
        }


        internal bool this[int y, int x, Cell key]
        {
            get => field[y, x, (int)key];
            set => field[y, x, (int)key] = value;
        }
        internal bool this[Coordinates coords, Cell key]
        {
            get => field[coords.Y, coords.X, (int)key];
            set => field[coords.Y, coords.X, (int)key] = value;
        }

        public Coordinates GetCoordinates(int X, int Y) => new(X, Y, this);

        internal void IterateAllCells(Action<Coordinates> onCell, Action<int>? onRowStart = null, Action<int>? onRowEnd = null)
        {
            for (int y = 0; y < Height; y++)
            {
                if (onRowStart is not null) onRowStart(y);
                for (int x = 0; x < Width; x++)
                {
                    onCell(GetCoordinates(x, y));
                }
                if (onRowEnd is not null) onRowEnd(y);
            }
        }


        internal void IterateNeighbor(Coordinates coords, Action<Coordinates, int> action,
            NeighborSearchStyle searchStyle = NeighborSearchStyle.SquareGrid,
            int searchSize = 1, bool includeCenterCell = true,
            bool excludeOutOfBounds = true)
        {
            Coordinates[] OffsetMap = Util.GetOffsetMap(searchStyle, searchSize);
            int n = 0;

            if (includeCenterCell) action(coords, -1);

            foreach (Coordinates offset in OffsetMap)
            {
                Coordinates OffsetCoords = coords + offset;

                if (IsInBounds(OffsetCoords) || !excludeOutOfBounds)
                {
                    action(OffsetCoords, n);
                }

                n++;
            }
        }


        internal bool IsInBounds(Coordinates coords) => IsInBounds(coords.Y, coords.X);
        internal bool IsInBounds(int Y, int X) => Y < Height && X < Width;


        internal void Display()
        {
            Util.Clear();
            Console.Write("\n   ");

            for (byte x = 0; x < Width; x++)
                Console.Write((char)(x + 65) + " ");

            Console.Write("\n");

            IterateAllCells(cell =>
            {
                if (!this[cell, Cell.isRevealed])
                    if (this[cell, Cell.isFlagged])
                        Console.Write("# ");
                    else
                        Console.Write("O ");
                else if (this[cell, Cell.isBomb])
                    Console.Write("* ");

                else
                {
                    byte BombNeighbors = cell.GetData().BombNeighbors;
                    if (BombNeighbors > 0)
                        Console.Write(BombNeighbors + " ");
                    else
                        Console.Write("  ");
                }
            }, row =>
            {
                if (row < 9) Console.Write(" ");

                Console.Write($"{row + 1}|");
            }, _ => Console.Write("\n"));
        }


        private bool CanReveal(Coordinates coords, Action? action = null)
        {
            CellData data = coords.GetData();

            bool hasNeighborBomb = data.BombNeighbors != 0;

            if (!hasNeighborBomb && this[coords, Cell.isRevealed] == true &&
                data.RevealedNeighbors + data.OutOfBoundNeighbors > 8)
            {
                if (action is not null) action();
                return true;
            }
            return false;
        }


        private Queue<Coordinates>? ProcessCell(out bool outStatus, bool enqueue = false)
        {
            bool hasRevealed = false;
            Queue<Coordinates>? QueuedCoords = enqueue ? new Queue<Coordinates>() : null;

            IterateAllCells(coords =>
            {
                CanReveal(coords, () =>
            {
                hasRevealed = true;

                IterateNeighbor(coords, Expose, includeCenterCell: false);
            });
            });

            void Expose(Coordinates coords, int _)
            {
                this[coords, Cell.isRevealed] = true;

                if (enqueue) QueuedCoords!.Enqueue(coords);
            }

            outStatus = hasRevealed;

            if (enqueue) return QueuedCoords;
            return null;
        }


        internal void FloodFillBFS(Coordinates? startingPoint = null)
        {
            Queue<Coordinates> queue = new();

            if (startingPoint is null)
                queue = ProcessCell(out _, true) ?? new Queue<Coordinates>();
            else
                queue.Enqueue(startingPoint.Value);

            while (queue.Count > 0)
            {
                Coordinates currentCoords = queue.Dequeue();

                if (CanReveal(currentCoords))
                {
                    IterateNeighbor(currentCoords, (coords, _) =>
                    {
                        if (!this[coords, Cell.isRevealed])
                        {
                            this[coords, Cell.isRevealed] = true;
                            queue.Enqueue(coords);
                        }
                    }, includeCenterCell: false);
                }
            }
        }


        internal void FloodFillLoop()
        {
            bool repeat;

            do
            {
                _ = ProcessCell(out repeat);
            } while (repeat);
        }




        internal void CheckForGameOver()
        {
            IterateAllCells(coords =>
            {
                if (this[coords, Cell.isRevealed] && this[coords, Cell.isBomb])
                {
                    GameEnded = true;
                    return;
                    //Rudimentary implementation as of now
                }
            });
        }

        internal Coordinates? GetCoordsFromCode(string input, bool inBounds = true)
        {
            if (string.IsNullOrEmpty(input)) return null;

            string code = input.Trim().ToLower();
            int length = code.Length;

            if (length < 2) return null;

            CodeConversionOrder order;
            if (char.IsLetter(code[0])) order = CodeConversionOrder.LetterFirst;
            else order = CodeConversionOrder.NumberFirst;

            int ChangeIndex = StringUtils.FindCodeSwitchIndex(code);
            if (ChangeIndex == -1) return null;

            string FirstPart = code[..ChangeIndex];
            string SecondPart = code[ChangeIndex..];

            bool isValid = order switch
            {
                CodeConversionOrder.NumberFirst => FirstPart.IsAllDigit() && SecondPart.IsAllLetter(),
                CodeConversionOrder.LetterFirst => FirstPart.IsAllLetter() && SecondPart.IsAllDigit(),
                _ => false
            };
            if (!isValid) return null;

            Coordinates coords = ConvertCodeToCoord(FirstPart, SecondPart, order);
            if (!IsInBounds(coords) && inBounds) return null;
            return coords;
        }

        internal static Coordinates ConvertCodeToCoord(string FirstPart, string SecondPart, CodeConversionOrder order)
        {
            FirstPart = FirstPart.Trim();
            SecondPart = SecondPart.Trim();

            string xstring = order switch { 
                CodeConversionOrder.LetterFirst => FirstPart,
                CodeConversionOrder.NumberFirst => SecondPart,
                _ => ""
            };
            string ystring = order switch
            {
                CodeConversionOrder.LetterFirst => SecondPart,
                CodeConversionOrder.NumberFirst => FirstPart,
                _ => ""
            };

            // Letter is X
            // Number is Y
            // That IS fixed

            int xcoord = StringUtils.GetNumberFromLetters(xstring);
            int ycoord = Convert.ToInt32(ystring);

            return new Coordinates(xcoord, ycoord);
        }


        internal void Action()
        {
            Console.Write("\n");
            string? input = Console.ReadLine();

            if (input is null)
            {
                Console.WriteLine("Digite um comando, use 'help' para ver a lista de comandos.");
                return;
            }

            input = input.Trim().ToLower();
            string[] words = input.Split(' ', 2);


            if (words.Length == 0)
            {
                Console.WriteLine("Digite um comando, use 'help' para ver a lista de comandos.");
                return;
            }   

            string command = words[0];
            string[] args = words[1].Split(" ");
            Commands(command, args);
        }
        internal void Perform(Action<Coordinates> action, string[] args)
        {
            Coordinates? target = GetCoordsFromCode(args[0]);

            if (target is not null)
            {
                action(target.Value);
                return;
            }
            else
            {
                Console.WriteLine("Coordenada invalida, tente novamente.");
            }
        }

        internal void Dig(Coordinates coords)
        {
            if (!this[coords, Cell.isFlagged])
            {
                this[coords, Cell.isRevealed] = true;

                CheckForGameOver();
                FloodFillBFS(coords);
                Display();
            }
            else
            {
                Console.WriteLine("Você não pode revelar uma posição marcada.");
            }
        }


        internal void Flag(Coordinates coords)
        {
            if (!this[coords, Cell.isRevealed])
            {
                this[coords, Cell.isFlagged] = !this[coords, Cell.isFlagged];
                Display();
            }
            else
            {
                Console.WriteLine("Você não pode desmarcar uma posição já revelada.");
            }
        }

        private void Commands(string command, string[] args)
        {
            switch (command)
            {
                case "display":
                case "disp":
                case "camp":
                case "c":
                    Display();
                    return;
                case "help":
                case "h":
                case "ajuda":
                case "ayuda":
                    Console.WriteLine(
                      "Aqui esta uma lista de comandos:\n" +
                      "\nhelp - Mostra todos comandos." +
                      "\ndisplay - Imprime o campo no terminal." +
                      "\ndig [a1] - Cava um espaço." +
                      "\nflag [a1] - marca um espaço com bandeira." +
                      "\nexit - Termina o código."
                      );
                    return;
                case "dig":
                case "d":
                    Perform(Dig, args);
                    return;
                case "flag":
                case "f":
                    Perform(Flag, args);
                    return;
                case "exit":
                case "quit":
                case "end":
                    Environment.Exit(0);
                    return;
                case "print":
                case "say":
                case "diz":
                    Console.WriteLine(string.Join(' ', args));
                    return;
                default:
                    Console.WriteLine("Comando inválido. Use 'help' para ver a lista de comando.");
                    return;
            }
        }

        public Gaussian Gaussian => new(this);
        public ConditionalRandom ConditionalRandom => new(this);
        public General GeneralSetup => new(this);
    }

    public enum Cell
    {
        isRevealed,
        isFlagged,
        isBomb
    }

    public enum NeighborSearchStyle
    {
        SquareGrid,
        DiamondGrid,
        Radial
    }

    public enum CodeConversionOrder
    {
        NumberFirst,
        LetterFirst
    }
}