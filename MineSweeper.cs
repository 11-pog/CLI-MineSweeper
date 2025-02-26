
internal enum Cell
  {
    isRevealed,
    isFlagged,
    isBomb
  }


internal class MineSweeper
{
  

  internal static readonly sbyte[,] Surrounding3x3 = {
    { 1, -1 },
    { 1, 0 },
    { 1, 1 },
    { 0, 1 },
    { -1, 1 },
    { -1, 0 },
    { -1, -1 },
    { 0, -1 },
    { 0, 0 }
  };

  internal static readonly sbyte[,] Surrounding5x5 = {
    {0, -2},
    {-1, -2},
    {-2, -2},
    {-2, -1},
    {-2, 0},
    {-2, 1},
    {-2, 2},
    {-1, 2},
    {0, 2},
    {1, 2},
    {2, 2},
    {2, 1},
    {2, 0},
    {2, -1},
    {2, -2},
    {1, -2},
    {0, 0}
  };

  internal byte height;
  internal byte width;
  private bool[,,] field;

  internal bool GameEnded = false;

  internal MineSweeper(byte height, byte width)
  {
    field = new bool[height, width, 3];
    //bombas[y,x,0] = Revelado ou não
    //bombas[y,x,1] = Flagado ou não
    //bombas[y,x,2] = Bomba ou não

    this.height = height;
    this.width = width;
  }


  internal bool this[byte y, byte x, Cell key]
  {
    get => field[y, x, (int)key];
    set => field[y, x, (int)key] = value;
  }


  internal void IterateAllCells(Action<byte, byte> Act)
  {
    for (byte y = 0; y < height; y++)
    {
      for (byte x = 0; x < width; x++)
      {
        Act(y, x);
      }
    }
  }


  internal void IterateNeighbor((byte, byte) Coords, Action<byte, byte, byte> Act, bool WithCenter = true, bool ExcludeOutOfBounds = true, bool is5x5 = false)
  {
    byte nForNeighbors = (byte)(is5x5 ? 16 : 8);
    byte nMax = (byte)(WithCenter ? nForNeighbors + 1 : nForNeighbors);
    byte y = Coords.Item1;
    byte x = Coords.Item2;

    for (byte n = 0; n < nMax; n++)
    {
      byte yoffset = GetOffset(y, n, 0, is5x5);
      byte xoffset = GetOffset(x, n, 1, is5x5);

      if (IsInBounds(yoffset, xoffset) || !ExcludeOutOfBounds)
      {
        Act(yoffset, xoffset, n);
      }
    }
  }


  internal static byte GetOffset(byte coord, byte n, byte YorX, bool is5x5 = false)
  {
    if (YorX != 0 && YorX != 1)
    {
      throw new ArgumentOutOfRangeException("YorX: Invalid y or x value.");
    }

    if (!is5x5)
    {
      return (byte)(coord + Surrounding3x3[n, YorX]);
    }

    return (byte)(coord + Surrounding5x5[n, YorX]);
  }


  internal bool IsInBounds(byte Y, byte X)
  {
    if (Y < height && X < width)
    {
      return true;
    }
    return false;
  }


  internal (byte, byte, byte) GetCellData((byte, byte) coords, bool is5x5 = false)
  {
    byte OutOfBoundNeighbors = 0;
    byte RevealedNeighbors = 0;
    byte NeighborBombs = 0;

    byte y = coords.Item1;
    byte x = coords.Item2;

    IterateNeighbor((y, x), (NewY, NewX, _) =>
    {
      if (IsInBounds(NewY, NewX))
      {
        if (this[NewY, NewX, Cell.isRevealed])
        {
          RevealedNeighbors++;
        }

        if (this[NewY, NewX, Cell.isBomb])
        {
          NeighborBombs++;
        }
      }
      else
      {
        OutOfBoundNeighbors++;
      }
    }, WithCenter: false, ExcludeOutOfBounds: false, is5x5: is5x5);

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


  internal void Dig((byte, byte) coords)
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

    for (byte x = 0; x < width; x++)
    {
      Console.Write((char)(x + 65) + " ");
    }

    Console.Write("\n");

    for (byte y = 0; y < height; y++)
    {
      if (y < 9)
      {
        Console.Write(" ");
      }
      Console.Write(y + 1 + "|");

      for (byte x = 0; x < width; x++)
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
        }, WithCenter: false);
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
