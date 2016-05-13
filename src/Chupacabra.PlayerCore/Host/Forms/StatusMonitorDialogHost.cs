using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chupacabra.PlayerCore.Host.Forms
{
    public class StatusMonitorDialogHost : IStatusMonitorDialog
    {
        private readonly string _title;
        private StatusMonitorDialog _dialog;
        private ApplicationContext _applicationContext;
        private Thread _uiThread;
        private AutoResetEvent _uiInitialized = new AutoResetEvent(false);
        private SynchronizationContext _synchronizationContext;

        public StatusMonitorDialogHost(string title)
        {
            _title = title;
            _uiThread = new Thread(UIThread);
            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.IsBackground = false;
            _uiThread.Start();
            _uiInitialized.WaitOne();
        }

        void UIThread()
        {
            _applicationContext = new ApplicationContext();
            _dialog = new StatusMonitorDialog();
            _dialog.Text = _title;
            _dialog.Hide();
            _synchronizationContext = SynchronizationContext.Current;   // Some control must be created first, so proper synchronization context is installed!
            _uiInitialized.Set();
            Application.Run(_applicationContext);
        }

        public void Dispose()
        {
            _applicationContext.ExitThread();
            _uiThread.Join();
        }

        private void ShowWindow()
        {
            _dialog.Show();
            _dialog.BringToFront();
        }

        public void Show()
        {
            _synchronizationContext.Post(_ => ShowWindow(), null);
        }

        public IStatusMonitor StatusMonitor { get { return this._dialog.StatusMonitor; } }
    }
}
