using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

                    // Usually games in DL24 are divided into matches. Each match has its own "world". To be able to reload
                    // player during single match the state of the world is saved after each turn and it is restored when
                    // player is restarted.
                    var state = StateHelper.Load<WorldState>() ?? new WorldState();

                    int tick = 0;
                    var turnStopwatch = new Stopwatch();
                    while (true)
                    {
                        // Begining of a new turn.
                        turnStopwatch.Restart();
                        try
                        {
                            try
                            {
                                ++tick;
                                var turnNo = service.GetTurn();

                                // Monitor is shown by pressing SPACE BAR in console window.
                                Monitor.SetValue("engine/turn", turnNo);
                                Monitor.SetValue("engine/tick", tick);
                                Logger.Debug("tick {0}, turn {1}", tick, turnNo);
                                state.Something = string.Format("Turn: {0}, tick: {1}", turnNo, tick);

                                if (tick % 5 == 0)
                                {
                                    // Simulate some command.
                                    Logger.Info("data {0}", string.Join(", ", service.GetPrices()));
                                }

                                if (tick % 10 == 0)
                                {
                                    // Simulate command limit reached exception.
                                    Enumerable.Range(0, 20).ToList().ForEach(_ => service.GetPrices());
                                }

                            }
                            finally
                            {
                                Monitor.ConfirmTurn();
                                StateHelper.Save(state);
                                Logger.Info($"Turn length {turnStopwatch.ElapsedMilliseconds} ms");
                            }

                            // Wait till next turn.
                            // If you want to do some calculations before next turn starts you can use the value returned
                            // from Wait method to estimate how much time you have.
                            service.Wait();
                            service.WaitEnd();
                        }
                        catch (CommandsLimitReachedException ex)
                        {
                            Logger.Warn(ex.Message);
                            service.WaitEnd();
                        }

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
