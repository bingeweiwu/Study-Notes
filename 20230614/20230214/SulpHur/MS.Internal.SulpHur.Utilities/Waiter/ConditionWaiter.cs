using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MS.Internal.SulpHur.Utilities.Waiter
{

    public class ConditionWaiter
    {
        public ConditionWaiter()
        {
        }

        public void Wait(Func<bool>  conditionFunc,  int timeout = 5000)
        {
            int interval = 100;
            int tries = 0;
            while (interval * tries < timeout) 
            {
                //check condition
                if (conditionFunc.Invoke())
                    return;

                //sleep
                System.Threading.Thread.Sleep(100);
                tries++;
            }
        }

        public void Wait(Func<bool> conditionFunc, TimeSpan timeout)
        {
            this.Wait(conditionFunc, (int)timeout.TotalMilliseconds);
        }
    }
}
