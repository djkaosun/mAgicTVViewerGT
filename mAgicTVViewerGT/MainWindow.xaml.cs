using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
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
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region GUI_EventHandler
        private void listViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            item.InputBindings.Add(
                    new MouseBinding(
                            ApplicationCommands.Open,
                            new MouseGesture(MouseAction.LeftDoubleClick, ModifierKeys.None)
                            )
                    );
        }

        private void treeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem)
            {
                ((TreeViewItem)sender).IsSelected = true;
                e.Handled = true;
            }
        }

        #region ListView Multi Selection
        private void listViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.None)
                {
                    // Control キーが押されていなかったら、すべて選択解除
                    if (((DataContextClass)this.LayoutRoot.DataContext).UnselectAllCommand.CanExecute(null))
                    {
                        ((DataContextClass)this.LayoutRoot.DataContext).UnselectAllCommand.Execute(null);
                    }
                }
            }
        }

        private void RibbonWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.A)
            {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
                {
                    // Control キーが押されていたら
                    if (((DataContextClass)this.LayoutRoot.DataContext).SelectAllCommand.CanExecute(null))
                    {
                        ((DataContextClass)this.LayoutRoot.DataContext).SelectAllCommand.Execute(null);
                    }
                }
            }
        }
        #endregion ListView Multi Selection

        #region GridView Sorting

        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        void GridViewColumnHeaderClickedHandler(object sender,
                                                RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked =
                  e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    string header = headerClicked.Column.Header as string;
                    Sort(header, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }


                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }
        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(TvProgramList.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
        #endregion GridView Sorting

        #region TreeView Drag and Drop
        Point startPoint = new Point();
        
        private void RibbonWindow_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.startPoint = new Point();
        }

        private void treeViewItem_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            this.startPoint = e.GetPosition(null);
        }
        private void treeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.startPoint != new Point())
            {
                Vector moveVector;
                moveVector = startPoint - e.GetPosition(null);
                if (e.LeftButton == MouseButtonState.Pressed && moveVector.Length > 20)
                {
                    this.startPoint = new Point();

                    // ドラッグ開始している対象を取得
                    mAgicTVViewerGT.Model.FilterCriteria.IHierarchicalFilter<mAgicTVViewerGT.Model.TvProgramWatcher.TvProgram> dummy =
                            new mAgicTVViewerGT.Model.TvProgramFilter.TvProgramHierarchicalFilter();

                    // データをパッケージする
                    DataObject data = new DataObject();
                    data.SetData("Object", dummy);

                    // ドラッグ＆ドロップ操作を初期化
                    DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
                }
            }
        }

        private void treeViewItem_Drop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData("Object") is mAgicTVViewerGT.Model.FilterCriteria.IHierarchicalFilter<mAgicTVViewerGT.Model.TvProgramWatcher.TvProgram>))
            {
                return;
            }

            ((DataContextClass)this.LayoutRoot.DataContext).FilterTreeNewParent =
                    (mAgicTVViewerGT.Model.TvProgramFilter.TvProgramHierarchicalFilter)((TreeViewItem)sender).DataContext;
            
            switch (e.KeyStates)
            {
                case DragDropKeyStates.ControlKey:
                    if (((DataContextClass)this.LayoutRoot.DataContext).CopyFilterCommand.CanExecute(null))
                    {
                        ((DataContextClass)this.LayoutRoot.DataContext).CopyFilterCommand.Execute(null);
                    }
                    break;
                default:
                    if (((DataContextClass)this.LayoutRoot.DataContext).MoveFilterCommand.CanExecute(null))
                    {
                        ((DataContextClass)this.LayoutRoot.DataContext).MoveFilterCommand.Execute(null);
                    }
                    break;
            }

            e.Handled = true;
        }
        #endregion TreeView Drag and Drop

        #region Command Call Handler
        private void PropertiesCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).EditStartCommand.CanExecute(e.Parameter);
        }

        private void PropertiesCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).EditStartCommand.Execute(e.Parameter);
            FilterSettingWindow confWindow = new FilterSettingWindow();
            confWindow.Owner = this;
            confWindow.LayoutRoot.DataContext = this.LayoutRoot.DataContext;
            confWindow.ShowDialog();
        }

        private void DeleteCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).DeleteCommand.CanExecute(e.Parameter);
        }

        private void DeleteCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MessageBoxResult confirm = MessageBox.Show(
                    "フィルターを完全に削除しますか？\n\nこの操作によりサブ フィルターも削除されます。",
                    "フィルターの削除",
                    MessageBoxButton.YesNo
                    );
            if (confirm == MessageBoxResult.Yes)
            {
                ((DataContextClass)this.LayoutRoot.DataContext).DeleteCommand.Execute(e.Parameter);
            }
        }

        private void NewCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).NewCommand.CanExecute(e.Parameter);
        }

        private void NewCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).NewCommand.Execute(e.Parameter);
        }

        private void DeleteProgramCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).DeleteProgramCommand.CanExecute(e.Parameter);
        }

        private void DeleteProgramCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).IsConfirmedToDeletingPrograms = false;
            DeleteConfirmWindow deleteWindow = new DeleteConfirmWindow();
            deleteWindow.Owner = this;
            deleteWindow.LayoutRoot.DataContext = this.LayoutRoot.DataContext;
            deleteWindow.ShowDialog();
            if (((DataContextClass)this.LayoutRoot.DataContext).IsConfirmedToDeletingPrograms)
            {
                ((DataContextClass)this.LayoutRoot.DataContext).DeleteProgramCommand.Execute(e.Parameter);
            }
            ((DataContextClass)this.LayoutRoot.DataContext).IsConfirmedToDeletingPrograms = false;
        }


        private void OpenFolderCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).OpenDirectoryCommand.CanExecute(e.Parameter);
        }

        private void OpenFolderCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //FolderBrowserDialogクラスのインスタンスを作成
            System.Windows.Forms.FolderBrowserDialog fbd =
                    new System.Windows.Forms.FolderBrowserDialog();

            //上部に表示する説明テキストを指定する
            fbd.Description = "フォルダを指定してください。";
            //ルートフォルダを指定する
            //デフォルトでDesktop
            fbd.RootFolder = Environment.SpecialFolder.Desktop;
            //最初に選択するフォルダを指定する
            //RootFolder以下にあるフォルダである必要がある
            fbd.SelectedPath = @"C:\";

            if (System.IO.Directory.Exists(((DataContextClass)this.LayoutRoot.DataContext).TvProgramDirectoryPath))
            {
                fbd.SelectedPath = ((DataContextClass)this.LayoutRoot.DataContext).TvProgramDirectoryPath;
            }
            else
            {
                string recordFolder = @"mAgicTVD\Record";
                string[] drives = System.IO.Directory.GetLogicalDrives();
                for (int i = 0; i < drives.Length; i++)
                {
                    if (System.IO.Directory.Exists(drives[i] + recordFolder))
                    {
                        fbd.SelectedPath = drives[i] + recordFolder;
                    }
                }
            }
            //ユーザーが新しいフォルダを作成できるようにする
            //デフォルトでTrue
            fbd.ShowNewFolderButton = true;

            //ダイアログを表示する
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((DataContextClass)this.LayoutRoot.DataContext).OpenDirectoryCommand.Execute(fbd.SelectedPath);
            }
        }

        private void OpenCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).OpenProgramCommand.CanExecute(e.Parameter);
        }

        private void OpenCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).OpenProgramCommand.Execute(e.Parameter);
        }

        private void NewOthersCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).NewOthersCommand.CanExecute(e.Parameter);
        }

        private void NewOthersCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).NewOthersCommand.Execute(e.Parameter);
        }

        private void NewFolderCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).NewFolderCommand.CanExecute(e.Parameter);
        }

        private void NewFolderCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).NewFolderCommand.Execute(e.Parameter);
        }

        private void MakeViewedCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).MakeViewedCommand.CanExecute(e.Parameter);
        }

        private void MakeViewedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).MakeViewedCommand.Execute(e.Parameter);
        }

        private void MakeUnviewedCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).MakeUnviewedCommand.CanExecute(e.Parameter);
        }

        private void MakeUnviewedCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).MakeUnviewedCommand.Execute(e.Parameter);
        }

        private void SelectAllCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataContextClass)this.LayoutRoot.DataContext).SelectAllCommand.CanExecute(e.Parameter);
        }

        private void SelectAllCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ((DataContextClass)this.LayoutRoot.DataContext).SelectAllCommand.Execute(e.Parameter);
        }

        private void CloseCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion Command Call Handler
        #endregion GUI_EventHandler
    }
}
