internal class MineSweeperCamp
{
  internal Random rdn = new();
  internal static readonly sbyte[,] surrounding = {
    { 1, 0 }, { 1, 1 }, { 0, 1 },
    { -1, 1 },            { -1, 0 },
    { -1, -1 }, { 0, -1 }, { 1, -1 },
    { 0, 0 }
  };

  internal byte ysize;
  internal byte xsize;
  internal bool[,,] field;

  internal MineSweeperCamp(byte ysize, byte xsize)
  {
    field = new bool[ysize, xsize, 3];
    //bombas[y,x,0] = Revelado ou não
    //bombas[y,x,1] = Flagado ou não
    //bombas[y,x,2] = Bomba ou não

    this.ysize = ysize;
    this.xsize = xsize;
  }


  internal void ForEachCell(Action<byte, byte> Act)
  {
    for (byte y = 0; y < ysize; y++)
    {
      for (byte x = 0; x < xsize; x++)
      {
        Act(y, x);
      }
    }
  }


  internal void ForEachNeighbor(byte y, byte x, Action<byte, byte> Act)
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

  internal byte[] GetCellData(byte y, byte x)
  {
    byte OutOfBoundNeighbors = 0;
    byte RevealedNeighbors = 0;
    byte NeighborBombs = 0;

    for (byte n = 0; n < 8; n++)
    {
      byte offsety = GetOffsetY(y, n);
      byte offsetx = GetOffsetX(x, n);

      if (offsety < ysize && offsetx < xsize)
      {
        if (field[offsety, offsetx, 0] == true)
        {
          RevealedNeighbors++;
        }

        if (field[offsety, offsetx, 2] == true)
        {
          NeighborBombs++;
        }
      }
      else
      {
        OutOfBoundNeighbors++;
      }
    }

    return [NeighborBombs, OutOfBoundNeighbors, RevealedNeighbors];
  }


  internal void SetupRdn(byte chance = 20)
  {
    void PerformSetup(byte y, byte x)
    {
      field[y, x, 0] = false;
      field[y, x, 1] = false;
      if (rdn.Next(0, chance) == 0)
      {
        field[y, x, 2] = true;
      }
      else
      {
        field[y, x, 2] = false;
      }
    }

    ForEachCell(PerformSetup);
  }

  /*
  internal static void SetupConway()
  {

  }


  internal void SetupPerlin()
  {
    void PerformSetup(byte y, byte x)
    {
      //int randomnumber = rdn.perlin
    }

    ForEachCell(PerformSetup);
  }
  */

  static void Clear()
  {
    for (int i = 0; i < 50; i++)
    {
      Console.WriteLine('\n');
    }
  }


  internal void Dig(byte y, byte x)
  {
    if (field[y, x, 1] == false)
    {
      field[y, x, 0] = true;

      IterateFloodFill();
      Display();
    }
    else
    {
      Console.WriteLine("Você não pode revelar uma posição marcada.");
    }
  }


