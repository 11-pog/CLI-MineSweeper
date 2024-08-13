using System;

class Program
{

  static readonly sbyte[,] surrounding = { { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 }, { -1, -1 }, { 0, -1 }, { 1, -1 }, { 0, 0 } };
  static void Main()
  {
    Console.WriteLine("           ---MINESWEEPER---\nCampo minado so que NO TERMINAL AAHAHHAHHHA");

    byte xsize = 16;
    byte ysize = 16;

  
    Console.WriteLine("Escolha o tamanho do campo:");
    Console.WriteLine("1 - Padrão (7x7)\n2 - Grande (16x16)\n3 - Personalizado");

    /*Lembretes:
    - POSSIBILIDADE - Lembrar de inverter as posições das letras e numeros 
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

    bool[,,] bombas = new bool[ysize, xsize, 3];
    //bombas[y,x,0] = Revelado ou não
    //bombas[y,x,1] = Flagado ou não
    //bombas[y,x,2] = Bomba ou não

    Random rdn = new Random();

    for (int i = 0; i < bombas.GetLength(0); i++)
    {
      for (int j = 0; j < bombas.GetLength(1); j++)
      {
        for (int h = 0; h < bombas.GetLength(2) - 1; h++)
        {
          bombas[i, j, h] = false;
        }
        if (rdn.Next(0, 5) == 0)
        {
          bombas[i, j, 2] = true;
        }
        else
        {
          bombas[i, j, 2] = false;
        }
      }
    }

    display(bombas);

    Console.Write("Digite as coordenadas para começar: ");

    byte[] coords = starterCodeInput(ysize, xsize);

    bombas[coords[0], coords[1], 2] = false;
    bombas[coords[0], coords[1], 0] = true; 

    for(byte i = 0; i < 8; i++)
    {
      byte yoffset = (byte)(coords[0] + surrounding[i, 0]);
      byte xoffset = (byte)(coords[1] + surrounding[i, 1]);

      if (yoffset < ysize && xoffset < xsize)
      {
        bombas[yoffset, xoffset, 2] = false;
        bombas[yoffset, xoffset, 0] = true;
      }
    }

    bool repeat = false;

    do
    {
      repeat = false;
      for (byte i = 0; i < ysize; i++)
      {
        for (byte j = 0; j < xsize; j++)
        {
          bool hasNeighborBomb = false;
          byte isCorner = 0;
          byte RevealedNeighbors = 0;

          for (int h = 0; h < 9; h++)
          {
            byte yoffset = (byte)(i + surrounding[h, 0]);
            byte xoffset = (byte)(j + surrounding[h, 1]);

            if (yoffset < ysize && xoffset < xsize)
            {
              if (bombas[yoffset, xoffset, 2] == true)
              {
                hasNeighborBomb = true;
              }
              if (bombas[yoffset, xoffset, 0] == true && h != 8)
              {
                RevealedNeighbors++;
              }
            }
            else
            {
              isCorner++;
            }
          }

          if (!hasNeighborBomb && bombas[i, j, 0] == true && ((RevealedNeighbors < 8 && isCorner == 0) || (RevealedNeighbors < 5 && isCorner == 3) || (RevealedNeighbors < 3 && isCorner == 5)))
          {
            repeat = true;
            for (int h = 0; h < 9; h++)
            {
              byte yoffset = (byte)(i + surrounding[h, 0]);
              byte xoffset = (byte)(j + surrounding[h, 1]);

              if (yoffset < ysize && xoffset < xsize)
              {
                bombas[yoffset, xoffset, 0] = true;
              }
            }
          }
        }
      }
    } while (repeat);

    display(bombas);

    Console.WriteLine("O jogo começou, use 'help' para ver a lista de comandos.");

    while (true)
    {
      byte[] tempVar = commandInterface(bombas);


      if (tempVar[0] == 1)
      {
        if (bombas[tempVar[1], tempVar[2], 1] == false)
        {
          bombas[tempVar[1], tempVar[2], 0] = true;

          do
          {
            repeat = false;
            for (byte i = 0; i < ysize; i++)
            {
              for (byte j = 0; j < xsize; j++)
              {
                bool hasNeighborBomb = false;
                byte isCorner = 0;
                byte RevealedNeighbors = 0;

                for (int h = 0; h < 9; h++)
                {
                  byte yoffset = (byte)(i + surrounding[h, 0]);
                  byte xoffset = (byte)(j + surrounding[h, 1]);

                  if (yoffset < ysize && xoffset < xsize)
                  {
                    if (bombas[yoffset, xoffset, 2] == true)
                    {
                      hasNeighborBomb = true;
                    }
                    if (bombas[yoffset, xoffset, 0] == true && h != 8)
                    {
                      RevealedNeighbors++;
                    }
                  }
                  else
                  {
                    isCorner++;
                  }
                }

                if (!hasNeighborBomb && bombas[i, j, 0] == true && ((RevealedNeighbors < 8 && isCorner == 0) || (RevealedNeighbors < 5 && isCorner == 3) || (RevealedNeighbors < 3 && isCorner == 5)))
                {
                  repeat = true;
                  for (int h = 0; h < 9; h++)
                  {
                    byte yoffset = (byte)(i + surrounding[h, 0]);
                    byte xoffset = (byte)(j + surrounding[h, 1]);

                    if (yoffset < ysize && xoffset < xsize)
                    {
                      bombas[yoffset, xoffset, 0] = true;
                    }
                  }
                }
              }
            }
          } while (repeat);

          display(bombas);
        }
        else
        {
          Console.WriteLine("Você não pode revelar uma posição marcada.");
        }
      }
      else if (tempVar[0] == 2)
      {
        if (bombas[tempVar[1], tempVar[2], 0] == false)
        {
          bombas[tempVar[1], tempVar[2], 1] = !bombas[tempVar[1], tempVar[2], 1];
          display(bombas);
        }
        else
        {
          Console.WriteLine("Você não pode desmarcar uma posição já revelada.");
        }
      }
    }
  }



  static byte[] starterCodeInput(byte ysize, byte xsize)
  {
    do
    {
      string input = Console.ReadLine();
      input = input.Trim();

      byte length = (byte)input.Length;
      char x2 = new();
      
      if (length == 2 || length == 3)
      {
        char y = input[0];
        char x = input[1];
        if (length == 3)
        {
          x2 = input[2];
        }

        if (char.IsLetter(y) && (char.IsDigit(x) && length == 2 || (char.IsDigit(x) && char.IsDigit(x2))))
        {
          byte[] output = codeToCoordConverter(input);
          if (output[0] <= ysize-1 && output[1] <= xsize-1)
          {
            return output;
          }
          Console.WriteLine("Oops, parece que a coordenada que você entrou esta fora da area do jogo.");
          Console.Write("Tente denovo: ");
        }
        else
        {
          Console.WriteLine("Oops, pareçe que você entrou com uma coordenada invalida, faça questão de escrever nesse formato: A1, B2, C27, etc (não é case sensitive).");
          Console.Write("Tente denovo: ");
        }
      }
      else
      {
        Console.WriteLine("Oops, pareçe que você entrou com uma coordenada invalida, faça questão de escrever nesse formato: A1, B2, C27, etc (não é case sensitive).");
        Console.Write("Tente denovo: ");
      }
    } while (true);
  }




  static byte[] commandInterface(bool[,,] map)
  {
    Console.Write("\n");
    string input = Console.ReadLine();
    input = input.Trim().ToLower();

    char x2 = new char();

    byte[] coords = new byte[2];
    byte[] output = new byte[3];

    string[] words = input.Split(' ', 2);

    if (words.Length == 0)
    {
      Console.WriteLine("Digite um comando, use 'help' para ver a lista de comandos.");
      output[0] = 0;
      return output;
    }

    switch (words[0])
    {
      case "display":
      case "disp":
      case "camp":
      case "c":
        display(map);
        break;
      case "help":
        Console.WriteLine("Aqui esta uma lista de comandos:\n\nhelp - Mostra todos comandos.\ndisplay - Imprime o campo no terminal.\ndig [a1] - Cava um espaço.\nflag [a1] - marca um espaço com bandeira.\nexit - Termina o codigo.");
        break;
      case "dig":
      case "d":
        if (words != null && words.Length > 1 && !String.IsNullOrEmpty(words[1]))
        {
          string code = words[1].Trim();
          byte length = (byte)code.Length;

          if (length == 2 || length == 3)
          {

            char y = code[0];
            char x = code[1];

            if (length == 3)
            {
              x2 = code[2];
            }

            if ((char.IsLetter(y) && (char.IsDigit(x) && length == 2 || (char.IsDigit(x) && char.IsDigit(x2)))))
            {
              coords = codeToCoordConverter(code);
              if (coords[0] <= (map.GetLength(0)-1) && coords[1] <= (map.GetLength(1)-1))
              {
                output[0] = 1;
                output[1] = coords[0];
                output[2] = coords[1];
                return output;
              }
            }
          }
        }

        Console.WriteLine("Coordenada invalida, tente novamente.");
        break;
      case "flag":
      case "f":
        if (words != null && words.Length > 1 && !String.IsNullOrEmpty(words[1]))
        {
          string code = words[1].Trim();
          byte length = (byte)code.Length;

          if (length == 2 || length == 3)
          {

            char y = code[0];
            char x = code[1];

            if (length == 3)
            {
              x2 = code[2];
            }

            if ((char.IsLetter(y) && (char.IsDigit(x) && length == 2 || (char.IsDigit(x) && char.IsDigit(x2)))))
            {
              coords = codeToCoordConverter(code);
              if (coords[0] <= (map.GetLength(0)-1) && coords[1] <= (map.GetLength(1)-1))
              {
                output[0] = 2;
                output[1] = coords[0];
                output[2] = coords[1];
                return output;
              }
            }
          }
        }

        Console.WriteLine("Coordenada invalida, tente novamente.");
        break;
      case "exit":
      case "quit":
      case "end":
        Environment.Exit(0);
        break;
      default:
        Console.WriteLine("Comando inválido. Use 'help' para ver a lista de comando.");
        break;

    }
    output[0] = 0;
    return output;
  }



  static void clearConsole()
  {
    for (int i = 0; i < 50; i++)
    {
      Console.WriteLine('\n');
    }
  }


  static byte[] codeToCoordConverter(string code)
  {
    string input = code;
    input = input.Trim();
    input = input.ToLower();
    string xstrcoord = input.Substring(0, 1);
    string ystrcoord = input.Substring(1);
    char xcharcoord = Convert.ToChar(xstrcoord);
    int xcoord = ((int)xcharcoord) - 97;
    int ycoord = Convert.ToInt32(ystrcoord) - 1;

    byte[] output = { (byte)ycoord, (byte)xcoord };
    return output;
  }




  static void display(bool[,,] bombas)
  {
    clearConsole();

    Console.Write("\n   ");

    for (byte i = 0; i < bombas.GetLength(1); i++)
    {
      Console.Write((char)(i + 65) + " ");
    }

    Console.Write("\n");

    for (byte i = 0; i < bombas.GetLength(0); i++)
    {
      if (i < 9)
      {
        Console.Write(" ");
      }
      Console.Write((i + 1) + "|");

      for (byte j = 0; j < bombas.GetLength(1); j++)
      {
        if (bombas[i, j, 0] == false)
        {
          if (bombas[i, j, 1])
          {
            Console.Write("# ");
          }
          else
          {
            Console.Write("O ");
          }
        }
        else if (bombas[i, j, 2])
        {
          Console.Write("* ");
        }
        else
        { 
          byte bombcount = 0;
          for(int h = 0; h < 8; h++)
          {
            byte yoffset = (byte)(i + surrounding[h, 0]);
            byte xoffset = (byte)(j + surrounding[h, 1]);

            if (yoffset < bombas.GetLength(0) && xoffset < bombas.GetLength(1) && bombas[yoffset, xoffset, 2] == true)
            {              
                bombcount++;        
            }
          }

          if(bombcount>0)
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