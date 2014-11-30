using System;
using System.Collections.Generic;
using mAgicTVViewerGT.Model.FilterCriteria;
using mAgicTVViewerGT.Model.TvProgramWatcher;

namespace mAgicTVViewerGT.Model.TvProgramFilter
{
    /// <summary>
    /// TvProgram に特化した階層フィルターのインターフェイスです。
    /// </summary>
    public interface ITvProgramHierarchicalFilter : IHierarchicalFilter<TvProgram>
    {
        /// <summary>
        /// 階層フィルター名。
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 階層フィルターを示すアイコン。
        /// </summary>
        string Icon { get; set; }

        /// <summary>
        /// カウントされた未視聴番組数
        /// </summary>
        int UnviewedNumber { get; }

        /// <summary>
        /// カウントされた未視聴番組数を示す文字列
        /// </summary>
        string UnviewedNumberString { get; }

        /// <summary>
        /// 階層フィルターに適合する番組であり、かつ、未視聴であった場合、カウント アップします。
        /// 子フィルターの UnviewedCountUp メソッドも呼び出します。
        /// </summary>
        /// <param name="tvPrg">カウントする番組</param>
        void UnviewedCountUp(TvProgram tvPrg);

        /// <summary>
        /// 階層フィルターに適合する番組であり、かつ、未視聴であった場合、カウント ダウンします。
        /// 子フィルターの UnviewedCountDown メソッドも呼び出します。
        /// </summary>
        /// <param name="tvPrg">カウントする番組</param>
        void UnviewedCountDown(TvProgram tvPrg);

        /// <summary>
        /// 未視聴番組数のカウントを 0 にします。
        /// 子フィルターの UnviewedCountClear メソッドも呼び出します。
        /// </summary>
        void UnviewedCountClear();

        /// <summary>
        /// 配列に含まれる番組のうち、階層フィルターに適合する番組数をカウントします。
        /// これまでのカウントはリセットされます。
        /// 子フィルターの UnviewedCount メソッドも呼び出します。
        /// </summary>
        /// <param name="tvPrg">カウントする番組のリスト</param>
        void UnviewedCount(TvProgram[] tvPrgs);
    }
}
