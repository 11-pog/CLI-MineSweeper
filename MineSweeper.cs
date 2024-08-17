internal enum CellKeys
{
  isRevealed,
  isFlagged,
  isBomb
}

internal class MineSweeperCamp
{
  private const CellKeys isRevealed = CellKeys.isRevealed;
  private const CellKeys isFlagged = CellKeys.isFlagged;
  private const CellKeys isBomb = CellKeys.isBomb;
  internal Random rdn = new();
  internal static readonly sbyte[,] surrounding = {
    { 1, 0 }, { 1, 1 }, { 0, 1 },
    { -1, 1 },            { -1, 0 },
    { -1, -1 }, { 0, -1 }, { 1, -1 },
    { 0, 0 }
  };

  internal byte height;
  internal byte width;
  internal bool[,,] field;

  internal MineSweeperCamp(byte height, byte width)
  {
    field = new bool[height, width, 3];
    //bombas[y,x,0] = Revelado ou não
    //bombas[y,x,1] = Flagado ou não
    //bombas[y,x,2] = Bomba ou não

    this.height = height;
    this.width = width;
  }


  internal bool this[byte y, byte x, CellKeys key]
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


  internal static void IterateNeighbor(byte y, byte x, Action<byte, byte> Act)
  {
    for (byte n = 0; n < 9; n++)
    {
      byte yoffset = GetOffsetY(y, n);
      byte xoffset = GetOffsetX(x, n);

      Act(yoffset, xoffset);
    }
  }


  internal static byte GetOffsetY(byte y, byte n)
  {
    return (byte)(y + surrounding[n, 0]);
  }


  internal static byte GetOffsetX(byte x, byte n)
  {
    return (byte)(x + surrounding[n, 1]);
  }

  internal bool IsInBounds(byte Y, byte X)
  {
    if (Y < height && X < width)
    {
      return true;
    }
    return false;
  }

  internal (byte, byte, byte) GetCellData(ValueTuple<byte, byte> coords)
  {
    byte OutOfBoundNeighbors = 0;
    byte RevealedNeighbors = 0;
    byte NeighborBombs = 0;

    byte y = coords.Item1;
    byte x = coords.Item2;

    void ForEachNeighbor(byte NewY, byte NewX)
    {
      if (IsInBounds(NewY, NewX))
      {
        if (this[NewY, NewX, isRevealed])
        {
          RevealedNeighbors++;
        }

        if (this[NewY, NewX, isBomb])
        {
          NeighborBombs++;
        }
      }
      else
      {
        OutOfBoundNeighbors++;
      }
    }

    IterateNeighbor(y, x, ForEachNeighbor);
    return (NeighborBombs, OutOfBoundNeighbors, RevealedNeighbors);
  }


  internal void SetupRdn(byte chance = 20)
  {
    void PerformSetup(byte y, byte x)
    {
      this[y, x, isRevealed] = false;
      this[y, x, isFlagged] = false;
      if (rdn.Next(0, chance) == 0)
      {
        this[y, x, isBomb] = true;
      }
      else
      {
        this[y, x, isBomb] = false;
      }
    }

    IterateAllCells(PerformSetup);
  }


  internal static void SetupNoGuessing(float Mod = 1.0f)
  {
    //PlaceHolder
  }


  static void Clear()
  {
    for (int i = 0; i < 50; i++)
    {
      Console.WriteLine('\n');
    }
  }


  internal void Dig(ValueTuple<byte, byte> coords)
  {
    byte x = coords.Item2;
    byte y = coords.Item1;

    if (!this[y, x, isFlagged])
    {
      this[y, x, isRevealed] = true;

      FloodFillLoop();
      Display();
    }
    else
    {
      Console.WriteLine("Você não pode revelar uma posição marcada.");
    }
  }


