using CLI_MineSweeper.Objects;

namespace CLI_MineSweeper
{

    class Program
    {
        static void Main()
        {
            Console.WriteLine("           ---MINESWEEPER---\nCampo minado so que NO TERMINAL AAHAHHAHHHA");

            byte X_Size = 16;
            byte Y_Size = 16;
            byte Difficulty = new();

            Console.WriteLine("Escolha o tamanho do campo:");
            Console.WriteLine("1 - Padrão (7x7)\n2 - Grande (16x16)\n3 - Personalizado");

            /*Lembretes:
            - POSSIBILIDADE - Lembrar de inverter as posições das letras e números 
            - PRO FUTURO - Fazer ganhar o jogo caso todas as bombas estiverem com bandeira
            */

            switch (UserInput(1, 4))
            {
                case 1:
                    X_Size = 9;
                    Y_Size = 9;
                    Difficulty = 3;
                    break;

                case 2:
                    X_Size = 16;
                    Y_Size = 16;
                    Difficulty = 8;
                    break;

                case 3:
                    Console.Write("Digite a largura do campo (de 7 até 26): "); //Pergunta o tamanho do campo
                    X_Size = (byte)UserInput(7, 26);
                    Console.Write("Digite a altura do campo (de 7 até 26): "); //Pergunta o tamanho do campo
                    Y_Size = (byte)UserInput(7, 26);

                    Difficulty = (byte)Math.Max(2, (X_Size + Y_Size) / 5 - Math.Abs((X_Size - Y_Size) / 8));
                    break;
            }

            MineSweeper Field = new(Y_Size, X_Size);

            Field.Gaussian.SetField();
            Field.Display();

            Console.Write("Digite as coordenadas para começar: ");


            Coordinates? startCoords = null;
            while (startCoords is null)
            {
                string? input = Console.ReadLine()?.Trim();
                if (input == null) continue;

                int length = input.Length;
                startCoords = Field.GetCoordsFromCode(input, inBounds: true);

                if (startCoords is null)
                {
                    Console.WriteLine("Oops, parece que você entrou com uma coordenada invalida, faça questão de escrever nesse formato: A1, B2, C27, etc (não é case sensitive).");
                    Console.WriteLine("Ou talves as coordenadas entradas estejam fora do campo.");
                    Console.Write("Tente de novo: ");
                }
            }

            Field.IterateNeighbor((startCoords.Value), (coords, _) =>
            {
                Field[coords, Cell.isBomb] = false;
                Field[coords, Cell.isRevealed] = true;
            });

            Field.FloodFillBFS();
            Field.Display();

            Console.WriteLine("O jogo começou, use 'help' para ver a lista de comandos.");

            while (true)
            {
                Field.Action();
                if (Field.GameEnded)
                {
                    break;
                }
            }

            Console.WriteLine("Boom");

            Thread.Sleep(500);

            Console.WriteLine("Perdeu");

            Thread.Sleep(500);
        }


        static int UserInput(int? LowerBoundary = null, int? UpperBoundary = null)
        {
            do
            {
                Console.Write("\n");
                if (int.TryParse(Console.ReadLine(), out int receivedNumber) == true)
                {
                    if ((UpperBoundary is null || receivedNumber <= UpperBoundary)
                      && (LowerBoundary is null || receivedNumber >= LowerBoundary))
                        return receivedNumber;

                    else
                    {
                        Console.WriteLine("O numero que você digitou esta fora dos parâmetros especificados (entre "
                           + LowerBoundary + " e " + UpperBoundary + ").");
                        Console.Write("Por favor digite um numero dentro dos parâmetros: ");
                    }
                }
                else
                {
                    Console.WriteLine("Você não entrou com um numero valido.");
                    Console.Write("Por favor entre com um numero valido: ");
                }
            } while (true);
        }
    }
}