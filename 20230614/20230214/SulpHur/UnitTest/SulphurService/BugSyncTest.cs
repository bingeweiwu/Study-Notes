using Microsoft.VisualStudio.TestTools.UnitTesting;
using MS.Internal.SulpHur.SulpHurService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTest.SulphurService
{
    [TestClass]
    public class BugSyncTest
    {
        [TestMethod]
        public void TimedTaskTest()
        {
            BugSync.BugSyncTask();
        }

        [TestMethod]
        public void TimerTest()
        {
            new BugSync().Start();
        }
        [TestMethod]
        public void SyncVSOBugTest()
        {
            BugSync.SyncVSOBug();
        }
    }
}
