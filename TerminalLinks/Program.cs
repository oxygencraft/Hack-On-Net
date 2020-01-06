using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalLinks
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length >= 8)
                AutoStartup(args);
            while (true)
            {
                StartMenu();
            }
        }

        static void StartMenu()
        {
            Console.Clear();
            Console.WriteLine("----  TerminalLinks v0.1  ----");
            Console.WriteLine("A terminal client for Hacklinks");
            Console.WriteLine("------------------------------------------------------------------------------------------------");
            Console.WriteLine("You can automatically login on startup by using the user and password parameters");
            Console.WriteLine("Example: TerminalLinks.exe -server 127.0.0.1 -port 27015 -user test -pass password\n");
            Console.WriteLine("To exit, leave all fields blank");
            Console.WriteLine("------------------------------------------------------------------------------------------------\n");
            Console.WriteLine("----  LOGIN  ----");
            Console.Write("SERVER:    ");
            string server = Console.ReadLine();
            Console.Write("\nPORT (leave blank if unknown):  ");
            int port = 27015;
            if (!int.TryParse(Console.ReadLine(), out port))
                port = 27015;
            Console.Write("\nUSERNAME:  ");
            string user = Console.ReadLine();
            Console.Write("\nPASSWORD:  ");
            string password = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(server))
                return;
            Console.WriteLine();
            TerminalLinks.StartGame(server, port, user, password);
        }

        static void AutoStartup(string[] args)
        {
            string server = args[1];
            int port = 27015;
            if (!int.TryParse(args[3], out port))
                port = 27015;
            string user = args[5];
            string password = args[7];
            TerminalLinks.StartGame(server, port, user, password);
        }
    }
}
