using System;
using System.Collections.ObjectModel;

namespace mAgicTVViewerGT.Model.TvProgramWatcher
{
    public class TvProgramCollection : ObservableCollection<TvProgram>
    {
        /// <summary>
        /// 指定したインデックス位置にある要素を置き換えます。
        /// </summary>
        /// <param name="index">置き換える要素の 0 から始まるインデックス番号。</param>
        /// <param name="item">指定したインデックス位置に存在する要素の新しい値。</param>
        public void ReplaceAt(int index, TvProgram item)
        {
            base.SetItem(index, item);
        }

        /// <summary>
        /// 指定した値を新しい要素に置き換えます。
        /// 古い値がコレクション内に存在しない場合は、なにもしません。
        /// </summary>
        /// <param name="index">置き換える要素の古い値。</param>
        /// <param name="item">置き換える要素の新しい値。</param>
        public void Replace(TvProgram oldItem, TvProgram newItem)
        {
            int index = base.IndexOf(oldItem);
            if (index >= 0)
            {
                base.SetItem(index, newItem);
            }
        }
    }
}
