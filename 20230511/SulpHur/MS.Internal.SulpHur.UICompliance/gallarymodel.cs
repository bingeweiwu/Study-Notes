using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace MS.Internal.SulpHur.UICompliance
{
    public class GallaryModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }

        public ListBox listbox;

        private ObservableCollection<Tile> tileList = new ObservableCollection<Tile>();

        public ObservableCollection<Tile> TileList
        {
            get { return tileList; }
            set { tileList = value; }
        }
    }
}
