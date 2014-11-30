using System;

namespace mAgicTVViewerGT.Model.FilterCriteria
{
    /// <summary>
    /// 抽象的なフィルターです。
    /// </summary>
    /// <typeparam name="T">フィルターが受け入れる型</typeparam>
    public abstract class AbstractFilter<T> : IFilter<T>
    {
        /// <summary>
        /// このフィルターの所有者。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public abstract object Owner { get; set; }

        /// <summary>
        /// このフィルターへの適合性を判断します。
        /// （inquirySource を null として Match(T item, object inquirySource) メソッドを呼び出します）
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public bool Match(T item)
        {
            return this.Match(item, null);
        }

        /// <summary>
        /// このフィルターへの適合性を判断します。
        /// inquirySource により判断結果が変化する可能性があります。
        /// inquirySource が null の時に既定の判断をします。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元/param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public abstract bool Match(T item, object inquirySource);
    }
}
