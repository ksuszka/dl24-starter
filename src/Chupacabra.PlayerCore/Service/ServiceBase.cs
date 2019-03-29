using System;
using System.Text.RegularExpressions;

namespace Chupacabra.PlayerCore.Service
{
    public abstract class ServiceBase : IDisposable
    {
        protected ServerTcpClient Client { get; private set; }

        protected ServiceBase(string hostname, int port)
        {
            this.Client = new ServerTcpClient(hostname, port);
        }

        public void Login(string login, string password)
        {
            var line = this.Client.ReadLine();
            if (line != "LOGIN") throw new Exception(string.Format("Unexpected login response: {0}!", line));
            this.Client.WriteLine(login);
            line = this.Client.ReadLine();
            if (line != "PASS") throw new Exception(string.Format("Unexpected login response: {0}!", line));
            this.Client.WriteLine(password);
            ProcessResponseHeader();
        }

        public void Dispose()
        {
            Client.Dispose();
        }

        static readonly Regex FailedResponse = new Regex(@"^\s*FAILED\s+(?<Code>\d+)\s+(?<Message>.+)\s*$");
        static readonly Regex ForcedWaitingResponse = new Regex(@"^\s*WAITING\s+(?<Time>[\d\.]+)\s*$");

        protected void ProcessResponseHeader()
        {
            var line = this.Client.ReadLine();
            if (line == "OK") return;
            //if (line.StartsWith("FAILED 6 ")) { return;   // last command.
            var match = FailedResponse.Match(line);

            if (!match.Success)
                throw new InvalidOperationException(string.Format("Unexpected login response: {0}!", line));

            var errorCode = Int32.Parse(match.Groups["Code"].Value);
            var message = match.Groups["Message"].Value;

            if (errorCode != 6) throw new ServerException(errorCode, message);

            line = this.Client.ReadLine();
            match = ForcedWaitingResponse.Match(line);
            if (match.Success)
            {
                var time = (int)(double.Parse(match.Groups["Time"].Value, System.Globalization.CultureInfo.InvariantCulture) * 1000);
                throw new CommandsLimitReachedException(time, errorCode, message);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Unexpected login response: {0}!", line));
            }
        }

        protected void SendCommand(string format, params object[] args)
        {
            var command = string.Format(format, args);
            this.Client.WriteLine(command);
            ProcessResponseHeader();
        }

        public int Wait()
        {
            SendCommand("WAIT");
            var tokens = this.Client.ReadLine().Split(' ');
            return (int)(double.Parse(tokens[1], System.Globalization.NumberFormatInfo.InvariantInfo) * 1000);
        }

        public void WaitEnd()
        {
            ProcessResponseHeader();
        }

        protected Tokenizer ReadTokens()
        {
            return new Tokenizer(this.Client);
        }

    }
}
