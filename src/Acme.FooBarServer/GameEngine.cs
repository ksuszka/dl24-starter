using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Acme.FooBarServer
{
    public class GameEngine
    {
        private readonly int _turnDuration;
        public IDictionary<int, Player> Players { get; private set; }

        public IDictionary<string, int> PlayerTokens { get; private set; }
        public long TurnNumber { get; private set; }
        public ManualResetEvent TurnTick { get; private set; }
        private Random random = new Random();
        private DateTime _lastTickTime;

        public int TimeToNextTick
        {
            get { return Math.Max(0, _turnDuration - (DateTime.UtcNow - _lastTickTime).Milliseconds); }
        }

        public GameEngine(int turnDuration)
        {
            this._turnDuration = turnDuration;
            this.Players = new Dictionary<int, Player>();
            this.PlayerTokens = new Dictionary<string, int>();
            this.TurnNumber = 0;
            this.TurnTick = new ManualResetEvent(false);
            this._lastTickTime = DateTime.UtcNow;
        }

        private string GetLoginHash(string login, string password)
        {
            return string.Format("{0}#{1}", login, password);
        }

        public void AddPlayers(IEnumerable<Tuple<string, string>> logins)
        {
            int nextId = 1;
            if (PlayerTokens.Any())
            {
                nextId = PlayerTokens.Values.Max() + 1;
            }

            foreach (var login in logins)
            {
                var hash = this.GetLoginHash(login.Item1, login.Item2);
                PlayerTokens.Add(hash, nextId);
                Players.Add(nextId, new Player());
                ++nextId;
            }
        }

        public int GetPlayerId(string login, string password)
        {
            var hash = this.GetLoginHash(login, password);
            int id = 0;
            if (this.PlayerTokens.TryGetValue(hash, out id))
            {
                return id;
            }

            return 0;
        }

        public long GetPlayerEnergy(int id)
        {
            return this.Players[id].Energy;
        }

        public void NextTurn()
        {
            // Increase energy for each player
            var newEnergy = random.Next(10);
            foreach (var player in Players.Values)
            {
                player.CallCount = 0;
                player.Energy += newEnergy;
            }

            ++this.TurnNumber;
            this._lastTickTime = DateTime.UtcNow;
            this.TurnTick.Set();
            this.TurnTick.Reset();
        }
    }
}