  internal void Flag(byte y, byte x)
  {
    if (field[y, x, 0] == false)
    {
      field[y, x, 1] = !field[y, x, 1];

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

    for (byte x = 0; x < xsize; x++)
    {
      Console.Write((char)(x + 65) + " ");
    }

    Console.Write("\n");

    for (byte y = 0; y < ysize; y++)
    {
      if (y < 9)
      {
        Console.Write(" ");
      }
      Console.Write((y + 1) + "|");

      for (byte x = 0; x < xsize; x++)
      {
        if (field[y, x, 0] == false)
        {
          if (field[y, x, 1])
          {
            Console.Write("# ");
          }
          else
          {
            Console.Write("O ");
          }
        }
        else if (field[y, x, 2])
        {
          Console.Write("* ");
        }
        else
        {
          byte bombcount = GetCellData(y, x)[0];

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


  internal void BFSFloodFill()
  {
    void Reveal(byte y, byte x)
    {
      if (y < ysize && x < xsize)
      {
        field[y, x, 0] = true;
      }
    }
    bool repeat;

    do
    {
      repeat = false;
      for (byte y = 0; y < ysize; y++)
      {
        for (byte x = 0; x < xsize; x++)
        {
          byte[] cellData = GetCellData(y, x);

          bool hasNeighborBomb = cellData[0] != 0;
          byte OutOfBoundsNeighbor = cellData[1];
          byte RevealedNeighbors = cellData[2];

          if (!hasNeighborBomb && field[y, x, 0] == true && ((RevealedNeighbors < 8 &&
          OutOfBoundsNeighbor == 0) || (RevealedNeighbors < 5 && OutOfBoundsNeighbor == 3) ||
          (RevealedNeighbors < 3 && OutOfBoundsNeighbor == 5)))
          {
            repeat = true;
            ForEachNeighbor(y, x, Reveal);
          }
        }
      }
    } while (repeat);
  }


  internal void IterateFloodFill()
  {
    void Reveal(byte y, byte x)
    {
      if (y < ysize && x < xsize)
      {
        field[y, x, 0] = true;
      }
    }
    bool repeat;

    do
    {
      repeat = false;
      for (byte y = 0; y < ysize; y++)
      {
        for (byte x = 0; x < xsize; x++)
        {
          byte[] cellData = GetCellData(y, x);

          bool hasNeighborBomb = cellData[0] != 0;
          byte OutOfBoundsNeighbor = cellData[1];
          byte RevealedNeighbors = cellData[2];

          if (!hasNeighborBomb && field[y, x, 0] == true && ((RevealedNeighbors < 8 &&
          OutOfBoundsNeighbor == 0) || (RevealedNeighbors < 5 && OutOfBoundsNeighbor == 3) ||
          (RevealedNeighbors < 3 && OutOfBoundsNeighbor == 5)))
          {
            repeat = true;
            ForEachNeighbor(y, x, Reveal);
          }
        }
      }
    } while (repeat);
  }



  internal byte[]? IsCodeValid(string[] input)
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
          if (coords[0] <= (ysize - 1) && coords[1] <= (xsize - 1))
          {
            return [coords[0], coords[1]];
          }
        }
      }
    }

    return null;
  }


  internal static void Perform(Action<byte, byte> action, byte[]? coords)
  {
    if (coords is not null)
    {
      action(coords[0], coords[1]);
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
    string xstrcoord = input.Substring(0, 1);
    string ystrcoord = input.Substring(1);
    char xcharcoord = Convert.ToChar(xstrcoord);
    int xcoord = xcharcoord - 97;
    int ycoord = Convert.ToInt32(ystrcoord) - 1;

    byte[] output = { (byte)ycoord, (byte)xcoord };
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

    byte[]? coords = IsCodeValid(words);

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

    Camp.SetupRdn();
    Camp.Display();

    Console.Write("Digite as coordenadas para começar: ");

    do
    {
      string? input = Console.ReadLine()?.Trim();
      byte? length = (byte?)input?.Length;

      if (input is not null && (length == 2 || length == 3))
      {
        char y = input[0];
        char x = input[1];
        char x2 = (length == 3) ? input[2] : '\0';

        if (char.IsLetter(y) && (char.IsDigit(x) && length == 2 || (char.IsDigit(x) && char.IsDigit(x2))))
        {
          byte[] output = MineSweeperCamp.CodeToCoordConverter(input);
          if (output[0] <= ysize - 1 && output[1] <= xsize - 1)
          {
            Camp.field[output[0], output[1], 2] = false;
            Camp.field[output[0], output[1], 0] = true;


            for (byte i = 0; i < 8; i++)
            {
              byte yoffset = (byte)(output[0] + MineSweeperCamp.surrounding[i, 0]);
              byte xoffset = (byte)(output[1] + MineSweeperCamp.surrounding[i, 1]);

              if (yoffset < ysize && xoffset < xsize)
              {
                Camp.field[yoffset, xoffset, 2] = false;
                Camp.field[yoffset, xoffset, 0] = true;
              }
            }
            break;
          }
          Console.WriteLine("Oops, parece que a coordenada que você entrou esta fora da area do jogo.");
        }
        else
        {
          Console.WriteLine("Oops, pareçe que você entrou com uma coordenada invalida, faça questão de escrever nesse formato: A1, B2, C27, etc (não é case sensitive).");
        }
      }
      else
      {
        Console.WriteLine("Oops, pareçe que você entrou com uma coordenada invalida, faça questão de escrever nesse formato: A1, B2, C27, etc (não é case sensitive).");
      }

      Console.Write("Tente denovo: ");
    } while (true);

    Camp.IterateFloodFill();
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