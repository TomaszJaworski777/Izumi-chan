using System;
using Engine;
using Engine.Move;
using Engine.Search.TranspositionTables;

namespace Interface
{
    internal class Program
    {
        static void Main( string[] args )
        {
            MoveHistory.Reset();
            TranspositionTable.Reset();

            ChessEngine engine = new();
            CommandProcessor generalProcessor = new GeneralCommandProcessor(engine);
            CommandProcessor? interfaceProcessor = null;
#if DEBUG
            Console.WriteLine("DEBUG MODE");
#endif
            while (true)
            {
                string? command = Console.ReadLine();
                if (command is null) 
                    continue;

                string[] commandSplit = command.Split(' ');
                generalProcessor.ProcessCommand( commandSplit );
                interfaceProcessor?.ProcessCommand( commandSplit );

                if (command == "uci")
                    interfaceProcessor = new UciCommandProcessor( engine );
            }
        }
    }
}
 
