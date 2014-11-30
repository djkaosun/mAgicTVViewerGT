using System;
using mAgicTVViewerGT.Model.FilterCriteria;
using mAgicTVViewerGT.Model.TvProgramWatcher;

namespace mAgicTVViewerGT.Model.TvProgramFilter
{
    /// <summary>
    /// TvProgram に特化したフィルターのインターフェイスです。
    /// </summary>
    public interface ITvProgramFilter : IFilter<TvProgram>, ICloneable
    {
        /// <summary>
        /// フィルター名。
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// フィルターを示すアイコン。
        /// </summary>
        string Icon { get; set; }
    }
}
