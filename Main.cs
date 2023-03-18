using System;
using Grpc.Core;
using System.Threading;
using InkGRPC;

namespace ink_runtime_grpc
{
    class Program
    {
        const int Port = 50051;
        const string StoryPath = "/Stories";

        public static void Main(string[] args)
        {
            var inkserver = new InkServer(StoryPath);
            int loaded = inkserver.LoadStories();
            Console.WriteLine("Loaded: " + loaded + " stories.");
            Server server = new Server
            {
                Services = { Story.BindService(inkserver) },
                Ports = { new ServerPort("0.0.0.0", Port, ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine("Ink gRPC server listening on port " + Port);

            Thread.Sleep(Timeout.Infinite);
            server.ShutdownAsync().Wait();
        }
    }
}
