using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chupacabra.PlayerCore.Host
{
    public abstract class EngineBase : IEngine
    {
        public string ServerHostname { get; set; }
        public int ServerPort { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

        protected CancellationToken _cancellationToken;
        protected Task _runningTask;

        public Task Start(CancellationToken cancellationToken)
        {
            lock (this)
            {
                if (_runningTask != null && !_runningTask.IsCompleted)
                    throw new InvalidOperationException("Engine is already running!");

                _cancellationToken = cancellationToken;
                _runningTask = Task.Factory.StartNew(Run, cancellationToken);
                return _runningTask;
            }
        }

        protected abstract void Run();
    }
}
