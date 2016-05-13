namespace Chupacabra.PlayerCore.Host
{
    public class CompositeStatusMonitor : IStatusMonitor
    {
        private readonly IStatusMonitor _firstMonitor;
        private readonly IStatusMonitor _secondMonitor;

        public CompositeStatusMonitor(IStatusMonitor firstMonitor, IStatusMonitor secondMonitor)
        {
            _firstMonitor = firstMonitor;
            _secondMonitor = secondMonitor;
        }

        public void Set(string key, object value)
        {
            _firstMonitor.Set(key, value);
            _secondMonitor.Set(key, value);
        }

        public void Delete(string key)
        {
            _firstMonitor.Delete(key);
            _secondMonitor.Delete(key);
        }

        public void DeleteChildren(string key)
        {
            _firstMonitor.DeleteChildren(key);
            _secondMonitor.DeleteChildren(key);
        }

        public void ConfirmTurn()
        {
            _firstMonitor.ConfirmTurn();
            _secondMonitor.ConfirmTurn();
        }
    }
}
