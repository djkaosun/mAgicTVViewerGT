using System;

namespace mAgicTVViewerGT.Model.TvProgramWatcher
{
    public class TvProgramRenamedEventArgs : EventArgs
    {
        public WatcherChangeTypes ChangeType { get; private set; }
        public TvProgram TvProgram { get; private set; }
        public TvProgram OldTvProgram { get; private set; }
        public TvProgramRenamedEventArgs(WatcherChangeTypes changeType, TvProgram TvProgram, TvProgram oldTvProgram)
        {
            this.ChangeType = changeType;
            this.TvProgram = TvProgram;
            this.OldTvProgram = oldTvProgram;
        }
    }
}
