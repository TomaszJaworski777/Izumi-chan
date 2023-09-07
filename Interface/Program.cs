using System;
using Engine;

namespace Interface
{
    internal class Program
    {
        static void Main( string[] args )
        {
            ChessEngine engine = new();
            CommandProcessor generalProcessor = new GeneralCommandProcessor(engine);

            // set uci as the default command processor (it is also, for now, the only protocol we have)
            CommandProcessor? interfaceProcessor = new UciCommandProcessor( engine );
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
 
