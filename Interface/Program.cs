using Engine;

namespace Interface
{
    internal class Program
    {
        static void Main( string[] args )
        {
            ChessEngine engine = new();
            CommandProcessor generalProcessor = new GeneralCommandProcessor(engine);
            CommandProcessor? interfaceProcessor = null;

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
 