using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using mAgicTVViewerGT.Model.FilterCriteria;
using mAgicTVViewerGT.Model.TvProgramFilter;
using mAgicTVViewerGT.Model.TvProgramWatcher;

namespace mAgicTVViewerGT.ViewModel
{
    public class DataContextClass : INotifyPropertyChanged, IDataErrorInfo
    {
        private static string CREATOR_NAME = "DJ_Kaosun";
        private static string SOFTWARE_NAME = "mtvFIlterViewer";
        private static string RECORD_SETTING_FILENAME = "mAgicTVD-Record_Path.xml";
        private static string FILTER_SETTING_DIRNAME = "Filter";
        private static double MAX_SCALE = 2.0;

        #region Property for Binding
        public TvProgramCollection FiltratedItems { get; private set; }
        public HierarchicalFilterChildren<TvProgram> FilterTreeRoot { get; private set; }
        public HashSet<TvProgram> SelectedPrograms { get; private set; }
        public TvProgramHierarchicalFilter FilterTreeNewParent { get; set; }
        public bool IsNotFolderOnEditData { get; private set; }

        private bool _IsConfirmedToDeletingPrograms;
        public bool IsConfirmedToDeletingPrograms {
            get { return this._IsConfirmedToDeletingPrograms; }
            set
            {
                this._IsConfirmedToDeletingPrograms = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("IsConfirmedToDeletingPrograms"));
            }
        }

