using System;
using System.Collections.Generic;
using mAgicTVViewerGT.Model.FilterCriteria;
using mAgicTVViewerGT.Model.TvProgramWatcher;

namespace mAgicTVViewerGT.Model.TvProgramFilter
{
    // > 1  < -1  = 0

    /// <summary>
    /// ITvProgramHierarchicalFilter の整列に使用する Comparer です。
    /// </summary>
    public class TvProgramHierarchicalFilterComparer : IComparer<IHierarchicalFilter<TvProgram>>
    {
        /// <summary>
        /// 2 つの IHierarchicalFilter&gt;TvProgram&lt; を比較して、一方が他方より小さいか、同じか、または大きいかを示す値を返します。
        /// x と y の大小が決められない場合は、= ではなく、x が大きいものとして扱われます。
        /// null &gt; 不明な階層フィルター &gt; 不明な TvProgram 特化型階層フィルター &gt; TvProgram 特化型階層フィルター フォルダー &gt; 「その他」でない既知の TvProgram 特化型階層フィルター &gt; 「その他」の既知の TvProgram 特化型階層フィルター。
        /// </summary>
        /// <remarks>
        /// <para>
        /// x が null の場合、y が何であれ x &gt; y (1) です。
        /// x が null でなく、y が null の場合 x &lt; y (-1) です。
        /// すなわち、null は常に大きいものとして扱われます。
        /// </para>
        /// <para>
        /// 上記を除き、x が ITvProgramHierarchicalFilter でない IHierarchicalFilter&gt;TvProgram&lt; である場合、y が何であれ x &gt; y (1) です。
        /// x が ITvProgramHierarchicalFilter であり、かつ、y が ITvProgramHierarchicalFilter でない IHierarchicalFilter&gt;TvProgram&lt; である場合、x &lt; y (-1) です。
        /// </para>
        /// <para>
        /// 上記を除き、x が TvProgramHierarchicalFilter でも、TvProgramHierarchicalFilterFolder でもない ITvProgramHierarchicalFilter である場合、y が何であれ x &gt; y (1) です。
        /// x が TvProgramHierarchicalFilter または TvProgramHierarchicalFilterFolder であり、かつ、y が TvProgramHierarchicalFilter でも TvProgramHierarchicalFilterFolder でもない ITvProgmraHierarchicalFilter である場合、x &lt; y (-1) です。
        /// </para>
        /// <para>
        /// 上記を除き、x が TvProgramHierarchicalFilterFolder であり、y が TvProgramHierarchicalFilterFolder でない場合、x &lt; y (-1) です。
        /// x が TvProgramHierarchicalFilterFolder であなく、y が TvProgramHierarchicalFilterFolder である場合、x &gt; y (1) です。
        /// </para>
        /// <para>
        /// 上記を除き、x の所有する IFilter&lt;TvProgram&gt; が TvProgramResudueFilter である場合、y なんであれ x &gt; y (1) です。
        /// x の所有する IFilter&lt;TvProgram&gt; が TvProgramResudueFilter でなく、かつ、y の所有する IFilter&lt;TvProgram&gt; が TvProgramResudueFilter である場合、x &gt; y (1) です。
        /// </para>
        /// <para>
        /// 上記を除くと、x および y は、両方とも TvProgramHierarchicalFilter であるか,、両方とも TvProgramHierarchicalFilterFolder であることになります。
        /// x および y が所有するフィルターは TvProgramResudueFilter ではありません。
        /// この時、x と y それぞれの Name プロパティを比較して、その結果を返します。等しいときは x が大きいとして扱われます。
        /// </para>
        /// </remarks>
        /// <param name="x">比較する最初のオブジェクト。</param>
        /// <param name="y">比較する 2 番目のオブジェクト。</param>
        /// <returns>x &lt; y のとき -1、x = y のとき 0、x &gt; y のとき 1。</returns>
        public int Compare(IHierarchicalFilter<TvProgram> x, IHierarchicalFilter<TvProgram> y)
        {
            if (x == null) return 1;
            if (y == null) return -1;

            if (!(x is ITvProgramHierarchicalFilter)) return 1;
            if (!(y is ITvProgramHierarchicalFilter)) return -1;

            if (!(x is TvProgramHierarchicalFilter)) return 1;
            if (!(y is TvProgramHierarchicalFilter)) return -1;

            if (x.Filter is TvProgramFolderFilter && !(y.Filter is TvProgramFolderFilter)) return -1;
            if (!(x.Filter is TvProgramFolderFilter) && y.Filter is TvProgramFolderFilter) return 1;

            if (x.Filter is TvProgramResudueFilter) return 1;
            if (y.Filter is TvProgramResudueFilter) return -1;

            if (((TvProgramHierarchicalFilter)x).Name.CompareTo(((TvProgramHierarchicalFilter)y).Name) < 0) return -1;
            else return 1;
        }
    }
}
