using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HackLinks_Server
{
    class Program
    {
        public class StateObject
        {
            public Socket workSocket = null;
            public const int BufferSize = 2048;
            public byte[] buffer = new byte[BufferSize];
            public StringBuilder sb = new StringBuilder();
        }

        public static bool recieving = false;

        static void Main(string[] args)
        {
            IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 27015);

            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    if(recieving == false)
                    {
                        Console.WriteLine("Waiting for a connection...");
                        listener.BeginAccept(
                            new AsyncCallback(AcceptCallback),
                            listener);
                        recieving = true;
                    }
                    Server.Instance.MainLoop();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            recieving = false;

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            Server.Instance.AddClient(handler);
        }

        

    }
}
