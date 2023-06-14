using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;

namespace ManagedPropertyReader
{
    public class ManagedInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        object propertyValue;
        public object PropertyValue
        {
            get { return propertyValue; }
            set { propertyValue = value; OnPropertyChanged(new PropertyChangedEventArgs("PropertyValue")); }
        }
    }
}
