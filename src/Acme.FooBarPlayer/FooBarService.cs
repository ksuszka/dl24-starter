using Chupacabra.PlayerCore.Service;
using System;
using System.Collections.Generic;

namespace Acme.FooBarPlayer
{
    class FooBarService : ServiceBase
    {
        public FooBarService(string hostname, int port)
            : base(hostname, port)
        {

        }

        public int GetTurn()
        {
            SendCommand("TURN");
            return ReadTokens().ReadInt();
        }

        public IEnumerable<double> GetPrices()
        {
            SendCommand("PRICES");
            // ReadTokens method can be used to read output from command returing data separated by white spaces.
            var reader = ReadTokens();
            return new Double[]
            {
                reader.ReadInt(),
                reader.ReadDouble()
            };
        }

        public void LowLevelCommandWithNoParameters()
        {
            this.Client.WriteLine("COMMAND0");
            ProcessResponseHeader();
        }

        public int LowLevelGetTurn()
        {
            // Same command as GetTurn but without using high level helper methods
            this.Client.WriteLine("TURN");
            ProcessResponseHeader();
            return int.Parse(this.Client.ReadLine());
        }
    }
}
