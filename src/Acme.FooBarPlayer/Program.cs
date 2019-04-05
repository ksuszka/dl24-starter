using Chupacabra.PlayerCore.Host;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;

namespace Acme.FooBarPlayer
{
    class Program
    {
        private FooBarEngine _engine;
        private FileStatusMonitor _fileStatusMonitor;
        // private IStatusMonitorDialog _formsStatusMonitor;

        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        private void Run(string[] args)
        {
            var config = ReadConfiguration();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();

            var serverPort = int.Parse((args.Length > 0) ? args[0] : config["ServerPort"] ?? "0");
            var title = $"FooBar {serverPort}";

            _fileStatusMonitor = new FileStatusMonitor("status.txt");
            // using (_formsStatusMonitor = new StatusMonitorDialogHost(title + " Status"))
            // {
            _engine = new FooBarEngine()
            {
                StateHelper = new StateHelper(config["StateFilename"] ?? "state.json"),
                ServerHostname = config["ServerHostname"] ?? "localhost",
                ServerPort = serverPort,
                Login = config["Login"],
                Password = config["Password"],
                //                    Monitor = new CompositeStatusMonitor(_fileStatusMonitor, _formsStatusMonitor.StatusMonitor)
                Monitor = _fileStatusMonitor
            };

            Core.RunConsole(_engine, title, KeyHandler);
            // }
        }

        void KeyHandler(ConsoleKeyInfo keyInfo)
        {
            // _formsStatusMonitor.Show();
        }

        IConfiguration ReadConfiguration()
        {
            // Search for valid configuration files
            var configFiles = new List<string> { "settings.json", "appsettings.json" };
            var files = new List<string>();
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                files.AddRange(configFiles
                    .Select(_ => Path.Combine(directory.FullName, _))
                    .Where(_ => File.Exists(_)));
                directory = directory.Parent;
            }

            files.Reverse();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            configBuilder = files.Aggregate(configBuilder, (cb, file) => cb.AddJsonFile(file, optional: true, reloadOnChange: true));

            return configBuilder.Build();
        }
    }
}
