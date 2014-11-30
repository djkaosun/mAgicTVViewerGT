using System;
using System.ComponentModel;
using mAgicTVViewerGT.Model.FilterCriteria;
using mAgicTVViewerGT.Model.TvProgramWatcher;

namespace mAgicTVViewerGT.Model.TvProgramFilter
{
    public class TvProgramFolderFilter : FolderFilter<TvProgram>, ITvProgramFilter
    {
        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set
            {
                this._Name = value;
            }
        }

        private string _Icon;
        public string Icon
        {
            get { return this._Icon; }
            set
            {
                this._Icon = value;
            }
        }

        public TvProgramFolderFilter()
        {
            this._Name = Properties.Resources.General_NewFolder;
            this._Icon = "Resources/Folder.png";
        }

        public Object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
