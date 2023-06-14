using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MS.Internal.SulpHur.Utilities;

namespace MS.Internal.SulpHur.UICompliance
{
    public class Tile : INotifyPropertyChanged
    {
        System.Drawing.Bitmap bitMap;

        public System.Drawing.Bitmap BitMap
        {
            get { return bitMap; }
            set { bitMap = value; 
                OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("BitMap")); }
        }

        string status="None";

        public string Status
        {
            get { return status; }
            set { status = value; OnPropertyChanged(new PropertyChangedEventArgs("Status")); }
        }

        public Tile(System.Drawing.Bitmap bitmap,string id) {
            this.bitMap = bitmap;
            this.id = id;
        }

        public Tile(System.Drawing.Bitmap bitmap, WebElementInfo id)
        {
            this.bitMap = bitmap;
            this.id = ExtensionMethods.ToXElement<WebElementInfo>(id).ToString();
        }

        public string id;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
