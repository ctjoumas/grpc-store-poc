using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Utils;

namespace Store
{
    public class StoreImpl : Store.StoreBase
    {
        readonly List<Item> _items;

        public StoreImpl(List<Item> items)
        {
            _items = items;
        }

        public override Task<Item> GetItem(Beer request, ServerCallContext context)
        {
            return Task.FromResult(CheckItem(request));
        }

        public override async Task ListStores(Beer request, IServerStreamWriter<StoreLocation> responseStream, ServerCallContext context)
        {
            var result = _items.FirstOrDefault((item) => item.Beer.Equals(request));
            foreach (var store in result.Stores)
            {
                await responseStream.WriteAsync(store);   
            }
        }

        public override async Task<TotalCost> GetTotalCost(IAsyncStreamReader<Beer> requestStream, ServerCallContext context)
        {
            int cost = 0;

            while (await requestStream.MoveNext())
            {
                var beer = requestStream.Current;

                var item = CheckItem(beer);

                if (item.Exists())
                {
                    cost += item.Cost;
                }
            }

            return new TotalCost { Cost = cost };
        }

        public override async Task GetEachItemCost(IAsyncStreamReader<Beer> requestStream, IServerStreamWriter<TotalCost> responseStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var beer = requestStream.Current;

                var item = CheckItem(beer);

                await responseStream.WriteAsync(new TotalCost { Cost = item.Cost });
            }
        }

        private Item CheckItem(Beer request)
        {
            var result = _items.FirstOrDefault((item) => item.Beer.Equals(request));

            if (result == null)
            {
                // No beer was found, return an beer with a negative item number
                return new Item() { ItemNumber = -1, Beer = request, Cost = -1 };
            }

            return result;
        }
    }
}
