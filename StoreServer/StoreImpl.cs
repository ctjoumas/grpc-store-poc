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

        public override Task<Item> GetItemNumber(Beer request, ServerCallContext context)
        {
            return Task.FromResult(CheckItem(request));
        }

        public override Task ListStores(Beer request, IServerStreamWriter<StoreLocation> responseStream, ServerCallContext context)
        {
            // TODO: Implement
            return base.ListStores(request, responseStream, context);
        }

        private Item CheckItem(Beer beer)
        {
            var result = _items.FirstOrDefault((b) => b.Equals(beer));

            if (result == null)
            {
                // No beer was found, return an unnamed beer.
                return new Item() { ItemNumber = -1, Beer = beer };
            }

            return result;
        }
    }
}
