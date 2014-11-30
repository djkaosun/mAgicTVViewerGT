using System;

namespace mAgicTVViewerGT.Model.FilterCriteria
{
    /// <summary>
    /// 所有者である階層フィルターの兄弟フィルターのいずれにも適合しない対象を抽出します。
    /// </summary>
    /// <typeparam name="T">このフィルターが受け入れる型</typeparam>
    public class FolderFilter<T> : AbstractFilter<T>
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
        /// 問い合わせ元が所有者の子なら親の回答を、それ以外なら子の回答の OR を返します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public override bool Match(T item, object inquirySource)
        {

            if (this._Owner == null) throw new InvalidOperationException("所有者が設定されていません。");

            IHierarchicalFilter<T> owner = (IHierarchicalFilter<T>)this._Owner;

            if (inquirySource != null
                    && inquirySource is IHierarchicalFilter<T>
                    && owner.IsSubFilter((IHierarchicalFilter<T>)inquirySource)
                    )
            {
                if (owner.IsRoot) { return true; }
                else { return owner.Parent.Match(item, inquirySource); }
            }

            if (!owner.IsRoot && !owner.Parent.Match(item, inquirySource)) return false;

            // 既定の戻り値。
            bool result = false;
            foreach (IHierarchicalFilter<T> child in owner.Children)
            {
                if (!(child.Filter is ResudueFilter<T>) && child.Filter.Match(item, inquirySource))
                {
                    result = true;
                    break;
                }
            }
            return result;
        }
    }
}
