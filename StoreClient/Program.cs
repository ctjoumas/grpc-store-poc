using Grpc.Core;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Store
{
    class Program
    {
        public class StoreClient
        {
            readonly Store.StoreClient _client;

            public StoreClient(Store.StoreClient client)
            {
                _client = client;
            }

            /// <summary>
            /// Unary call example to get the item object of a specific beer
            /// </summary>
            public void GetItem(string style, string name, string brewery)
            {
                try
                {
                    Console.WriteLine(string.Format("*** GetItem: style={0} name={1} brewery={2}", style, name, brewery));

                    Beer request = new Beer { Style = style, Name = name, Brewery = brewery };

                    Item item = _client.GetItem(request);

                    if (item.Exists())
                    {
                        Console.WriteLine(string.Format("Found item with item number {0} \n", item.ItemNumber));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Could not find item with name {0} \n", item.Beer.Name));
                    }
                }
                catch (RpcException e)
                {
                    Console.WriteLine("RPC failed " + e);
                    throw;
                }
            }

            /// <summary>
            /// Asks for all stores for a given beer and prints the response (store location) as it arrives from the server.
            /// </summary>
            /// <returns></returns>
            public async Task ListStores(string style, string name, string brewery)
            {
                try
                {
                    Console.WriteLine(string.Format("*** ListStores: style={0} name={1} brewery={2}", style, name, brewery));

                    Beer request = new Beer { Style = style, Name = name, Brewery = brewery };

                    using (var call = _client.ListStores(request))
                    {
                        var responseStream = call.ResponseStream;
                        StringBuilder responseLog = new StringBuilder("Result: ");

                        while (await responseStream.MoveNext())
                        {
                            StoreLocation storeLocation = responseStream.Current;
                            responseLog.Append(storeLocation.ToString());
                        }
                        Console.WriteLine(responseLog.ToString() + "\n");
                    }
                }
                catch (RpcException e)
                {
                    Console.WriteLine("RPC failed " + e);
                    throw;
                }
            }
        }

        static void Main(string[] args)
        {
            var channel = new Channel("127.0.0.1:50052", ChannelCredentials.Insecure);
            var client = new StoreClient(new Store.StoreClient(channel));

            // Look for a valid beer
            client.GetItem("Stout", "Mocha Merlin", "Firestone Walker Brewing Company");

            // Look for an beer which doesn't exist
            client.GetItem("IPA", "90 Minute IPA", "Dogfish Head");

            // Look for a list of stores which sells a valid beer
            client.ListStores("Stout", "Mocha Merlin", "Firestone Walker Brewing Company").Wait();

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}