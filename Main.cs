using System;
using Grpc.Core;
using System.Threading;
using InkGRPC;

namespace ink_runtime_grpc
{
    class Program
    {
        const int Port = 50051;

        public static void Main(string[] args)
        {
            Server server = new Server
            {
                Services = { Story.BindService(new ServerImpl()) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            Thread.Sleep(1);
            Console.WriteLine("Greeter server listening on port " + Port);

            InkClient.Run();


            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();

        }
    }
}
