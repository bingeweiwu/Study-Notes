using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MS.Internal.SulpHur.SulpHurClient.ServerContact
{
    public class ConnectionWaiter
    {
        public event EventHandler ServerConnected;

        private Thread worker;
        private ServerContacter serverContacter;
        private ManualResetEvent stopEvent;
        private TimeSpan waitInterval = TimeSpan.FromMinutes(30);

        public ConnectionWaiter(ServerContacter serverContacter)
        { 
            this.serverContacter = serverContacter;
        }

        public void Start()
        {
            this.stopEvent = new ManualResetEvent(false);

            //start work
            this.worker = new Thread(new ThreadStart(this.Wait));
            this.worker.IsBackground = true;
            this.worker.Start();
        }

        public void Stop()
        {
            this.stopEvent.Set();
            this.worker.Join();
            this.stopEvent.Close();
            this.stopEvent.Dispose();
        }

        private void Wait()
        {
            do
            {
                if (this.serverContacter.Connect(this.serverContacter.ServerName) == true)
                {
                    ServerConnected(this, null);
                }
            } while (stopEvent.WaitOne(waitInterval) == false);
        }
    }
}
