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
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("customsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
            // ReadCustomSettings();

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

        // void ReadCustomSettings()
        // {
        //     const string customSettingFile = "settings.json";
        //     var files = new List<string>();
        //     var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        //     while (directory != null)
        //     {
        //         var file = Path.Combine(directory.FullName, customSettingFile);
        //         if (File.Exists(file))
        //         {
        //             files.Add(file);
        //         }
        //         directory = directory.Parent;
        //     }

        //     files.Reverse();
        //     if (files.Any())
        //     {
        //         files.ForEach(file =>
        //         {
        //             Log.Information("Reading settings from {File} file.", file);
        //             var settings = File.ReadAllText(file);
        //             JsonConvert.PopulateObject(settings, Properties.Settings.Default);
        //         });
        //     }
        //     else
        //     {
        //         Log.Information("No custom settings file found.");
        //     }
        // }
    }
}
