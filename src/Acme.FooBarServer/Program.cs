using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Acme.FooBarServer
{
    /// <summary>
    /// Minimal DL24-like server implementation for testing purposes.
    /// </summary>
    class Program
    {
        enum State
        {
            WaitingForLogin,
            Disconnected,
            WaitingForCommand
        }

        private delegate void CommandHandler(
            StreamReader reader, StreamWriter writer, GameEngine engine, int playerId, string parameters);

        static void WaitCommand(StreamReader reader, StreamWriter writer, GameEngine engine, int playerId, string parameters)
        {
            writer.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, "OK\nWAITING {0:F6}", engine.TimeToNextTick / 1000.0));
            engine.WaitForNextTurn();
            writer.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, "OK"));
        }

        static void TurnCommand(StreamReader reader, StreamWriter writer, GameEngine engine, int playerId, string parameters)
        {
            var turnNumber = engine.TurnNumber;
            writer.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, "OK\n{0}", turnNumber));
        }

        private static void EnergyCommand(StreamReader reader, StreamWriter writer, GameEngine engine, int playerId, string parameters)
        {
            var energy = engine.GetPlayerEnergy(playerId);
            writer.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, "OK\n{0}", energy));
        }

        private static void PricesCommand(StreamReader reader, StreamWriter writer, GameEngine engine, int playerId, string parameters)
        {
            var turnNumber = engine.TurnNumber;
            var sb = new StringBuilder();
            sb.AppendLine("OK");
            sb.AppendLine(string.Format(NumberFormatInfo.InvariantInfo, "{0}", 42));
            sb.AppendLine(string.Format(NumberFormatInfo.InvariantInfo, "{0}", Math.Log10(turnNumber + 1)));
            writer.Write(sb);
        }

        static void HandleClient(TcpClient client, GameEngine engine)
        {
            var commandHandlers = new Dictionary<string, CommandHandler>()
            {
                {"WAIT", WaitCommand},
                {"TURN", TurnCommand},
                {"ENERGY", EnergyCommand},
                {"PRICES", PricesCommand}
            };

            try
            {
                Console.WriteLine("Accepted connection from {0}.", client.Client.RemoteEndPoint);
                client.NoDelay = true;
                using (var writer = new StreamWriter(client.GetStream()))
                {
                    writer.AutoFlush = true;
                    using (var reader = new StreamReader(client.GetStream()))
                    {
                        int playerId = 0;

                        long lastTurnNo = 0;
                        int commandCount = 0;

                        var currentState = State.WaitingForLogin;
                        bool isConnected = true;
                        while (isConnected)
                        {
                            switch (currentState)
                            {
                                case State.WaitingForLogin:
                                    {
                                        writer.WriteLine("LOGIN");
                                        var login = reader.ReadLine();
                                        writer.WriteLine("PASS");
                                        var password = reader.ReadLine();
                                        playerId = engine.GetPlayerId(login, password);
                                        if (playerId > 0)
                                        {
                                            writer.WriteLine("OK");
                                            currentState = State.WaitingForCommand;
                                        }
                                        else
                                        {
                                            writer.WriteLine(ErrorResponses.BadLoginOrPassword);
                                            currentState = State.Disconnected;
                                        }
                                    }
                                    break;
                                case State.Disconnected:
                                    {
                                        isConnected = false;
                                    }
                                    break;
                                case State.WaitingForCommand:
                                    {
                                        // TODO: make it OOD way.
                                        var rawCommand = reader.ReadLine();
                                        var tokens = rawCommand.Split(" \t".ToArray());
                                        var commandName = tokens.First().ToUpper();
                                        var commandParameters = tokens.Skip(1).FirstOrDefault();
                                        CommandHandler handler;
                                        if (commandHandlers.TryGetValue(commandName, out handler))
                                        {
                                            // Count commands in turn
                                            var turnNo = engine.TurnNumber;
                                            if (turnNo > lastTurnNo)
                                            {
                                                commandCount = 0;
                                                lastTurnNo = turnNo;
                                            }
                                            commandCount++;
                                            if (commandCount > 10)
                                            {
                                                writer.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, "{0}\nWAITING {1:F6}", ErrorResponses.CommandLimitReaching, engine.TimeToNextTick / 1000.0));
                                                engine.WaitForNextTurn();
                                                writer.WriteLine(string.Format(NumberFormatInfo.InvariantInfo, "OK"));
                                            }
                                            else
                                            {
                                                handler(reader, writer, engine, playerId, commandParameters);
                                            }
                                        }
                                        else
                                        {
                                            writer.WriteLine(ErrorResponses.UnknownCommand);
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
                client.Close();
                Console.WriteLine("Connection closed.");
            }
            catch (Exception se)
            {
                Console.WriteLine("Connection closed with errors: {0}", se);
            }
        }


        static void Main(string[] args)
        {
            int turnDuration = 2000;    // ms
            var engine = new GameEngine(turnDuration);

            engine.AddPlayers(new Tuple<string, string>[]
                {
                    Tuple.Create("zenek", "gitara"),
                    Tuple.Create("a", "a"),
                    Tuple.Create("b", "b"),
                    Tuple.Create("c", "d"),
                    Tuple.Create("d", "d")
                });
            int port = 20000;

            Task.Factory.StartNew(
                () =>
                {
                    while (true)
                    {
                        Thread.Sleep(turnDuration);
                        Console.WriteLine("Turn {0}...", engine.TurnNumber);
                        engine.NextTurn();
                    }
                });

            Console.WriteLine("Starting server on port {0}...", port);
            var listener = new TcpListener(new IPAddress(new byte[] { 0, 0, 0, 0 }), port);
            listener.Start();
            while (true)
            {
                var client = listener.AcceptTcpClient();
                Task.Factory.StartNew(() => HandleClient(client, engine));
            }
        }
    }
}
