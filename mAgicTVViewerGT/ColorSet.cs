using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace mAgicTVViewerGT
{
    public class ColorSet : INotifyPropertyChanged
    {
        private static string TREEVIEW_BACKGROUND = "#FFFFFFFF";
        private static string TREEVIEW_FOREGROUND = "#FF000000";
        private static string TREEVIEW_UNVIEWED = "#C0E04000";
        private static string LISTVIEW_BACKGROUND = "#FFFFFFFF";
        private static string LISTVIEW_FOREGROUND = "#FF000000";
        private static string GRIDVIEW_BACKGROUND = "#FFFEFEFE";
        private static string GRIDVIEW_FOREGROUND = "#FF000000";

        private string _TreeViewBackground;
        public string TreeViewBackground {
            get
            {
                return this._TreeViewBackground;
            }
            set
            {
                this._TreeViewBackground = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("TreeViewBackground"));
            }
        }
        private string _TreeViewForeground;
        public string TreeViewForeground
        {
            get
            {
                return this._TreeViewForeground;
            }
            set
            {
                this._TreeViewForeground = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("TreeViewForeground"));
            }
        }
        private string _TreeViewUnviewedCount;
        public string TreeViewUnviewedCount
        {
            get
            {
                return this._TreeViewUnviewedCount;
            }
            set
            {
                this._TreeViewUnviewedCount = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("TreeViewUnviewedCount"));
            }
        }
        private string _ListViewBackground;
        public string ListViewBackground
        {
            get
            {
                return this._ListViewBackground;
            }
            set
            {
                this._ListViewBackground = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("ListViewBackground"));
            }
        }
        private string _ListViewForeground;
        public string ListViewForeground
        {
            get
            {
                return this._ListViewForeground;
            }
            set
            {
                this._ListViewForeground = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("ListViewForeground"));
            }
        }
        private string _GridViewBackground;
        public string GridViewBackground
        {
            get
            {
                return this._GridViewBackground;
            }
            set
            {
                this._GridViewBackground = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("GridViewBackground"));
            }
        }
        private string _GridViewForeground;
        public string GridViewForeground
        {
            get
            {
                return this._GridViewForeground;
            }
            set
            {
                this._GridViewForeground = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("GridViewForeground"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ColorSet()
        {
            this._TreeViewBackground = ColorSet.TREEVIEW_BACKGROUND;
            this._TreeViewForeground = ColorSet.TREEVIEW_FOREGROUND;
            this._TreeViewUnviewedCount = ColorSet.TREEVIEW_UNVIEWED;
            this._ListViewBackground = ColorSet.LISTVIEW_BACKGROUND;
            this._ListViewForeground = ColorSet.LISTVIEW_FOREGROUND;
            this._GridViewBackground = ColorSet.GRIDVIEW_BACKGROUND;
            this._GridViewForeground = ColorSet.GRIDVIEW_FOREGROUND;
        }
    }
}
