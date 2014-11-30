using System;

namespace mAgicTVViewerGT.Model.TvProgramWatcher
{
    public class TvProgramEventArgs : EventArgs
    {
        public WatcherChangeTypes ChangeType { get; private set; }
        public TvProgram TvProgram { get; private set; }
        public TvProgramEventArgs(WatcherChangeTypes changeType, TvProgram tvProgram)
        {
            this.ChangeType = changeType;
            this.TvProgram = tvProgram;
        }
    }
}
