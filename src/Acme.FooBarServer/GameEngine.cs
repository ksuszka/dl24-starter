using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Acme.FooBarServer
{
    public class GameEngine
    {
        private readonly int _turnDuration;
        private DateTime _lastTickTime;
        private readonly Random random = new Random();
        private readonly IDictionary<string, int> _playerTokens = new Dictionary<string, int>();
        private readonly IDictionary<int, Player> _players = new Dictionary<int, Player>();
        private readonly ManualResetEvent _turnTick = new ManualResetEvent(false);

        public long TurnNumber
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get; private set;
        }

        public int TimeToNextTick
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            get { return Math.Max(0, _turnDuration - (DateTime.UtcNow - _lastTickTime).Milliseconds); }
        }

        public GameEngine(int turnDuration)
        {
            this._turnDuration = turnDuration;
            this.TurnNumber = 0;
            this._lastTickTime = DateTime.UtcNow;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private string GetLoginHash(string login, string password)
        {
            return string.Format("{0}#{1}", login, password);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddPlayers(IEnumerable<Tuple<string, string>> logins)
        {
            int nextId = 1;
            if (_playerTokens.Any())
            {
                nextId = _playerTokens.Values.Max() + 1;
            }

            foreach (var login in logins)
            {
                var hash = this.GetLoginHash(login.Item1, login.Item2);
                _playerTokens.Add(hash, nextId);
                _players.Add(nextId, new Player());
                ++nextId;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetPlayerId(string login, string password)
        {
            var hash = this.GetLoginHash(login, password);
            int id = 0;
            if (this._playerTokens.TryGetValue(hash, out id))
            {
                return id;
            }

            return 0;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public long GetPlayerEnergy(int id)
        {
            return this._players[id].Energy;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void NextTurn()
        {
            // Increase energy for each player
            var newEnergy = random.Next(10);
            foreach (var player in _players.Values)
            {
                player.CallCount = 0;
                player.Energy += newEnergy;
            }

            ++this.TurnNumber;
            this._lastTickTime = DateTime.UtcNow;
            this._turnTick.Set();
            this._turnTick.Reset();
        }

        public void WaitForNextTurn()
        {
            this._turnTick.WaitOne();
        }
    }
}
