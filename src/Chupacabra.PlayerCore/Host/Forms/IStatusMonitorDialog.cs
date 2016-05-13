using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Chupacabra.PlayerCore.Host.Forms
{
    public interface IStatusMonitorDialog : IDisposable
    {
        void Show();

        IStatusMonitor StatusMonitor { get; }
    }
}
