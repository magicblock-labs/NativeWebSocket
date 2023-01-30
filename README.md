# NativeWebSocket

NativeWebSocket uses a combination of [NativeWebSocket](https://github.com/endel/NativeWebSocket) and [websocket-sharp](https://github.com/sta/websocket-sharp) to work on all the tested Unity build platform (WebGL, IOS, Android, Dektop).

Both packages has been made compatible with Unity, compiling them for dotnet 2.0 and the convience interfaca make the internal implementation transparents to the users.

## How to Install

Drag into Unity the 3 needed files:

- NativeWebSocket.dll
- NativeWebSocket.jslib
- websocket-sharp-latest.dll

The files are available at the latest release: https://github.com/garbles-labs/NativeWebSocket/releases


## How to Use:


### Creating a connection

```csharp
webSocket = NativeWebSocket.WebSocket.Create("wss://api.mainnet-beta.solana.com:443");
```

### Opening the connection, sending and receive messages

```csharp
webSocket.OnOpen += () =>
{
    Debug.Log("Connection open!");
    string accountSubscribeParams =
        "{\"jsonrpc\":\"2.0\",\"id\":" + 0 +
        ",\"method\":\"accountSubscribe\",\"params\":[\"" + acc.Key +
        "\",{\"encoding\":\"jsonParsed\",\"commitment\":\"confirmed\"}]}";
    webSocket.Send(System.Text.Encoding.UTF8.GetBytes(accountSubscribeParams)).RunSynchronously();
};
webSocket.OnMessage += bytes =>
{
    var message = System.Text.Encoding.UTF8.GetString(bytes);
    Debug.Log("SocketMessage:" + message);
};
webSocket.OnClose += (e) => Debug.Log("Connection closed!");
webSocket.Connect();
```


