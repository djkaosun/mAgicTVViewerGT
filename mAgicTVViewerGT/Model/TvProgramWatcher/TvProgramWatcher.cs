using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace mAgicTVViewerGT.Model.TvProgramWatcher
{
    public delegate void TvProgramEventHandler(Object sender, TvProgramEventArgs e);
    public class TvProgramWatcher
    {
        private HashSet<string> files;
        private FileSystemWatcher fsw;
        private static string FILE_NAME_FILTER = "*.dgno";
        private static string FILE_EXT = ".dgno";
        public bool IsSearchingNow { get; private set; }
        public double SearchProgress { get; private set; }
        private Thread searchThread = null;
        private Object lockTarget = new Object();

        private string directory = String.Empty;
        public string Path {
            get
            {
                return this.directory;
            }
            set
            {
                value = (value == null) ? String.Empty : value;
                if (String.Compare(this.directory, value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (!Directory.Exists(value)) throw new ArgumentException("パスの形式が無効です。");

                    if (this.enabled)
                    {
                        this.StopRaisingEvents();
                    }

                    this.directory = value;
                    this.fsw.Path = value;
                    this.files.Clear();

                    if (this.enabled)
                    {
                        this.StartRaisingEvents();
                    }
                }
            }
        }

        private bool enabled = false;
        public bool EnableRaisingEvents {
            get
            {
                return this.enabled;
            }
            set
            {
                if (this.enabled == value)
                {
                    return;
                }

                this.enabled = value;

                if (this.enabled)
                {
                    this.StartRaisingEvents();
                }
                else
                {
                    this.StopRaisingEvents();
                }
            }
        }

        public event EventHandler<TvProgramEventArgs> Found;
        public event EventHandler<TvProgramEventArgs> Created;
        public event EventHandler<TvProgramEventArgs> Changed;
        public event EventHandler<TvProgramEventArgs> Deleted;
        public event EventHandler<TvProgramRenamedEventArgs> Renamed;

        public TvProgramWatcher()
        {
            this.files = new HashSet<string>();
            this.fsw = new FileSystemWatcher();
            this.fsw.InternalBufferSize = 65536;
            this.fsw.Created += this.fswCreatedHandler;
            this.fsw.Changed += this.fswChangedHandler;
            this.fsw.Deleted += this.fswDeletedHandler;
            this.fsw.Renamed += this.fswRenamedHandler;
            //this.fsw.NotifyFilter = (NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName);
            this.fsw.NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.FileName);
            this.fsw.IncludeSubdirectories = true;
        }

        /// <summary>
        /// このクラスの EnableRaisingEvents プロパティが true になった時に呼び出される。
        /// FileSystemWatcher.EnableRaisingEvents プロパティはこのメソッド内で true にされる。
        /// </summary>
        private void StartRaisingEvents()
        {
            // FileSystemWatcher による監視を開始
            this.fsw.EnableRaisingEvents = true;

            // 指定フォルダ以下のファイルを検索するスレッドを開始。
            this.searchThread = new Thread(this.RaisingEvents);
            this.searchThread.IsBackground = true;
            searchThread.Start();
        }

        /// <summary>
        /// このクラスの EnableRaisingEvents プロパティが false になった時に呼び出される。
        /// FileSystemWatcher.EnableRaisingEvents プロパティはこのメソッド内で false にされる。
        /// </summary>
        private void StopRaisingEvents()
        {
            // FileSystemWatcher による監視を停止
            this.fsw.EnableRaisingEvents = false;

            // スレッドを中断
            if (this.searchThread != null)
            {
                this.searchThread.Abort();
                this.searchThread.Join();
            }

            // クリーンアップ
            this.files.Clear();
            this.searchThread = null;
            this.SearchProgress = 0;
        }

        /// <summary>
        /// ファイル検索スレッド用メソッド。
        /// </summary>
        private void RaisingEvents()
        {
            try
            {
                // 協調動作の開始
                this.IsSearchingNow = true;

                // 検索処理
                SearchAllFiles(this.Path, true);

                // 協調動作の終了
                this.IsSearchingNow = false;
                this.SearchProgress = 1;

                // 協調動作後のクリーンアップ
                //lock (lockTarget)
                //{
                //    this.files.Clear();
                //}
            }
            catch (ThreadAbortException) {
                Console.WriteLine("RaisingEvents スレッドを中断しました。");
            }
        }

        /// <summary>
        /// 指定ディレクトリ以下のファイルを探索し、ファイル発見ごとにこのクラスの Found イベントを発生する。
        /// 更新日時が新しいものから順にイベントを発生する。
        /// </summary>
        /// <param name="curdirstr">ファイル検索対象のフォルダー</param>
        private void SearchAllFiles(String currentdir, bool isRecursionEntry)
        {
            // 順次 Found イベントを発生させるため、再帰的なファイル検索を独自に実装した。

            int allnum = 0;
            int proceednum = 0;
            if (isRecursionEntry)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(currentdir);
                allnum = dirInfo.GetFileSystemInfos().Length;
            }

            // 直下のファイルの検索
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(currentdir);
                List<FileInfo> fileList = new List<FileInfo>(dirInfo.GetFiles(TvProgramWatcher.FILE_NAME_FILTER));
                //fileList.Sort(delegate(FileInfo x, FileInfo y) { return -x.LastWriteTime.CompareTo(y.LastWriteTime); });
                fileList.Sort(delegate(FileInfo x, FileInfo y) { return -x.FullName.CompareTo(y.FullName); });
                foreach (FileInfo f in fileList)
                {
                    lock (lockTarget)
                    {
                        if (!this.files.Contains(f.FullName.ToLower()))
                        {
                            try
                            {
                                TvProgram tvprg = new TvProgram(f.FullName);
                                TvProgramEventArgs e = new TvProgramEventArgs(WatcherChangeTypes.Created, tvprg);
                                this.Found(this, e);
                                this.files.Add(f.FullName.ToLower());
                            }
                            catch (UnauthorizedAccessException)
                            {
                                Console.WriteLine("アクセス権限がないファイルを無視しました。");
                            }
                            catch (FileNotFoundException)
                            {
                                Console.WriteLine("処理中にファイルが削除されました。");
                            }
                        }
                    }
                }
                if (isRecursionEntry)
                {
                    proceednum++;
                    this.SearchProgress = Math.Min((1.0 * proceednum) / allnum, 1.0);
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("アクセス権限がないフォルダを無視しました。");
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("処理中にフォルダが削除されました。");
            }

            // サブディレクトリの検索
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(currentdir);
                List<DirectoryInfo> subDirList = new List<DirectoryInfo>(dirInfo.GetDirectories());
                //subDirList.Sort(delegate(DirectoryInfo x, DirectoryInfo y) { return -x.LastWriteTime.CompareTo(y.LastWriteTime); });
                subDirList.Sort(delegate(DirectoryInfo x, DirectoryInfo y) { return -x.FullName.CompareTo(y.FullName); });
                foreach (DirectoryInfo subdir in subDirList)
                {
                    SearchAllFiles(subdir.FullName, false);
                    if (isRecursionEntry)
                    {
                        proceednum++;
                        this.SearchProgress = Math.Min((1.0 * proceednum) / allnum, 1.0);
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("アクセス権限がないフォルダを無視しました。");
                if (isRecursionEntry)
                {
                    proceednum++;
                    this.SearchProgress = Math.Min((1.0 * proceednum) / allnum, 1.0);
                }
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("処理中にフォルダが削除されました。");
                if (isRecursionEntry)
                {
                    proceednum++;
                    this.SearchProgress = Math.Min((1.0 * proceednum) / allnum, 1.0);
                }
            }
        }

        /// <summary>
        /// FileSystemWatcher.Created イベント ハンドラー。
        /// 内部的に、fswCteatedHandler メソッドを実行している。
        /// </summary>
        /// <param name="sender">イベントの発生元 (FileSystemWatcher)</param>
        /// <param name="e">イベント発生元からの引数</param>
        private void fswCreatedHandler(Object sender, FileSystemEventArgs e)
        {
            if (!e.Name.ToLower().EndsWith(TvProgramWatcher.FILE_EXT)) return;

            lock (lockTarget)
            {
                this.onCreated(sender, e);
            }
        }

        /// <summary>
        /// 新しいファイルが見つかった時に、このクラスの Created イベントを発生する。
        /// </summary>
        /// <param name="sender">イベントの発生元 (FileSystemWatcher)</param>
        /// <param name="args">イベント発生元からの引数</param>
        private void onCreated(Object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(5);
            string lowerFullPath = e.FullPath.ToLower();
            if (!this.files.Contains(lowerFullPath))
            {
                try
                {
                    TvProgram tvprg = new TvProgram(e.FullPath);
                    TvProgramEventArgs tvargs = new TvProgramEventArgs(WatcherChangeTypes.Created, tvprg);
                    // ファイルを読み取る前に消えた場合、例外が発生しうるけど、とりあえず気にしない。
                    this.Created(this, tvargs);
                }
                catch (UnauthorizedAccessException) { } // アクセス権限がない場合は無視する
                catch (FileNotFoundException) { } // ファイルが見当たらなくても無視して、Deleted イベントに任せる。

                this.files.Add(lowerFullPath);
            }
        }

        /// <summary>
        /// FileSystemWatcher.Changed イベント ハンドラー。
        /// 内部的に、fswChangedHandler メソッドを実行している。
        /// </summary>
        /// <param name="sender">イベントの発生元 (FileSystemWatcher)</param>
        /// <param name="e">イベント発生元からの引数</param>
        private void fswChangedHandler(Object sender, FileSystemEventArgs e)
        {
//System.Threading.Thread.Sleep(1000);
            if (!e.Name.ToLower().EndsWith(TvProgramWatcher.FILE_EXT)) return;

            lock (lockTarget)
            {
                this.onChanged(sender, e);
            }
        }

        /// <summary>
        /// ファイルが更新されたときに、このクラスの Changed イベントを発生する。
        /// </summary>
        /// <param name="sender">イベントの発生元 (FileSystemWatcher)</param>
        /// <param name="args">イベント発生元からの引数</param>
        private void onChanged(Object sender, FileSystemEventArgs e)
        {
            Thread.Sleep(5);
            string lowerFullPath = e.FullPath.ToLower();
            if (this.files.Contains(lowerFullPath))
            {
                try
                {
                    TvProgram tvprg = new TvProgram(e.FullPath);
                    TvProgramEventArgs tvargs = new TvProgramEventArgs(WatcherChangeTypes.Changed, tvprg);
                    // ファイルを読み取る前に消えた場合、例外が発生しうるけど、とりあえず気にしない。
                    this.Changed(this, tvargs);
                }
                catch (UnauthorizedAccessException) { } // アクセス権限がない場合は無視する
                catch (FileNotFoundException) { } // ファイルが見当たらなくても無視して、Deleted イベントに任せる。
            }
        }

        /// <summary>
        /// FileSystemWatcher.Deleted イベント ハンドラー。
        /// 内部的に、fswDeletedHandler メソッドを実行している。
        /// </summary>
        /// <param name="sender">イベントの発生元 (FileSystemWatcher)</param>
        /// <param name="e">イベント発生元からの引数</param>
        private void fswDeletedHandler(Object sender, FileSystemEventArgs e)
        {
            if (!e.Name.ToLower().EndsWith(TvProgramWatcher.FILE_EXT)) return;

            lock (lockTarget)
            {
                this.onDeleted(sender, e);
            }
        }

        /// <summary>
        /// ファイルが削除されたときに、このクラスの Deleted イベントを発生する。
        /// </summary>
        /// <param name="sender">イベントの発生元 (FileSystemWatcher)</param>
        /// <param name="args">イベント発生元からの引数</param>
        private void onDeleted(Object sender, FileSystemEventArgs e)
        {
            string lowerFullPath = e.FullPath.ToLower();
            if (this.files.Contains(lowerFullPath))
            {
                this.files.Remove(lowerFullPath);

                TvProgram tvprg = new TvProgram(e.FullPath, false);
                TvProgramEventArgs tvargs = new TvProgramEventArgs(WatcherChangeTypes.Deleted, tvprg);
                this.Deleted(this, tvargs);
            }
        }

        /// <summary>
        /// FileSystemWatcher.Renamed イベント ハンドラー。
        /// 内部的に、onRenamedOrCreateOrDelete メソッドを実行している。
        /// </summary>
        /// <param name="sender">イベントの発生元 (FileSystemWatcher)</param>
        /// <param name="e">イベント発生元からの引数</param>
        private void fswRenamedHandler(Object sender, System.IO.RenamedEventArgs e)
        {
            bool isNewTarget = e.Name.ToLower().EndsWith(TvProgramWatcher.FILE_EXT);
            bool isOldTarget = e.OldName.ToLower().EndsWith(TvProgramWatcher.FILE_EXT);
            if (!isNewTarget && !isOldTarget) return;

            lock (lockTarget)
            {
                onRenamedOrCreateOrDelete(sender, e, isNewTarget, isOldTarget);
            }
        }
        
        private void onRenamedOrCreateOrDelete(object sender, RenamedEventArgs e, bool isNewTarget, bool isOldTarget)
        {
            if (isNewTarget && isOldTarget)
            {
                this.onRenamed(sender, e);
            }
            else if (isNewTarget)
            {
                this.onCreated(sender, e);
            }
            else if (isOldTarget)
            {
                this.onDeleted(sender,
                        new FileSystemEventArgs(System.IO.WatcherChangeTypes.Deleted, this.Path, e.OldName));
            }
        }


        private void onRenamed(object sender, RenamedEventArgs e)
        {
            Thread.Sleep(5);
            string lowerNewFullPath = e.FullPath.ToLower();
            string lowerOldFullPath = e.OldFullPath.ToLower();

            bool IsNewFullPathExist = this.files.Contains(lowerNewFullPath);
            bool IsOldFullPathExist = this.files.Contains(lowerOldFullPath);

            if (!IsNewFullPathExist && IsOldFullPathExist)
            {
                this.files.Remove(lowerOldFullPath);

                try
                {
                    TvProgram tvprg = new TvProgram(e.FullPath);
                    // ファイルを読み取る前に消えた場合、例外が発生しうるけど、とりあえず気にしない。
                    TvProgramEventArgs tvargs = new TvProgramEventArgs(WatcherChangeTypes.Created, tvprg);

                    TvProgram otvprg = new TvProgram(e.OldFullPath, false);

                    TvProgramRenamedEventArgs tvrargs = new TvProgramRenamedEventArgs(WatcherChangeTypes.Renamed, tvprg, otvprg);
                    this.Renamed(this, tvrargs);
                }
                catch (UnauthorizedAccessException) { } // アクセス権限がない場合は無視する
                catch (FileNotFoundException) { } // ファイルが見当たらなくても無視して、Deleted イベントに任せる。

                this.files.Add(lowerNewFullPath);
            }
            else if (IsNewFullPathExist && IsOldFullPathExist)
            {
                this.onDeleted(sender,
                        new FileSystemEventArgs(System.IO.WatcherChangeTypes.Deleted, this.Path, e.OldName));
                this.onChanged(sender, e);
            }
            else if (IsNewFullPathExist && !IsOldFullPathExist)
            {
                this.onChanged(sender, e);
            }
            else
            {
                this.onCreated(sender, e);
            }
        }
    }
}
