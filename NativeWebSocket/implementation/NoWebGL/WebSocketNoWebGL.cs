using System.Threading.Tasks;

namespace NativeWebSocket.implementation.NoWebGL
{
    internal class WebSocket : IWebSocket
    {
        public event WebSocketOpenEventHandler OnOpen;
        public event WebSocketMessageEventHandler OnMessage;
        public event WebSocketErrorEventHandler OnError;
        public event WebSocketCloseEventHandler OnClose;

        private WebSocketSharp.WebSocket sharpWebSocket;
        private string websocketUrl;
        private string[] subprotocols;
        
        public WebSocket(string uri)
        {
            websocketUrl = uri;
        }

        public WebSocket(string uri, string subprotocol)
        {
            websocketUrl = uri;
            subprotocols = new string[] { subprotocol };
        }
        
        public WebSocketState State
        {
            get
            {
                if(sharpWebSocket == null) return WebSocketState.None;
                switch (sharpWebSocket.ReadyState)
                {
                    case WebSocketSharp.WebSocketState.New:
                        return WebSocketState.Connecting;
                    case WebSocketSharp.WebSocketState.Connecting:
                        return WebSocketState.Connecting;
                    case WebSocketSharp.WebSocketState.Open:
                        return WebSocketState.Open;
                    case WebSocketSharp.WebSocketState.Closing:
                        return WebSocketState.Closing;
                    case WebSocketSharp.WebSocketState.Closed:
                        return WebSocketState.Closed;
                    default:
                        return WebSocketState.Closed;
                }
            }
        }

        public Task Connect(bool awaitConnection = true)
        {
            var connectionTask = new TaskCompletionSource<object>();
            sharpWebSocket = new WebSocketSharp.WebSocket(websocketUrl, subprotocols);
            sharpWebSocket.OnOpen += (sender, args) =>
            {
                connectionTask.TrySetResult(null);
                MainThreadUtil.Run(() => OnOpen?.Invoke());
            };
            sharpWebSocket.OnMessage += (sender, args) =>
            {
                MainThreadUtil.Run(() => OnMessage?.Invoke(args.RawData));
            };
            sharpWebSocket.OnClose += (sender, args) =>
            {
                MainThreadUtil.Run(() => OnClose?.Invoke (WebSocketHelpers.ParseCloseCodeEnum (args.Code)));
            };
            sharpWebSocket.OnError += (sender, args) =>
            {
                OnError?.Invoke(args.Message);
                MainThreadUtil.Run(() => OnError?.Invoke(args.Message));
            };

            sharpWebSocket.ConnectAsync();
            return awaitConnection ? connectionTask.Task : Task.CompletedTask;
        }

        public Task Close()
        {
            sharpWebSocket.Close();
            return Task.CompletedTask;
        }

        public Task Send(byte[] bytes)
        {
            sharpWebSocket.Send(bytes);
            return Task.CompletedTask;
        }

        public Task SendText(string message)
        {
            sharpWebSocket.Send(message);
            return Task.CompletedTask;
        }
    }
}