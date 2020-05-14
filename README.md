# grpc-store-poc
Implements a simple store gRPC service which demonstrates the four types of RPC:
- Simple RPC which returns a single message
- Server-to-client streaming RPC which accepts a single message and returns a stream of messages
- Client-to-server streaming RPC which accepts a stream of messages and returns a single message
- Bidirectional streaming RPC whcih accepts a stream of messages and returns a stream of messages

## Store project
This is the gRPC service, defined in the proto (protocol buffers) file, which describes the service interface and the structure of the payload messages. The Grpc.Tools NuGet package is added as a dependency and whill compile the protocol buffer (.proto file) and generate the necessary client and server-side code. Users will call the API from the client side and implement the corresponding API on the server side.

The key to ensuring the project is aware of the .proto file is to add the Protobuf element to the project file:

```
<ItemGroup>
    <Protobuf Include="protos\store.proto" Link="protos\store.proto" />
</ItemGroup>
```

Note that the generated files will be created in the obj\Debug\netcoreapp2.1 folder.

## StoreServer project
Listens on port 50052 of the localhost and implements the API generated from the protocol buffer.

### Example of implementing the simple RPC
```
public override Task<Item> GetItem(Beer request, ServerCallContext context)
{
    return Task.FromResult(CheckItem(request));
}
```

## StoreClient project
Connects to the server and calls the API.

### Example of calling the simple RPC
```
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
```

## To run the project
### Server
From the command line, navigate to the StoreServer\bin\Debug\netcoreapp2.1 folder and run <i>dotnet exec StoreServer.dll</i>

### Client
You can run this with the same command as for the Server or in Visual Studio, set the client as the startup project and run from there.
