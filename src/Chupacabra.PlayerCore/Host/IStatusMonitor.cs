using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chupacabra.PlayerCore.Host
{
    public interface IStatusMonitor
    {
        void Set(string key, object value);
        void Delete(string key);
        void DeleteChildren(string key);
        void ConfirmTurn();
    }
}
