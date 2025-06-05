using CLI_MineSweeper;

internal class MineSweeper
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


    internal void IterateAllCells(Action<Coordinates> Act)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Act(new Coordinates(x, y));
            }
        }
    }


    internal void IterateNeighbor(Coordinates coords, Action<Coordinates, int> action,
        NeighborSearchStyle searchStyle = NeighborSearchStyle.SquareGrid,
        int searchSize = 1, bool includeCenterCell = true,
        bool excludeOutOfBounds = true)
    {
        List<Coordinates> OffsetMap = GetOffsetMap(searchStyle, searchSize);


        foreach (Coordinates offset in OffsetMap)
        {
            byte yoffset = GetOffset(y, n, 0, is5x5);
            byte xoffset = GetOffset(x, n, 1, is5x5);

            if (IsInBounds(yoffset, xoffset) || !excludeOutOfBounds)
            {
                action(new Coordinates(yoffset, xoffset), n);
            }
        }
    }

    private List<Coordinates> GetOffsetMap(NeighborSearchStyle searchStyle, int searchSize)
    {
        List<Coordinates> OffsetMap = [];

        for (int i = -searchSize; i <= searchSize; i++)
            for (int j = -searchSize; j <= searchSize; j++)
            {
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

        return OffsetMap;
    }


    internal bool IsInBounds(Coordinates coords) => IsInBounds(coords.Y, coords.X);
    internal bool IsInBounds(int Y, int X) => Y < Height && X < Width;


    internal (byte, byte, byte) GetCellData(Coordinates coords, bool is5x5 = false)
    {
        byte OutOfBoundNeighbors = 0;
        byte RevealedNeighbors = 0;
        byte NeighborBombs = 0;

        IterateNeighbor(coords, (coords, _) =>
        {
            if (IsInBounds(coords))
            {
                if (this[coords, Cell.isRevealed])
                {
                    RevealedNeighbors++;
                }

                if (this[coords, Cell.isBomb])
                {
                    NeighborBombs++;
                }
            }
            else
            {
                OutOfBoundNeighbors++;
            }
        }, includeCenterCell: false, excludeOutOfBounds: false);

        return (NeighborBombs, OutOfBoundNeighbors, RevealedNeighbors);
    }


    internal (byte, byte, byte) GetDataAllIn5x5((byte, byte) Coords)
    {
        (byte, byte, byte) NeighborData3x3 = GetCellData(Coords, is5x5: false);
        (byte, byte, byte) NeighborData5x5 = GetCellData(Coords, is5x5: true);

        return (
            (byte)(NeighborData3x3.Item1 + NeighborData5x5.Item1),
            (byte)(NeighborData3x3.Item2 + NeighborData5x5.Item2),
            (byte)(NeighborData3x3.Item3 + NeighborData5x5.Item3)
        );
    }


    static void Clear()
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


    internal void Dig((int, int) coords)
    {
        byte x = coords.Item2;
        byte y = coords.Item1;

        if (!this[y, x, Cell.isFlagged])
        {
            this[y, x, Cell.isRevealed] = true;

            CheckForGameOver();
            FloodFillBFS((y, x));
            Display();
        }
        else
        {
            Console.WriteLine("Você não pode revelar uma posição marcada.");
        }
    }


    internal void Flag((byte, byte) coords)
    {
        byte x = coords.Item2;
        byte y = coords.Item1;

        if (!this[y, x, Cell.isRevealed])
        {
            this[y, x, Cell.isFlagged] = !this[y, x, Cell.isFlagged];

            Display();
        }
        else
        {
            Console.WriteLine("Você não pode desmarcar uma posição já revelada.");
        }
    }



    internal void Display()
    {
        Clear();
        Console.Write("\n   ");

        for (byte x = 0; x < Width; x++)
        {
            Console.Write((char)(x + 65) + " ");
        }

        Console.Write("\n");

        for (byte y = 0; y < Height; y++)
        {
            if (y < 9)
            {
                Console.Write(" ");
            }
            Console.Write(y + 1 + "|");

            for (byte x = 0; x < Width; x++)
            {
                if (!this[y, x, Cell.isRevealed])
                {
                    if (this[y, x, Cell.isFlagged])
                    {
                        Console.Write("# ");
                    }
                    else
                    {
                        Console.Write("O ");
                    }
                }
                else if (this[y, x, Cell.isBomb])
                {
                    Console.Write("* ");
                }
                else
                {
                    byte bombcount = GetCellData((y, x)).Item1;

                    if (bombcount > 0)
                    {
                        Console.Write(bombcount + " ");
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
            }

            Console.Write("\n");
        }
    }


    private bool CanReveal((byte, byte) Coords, Action? Act = null)
    {
        (byte, byte, byte) cellData = GetCellData(Coords);

        bool hasNeighborBomb = cellData.Item1 != 0;
        byte OutOfBoundsNeighbor = cellData.Item2;
        byte RevealedNeighbors = cellData.Item3;

        if (!hasNeighborBomb && this[Coords.Item1, Coords.Item2, Cell.isRevealed] == true &&
            ((RevealedNeighbors < 8 && OutOfBoundsNeighbor == 0)
            || (RevealedNeighbors < 5 && OutOfBoundsNeighbor == 3)
            || (RevealedNeighbors < 3 && OutOfBoundsNeighbor == 5)))
        {
            if (Act is not null)
            {
                Act();
            }
            return true;
        }
        return false;
    }


    private (bool, Queue<(byte, byte)>?) ProcessCell(bool Enqueue = false)
    {
        bool didReveal = false;
        Queue<(byte, byte)>? QueuedCoords = Enqueue ? new Queue<(byte, byte)>() : null;

        IterateAllCells((y, x) =>
        {
            CanReveal((y, x), () =>
        {
            didReveal = true;

            IterateNeighbor((y, x), Expose, WithCenter: false);
        });
        });

        void Expose(byte y, byte x, byte _)
        {
            this[y, x, Cell.isRevealed] = true;

            if (Enqueue) QueuedCoords!.Enqueue((y, x));
        }



        if (Enqueue)
        {
            return (didReveal, QueuedCoords);
        }

        return (didReveal, null);
    }


    internal void FloodFillBFS((byte, byte)? startingPoint = null)
    {
        Queue<(byte, byte)> queue = new();

        if (startingPoint is null)
        {
            queue = ProcessCell(true).Item2!;
        }
        else
        {
            queue.Enqueue(((byte, byte))startingPoint);
        }

        while (queue.Count > 0)
        {
            (byte, byte) currentCoords = queue.Dequeue();

            if (CanReveal(currentCoords))
            {
                IterateNeighbor(currentCoords, (y, x, _) =>
                {
                    if (!this[y, x, Cell.isRevealed])
                    {
                        this[y, x, Cell.isRevealed] = true;
                        queue.Enqueue((y, x));
                    }
                }, IncludeCenterCell: false);
            }
        }
    }


    internal void FloodFillLoop()
    {
        bool repeat;

        do
        {
            repeat = ProcessCell().Item1;
        } while (repeat);
    }


    internal (byte, byte)? IsCodeValid(string[]? input, bool bound = true)
    {
        if (input != null && input.Length > 1 && !string.IsNullOrEmpty(input[1]))
        {
            string code = input[1].Trim();
            byte length = (byte)code.Length;

            if (length == 2 || length == 3)
            {
                char y = code[0];
                char x = code[1];
                char x2 = (length == 3) ? code[2] : '\0';

                if (char.IsLetter(y) && (char.IsDigit(x) && length == 2 || (char.IsDigit(x) && char.IsDigit(x2))))
                {
                    byte[] coords = CodeToCoordConverter(code);

                    if (IsInBounds(coords[0], coords[1]) || !bound)
                    {
                        return (coords[0], coords[1]);
                    }
                }
            }
        }

        return null;
    }


    internal void CheckForGameOver()
    {
        IterateAllCells((y, x) =>
        {
            if (this[y, x, Cell.isRevealed] && this[y, x, Cell.isBomb])
            {
                GameEnded = true;
                return;
                //Rudimentary implementation as of now
            }
        });
    }

    internal static void Perform(Action<(byte, byte)> action, (byte, byte)? coords)
    {
        if (coords is not null)
        {
            action(coords.Value);
            return;
        }
        else
        {
            Console.WriteLine("Coordenada invalida, tente novamente.");
        }
    }

    internal static byte[] CodeToCoordConverter(string code)
    {
        string input = code;
        input = input.Trim();
        input = input.ToLower();

        string ystrcoord = input[1..];
        string xstrcoord = input[..1];

        char xcharcoord = Convert.ToChar(xstrcoord);
        int xcoord = xcharcoord - 97;
        int ycoord = Convert.ToInt32(ystrcoord) - 1;

        byte[] output = [(byte)ycoord, (byte)xcoord];
        return output;
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

        (byte, byte)? coords = IsCodeValid(words);

        switch (words[0])
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
                Perform(Dig, coords);
                return;
            case "flag":
            case "f":
                Perform(Flag, coords);
                return;
            case "exit":
            case "quit":
            case "end":
                Environment.Exit(0);
                return;
            case "print":
            case "say":
            case "diz":
                Console.WriteLine(words[1]);
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