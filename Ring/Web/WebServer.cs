using Ring.Web.Core.Extensions;
using System;
using System.Net;
using System.Threading;

namespace Ring.Web
{
    /// <summary>
    /// Http listener
    /// </summary>
    public sealed class WebServer : IDisposable
    {
        private readonly HttpListener _listener = new HttpListener();
        private readonly int _port;

        /// <summary>
        /// Ctor
        /// </summary>
        public WebServer(int port)
        {
            _port = port;
            _listener.Prefixes.Add(string.Format(Constants.HttpListenerPrefix, _port.ToString()));
        }

        public int Port => _port;

        /// <summary>
        /// Start listener
        /// </summary>
        public void Start()
        {
            _listener.Start();
            if (!HttpListener.IsSupported) throw new NotSupportedException(Constants.MsgClassNotSupported);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                Console.WriteLine(Constants.MsgServerRunning);
                ThreadPool.SetMinThreads(10, 10);
                ThreadPool.SetMaxThreads(1024, 1024);
                try
                {
                    while (_listener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem(c =>
                        {
                            try
                            {
                                (c as HttpListenerContext)?.HandleRestApi();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }, _listener.GetContext());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }, System.Threading.Tasks.TaskCreationOptions.LongRunning);
        }
        public void Dispose()
        {
            Stop();
        }
        public void Stop()
        {
            if (!_listener.IsListening) return;
            _listener.Stop();
            _listener.Close();
        }
    }
}
