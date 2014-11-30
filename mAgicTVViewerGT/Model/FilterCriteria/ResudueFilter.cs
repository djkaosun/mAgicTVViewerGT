using System;

namespace mAgicTVViewerGT.Model.FilterCriteria
{
    /// <summary>
    /// 所有者である階層フィルターの兄弟フィルターのいずれにも適合しない対象を抽出します。
    /// </summary>
    /// <typeparam name="T">このフィルターが受け入れる型</typeparam>
    public class ResudueFilter<T> : AbstractFilter<T>
    {
        private IHierarchicalFilter<T> _Owner;
        /// <summary>
        /// このフィルターの所有者。
        /// HierarchicalFilter&lt;T&gt; でないオブジェクトを set すると例外が発生します。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public override object Owner
        {
            get
            {
                return this._Owner;
            }
            set
            {
                this._Owner = (IHierarchicalFilter<T>)value;
            }
        }

        /// <summary>
        /// 所有者である階層フィルターの兄弟フィルターのいずれにも適合しないか判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public override bool Match(T item, object inquirySource)
        {
            if (this._Owner == null) throw new InvalidOperationException("所有者が設定されていません。");

            IHierarchicalFilter<T> owner = (IHierarchicalFilter<T>)this._Owner;

            if (owner.Parent == null) throw new InvalidOperationException("親のいない HierarchicalFilter に所有されています。");

            // 親にマッチしなければ false
            if (!owner.Parent.Match(item, inquirySource)) return false;

            // 兄弟 (自分以外の親の子) にマッチすれば false
            foreach (IHierarchicalFilter<T> bros in owner.Parent.Children)
            {
                if (!(bros.Filter is ResudueFilter<T>) && bros.Filter.Match(item, inquirySource)) return false;
            }

            // ここまでたどり着いたら ture
            return true;
        }
    }
}
