using Grpc.Core;
using System;
using System.Collections.Generic;
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

            public async Task GetTotalCost(List<Beer> beers)
            {
                try
                {
                    Console.WriteLine("*** GetTotalCost");
                    using (var call = _client.GetTotalCost())
                    {
                        Random random = new Random();

                        foreach (Beer beer in beers)
                        {
                            Item item = _client.GetItem(beer);

                            if (item.Exists())
                            {
                                Console.WriteLine(string.Format("*** Getting cost of beer: style={0} name={1} brewery={2}", beer.Style, beer.Name, beer.Brewery));

                                await call.RequestStream.WriteAsync(beer);

                                // add a small delay before sending the next one
                                await Task.Delay(random.Next(1000) + 500);
                            }
                            else
                            {
                                Console.WriteLine(string.Format("Could not find item with name {0} \n", item.Beer.Name));
                            }
                        }
                        await call.RequestStream.CompleteAsync();

                        TotalCost cost = await call.ResponseAsync;
                        Console.WriteLine(string.Format("*** Total cost of beers is ${0}\n", cost.Cost));
                    }
                }
                catch (RpcException e)
                {
                    Console.WriteLine(string.Format("RPC failed", e));
                    throw;
                }
            }

            public async Task GetEachItemCost(List<Beer> beers)
            {
                try
                {
                    Console.WriteLine("*** GetEachItemCost");

                    using (var call = _client.GetEachItemCost())
                    {
                        var responseReaderTask = Task.Run(async () =>
                        {
                            while (await call.ResponseStream.MoveNext())
                            {
                                var totalCost = call.ResponseStream.Current;
                                Console.WriteLine(string.Format("Got message \"${0}\"", totalCost.Cost));
                            }
                        });

                        foreach (Beer beer in beers)
                        {
                            Item item = _client.GetItem(beer);

                            if (item.Exists())
                            {
                                Console.WriteLine(string.Format("Sending message style={0}, name={1}, brewery={2}", beer.Style, beer.Name, beer.Brewery));

                                await call.RequestStream.WriteAsync(beer);
                            }
                            else
                            {
                                Console.WriteLine(string.Format("Could not find item with name {0} \n", item.Beer.Name));
                            }
                        }
                        await call.RequestStream.CompleteAsync();
                        await responseReaderTask;

                        Console.WriteLine("Finished GetEachItemCost");
                    }
                }

                catch (RpcException e)
                {
                    Console.WriteLine(string.Format("RPC failed", e));
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

            // Create Beer objects to get a total cost
            List<Beer> beers = new List<Beer>()
            {
                new Beer() { Style = "Stout", Name = "Mocha Merlin", Brewery = "Firestone Walker Brewing Company" },
                new Beer() { Style = "Stout", Name = "Chicory Stout", Brewery = "Dogfish Head" },
                new Beer() { Style = "Imperial IPA", Name = "Fake Beer", Brewery = "My Awesome Brewing Company" },
                new Beer() { Style = "Imperial IPA", Name = "Double Trouble IPA", Brewery = "Founders Brewing Company" }
            };

            client.GetTotalCost(beers).Wait();

            client.GetEachItemCost(beers).Wait();

            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}