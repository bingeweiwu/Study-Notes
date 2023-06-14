using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RuleManager
{
    public class WindowDispatcher
    {
        ServiceManager sm;
        public WindowDispatcher(ServiceManager sm)
        {
            this.sm = sm;
        }

        public string AdminConsolePath
        {
            get
            {
                string textBoxPath = "";
                sm.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    textBoxPath = sm.tbxConsole.Text.Trim();
                }));
                System.Threading.Thread.Sleep(50);
                return textBoxPath;
            }
        }
        public string InheriBuild
        {
            get
            {
                string inheriBuild = "";
                sm.Dispatcher.BeginInvoke(new Action(delegate()
                {
                    inheriBuild = sm.cbAvailableBuild1.SelectedValue.ToString();
                }));
                System.Threading.Thread.Sleep(50);
                return inheriBuild;
            }
        }

        public void AddMissUI(PageControl p)
        {
            sm.Dispatcher.BeginInvoke(new Action(delegate()
            {
                sm.view.MissUI.Add(p);
            }));
            System.Threading.Thread.Sleep(100);
        }
    }
}
