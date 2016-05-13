using System;
using System.Collections.Generic;
using Chupacabra.PlayerCore.Host;
using Chupacabra.PlayerCore.Service;
using NLog;

namespace Acme.FooBarPlayer
{
    class FooBarEngine : EngineBase
    {
        public IStatusMonitor Monitor { get; set; }

        void IgnoreErrors(Action action, IList<int> errorCodes)
        {
            try 
            {
                action();
            }
            catch (ServerException se)
            {
                if (errorCodes.Contains(se.ErrorCode))
                {
                    Logger.Warn("IGNORING: {0}", se.Message);
                }
                else
                    throw;
            }

        }

        protected override void Run()
        {
            try
            {
                Logger.Info("Processing started.");
                using (var service = new FooBarService(ServerHostname, ServerPort))
                {
                    service.Login(Login, Password);

                    var state = StateHelper.Load<Dictionary<int, int>>() ?? new Dictionary<int, int>();
                    int tick = 0;
                    while (true)
                    {
                        //ProcessCommands();
                        ++tick;
                        var turnNo = service.GetTurn();
                        Monitor.SetValue("engine/turn", turnNo);
                        Monitor.SetValue("engine/tick", tick);
                        Logger.Debug("tick {0}, turn {1}", tick, turnNo);
                        state[turnNo] = tick;
                        if (tick%10 == 0)
                        {
                            Logger.Info("data {0}", string.Join(", ", service.GetPrices()));
                        }
                        Monitor.ConfirmTurn();
                        StateHelper.Save(state);

                        // Wait till next turn.
                        // If you want to do some calculations before next turn starts you can use the value returned
                        // from Wait method to estimate how much time you have.
                        service.Wait();
                        service.WaitEnd();
                    }
                }
                Logger.Info("Processing finished.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                LogManager.Flush();
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // If this code is run under a debugger then we signal it that something went really wrong.
                    System.Diagnostics.Debugger.Break();
                }
            }
        }
    }
}
