using Chupacabra.PlayerCore.Host;
using Chupacabra.PlayerCore.Host.Forms;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Acme.FooBarPlayer
{
    class Program
    {
        protected readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private FooBarEngine _engine;
        private FileStatusMonitor _fileStatusMonitor;
        private IStatusMonitorDialog _formsStatusMonitor;

        static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        private void Run(string[] args)
        {

            ReadCustomSettings();

            var title = $"FooBar {Properties.Settings.Default.ServerPort}";

            _fileStatusMonitor = new FileStatusMonitor("status.txt");
            using (_formsStatusMonitor = new StatusMonitorDialogHost(title + " Status"))
            {
                _engine = new FooBarEngine()
                {
                    ServerHostname = Properties.Settings.Default.ServerHostname,
                    ServerPort = Properties.Settings.Default.ServerPort,
                    Login = Properties.Settings.Default.Login,
                    Password = Properties.Settings.Default.Password,
                    Monitor = new CompositeStatusMonitor(_fileStatusMonitor, _formsStatusMonitor.StatusMonitor)
                };

                if (args.Length > 0)
                {
                    _engine.ServerPort = int.Parse(args[0]);
                }

                Core.RunConsole(_engine, title, KeyHandler);
            }
        }

        void KeyHandler(ConsoleKeyInfo keyInfo)
        {
            _formsStatusMonitor.Show();
        }

        void ReadCustomSettings()
        {
            const string customSettingFile = "settings.json";
            var files = new List<string>();
            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (directory != null)
            {
                var file = Path.Combine(directory.FullName, customSettingFile);
                if (File.Exists(file))
                {
                    files.Add(file);
                }
                directory = directory.Parent;
            }

            files.Reverse();
            if (files.Any())
            {
                files.ForEach(file =>
                {
                    Logger.Info($"Reading settings from {file} file.");
                    var settings = File.ReadAllText(file);
                    JsonConvert.PopulateObject(settings, Properties.Settings.Default);
                });
            }
            else
            {
                Logger.Info("No custom settings file found.");
            }
        }
    }
}
