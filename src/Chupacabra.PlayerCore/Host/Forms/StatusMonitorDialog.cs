// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Data;
// using System.Drawing;
// using System.Linq;
// using System.Runtime.InteropServices;
// using System.Text;
// using System.Threading.Tasks;
// using System.Windows.Forms;

// namespace Chupacabra.PlayerCore.Host.Forms
// {
//     public partial class StatusMonitorDialog : Form, IStatusMonitorDialog
//     {
//         [DllImport("user32.dll", SetLastError = true)]
//         static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);

//         public StatusMonitorDialog()
//         {
//             InitializeComponent();
//         }

//         private void StatusMonitorDialog_FormClosing(object sender, FormClosingEventArgs e)
//         {
//             e.Cancel = true;
//             Hide();
//         }

//         public new void BringToFront()
//         {
//             SwitchToThisWindow(this.Handle, true);
//         }

//         public IStatusMonitor StatusMonitor { get { return this.statusMonitorControl1; } }
//     }
// }
