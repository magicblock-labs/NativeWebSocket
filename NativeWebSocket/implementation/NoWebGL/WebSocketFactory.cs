namespace NativeWebSocket.implementation.NoWebGL
{
    internal static class WebSocketFactory
    {
        /// <summary>
        /// Create WebSocket client instance
        /// </summary>
        /// <returns>The WebSocket instance.</returns>
        /// <param name="url">WebSocket valid URL.</param>
        internal static WebSocket CreateInstance(string url)
        {
            return new WebSocket(url);
        }
    }
}