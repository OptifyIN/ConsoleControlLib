using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace ConsoleControls
{
    public class CCTimer
    {
        private int _interval = 1000;
        public int Inteval { get { return _interval; } set { _interval = value; } }
        private bool _enabled = false;
        public bool Enabled { get { return _enabled; } set { SwitchState(value); } }
        public object Tag { get; set; }

        private Thread timerThread;
        private Stopwatch sw = new Stopwatch();

        private void SwitchState(bool value)
        {
            if (value)
            {
                timerThread = new Thread(TickThread);
                timerThread.Start();
            }
            else
            {
                timerThread.Interrupt();
                sw.Stop();
            }
            _enabled = value;
        }

        public event EventHandler Tick;
        public void TickInvoke(object sender)
        {
            Tick?.Invoke(sender, EventArgs.Empty);
        }

        //////////////////////////////

        private void TickThread()
        {
            sw.Reset();
            sw.Start();
            while (true)
            {
                if(sw.ElapsedMilliseconds >= _interval)
                {
                    sw.Restart();
                    TickInvoke(this);
                }
            }
        }
    }
}
