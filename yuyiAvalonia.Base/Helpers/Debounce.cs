using System;
using System.Threading;

namespace yuyiAvalonia.Base.Helpers
{
    public class Debounce
    {
        private readonly int _delay;

        private Timer? _timer;

        public Debounce(int delay)
        {
            _delay = delay;
        }

        public void Trigger(Action action)
        {
            _timer?.Dispose();
            _timer = null;
            _timer = new Timer(_ =>
            {
                action?.Invoke();
                _timer?.Dispose();
                _timer = null;
            }, null, _delay, Timeout.Infinite);
        }
    }
}
