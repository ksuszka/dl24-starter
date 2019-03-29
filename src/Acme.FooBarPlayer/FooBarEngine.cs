﻿using Chupacabra.PlayerCore.Host;
using Chupacabra.PlayerCore.Service;
using System;
using System.Diagnostics;
using System.Linq;
using Serilog;

namespace Acme.FooBarPlayer
{
    class FooBarEngine : EngineBase
    {
        public StateHelper StateHelper { get; set; }
        public IStatusMonitor Monitor { get; set; }

        void IgnoreErrors(Action action, params int[] errorCodes)
        {
            try 
            {
                action();
            }
            catch (ServerException se)
            {
                if (errorCodes.Contains(se.ErrorCode))
                {
                    Log.Warning("IGNORING: {Message}", se);
                }
                else
                    throw;
            }
        }

        void IgnoreAllErrors(Action action)
        {
            try
            {
                action();
            }
            catch (ServerException se)
            {
                Log.Warning("IGNORING: {Message}", se);
            }
        }

        protected override void Run()
        {
            try
            {
                Log.Information("Processing started.");
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
                        // Beginning of a new turn.
                        turnStopwatch.Restart();
                        try
                        {
                            try
                            {
                                // If the game can have multiple matches/worlds here should be code which detects if world has changed
                                // and resets world state. Something like:
                                // var world = service.DescribeWorld();
                                // if (world.turnNo < state.world.turnNo || world.boardSize != state.world.boardSize || ...)
                                // {
                                //     state.world = world;
                                //     // reset other things
                                // }

                                ++tick;
                                var turnNo = service.GetTurn();

                                // Monitor is shown by pressing SPACE BAR in console window.
                                Monitor.Set("engine/turn", turnNo);
                                Monitor.Set("engine/tick", tick);
                                Log.Debug("tick {Tick}, turn {TurnNo}", tick, turnNo);
                                state.Something = string.Format("Turn: {0}, tick: {1}", turnNo, tick);

                                // More complex sample of using monitor
                                Monitor.Delete("example1");
                                Monitor.Set(string.Format("example1/u_{0}", turnNo), tick);

                                Monitor.DeleteChildren("example2");
                                Monitor.Set(string.Format("example2/u_{0}", turnNo), tick);

                                if (tick % 5 == 0)
                                {
                                    // Simulate some command.
                                    Log.Information("data {0}", string.Join(", ", service.GetPrices()));

                                    // Another command for which we ignore some errors
                                    IgnoreErrors(() =>
                                    {
                                        Log.Information("data {0}", string.Join(", ", service.GetPrices()));
                                    }, 101, 102);
                                }

                                if (tick % 10 == 0)
                                {
                                    // Simulate command limit reached exception.
                                    Enumerable.Range(0, 20).ToList().ForEach(_ => service.GetPrices());
                                }
                            }
                            finally
                            {
                                // Mark turn as finished even in case of error.
                                Monitor.ConfirmTurn();
                                StateHelper.Save(state);
                                Log.Information("Turn length {TurnLength} ms", turnStopwatch.ElapsedMilliseconds);
                            }

                            // Wait till next turn.
                            // If you want to do some calculations before next turn starts you can use the value returned
                            // from Wait method to estimate how much time you have.
                            service.Wait();
                            service.WaitEnd();
                        }
                        catch (CommandsLimitReachedException ex)
                        {
                            Log.Warning("{Message}", ex);
                            service.WaitEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception {Exception}");
                Log.CloseAndFlush();
                if (Debugger.IsAttached)
                {
                    // If this code is run under a debugger then we signal it that something went really wrong.
                    Debugger.Break();
                }
            }
        }
    }
}
