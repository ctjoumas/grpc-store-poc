using System;
using Grpc.Core;

namespace Store
{
    class Program
    {
        static void Main(string[] args)
        {
            const int Port = 50052;

            var items = StoreUtil.ParseItems(StoreUtil.DefaultStoreFile);

            Server server = new Server
            {
                Services = { Store.BindService(new StoreImpl(items)) },
                Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
            };
            server.Start();

            Console.WriteLine("Store server listening on port " + Port);
            Console.WriteLine("Press any key to stop the server...");
            Console.ReadKey();

            server.ShutdownAsync().Wait();
        }
    }
}