        private  string _TvProgramDirectoryPath;
        public string TvProgramDirectoryPath
        {
            get { return this._TvProgramDirectoryPath; }
            set {
                this._TvProgramDirectoryPath = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("TvProgramDirectoryPath"));
            }
        }

        private double _Scale = 1.0;
        public double Scale
        {
            get { return this._Scale; }
            private set
            {
                this._Scale = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("Scale"));
            }
        }

        private ColorSet _CurrentColorSet;
        public ColorSet CurrentColorSet
        {
            get { return this._CurrentColorSet; }
            private set
            {
                this._CurrentColorSet = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("CurrentColorSet"));
            }
        }
        #region for Status Bar
        private string _allnum;
        public string AllNumberString {
            get
            {
                return this._allnum;
            }
            private set
            {
                this._allnum = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("AllNumberString"));
                }
            }
        }

        private string _filteredResultNum;
        public string FilterResultNumberString {
            get
            {
                return this._filteredResultNum;
            }
            private set
            {
                this._filteredResultNum = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("FilterResultNumberString"));
                }
            }
        }

        private double _searchProg;
        public double SearchProgress
        {
            get
            {
                return this._searchProg;
            }
            private set
            {
                this._searchProg = value;
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs("SearchProgress"));
                }


                if (this.tvpw.IsSearchingNow && this._searchProg < 1.0)
                {
                    if (this._progVisible != Visibility.Visible)
                    {
                        this._progVisible = Visibility.Visible;
                        if (this.PropertyChanged != null)
                        {
                            this.PropertyChanged(this, new PropertyChangedEventArgs("ProgressVisibility"));
                        }
                    }
                }
                else
                {
                    if (this._progVisible != Visibility.Hidden)
                    {
                        this._progVisible = Visibility.Hidden;
                        if (this.PropertyChanged != null)
                        {
                            this.PropertyChanged(this, new PropertyChangedEventArgs("ProgressVisibility"));
                        }
                    }
                }
            }
        }

        private Visibility _progVisible = Visibility.Visible;
        public Visibility ProgressVisibility
        {
            get
            {
                return this._progVisible;
            }
            // ProgressVisibiliy は SearchProgress の変更に連動して変わるので set は無い。
        }
        #endregion for Status Bar

        #region EditData Accessor Property
        private ITvProgramFilter EditData;

        private bool _IsChangedEditData;
        public bool IsChangedEditData {
            get { return this._IsChangedEditData; }
            set
            {
                this._IsChangedEditData = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("IsChangedEditData"));
            }
        }

        private bool _HasErrorEditData;
        public bool HasErrorEditData
        {
            get { return this._HasErrorEditData; }
            set
            {
                this._HasErrorEditData = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("HasErrorEditData"));
            }
        }

        public string EditDataName
        {
            get { return this.EditData.Name; }
            set
            {

                string propertyName = "EditDataName";
                ValidationResult result = this.nameFormatValidationRule.Validate(value, null);
                if (result.IsValid)
                {
                    this._Errors.Remove(propertyName);
                    if (this._Errors.Count < 1) this.HasErrorEditData = false;
                }
                else
                {
                    this._Errors[propertyName] = result.ErrorContent.ToString();
                    if (this._Errors.Count == 1) this.HasErrorEditData = true;
                }
                this.EditData.Name = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public string EditDataTitle
        {
            get {
                if (!(this.EditData is TvProgramStandardFilter)) return String.Empty;
                
                return ((TvProgramStandardFilter)this.EditData).Title;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Title = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataTitle"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        private string tempStartDateMin;
        public string EditDataStartDateMin
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return String.Empty;

                if (this.tempStartDateMin == null) return ((TvProgramStandardFilter)this.EditData).StartDateMin;
                else return this.tempStartDateMin;
            }
            set
            {
                string propertyName = "EditDataStartDateMin";
                ValidationResult result = this.dateFormatValidationRule.Validate(value, null);
                if (result.IsValid)
                {
                    this.tempStartDateMin = null;
                    this._Errors.Remove(propertyName);
                    ((TvProgramStandardFilter)this.EditData).StartDateMin = value;
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    if (!this.IsChangedEditData) this.IsChangedEditData = true;
                    if (this._Errors.Count < 1) this.HasErrorEditData = false;
                }
                else
                {
                    this.tempStartDateMin = value;
                    this._Errors[propertyName] = result.ErrorContent.ToString();
                    if (this._Errors.Count == 1) this.HasErrorEditData = true;
                }
            }
        }

        private string tempStartDateMax;
        public string EditDataStartDateMax
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return String.Empty;

                if (this.tempStartDateMax == null) return ((TvProgramStandardFilter)this.EditData).StartDateMax;
                else return this.tempStartDateMax;
            }
            set
            {
                string propertyName = "EditDataStartDateMax";
                ValidationResult result = this.dateFormatValidationRule.Validate(value, null);
                if (result.IsValid)
                {
                    this.tempStartDateMax = null;
                    this._Errors.Remove(propertyName);
                    ((TvProgramStandardFilter)this.EditData).StartDateMax = value;
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    if (!this.IsChangedEditData) this.IsChangedEditData = true;
                    if (this._Errors.Count < 1) this.HasErrorEditData = false;
                }
                else
                {
                    this.tempStartDateMax = value;
                    this._Errors[propertyName] = result.ErrorContent.ToString();
                    if (this._Errors.Count == 1) this.HasErrorEditData = true;
                }
            }
        }

        private string tempStartTimeMin;
        public string EditDataStartTimeMin
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return String.Empty;

                if (this.tempStartTimeMin == null) return ((TvProgramStandardFilter)this.EditData).StartTimeMin;
                else return this.tempStartTimeMin;
            }
            set
            {
                string propertyName = "EditDataStartTimeMin";
                ValidationResult result = this.dateFormatValidationRule.Validate(value, null);
                if (result.IsValid)
                {
                    this.tempStartTimeMin = null;
                    this._Errors.Remove(propertyName);
                    ((TvProgramStandardFilter)this.EditData).StartTimeMin = value;
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    if (!this.IsChangedEditData) this.IsChangedEditData = true;
                    if (this._Errors.Count < 1) this.HasErrorEditData = false;
                }
                else
                {
                    this.tempStartTimeMin = value;
                    this._Errors[propertyName] = result.ErrorContent.ToString();
                    if (this._Errors.Count == 1) this.HasErrorEditData = true;
                }
            }
        }

        private string tempStartTimeMax;
        public string EditDataStartTimeMax
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return String.Empty;

                if (this.tempStartTimeMax == null) return ((TvProgramStandardFilter)this.EditData).StartTimeMax;
                else return this.tempStartTimeMax;
            }
            set
            {
                this.tempStartTimeMax = null;
                string propertyName = "EditDataStartTimeMax";
                ValidationResult result = this.dateFormatValidationRule.Validate(value, null);
                if (result.IsValid)
                {
                    this._Errors.Remove(propertyName);
                    ((TvProgramStandardFilter)this.EditData).StartTimeMin = value;
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    if (!this.IsChangedEditData) this.IsChangedEditData = true;
                    if (this._Errors.Count < 1) this.HasErrorEditData = false;
                }
                else
                {
                    this.tempStartTimeMax = value;
                    this._Errors[propertyName] = result.ErrorContent.ToString();
                    if (this._Errors.Count == 1) this.HasErrorEditData = true;
                }
            }
        }


        public bool EditDataSunday
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Sunday;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Sunday = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataSunday"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public bool EditDataMonday
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Monday;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Monday = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataMonday"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public bool EditDataTuesday
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Tuesday;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Tuesday = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataTuesday"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public bool EditDataWednesday
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Wednesday;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Wednesday = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataWednesday"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public bool EditDataThursday
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Thursday;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Thursday = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataThursday"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public bool EditDataFriday
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Friday;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Friday = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataFriday"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public bool EditDataSaturday
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Saturday;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Saturday = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataSaturday"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public bool EditDataViewed
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Viewed;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Viewed = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataViewed"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }

        public bool EditDataUnviewed
        {
            get
            {
                if (!(this.EditData is TvProgramStandardFilter)) return false;

                return ((TvProgramStandardFilter)this.EditData).Unviewed;
            }
            set
            {
                ((TvProgramStandardFilter)this.EditData).Unviewed = value;
                this.PropertyChanged(this, new PropertyChangedEventArgs("EditDataUnviewed"));
                if (!this.IsChangedEditData) this.IsChangedEditData = true;
            }
        }
        #endregion EditData Accessor Property
        #endregion Property for Binding

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        private Dictionary<string, string> _Errors;
        public string this[string columnName]
        {
            get
            {
                if (_Errors.ContainsKey(columnName))
                {
                    return _Errors[columnName];
                }

                return null;
            }
        }

        #region Routed Command
        public static readonly ICommand NewOthers = new RoutedCommand("NewOthers", typeof(DataContextClass));
        public static readonly ICommand NewFolder = new RoutedCommand("NewFolder", typeof(DataContextClass));
        public static readonly ICommand Move = new RoutedCommand("Move", typeof(DataContextClass));
        public static readonly ICommand EditOk = new RoutedCommand("EditOk", typeof(DataContextClass));
        public static readonly ICommand EditCancel = new RoutedCommand("EditCansel", typeof(DataContextClass));
        public static readonly ICommand EditApply = new RoutedCommand("EditApply", typeof(DataContextClass));
        public static readonly ICommand MakeViewed = new RoutedCommand("MakeViewed", typeof(DataContextClass));
        public static readonly ICommand MakeUnviewed = new RoutedCommand("MakeUnviewed", typeof(DataContextClass));
        #endregion Routed Command

        #region Command Property
        public ICommand OpenDirectoryCommand { get; private set; }
        public ICommand EditStartCommand { get; private set; }
        public ICommand NewCommand { get; private set; }
        public ICommand NewOthersCommand { get; private set; }
        public ICommand NewFolderCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand CopyFilterCommand { get; private set; }
        public ICommand MoveFilterCommand { get; private set; }
        public ICommand EditOkCommand { get; private set; }
        public ICommand EditCancelCommand { get; private set; }
        public ICommand EditApplyCommand { get; private set; }
        public ICommand OpenProgramCommand { get; private set; }
        public ICommand MakeViewedCommand { get; private set; }
        public ICommand MakeUnviewedCommand { get; private set; }
        public ICommand DeleteProgramCommand { get; private set; }
        public ICommand SelectAllCommand { get; private set; }
        public ICommand UnselectAllCommand { get; private set; }
        public ICommand ScaleUpCommand { get; private set; }
        public ICommand ScaleDownCommand { get; private set; }
        public ICommand ScaleResetCommand { get; private set; }
        #endregion Command Property

        #region Private Field
        private TvProgramWatcher tvpw;
        private Object lockTargetForFiltratedItems = new Object();
        private Object lockTargetForAllTvPrograms = new Object();
        private TvProgramCollection allTvPrograms;
        private NameFormatValidationRule nameFormatValidationRule;
        private DateFormatValidationRule dateFormatValidationRule;
        private TimeFormatValidationRule timeFormatValidationRule;
        private TvProgramHierarchicalFilterSerializer filterSerializer;
        private string appPath;
        #endregion Private Field

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// コンストラクター。
        /// </summary>
        public DataContextClass()
        {
            //appPath = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).Directory.FullName + Path.DirectorySeparatorChar + "Settings";
            appPath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Path.DirectorySeparatorChar + DataContextClass.CREATOR_NAME + Path.DirectorySeparatorChar + DataContextClass.SOFTWARE_NAME;

            // エラー記録用ディクショナリの生成
            this._Errors = new Dictionary<string, string>();

            // ValidationRule の生成
            this.nameFormatValidationRule = new NameFormatValidationRule();
            this.dateFormatValidationRule = new DateFormatValidationRule();
            this.timeFormatValidationRule = new TimeFormatValidationRule();

            // 全番組投入用のコレクションの生成
            this.allTvPrograms = new TvProgramCollection();
            this.allTvPrograms.CollectionChanged += this.allTvPrograms_CollectionChanged;

            // フィルタ結果投入用のコレクションの生成
            this.FiltratedItems = new TvProgramCollection();
            this.FiltratedItems.CollectionChanged += this.filtratedItems_CollectionChanged;
            BindingOperations.EnableCollectionSynchronization(this.FiltratedItems, this.lockTargetForFiltratedItems);

            // 選択された番組を保存するコレクションの生成
            this.SelectedPrograms = new HashSet<TvProgram>();

            // フィルター ツリーのルートの生成
            this.FilterTreeRoot = new HierarchicalFilterChildren<TvProgram>();
            this.FilterTreeRoot.Add(
                    new TvProgramHierarchicalFilter()
                    {
                        Filter = new TvProgramStandardFilter()
                        {
                            Name = Properties.Resources.Tree_TopItem,
                            Icon = "Resources/UnivSet.png"
                        }
                    }
                    );

            // テスト用。色セット。
            this.CurrentColorSet = new ColorSet() {
                    TreeViewBackground = "#FF101018",
                    TreeViewForeground = "#FFC0C0D0",
                    ListViewBackground = "#FF101018",
                    ListViewForeground = "#FFC0C0D0",
                    GridViewBackground = "#FF606066",
                    GridViewForeground = "#FFFFFFFF"
                    };

            // シリアライザーの生成
            this.filterSerializer = new TvProgramHierarchicalFilterSerializer() {
                    DirectoryPath = this.appPath + Path.DirectorySeparatorChar + DataContextClass.FILTER_SETTING_DIRNAME,
                    BackupGeneration = 5
                    };

            // 設定のロード
            this.loadSettings();

            // フィルター ツリーのルートを選択する
            this.FilterTreeRoot[0].IsSelected = true;

            // フィルター変更時イベント ハンドラーの設定
            this.FilterTreeRoot[0].PropertyChanged += this.filterTreePropertyChanged;

            // TvProgramWatcher の生成
            this.tvpw = new TvProgramWatcher();
            this.tvpw.Found += this.tvpw_Found;
            this.tvpw.Created += this.tvpw_Created;
            this.tvpw.Changed += this.tvpw_Changed;
            this.tvpw.Deleted += this.tvpw_Deleted;
            this.tvpw.Renamed += this.tvpw_Renamed;
            if (Directory.Exists(this.TvProgramDirectoryPath))
            {
                this.tvpw.Path = this.TvProgramDirectoryPath;
                this.tvpw.EnableRaisingEvents = true;
            }

            // フォルダー変更時イベント ハンドラーの設定
            this.PropertyChanged += tvpdirParamaterChangedEventHandler;

            // コマンド オブジェクトの生成
            this.OpenDirectoryCommand = new DataContextClass.OpenDirectoryCommandImpl(this);
            this.EditStartCommand = new DataContextClass.EditStartCommandImpl(this);
            this.NewCommand = new DataContextClass.NewCommandImpl(this);
            this.NewOthersCommand = new DataContextClass.NewOthersCommandImpl(this);
            this.NewFolderCommand = new DataContextClass.NewFolderCommandImpl(this);
            this.DeleteCommand = new DataContextClass.DeleteCommandImpl(this);
            this.CopyFilterCommand = new DataContextClass.CopyFilterCommandImpl(this);
            this.MoveFilterCommand = new DataContextClass.MoveFilterCommandImpl(this);
            this.EditOkCommand = new DataContextClass.EditOkCommandImpl(this);
            this.EditCancelCommand = new DataContextClass.EditCancelCommandImpl(this);
            this.EditApplyCommand = new DataContextClass.EditApplyCommandImpl(this);
            this.OpenProgramCommand = new DataContextClass.OpenProgramCommandImpl(this);
            this.MakeViewedCommand = new DataContextClass.MakeViewedCommandImpl(this);
            this.MakeUnviewedCommand = new DataContextClass.MakeUnviewedCommandImpl(this);
            this.DeleteProgramCommand = new DataContextClass.DeleteProgramCommandImpl(this);
            this.SelectAllCommand = new DataContextClass.SelectAllCommandImpl(this);
            this.UnselectAllCommand = new DataContextClass.UnselectAllCommandImpl(this);
            this.ScaleUpCommand = new DataContextClass.ScaleUpCommandImpl(this);
            this.ScaleDownCommand = new DataContextClass.ScaleDownCommandImpl(this);
            this.ScaleResetCommand = new DataContextClass.ScaleResetCommandImpl(this);
        }

        #region Private Method
        /// <summary>
        /// グリッドのリフレッシュ。フィルター済み番組数も合わせて更新する。
        /// </summary>
        private void refleshFilterdPrograms()
        {
            // 先に allTvPrograms の更新を止める
            lock (this.lockTargetForAllTvPrograms)
            {
                lock (this.lockTargetForFiltratedItems)
                {
                    this.FiltratedItems.Clear();

                    foreach (TvProgram tvprg in this.allTvPrograms)
                    {
                        if (this.FilterTreeRoot[0].SelectedItem.Match(tvprg))
                        {
                            this.FiltratedItems.Add(tvprg);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 未視聴数の全更新。
        /// </summary>
        private void refleshUnviewedNumber()
        {
            lock (lockTargetForAllTvPrograms)
            {
                TvProgram[] tvPrgs = new TvProgram[this.allTvPrograms.Count];
                this.allTvPrograms.CopyTo(tvPrgs, 0);
                ((TvProgramHierarchicalFilter)this.FilterTreeRoot[0]).UnviewedCount(tvPrgs);
            }
        }

        /// <summary>
        /// フィルターの追加処理。
        /// TvProgramHierarchicalFilterComparer を使用して、整列された状態で追加する。
        /// </summary>
        /// <param name="newFilter"></param>
        /// <param name="parentFilter"></param>
        private void addFilter(IHierarchicalFilter<TvProgram> newFilter, IHierarchicalFilter<TvProgram> parentFilter)
        {
            TvProgramHierarchicalFilterComparer comparer = new TvProgramHierarchicalFilterComparer();

            int index = 0;
            while(index < parentFilter.Children.Count &&
                    0 < comparer.Compare(newFilter, parentFilter.Children[index])
                    )
            {
                index++;
            }

//MessageBox.Show("「" + ((TvProgramHierarchicalFilter)parentFilter).Name + "」に「" + ((TvProgramHierarchicalFilter)newFilter).Name + "」を追加します。");
            parentFilter.Children.Insert(index, newFilter);
        }

        /// <summary>
        /// 設定の保存。
        /// 監視対象フォルダ、フィルター ツリーを保存する。
        /// </summary>
        private void saveSettings()
        {
            DirectoryInfo settingDir = Directory.CreateDirectory(this.appPath);

//MessageBox.Show(settingDir.FullName + Path.DirectorySeparatorChar + DataContextClass.RECORD_SETTING_FILENAME);
            XmlSerializer serializer = null;
            StreamWriter sw = null;

            // 録画フォルダ設定の保存
            try
            {
                serializer = new XmlSerializer(typeof(string));
                sw = new StreamWriter(
                        settingDir.FullName + Path.DirectorySeparatorChar + DataContextClass.RECORD_SETTING_FILENAME,
                        false,
                        new System.Text.UTF8Encoding(false)
                        );

                serializer.Serialize(sw, this._TvProgramDirectoryPath);
            }
            finally
            {
                if (sw != null) sw.Close();
                sw = null;
                serializer = null;
            }

            // フィルター ツリーの保存
            this.filterSerializer.Serialize((TvProgramHierarchicalFilter)this.FilterTreeRoot[0]);
        }

        /// <summary>
        /// 設定の復元。
        /// 監視対象フォルダ、フィルター ツリーを復元する。
        /// </summary>
        private void loadSettings()
        {
            DirectoryInfo settingDir = new DirectoryInfo(this.appPath);

            XmlSerializer serializer = null;
            StreamReader sr = null;

            // 録画フォルダ設定の復元
            if (settingDir.Exists)
            {
                serializer = new XmlSerializer(typeof(string));
                try
                {
                    sr = new StreamReader(
                            settingDir.FullName + Path.DirectorySeparatorChar + DataContextClass.RECORD_SETTING_FILENAME,
                            new System.Text.UTF8Encoding(false)
                            );

                    this._TvProgramDirectoryPath = (string)serializer.Deserialize(sr);
                }
                catch (FileNotFoundException e)
                {
                    System.Console.WriteLine(e);
                    this._TvProgramDirectoryPath = String.Empty;
                }
                finally
                {
                    if (sr != null) sr.Close();
                    sr = null;
                    serializer = null;
                }
            }

            // フィルター ツリーの復元
            if (Directory.Exists(this.appPath + Path.DirectorySeparatorChar + DataContextClass.FILTER_SETTING_DIRNAME))
            {
                this.FilterTreeRoot[0] = this.filterSerializer.Deserialize();
            }
        }
        #endregion Private Method

        #region Event Handler
        /// <summary>
        /// TvProgram の PropertyChanged イベントに設定するイベント ハンドラー。
        /// TvProgram の選択状態が変わったらこのクラスの SelectedPrograms に追加または削除し、このクラスの PropertyChanged イベントを発生する。
        /// TvProgramWatcher に設定されるイベント ハンドラーの内部で、各 TvProgram に設定される。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void tvProgramPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsSelected":
                    TvProgram tvprg = (TvProgram)sender;

                    if (tvprg.IsSelected) this.SelectedPrograms.Add(tvprg);
                    else this.SelectedPrograms.Remove(tvprg);

                    if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("SelectedPrograms"));

                    break;
            }
        }

        /// <summary>
        /// フィルター ツリーのルートに設定するイベント ハンドラー
        /// ノードの選択に変更があったら DataGrid をリフレッシュする。
        /// このクラスの PropertyChanged イベントに、コンストラクターで設定される。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void filterTreePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedItem":
                    refleshFilterdPrograms();
                    break;
            }
        }

        /// <summary>
        /// 監視フォルダ変更時のイベント ハンドラー。
        /// TvProgramWatcher の Path 設定変更と allTvPrograms のクリアを行う。
        /// allTvPrograms のクリアによりイベントが発生し、allTvProgramsCollection_EventHandlers により FilteredItems もクリアされる。
        /// このクラスの PropertyChanged イベントに、コンストラクターで設定される。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void tvpdirParamaterChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TvProgramDirectoryPath":
                    this.tvpw.EnableRaisingEvents = false;
                    this.tvpw.Path = this.TvProgramDirectoryPath;
                    lock (this.lockTargetForAllTvPrograms)
                    {
                        this.allTvPrograms.Clear();
                    }
                    this.tvpw.EnableRaisingEvents = true;
                    break;
            }
        }

        #region TvProgramWatcher_EventHandlers
        /// <summary>
        /// TvProgramWatcher の Found イベント ハンドラー。
        /// allTvPrograms に、新しい TvProgram を先頭に Insert する。
        /// 新しい TvProgram の PropertyChanged イベントに、tvProgramPropertyChanged をイベント ハンドラーとして設定する。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void tvpw_Found(object sender, TvProgramEventArgs e)
        {
            lock (this.lockTargetForAllTvPrograms)
            {
                e.TvProgram.PropertyChanged += this.tvProgramPropertyChanged;
                this.allTvPrograms.Insert(0, e.TvProgram);
            }
        }

        /// <summary>
        /// TvProgramWatcher の Created イベント ハンドラー。
        /// allTvPrograms に、新しい TvProgram を Add する。
        /// 新しい TvProgram の PropertyChanged イベントに、tvProgramPropertyChanged をイベント ハンドラーとして設定する。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void tvpw_Created(object sender, TvProgramEventArgs e)
        {
            lock (this.lockTargetForAllTvPrograms)
            {
                e.TvProgram.PropertyChanged += this.tvProgramPropertyChanged;
                this.allTvPrograms.Add(e.TvProgram);
            }
        }

        /// <summary>
        /// TvProgramWatcher の Changed イベント ハンドラー。
        /// allTvPrograms の、古い TvProgram を新しい TvProgram に替える (ReplaceAt)。
        /// 古い TvProgram の PropertyChanged イベントから、tvProgramPropertyChanged イベント ハンドラーを設定解除する。
        /// 新しい TvProgram の PropertyChanged イベントに、tvProgramPropertyChanged をイベント ハンドラーとして設定する。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void tvpw_Changed(object sender, TvProgramEventArgs e)
        {
            lock (this.lockTargetForAllTvPrograms)
            {
                int idx = this.allTvPrograms.IndexOf(e.TvProgram);
                this.allTvPrograms[idx].IsSelected = false;
                this.allTvPrograms[idx].PropertyChanged -= this.tvProgramPropertyChanged;
                e.TvProgram.PropertyChanged += this.tvProgramPropertyChanged;
                this.allTvPrograms.ReplaceAt(idx, e.TvProgram);
            }
        }

        /// <summary>
        /// TvProgramWatcher の Deleted イベント ハンドラー。
        /// allTvPrograms から、TvProgram を削除する。
        /// 古い TvProgram の PropertyChanged イベントから、tvProgramPropertyChanged イベント ハンドラーを設定解除する。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void tvpw_Deleted(object sender, TvProgramEventArgs e)
        {
            lock (this.lockTargetForAllTvPrograms)
            {
                int idx = this.allTvPrograms.IndexOf(e.TvProgram);
                this.allTvPrograms[idx].IsSelected = false;
                this.allTvPrograms.Remove(e.TvProgram);
                e.TvProgram.PropertyChanged -= this.tvProgramPropertyChanged;
            }
        }

        /// <summary>
        /// TvProgramWatcher の Renamed イベント ハンドラー。
        /// allTvPrograms の、古い TvProgram を新しい TvProgram に替える (ReplaceAt)。
        /// 古い TvProgram の PropertyChanged イベントから、tvProgramPropertyChanged イベント ハンドラーを設定解除する。
        /// 新しい TvProgram の PropertyChanged イベントに、tvProgramPropertyChanged をイベント ハンドラーとして設定する。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void tvpw_Renamed(object sender, TvProgramRenamedEventArgs e)
        {
            lock (this.lockTargetForAllTvPrograms)
            {
                int idx = this.allTvPrograms.IndexOf(e.TvProgram);
                this.allTvPrograms[idx].IsSelected = false;
                this.allTvPrograms[idx].PropertyChanged -= this.tvProgramPropertyChanged;
                e.TvProgram.PropertyChanged += this.tvProgramPropertyChanged;
                this.allTvPrograms.ReplaceAt(idx, e.TvProgram);
            }
        }
        #endregion TvProgramWatcher_EventHandlers

        #region allTvProgramsCollection_EventHandlers
        /// <summary>
        /// allTvPrograms のコレクション変更イベント ハンドラー。
        /// allTvPrograms の変更に合わせて、FiltratedItems を更新する。
        /// また、ステータス バーに表示する全番組数を更新する。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="args">イベントの引数</param>
        private void allTvPrograms_CollectionChanged(Object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (args.NewStartingIndex == 0)
                    {
                        foreach (Object o in args.NewItems)
                        {
                            this.onInserted((TvProgram)o);
                        }
                    }
                    else
                    {
                        foreach (Object o in args.NewItems)
                        {
                            this.onAdded((TvProgram)o);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (Object o in args.OldItems)
                    {
                        this.onRemoved((TvProgram)o);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    int limit = Math.Min(args.NewItems.Count, args.OldItems.Count);
                    for(int i = 0; i < limit; i++)
                    {
                        this.onReplaced((TvProgram)args.OldItems[i], (TvProgram)args.NewItems[i]);
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.onCleared();
                    break;
                default:
                    throw new NotImplementedException();
            }
            
            this.updateAllNumber();
        }

        /// <summary>
        /// 全番組数を更新する。allTvPrograms コレクションのイベント ハンドラーから呼ばれる。
        /// </summary>
        private void updateAllNumber()
        {
            this.AllNumberString = Properties.Resources.Status_All + ": " + this.allTvPrograms.Count;
            if (this.tvpw.IsSearchingNow)
            {
                this.SearchProgress = this.tvpw.SearchProgress;
            }
            else
            {
                this.SearchProgress = 1;
            }
        }

        /// <summary>
        /// allTvPrograms の先頭に追加された場合の処理。
        /// フィルターにマッチする場合、FiltratedItems の先頭に挿入される。
        /// </summary>
        /// <param name="tvprg">挿入された TvProgram</param>
        private void onInserted(TvProgram tvprg)
        {
            ((TvProgramHierarchicalFilter)this.FilterTreeRoot[0]).UnviewedCountUp(tvprg);

            if (this.FilterTreeRoot[0].SelectedItem.Match(tvprg))
            {
                lock (this.lockTargetForFiltratedItems)
                {
                    this.FiltratedItems.Insert(0, tvprg);
                }
            }
        }

        /// <summary>
        /// allTvPrograms の先頭以外に追加された場合の処理。
        /// フィルターにマッチする場合、FiltratedItems の末尾に追加される。
        /// </summary>
        /// <param name="tvprg">追加された TvProgram</param>
        private void onAdded(TvProgram tvprg)
        {
            ((TvProgramHierarchicalFilter)this.FilterTreeRoot[0]).UnviewedCountUp(tvprg);

            if (this.FilterTreeRoot[0].SelectedItem.Match(tvprg))
            {
                lock (this.lockTargetForFiltratedItems)
                {
                    this.FiltratedItems.Add(tvprg);
                }
            }
        }

        /// <summary>
        /// allTvPrograms から削除された場合の処理。
        /// FiltratedItems に tvPrg が存在する場合、削除する。
        /// </summary>
        /// <param name="tvprg">削除された TvProgram</param>
        private void onRemoved(TvProgram tvprg)
        {
            ((TvProgramHierarchicalFilter)this.FilterTreeRoot[0]).UnviewedCountDown(tvprg);
            lock (this.lockTargetForFiltratedItems)
            {
                int index = this.FiltratedItems.IndexOf(tvprg);
                if (index >= 0)
                {
                    this.FiltratedItems[index].IsSelected = false;
                }
                this.FiltratedItems.Remove(tvprg);
            }
        }

        /// <summary>
        /// allTvPrograms で置き換えられた場合の処理。
        /// FiltratedItems に oldTvPrg が存在する場合、newTvPrg に置き換える。
        /// </summary>
        /// <param name="oldTvprg">置き換え前の TvProgram</param>
        /// <param name="newTvprg">置き換え後の TvProgram</param>
        private void onReplaced(TvProgram oldTvprg, TvProgram newTvprg)
        {
            ((TvProgramHierarchicalFilter)this.FilterTreeRoot[0]).UnviewedCountDown(oldTvprg);
            ((TvProgramHierarchicalFilter)this.FilterTreeRoot[0]).UnviewedCountUp(newTvprg);
            lock (this.lockTargetForFiltratedItems)
            {
                int index = this.FiltratedItems.IndexOf(oldTvprg);
                if (index >= 0)
                {
                    this.FiltratedItems[index].IsSelected = false;
                }
                this.FiltratedItems.Replace(oldTvprg, newTvprg);
            }
        }

        /// <summary>
        /// allTvPrograms でクリアされた場合の処理。
        /// FiltratedItems をクリアする。
        /// </summary>
        private void onCleared()
        {
            lock (this.lockTargetForFiltratedItems)
            {
                this.FiltratedItems.Clear();
                ((TvProgramHierarchicalFilter)this.FilterTreeRoot[0]).UnviewedCountClear();
            }
        }
        #endregion allTvProgramsCollection_EventHandlers
        
        #region FiltrateItemsCollection_EventHandlers
        /// <summary>
        /// FiltratedItems のコレクション変更イベント ハンドラー。
        /// ステータス バーに表示する、現在表示中の番組数を更新する。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="args">イベントの引数</param>
        private void filtratedItems_CollectionChanged(Object sender, NotifyCollectionChangedEventArgs args)
        {
            this.FilterResultNumberString = Properties.Resources.Status_Filtered + ": " + this.FiltratedItems.Count;
        }
        #endregion FiltrateItemsCollection_EventHandlers
        #endregion Event Handler

        #region Command
        /// <summary>
        /// 監視するディレクトリを設定するコマンド。
        /// </summary>
        private class OpenDirectoryCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public OpenDirectoryCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            /// <summary>
            /// 監視するディレクトリを設定する。
            /// その際、フィルターを含めた全設定が保存される。
            /// </summary>
            /// <param name="parameter">監視するディレクトリのフル パスを示す文字列。</param>
            public void Execute(object parameter)
            {
                string newPath = (string)parameter;

                if (Directory.Exists(newPath))
                {
                    if (newPath != owner.TvProgramDirectoryPath)
                    {
                        owner.TvProgramDirectoryPath = newPath;
                        owner.saveSettings();
                    }
                }
            }
        }

        /// <summary>
        /// フィルターの編集を開始するコマンド。
        /// </summary>
        private class EditStartCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public EditStartCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.FilterTreeRoot[0].PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (this.owner.FilterTreeRoot[0].SelectedItem == null) return false;
                if (this.owner.FilterTreeRoot[0].SelectedItem.IsRoot) return false;

                if (this.owner.FilterTreeRoot[0].SelectedItem.Filter is TvProgramStandardFilter ||
                        this.owner.FilterTreeRoot[0].SelectedItem.Filter is TvProgramFolderFilter)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// フィルターの編集を開始する。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                owner.EditData = (ITvProgramFilter)((ITvProgramFilter)owner.FilterTreeRoot[0].SelectedItem.Filter).Clone();
                if (owner.FilterTreeRoot[0].SelectedItem.Filter is TvProgramFolderFilter)
                {
                    owner.IsNotFolderOnEditData = false;
                    //owner.EditData = new TvProgramStandardFilter() { Name = ((TvProgramHierarchicalFilter)owner.FilterTreeRoot[0].SelectedItem).Name };
                }
                else
                {
                    owner.IsNotFolderOnEditData = true;
                    //owner.EditData = (TvProgramStandardFilter)((TvProgramStandardFilter)owner.FilterTreeRoot[0].SelectedItem.Filter).Clone();
                }
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedItem":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// 新しいフィルターを作成するコマンド。
        /// </summary>
        private class NewCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public NewCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.FilterTreeRoot[0].PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (this.owner.FilterTreeRoot[0].SelectedItem == null) return false;
                return true;
            }

            /// <summary>
            /// 新しいフィルターを作成する。
            /// その際、監視するディレクトリを含めたすべての設定が保存される。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                TvProgramHierarchicalFilter hFilter = new TvProgramHierarchicalFilter() { Filter = new TvProgramStandardFilter() };
                owner.addFilter(hFilter, this.owner.FilterTreeRoot[0].SelectedItem);
                owner.saveSettings();
                owner.refleshUnviewedNumber();
                hFilter.IsSelected = true;
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedItem":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// 「その他」フィルターを作成するコマンド。
        /// </summary>
        private class NewOthersCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public NewOthersCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.FilterTreeRoot[0].PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (this.owner.FilterTreeRoot[0].SelectedItem == null) return false;
                return true;
            }

            /// <summary>
            /// 「その他」フィルターを作成する。
            /// その際、監視するディレクトリを含めたすべての設定が保存される。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                TvProgramHierarchicalFilter hFilter = new TvProgramHierarchicalFilter();
                hFilter.Filter = new TvProgramResudueFilter();
                owner.addFilter(hFilter, this.owner.FilterTreeRoot[0].SelectedItem);
                owner.saveSettings();
                owner.refleshUnviewedNumber();
                hFilter.IsSelected = true;
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedItem":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// 新しいフォルダーを作成するコマンド。
        /// </summary>
        private class NewFolderCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public NewFolderCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.FilterTreeRoot[0].PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (this.owner.FilterTreeRoot[0].SelectedItem == null) return false;
                return true;
            }

            /// <summary>
            /// 新しいフォルダーを作成する。
            /// その際、監視するディレクトリを含めたすべての設定が保存される。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                TvProgramHierarchicalFilter hFilter = new TvProgramHierarchicalFilter() { Filter = new TvProgramFolderFilter() };
                owner.addFilter(hFilter, this.owner.FilterTreeRoot[0].SelectedItem);
                owner.saveSettings();
                owner.refleshUnviewedNumber();
                hFilter.IsSelected = true;
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedItem":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// フィルターを削除するコマンド。
        /// </summary>
        private class DeleteCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public DeleteCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.FilterTreeRoot[0].PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (this.owner.FilterTreeRoot[0].SelectedItem == null) return false;
                if (this.owner.FilterTreeRoot[0].SelectedItem.IsRoot) return false;
                return true;
            }

            /// <summary>
            /// フィルターを削除する。
            /// その際、監視するディレクトリを含めたすべての設定が保存される。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                IHierarchicalFilter<TvProgram> currentItem = owner.FilterTreeRoot[0].SelectedItem;
                currentItem.Parent.Children.Remove(currentItem);
                owner.saveSettings();
                owner.refleshUnviewedNumber();
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedItem":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// フィルターをコピーするコマンド。
        /// </summary>
        private class CopyFilterCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public CopyFilterCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.FilterTreeRoot[0].PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (this.owner.FilterTreeRoot[0].SelectedItem == null) return false;
                if (this.owner.FilterTreeRoot[0].SelectedItem.IsRoot) return false;
                if (this.owner.FilterTreeNewParent == this.owner.FilterTreeRoot[0].SelectedItem) return false; // 自分はダメ
                if (this.owner.FilterTreeNewParent == this.owner.FilterTreeRoot[0].SelectedItem.Parent) return false; // 現在の親はダメ
                if (this.owner.FilterTreeRoot[0].SelectedItem.IsSubFilter(this.owner.FilterTreeNewParent)) return false; // 子や孫はダメ

                return true;
            }

            public void Execute(object parameter)
            {
                MessageBox.Show("フィルターのコピー操作は未実装です。実装予定です。");
                owner.saveSettings();
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedItem":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// フィルターを移動するコマンド。
        /// </summary>
        private class MoveFilterCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public MoveFilterCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.FilterTreeRoot[0].PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (this.owner.FilterTreeRoot[0].SelectedItem == null) return false;
                if (this.owner.FilterTreeRoot[0].SelectedItem.IsRoot) return false;
                if (this.owner.FilterTreeNewParent == this.owner.FilterTreeRoot[0].SelectedItem) return false; // 自分はダメ
                if (this.owner.FilterTreeNewParent == this.owner.FilterTreeRoot[0].SelectedItem.Parent) return false; // 現在の親はダメ
                if (this.owner.FilterTreeRoot[0].SelectedItem.IsSubFilter(this.owner.FilterTreeNewParent)) return false; // 子や孫はダメ

                return true;
            }

            public void Execute(object parameter)
            {
                IHierarchicalFilter<TvProgram> draggedItem = owner.FilterTreeRoot[0].SelectedItem;
                IHierarchicalFilter<TvProgram> dropTargetItem = owner.FilterTreeNewParent;

                draggedItem.Parent.IsSelected = true;
                owner.FilterTreeRoot[0].SelectedItem.Children.Remove(draggedItem); // この時点で SelectedItem は親に移ってる。
                owner.addFilter(draggedItem, dropTargetItem);
                draggedItem.IsSelected = true;
                owner.saveSettings();
                owner.refleshUnviewedNumber();
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedItem":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// フィルターの編集を適用し、編集を終了するコマンド。
        /// </summary>
        private class EditOkCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public EditOkCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (owner.HasErrorEditData) return false;
                return true;
            }

            public void Execute(object parameter)
            {
                if (this.owner.IsChangedEditData)
                {
                    /*if (owner.IsNotFolderOnEditData)
                    {*/
                        TvProgramHierarchicalFilter parent = (TvProgramHierarchicalFilter)this.owner.FilterTreeRoot[0].SelectedItem.Parent;
                        TvProgramHierarchicalFilter current = (TvProgramHierarchicalFilter)this.owner.FilterTreeRoot[0].SelectedItem;

                        TvProgramHierarchicalFilter hFilter = new TvProgramHierarchicalFilter();
                        hFilter.Filter = this.owner.EditData;
                        foreach (TvProgramHierarchicalFilter child in this.owner.FilterTreeRoot[0].SelectedItem.Children)
                        {
                            hFilter.Children.Add(child);
                        }
                        foreach (TvProgramHierarchicalFilter child in hFilter.Children)
                        {
                            this.owner.FilterTreeRoot[0].SelectedItem.Children.Remove(child);
                        }

                        owner.addFilter(hFilter, this.owner.FilterTreeRoot[0].SelectedItem.Parent);

                        hFilter.IsSelected = true;

                        parent.Children.Remove(current);
                    /*}
                    else
                    {
                        TvProgramHierarchicalFilter parent = (TvProgramHierarchicalFilter)this.owner.FilterTreeRoot[0].SelectedItem.Parent;
                        TvProgramHierarchicalFilter current = (TvProgramHierarchicalFilter)this.owner.FilterTreeRoot[0].SelectedItem;

                        TvProgramHierarchicalFilter hFilter = new TvProgramHierarchicalFilterFolder();
                        hFilter.Name = this.owner.EditData.Name;
                        foreach (TvProgramHierarchicalFilter child in this.owner.FilterTreeRoot[0].SelectedItem.Children)
                        {
                            hFilter.Children.Add(child);
                        }
                        foreach (TvProgramHierarchicalFilter child in hFilter.Children)
                        {
                            this.owner.FilterTreeRoot[0].SelectedItem.Children.Remove(child);
                        }

                        owner.addFilter(hFilter, this.owner.FilterTreeRoot[0].SelectedItem.Parent);

                        hFilter.IsSelected = true;

                        parent.Children.Remove(current);
                    }*/
                }

                owner.saveSettings();
                owner.refleshUnviewedNumber();
                this.owner.EditData = null;
                this.owner.IsChangedEditData = false;
                this.owner.IsNotFolderOnEditData = true;
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "HasErrorEditData":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// フィルターの編集を破棄し、編集を終了するコマンド。
        /// </summary>
        private class EditCancelCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public EditCancelCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            /// <summary>
            /// フィルターの編集を破棄し、編集を終了する。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                this.owner.EditData = null;
                this.owner.IsChangedEditData = false;
            }
        }

        /// <summary>
        /// フィルターの編集を適用するコマンド。
        /// </summary>
        private class EditApplyCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public EditApplyCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (owner.HasErrorEditData) return false;
                if (!owner.IsChangedEditData) return false;

                return true;
            }

            public void Execute(object parameter)
            {
                /*if (owner.IsNotFolderOnEditData)
                {*/
                    TvProgramHierarchicalFilter parent = (TvProgramHierarchicalFilter)this.owner.FilterTreeRoot[0].SelectedItem.Parent;
                    TvProgramHierarchicalFilter current = (TvProgramHierarchicalFilter)this.owner.FilterTreeRoot[0].SelectedItem;

                    TvProgramHierarchicalFilter hFilter = new TvProgramHierarchicalFilter();
                    hFilter.Filter = (TvProgramStandardFilter)this.owner.EditData.Clone();
                    foreach (TvProgramHierarchicalFilter child in this.owner.FilterTreeRoot[0].SelectedItem.Children)
                    {
                        hFilter.Children.Add(child);
                    }
                    foreach (TvProgramHierarchicalFilter child in hFilter.Children)
                    {
                        this.owner.FilterTreeRoot[0].SelectedItem.Children.Remove(child);
                    }

                    owner.addFilter(hFilter, this.owner.FilterTreeRoot[0].SelectedItem.Parent);

                    hFilter.IsSelected = true;
                    parent.Children.Remove(current);
                /*}
                else
                {
                    TvProgramHierarchicalFilter parent = (TvProgramHierarchicalFilter)this.owner.FilterTreeRoot[0].SelectedItem.Parent;
                    TvProgramHierarchicalFilter current = (TvProgramHierarchicalFilter)this.owner.FilterTreeRoot[0].SelectedItem;

                    TvProgramHierarchicalFilter hFilter = new TvProgramHierarchicalFilterFolder();
                    hFilter.Name = owner.EditData.Name;
                    foreach (TvProgramHierarchicalFilter child in this.owner.FilterTreeRoot[0].SelectedItem.Children)
                    {
                        hFilter.Children.Add(child);
                    }
                    foreach (TvProgramHierarchicalFilter child in hFilter.Children)
                    {
                        this.owner.FilterTreeRoot[0].SelectedItem.Children.Remove(child);
                    }

                    owner.addFilter(hFilter, this.owner.FilterTreeRoot[0].SelectedItem.Parent);

                    hFilter.IsSelected = true;
                    parent.Children.Remove(current);
                }*/

                owner.saveSettings();
                owner.refleshUnviewedNumber();
                this.owner.IsChangedEditData = false;
                //this.owner.refleshFilterdPrograms();
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "HasErrorEditData":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                    case "IsChangedEditData":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }
        
        /// <summary>
        /// 選択されている 1 つの番組を開くコマンド。
        /// </summary>
        private class OpenProgramCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public OpenProgramCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (owner.SelectedPrograms.Count != 1) return false;
                return true;
            }

            /// <summary>
            /// 選択されている 1 つの番組を開く。開いた際に視聴済みにする。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                TvProgram selectedItem = null;
                foreach (TvProgram tvPrg in owner.SelectedPrograms)
                {
                    System.Diagnostics.Process p = System.Diagnostics.Process.Start(tvPrg.FilePath);
                    if(selectedItem == null) selectedItem = tvPrg;
                }

                // なぜか選択解除が必要
                selectedItem.IsSelected = false;
                
                TvProgramManipurator.MakeViewed(selectedItem);
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedPrograms":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// 選択されている番組を視聴済み状態にするコマンド。
        /// </summary>
        private class MakeViewedCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public MakeViewedCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (owner.SelectedPrograms.Count < 1) return false;
                return true;
            }

            /// <summary>
            /// 選択されている番組を視聴済み状態にする。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                TvProgram[] selectedPrograms = new TvProgram[owner.SelectedPrograms.Count];
                owner.SelectedPrograms.CopyTo(selectedPrograms);
                foreach (TvProgram tvPrg in selectedPrograms)
                {
                    TvProgramManipurator.MakeViewed(tvPrg);
                }
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedPrograms":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// 選択されている番組を未視聴状態にするコマンド。
        /// </summary>
        private class MakeUnviewedCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public MakeUnviewedCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (owner.SelectedPrograms.Count < 1) return false;
                return true;
            }

            /// <summary>
            /// 選択されている番組を未視聴にする。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                TvProgram[] selectedPrograms = new TvProgram[owner.SelectedPrograms.Count];
                owner.SelectedPrograms.CopyTo(selectedPrograms);
                foreach (TvProgram tvPrg in selectedPrograms)
                {
                    TvProgramManipurator.MakeUnviewed(tvPrg);
                }
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedPrograms":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// 選択されている番組を削除するコマンド。
        /// </summary>
        private class DeleteProgramCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public DeleteProgramCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (owner.SelectedPrograms.Count < 1) return false;
                return true;
            }

            /// <summary>
            /// 選択されている番組を削除する。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                if (owner.IsConfirmedToDeletingPrograms)
                {
                    TvProgram[] tvProgs = new TvProgram[owner.SelectedPrograms.Count];
                    owner.SelectedPrograms.CopyTo(tvProgs, 0);
                    TvProgramManipurator.Delete(tvProgs);
                    owner.IsConfirmedToDeletingPrograms = false;
                }
                else { throw new InvalidOperationException("IsConfirmedDeleteingProgram が true ではありません。");}
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "SelectedPrograms":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        /// <summary>
        /// 表示中の番組すべてを選択するコマンド。
        /// </summary>
        private class SelectAllCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public SelectAllCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            /// <summary>
            /// 表示中の番組すべてを選択する。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                foreach (TvProgram tvPrg in owner.FiltratedItems)
                {
                    tvPrg.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// 選択されているすべての番組を選択解除するコマンド。
        /// </summary>
        private class UnselectAllCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public UnselectAllCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
            }

            public bool CanExecute(object parameter)
            {
                if (owner.SelectedPrograms.Count <= 1) return false;

                return true;
            }

            /// <summary>
            /// 選択されているすべての番組を選択解除する。
            /// </summary>
            /// <param name="parameter">パラメーター (使用されないため、null で問題ない)</param>
            public void Execute(object parameter)
            {
                TvProgram[] selectedPrograms = new TvProgram[owner.SelectedPrograms.Count];
                owner.SelectedPrograms.CopyTo(selectedPrograms);
                foreach (TvProgram tvPrg in selectedPrograms)
                {
                    tvPrg.IsSelected = false;
                }
            }
        }

        private class ScaleUpCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public ScaleUpCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if (owner.Scale >= MAX_SCALE) return false;

                return true;
            }

            public void Execute(object parameter)
            {
                owner.Scale = owner.Scale * 1.1;
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "Scale":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }
        
        private class ScaleDownCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public ScaleDownCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
                this.owner.PropertyChanged += this.propertyChangedEventHandler;
            }

            public bool CanExecute(object parameter)
            {
                if(owner.Scale <= 1.0) return false;

                return true;
            }

            public void Execute(object parameter)
            {
                double newScale = owner.Scale / 1.1;
                owner.Scale = newScale > 1.0 ? newScale : 1.0;
            }

            private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "Scale":
                        if (this.CanExecuteChanged != null) this.CanExecuteChanged(this, EventArgs.Empty);
                        break;
                }
            }
        }

        private class ScaleResetCommandImpl : ICommand
        {
            private DataContextClass owner;

            public event EventHandler CanExecuteChanged;

            public ScaleResetCommandImpl(DataContextClass owner)
            {
                this.owner = owner;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                owner.Scale = 1.0;
            }
        }
        #endregion Command
    }
}
