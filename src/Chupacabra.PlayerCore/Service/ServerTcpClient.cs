﻿using System;
using System.IO;
using System.Net.Sockets;
using NLog;

namespace Chupacabra.PlayerCore.Service
{
    public class ServerTcpClient : IDisposable, ILineReader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        readonly TcpClient _client;
        readonly StreamWriter _writer;
        readonly StreamReader _reader;

        public ServerTcpClient(string hostname, int port)
        {
            Logger.Info("Connecting to {0}:{1}...", hostname, port);
            this._client = new TcpClient(hostname, port)
            {
                NoDelay = true
            };
            this._writer = new StreamWriter(this._client.GetStream())
            {
                AutoFlush = true
            };
            this._reader = new StreamReader(this._client.GetStream());
            Logger.Info("Connection to {0}:{1} established.", hostname, port);

        }

        public void Dispose()
        {
            this._reader.Dispose();
            this._writer.Dispose();
            this._client.Close();
            Logger.Info("Connection closed.");
        }

        public void WriteLine(string text)
        {
            Logger.Info("Sending {0}", text);
            this._writer.WriteLine(text);
        }

        public string ReadLine()
        {
            var line = this._reader.ReadLine();
            Logger.Info("Response {0}", line);
            return line;
        }
    }
}
