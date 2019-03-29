using System;
using System.IO;
using System.Net.Sockets;
using Serilog;

namespace Chupacabra.PlayerCore.Service
{
    public class ServerTcpClient : IDisposable, ILineReader
    {
        readonly TcpClient _client;
        readonly StreamWriter _writer;
        readonly StreamReader _reader;

        public ServerTcpClient(string hostname, int port)
        {
            Log.Debug("Connecting to {Hostname}:{Port}...", hostname, port);
            this._client = new TcpClient(hostname, port)
            {
                NoDelay = true
            };
            this._writer = new StreamWriter(this._client.GetStream())
            {
                AutoFlush = true
            };
            this._reader = new StreamReader(this._client.GetStream());
            Log.Debug("Connection to {Hostname}:{Port} established.", hostname, port);

        }

        public void Dispose()
        {
            this._reader.Dispose();
            this._writer.Dispose();
            this._client.Close();
            Log.Debug("Connection closed.");
        }

        public void WriteLine(string text)
        {
            Log.Debug("-> {0}", text);
            this._writer.WriteLine(text);
        }

        public string ReadLine()
        {
            var line = this._reader.ReadLine();
            Log.Debug("<- {0}", line);
            return line;
        }
    }
}
