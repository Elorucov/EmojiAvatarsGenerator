using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace EmojiAvatar {
    public class DelayedAction {
        private Action _action;
        private DispatcherTimer _timer;

        public DelayedAction(Action action, TimeSpan delay) {
            _action = action;

            _timer = new DispatcherTimer { 
                Interval = delay
            };
            _timer.Tick += _timer_Tick;
        }

        public void PrepareToExecute() {
            if (_timer.IsEnabled) _timer.Stop();
            _timer.Start();
        }

        private void _timer_Tick(object sender, object e) {
            _timer.Stop();
            _action?.Invoke();
        }
    }
}
