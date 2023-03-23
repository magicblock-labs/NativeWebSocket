using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NativeWebSocket.implementation.WebGL
{
    internal class WebSocket : IWebSocket {

        /* WebSocket JSLIB functions */
        [DllImport ("__Internal")]
        public static extern int WebSocketConnect (int instanceId);

        [DllImport ("__Internal")]
        public static extern int WebSocketClose (int instanceId, int code, string reason);

        [DllImport ("__Internal")]
        public static extern int WebSocketSend (int instanceId, byte[] dataPtr, int dataLength);

        [DllImport ("__Internal")]
        public static extern int WebSocketSendText (int instanceId, string message);

        [DllImport ("__Internal")]
        public static extern int WebSocketGetState (int instanceId);

        protected int instanceId;

        public event WebSocketOpenEventHandler OnOpen;
        public event WebSocketMessageEventHandler OnMessage;
        public event WebSocketErrorEventHandler OnError;
        public event WebSocketCloseEventHandler OnClose;

        private TaskCompletionSource<object> _connectionTask;

        public WebSocket (string url, Dictionary<string, string> headers = null) {
          if (!WebSocketFactory.isInitialized) {
            WebSocketFactory.Initialize ();
          }

          int instanceId = WebSocketFactory.WebSocketAllocate (url);
          WebSocketFactory.instances.Add (instanceId, this);

          this.instanceId = instanceId;
        }

        public WebSocket (string url, string subprotocol, Dictionary<string, string> headers = null) {
          if (!WebSocketFactory.isInitialized) {
            WebSocketFactory.Initialize ();
          }

          int instanceId = WebSocketFactory.WebSocketAllocate (url);
          WebSocketFactory.instances.Add (instanceId, this);

          WebSocketFactory.WebSocketAddSubProtocol(instanceId, subprotocol);

          this.instanceId = instanceId;
        }

        public WebSocket (string url, List<string> subprotocols, Dictionary<string, string> headers = null) {
          if (!WebSocketFactory.isInitialized) {
            WebSocketFactory.Initialize ();
          }

          int instanceId = WebSocketFactory.WebSocketAllocate (url);
          WebSocketFactory.instances.Add (instanceId, this);

          foreach (string subprotocol in subprotocols) {
            WebSocketFactory.WebSocketAddSubProtocol(instanceId, subprotocol);
          }

          this.instanceId = instanceId;
        }

        ~WebSocket () {
          WebSocketFactory.HandleInstanceDestroy (instanceId);
        }

        public int GetInstanceId () {
          return instanceId;
        }

        public Task Connect (bool awaitConnection = true) {
          _connectionTask = new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
          int ret = WebSocketConnect (instanceId);

          if (ret < 0)
            throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);
          
          return awaitConnection ? _connectionTask.Task : Task.CompletedTask;
        }

        public void CancelConnection () {
	        if (State == WebSocketState.Open)
		        Close (WebSocketCloseCode.Abnormal);
        }
        public Task Close () {
          int ret = WebSocketClose (instanceId, (int) WebSocketCloseCode.Normal, null);

          if (ret < 0)
            throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);

          return Task.CompletedTask;
        }

        public Task Close (WebSocketCloseCode code = WebSocketCloseCode.Normal, string reason = null) {
          int ret = WebSocketClose (this.instanceId, (int) code, reason);

          if (ret < 0)
            throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);

          return Task.CompletedTask;
        }

        public Task Send (byte[] data) {
          int ret = WebSocketSend (this.instanceId, data, data.Length);

          if (ret < 0)
            throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);

          return Task.CompletedTask;
        }

        public Task SendText (string message) {
          int ret = WebSocketSendText (this.instanceId, message);

          if (ret < 0)
            throw WebSocketHelpers.GetErrorMessageFromCode (ret, null);

          return Task.CompletedTask;
        }

        public void DispatchMessageQueue() { }

        public WebSocketState State {
          get {
            int state = WebSocketGetState (instanceId);

            if (state < 0)
              throw WebSocketHelpers.GetErrorMessageFromCode (state, null);

            switch (state) {
              case 0:
                return WebSocketState.Connecting;

              case 1:
                return WebSocketState.Open;

              case 2:
                return WebSocketState.Closing;

              case 3:
                return WebSocketState.Closed;

              default:
                return WebSocketState.Closed;
            }
          }
        }

        public void DelegateOnOpenEvent ()
        {
          MainThreadUtil.Run(() => _connectionTask.TrySetResult(null));
          OnOpen?.Invoke ();
        }

        public void DelegateOnMessageEvent (byte[] data) {
          OnMessage?.Invoke (data);
        }

        public void DelegateOnErrorEvent (string errorMsg) {
          OnError?.Invoke (errorMsg);
        }

        public void DelegateOnCloseEvent (int closeCode) {
          OnClose?.Invoke (WebSocketHelpers.ParseCloseCodeEnum (closeCode));
        }

    }
}