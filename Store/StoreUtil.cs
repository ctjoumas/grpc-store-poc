using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Store
{
    public static class StoreUtil
    {
        public const string DefaultStoreFile = "store_db.json";

        /// <summary>
        /// Indicates whether the given item exists (i.e. has a valid item number)
        /// </summary>
        public static bool Exists(this Item item)
        {
            return item != null && (item.ItemNumber != -1);
        }

        public static List<Item> ParseItems(string filename)
        {
            var items = new List<Item>();
            var jsonItems = JsonConvert.DeserializeObject<List<JsonItem>>(File.ReadAllText(filename));

            foreach (var jsonItem in jsonItems)
            {
                items.Add(new Item
                {
                    ItemNumber = jsonItem.itemNumber,
                    Beer = new Beer { Name = jsonItem.beer.name, Style = jsonItem.beer.style, Brewery = jsonItem.beer.brewery },
                    Stores = { jsonItem.stores.Select(x => new StoreLocation() { Name = x }).ToList() }
                });
            }

            return items;
        }

#pragma warning disable 0649
        private class JsonItem
        {
            public int itemNumber;
            public JsonBeer beer;
            public List<string> stores;
        }

        private class JsonBeer
        {
            public string style;
            public string name;
            public string brewery;
        }
#pragma warning restore 0649
    }
}