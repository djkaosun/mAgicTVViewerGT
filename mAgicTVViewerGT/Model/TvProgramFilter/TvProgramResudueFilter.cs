using System;
using mAgicTVViewerGT.Model.FilterCriteria;
using mAgicTVViewerGT.Model.TvProgramWatcher;

namespace mAgicTVViewerGT.Model.TvProgramFilter
{
    public class TvProgramResudueFilter : ResudueFilter<TvProgram>, ITvProgramFilter
    {
        [System.Xml.Serialization.XmlIgnore]
        public string Name {
            get { return Properties.Resources.General_Others; }
            set
            {
                throw new InvalidOperationException("このクラスは名前を設定できません。");
            }
        }

        private string _Icon = "Resources/Resudue.png";
        public string Icon
        {
            get { return this._Icon; }
            set
            {
                this._Icon = value;
            }
        }

        public Object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
