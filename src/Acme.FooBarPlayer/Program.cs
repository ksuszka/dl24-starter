using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chupacabra.PlayerCore.Host;
using Chupacabra.PlayerCore.Host.Forms;
using Newtonsoft.Json;
using NLog;

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

            var title = string.Format("FooBar {0}", Properties.Settings.Default.ServerPort);

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
            if (File.Exists(customSettingFile))
            {
                Logger.Info("Reading settings from {0} file.", customSettingFile);
                var settings = File.ReadAllText(customSettingFile);
                JsonConvert.PopulateObject(settings, Properties.Settings.Default);
            }
            else
            {
                Logger.Info("No custom settings file found.");
            }
        }
    }
}
