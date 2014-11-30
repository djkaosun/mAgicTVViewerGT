using System;
using System.ComponentModel;

namespace mAgicTVViewerGT.Model.FilterCriteria
{
    /// <summary>
    /// 階層フィルターのインターフェイス。
    /// </summary>
    /// <typeparam name="T">階層フィルターが受け入れる型</typeparam>
    public interface IHierarchicalFilter<T> : INotifyPropertyChanged
    {
        /// <summary>
        /// 階層フィルターが所有するフィルター。
        /// 内部的に、Match メソッドで利用されます。
        /// 階層フィルターとしての結果ではなく、このノードのみでの適合性が必要な場合は、
        /// この Filter プロパティに設定されている IFilter の Match メソッドを使用します。
        /// </summary>
        IFilter<T> Filter { get; set; }

        /// <summary>
        /// 親フィルター。
        /// </summary>
        IHierarchicalFilter<T> Parent { get; }

        /// <summary>
        /// 子フィルターを入れるコレクション。
        /// </summary>
        HierarchicalFilterChildren<T> Children { get; }

        /// <summary>
        /// ルート フィルターかどうか。
        /// </summary>
        bool IsRoot { get; }

        /// <summary>
        /// この階層フィルターが選択状態かどうか。
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// この階層フィルターの子孫で、最後に選択状態になったもの。
        /// </summary>
        IHierarchicalFilter<T> SelectedItem { get; }

        /// <summary>
        /// このノードでの階層フィルターとしての既定の適合性を判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        bool Match(T item);

        /// <summary>
        /// このノードでの階層フィルターとしての既定の適合性を判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        bool Match(T item, object inquirySource);

        /// <summary>
        /// ある階層フィルターが、この階層フィルターの子孫かどうかチェックします。
        /// </summary>
        /// <param name="item">子孫かどうかチェックする階層フィルター。</param>
        /// <returns>子孫の場合 true、子孫ではない場合 false。</returns>
        bool IsSubFilter(IHierarchicalFilter<T> item);

        /// <summary>
        /// ある階層フィルターが、この階層フィルターの祖先かどうかチェックします。
        /// </summary>
        /// <param name="item">祖先かどうかチェックする階層フィルター。</param>
        /// <returns>祖先の場合 true、祖先ではない場合 false。</returns>
        bool IsSuperFilter(IHierarchicalFilter<T> item);
    }
}
