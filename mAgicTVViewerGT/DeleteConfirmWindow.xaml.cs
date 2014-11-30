using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using mAgicTVViewerGT.ViewModel;

namespace mAgicTVViewerGT
{
    /// <summary>
    /// Window1.xaml の相互作用ロジック
    /// </summary>
    public partial class DeleteConfirmWindow : Window
    {
        public DeleteConfirmWindow()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).IsConfirmedToDeletingPrograms = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).IsConfirmedToDeletingPrograms = false;
            this.Close();
        }
    }
}