  internal void Flag(ValueTuple<byte, byte> coords)
  {
    byte x = coords.Item2;
    byte y = coords.Item1;

    if (!this[y, x, isRevealed])
    {
      this[y, x, isFlagged] = !this[y, x, isFlagged];

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
      Console.Write((y + 1) + "|");

      for (byte x = 0; x < width; x++)
      {
        if (!this[y, x, isRevealed])
        {
          if (this[y, x, isFlagged])
          {
            Console.Write("# ");
          }
          else
          {
            Console.Write("O ");
          }
        }
        else if (this[y, x, isBomb])
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


  private void IsRevealable(Action Act, ValueTuple<byte, byte> Coords)
  {
    ValueTuple<byte, byte, byte> cellData = GetCellData(Coords);

    bool hasNeighborBomb = cellData.Item1 != 0;
    byte OutOfBoundsNeighbor = cellData.Item2;
    byte RevealedNeighbors = cellData.Item3;

    if (!hasNeighborBomb && this[Coords.Item1, Coords.Item2, isRevealed] == true && ((RevealedNeighbors < 8 && OutOfBoundsNeighbor == 0)
                                                                             || (RevealedNeighbors < 5 && OutOfBoundsNeighbor == 3)
                                                                             || (RevealedNeighbors < 3 && OutOfBoundsNeighbor == 5)))
    {
      Act();
    }
  }


  internal void FloodFillBFS()
  {
    Queue<(byte, byte)> queue = ProcessCell(true).Item2!;

    do
    {
      //wip
    } while (queue.Count > 0);
  }


  private (bool, Queue<(byte, byte)>?) ProcessCell(bool Enqueue = false)
  {
    bool didReveal = false;
    Queue<(byte, byte)>? QueuedCoords = Enqueue ? new Queue<(byte, byte)>() : null;

    void Check(byte y, byte x)
    {
      void Act()
      {
        didReveal = true;

        IterateNeighbor(y, x, Expose);
      }

      IsRevealable(Act, (y, x));
    }

    void Expose(byte y, byte x)
    {
      if (y < height && x < width)
      {
        this[y, x, isRevealed] = true;

        if (Enqueue) QueuedCoords!.Enqueue((y, x));
      }
    }

    IterateAllCells(Check);

    if (Enqueue)
    {
      return (didReveal, QueuedCoords);
    }

    return (didReveal, null);
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


  internal static void Perform(Action<(byte, byte)> action, ValueTuple<byte, byte>? coords)
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

    ValueTuple<byte, byte>? coords = IsCodeValid(words);

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
          "\nexit - Termina o codigo."
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
}



class Program
{
  private const CellKeys isRevealed = CellKeys.isRevealed;
  private const CellKeys isFlagged = CellKeys.isFlagged;
  private const CellKeys isBomb = CellKeys.isBomb;
  static void Main()
  {
    Console.WriteLine("           ---MINESWEEPER---\nCampo minado so que NO TERMINAL AAHAHHAHHHA");

    byte xsize = 16;
    byte ysize = 16;

    Console.WriteLine("Escolha o tamanho do campo:");
    Console.WriteLine("1 - Padrão (7x7)\n2 - Grande (16x16)\n3 - Personalizado");

    /*Lembretes:
    - POSSIBILIDADE - Lembrar de inverter as posições das letras e numeros 
    - PRO FUTURO - Fazer perder o jogo quando revela uma bomba
    - PRO FUTURO - Fazer ganhar o jogo caso todas as bombas estiverem com bandeira
    */

    switch (UserInput(1, 4, true))
    {
      case 1:
        xsize = 7;
        ysize = 7;
        break;

      case 2:
        xsize = 16;
        ysize = 16;
        break;

      case 3:
        Console.Write("Digite a largura do campo (de 7 até 26): "); //Pergunta o tamanho do campo
        xsize = (byte)UserInput(7, 26, true);
        Console.Write("Digite a altura do campo (de 7 até 26): "); //Pergunta o tamanho do campo
        ysize = (byte)UserInput(7, 26, true);
        break;
    }

    MineSweeperCamp Camp = new(ysize, xsize);

    Camp.SetupRdn(chance: 3);
    Camp.Display();

    Console.Write("Digite as coordenadas para começar: ");

    while (true)
    {
      string? input = Console.ReadLine()?.Trim();
      byte? length = (byte?)input?.Length;

      ValueTuple<byte, byte>? Output = Camp.IsCodeValid(["\0", input is not null ? input : "\0"], bound:false);

      if (Output is not null)
      {
        byte OutputY = Output.Value.Item1;
        byte OutputX = Output.Value.Item2;

        if (Camp.IsInBounds(OutputY, OutputX))
        {
          Camp[OutputY, OutputX, isBomb] = false;
          Camp[OutputY, OutputX, isRevealed] = true;

          void Initiate(byte y, byte x)
          {
            if (Camp.IsInBounds(y, x))
            {
              Camp[y, x, isBomb] = false;
              Camp[y, x, isRevealed] = true;
            }
          }

          MineSweeperCamp.IterateNeighbor(OutputY, OutputX, Initiate);
          break;
        }
        else
        {
          Console.WriteLine("Oops, parece que a coordenada que você entrou esta fora da area do jogo.");
        }
      }
      else
      {
        Console.WriteLine("Oops, pareçe que você entrou com uma coordenada invalida, faça questão de escrever nesse formato: A1, B2, C27, etc (não é case sensitive).");
      }

      Console.Write("Tente denovo: ");
    }
    Camp.FloodFillLoop();
    Camp.Display();

    Console.WriteLine("O jogo começou, use 'help' para ver a lista de comandos.");

    while (true)
    {
      Camp.Action();
    }
  }



  static int UserInput(int LowerBoundary, int UpperBoundary, bool ranged)
  {
    if (ranged)
    {
      do
      {
        Console.Write("\n");
        if (int.TryParse(Console.ReadLine(), out int receivedNumber) == true)
        {

          if (receivedNumber <= UpperBoundary
            && receivedNumber >= LowerBoundary)
          {
            return receivedNumber;
          }
          else
          {
            Console.WriteLine
              ("O numero que você digitou esta fora dos parametros especificados (entre "
               + LowerBoundary + " e " + UpperBoundary + ").");
            Console.Write("Por favor digite um numero dentro dos parametros: ");
          }
        }
        else
        {
          Console.WriteLine
            ("Você não entrou com um numero valido.");
          Console.Write("Por favor entre com um numero valido: ");
        }
      } while (true);
    }
    else
    {
      do
      {
        if (int.TryParse(Console.ReadLine(), out int receivedNumber) == true)
        {
          return receivedNumber;
        }
        else
        {
          Console.WriteLine("You did not input a valid number.");
          Console.WriteLine("Please input a valid number.");
        }
      } while (true);
    }
  }
}