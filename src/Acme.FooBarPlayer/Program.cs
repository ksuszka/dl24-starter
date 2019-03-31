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

            var title = $"FooBar {config["ServerPort"]}";

            _fileStatusMonitor = new FileStatusMonitor("status.txt");
            // using (_formsStatusMonitor = new StatusMonitorDialogHost(title + " Status"))
            // {
            _engine = new FooBarEngine()
            {
                StateHelper = new StateHelper(config["StateFilename"]),
                ServerHostname = config["ServerHostname"],
                ServerPort = int.Parse(config["ServerPort"]),
                Login = config["Login"],
                Password = config["Password"],
                //                    Monitor = new CompositeStatusMonitor(_fileStatusMonitor, _formsStatusMonitor.StatusMonitor)
                Monitor = _fileStatusMonitor
            };

            if (args.Length > 0)
            {
                _engine.ServerPort = int.Parse(args[0]);
            }

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
