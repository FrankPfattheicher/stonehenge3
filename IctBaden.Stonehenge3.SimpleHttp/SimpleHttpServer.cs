using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace IctBaden.Stonehenge3.SimpleHttp
{
    /// <summary>
    /// Based on David Jeske's work.
    /// https://github.com/jeske/SimpleHttpServer
    /// </summary>
    internal class SimpleHttpServer
    {
        protected int Port;
        private TcpListener _listenerSocket;

        private Thread _listenerThread;

        public bool IsActive { get; private set; }

        internal SimpleHttpServer(int port)
        {
            Port = port;
            IsActive = false;
        }

        public void Start()
        {
            IsActive = true;
            _listenerThread = new Thread(Listen);
            _listenerThread.Start();
        }
        public void Terminate()
        {
            IsActive = false;

            var listener = _listenerSocket;
            _listenerSocket = null;

            listener?.Stop();

            var thread = _listenerThread;
            _listenerThread = null;

            thread?.Abort();
        }

        public void Listen()
        {
            _listenerSocket = new TcpListener(IPAddress.Any, Port);
            _listenerSocket.Start();
            while (IsActive)
            {
                try
                {
                    var socket = _listenerSocket.AcceptTcpClient();
                    var processor = new SimpleHttpProcessor(socket, this);
                    var processThread = new Thread(processor.Process);
                    processThread.Start();
                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }


        public event Action<SimpleHttpProcessor> HandleGet;
        public event Action<SimpleHttpProcessor, StreamReader> HandlePost;

        internal void HandleGetRequest(SimpleHttpProcessor processor)
        {
            HandleGet?.Invoke(processor);
        }

        internal void HandlePostRequest(SimpleHttpProcessor processor, StreamReader contentStream)
        {
            HandlePost?.Invoke(processor, contentStream);
        }
    }

}
