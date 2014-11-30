using System;

namespace mAgicTVViewerGT.Model.FilterCriteria
{
    /// <summary>
    /// 常に適合するフィルター。
    /// </summary>
    /// <typeparam name="T">フィルターが受け入れる型</typeparam>
    public class NotAFilter<T> : AbstractFilter<T>
    {
        /// <summary>
        /// このフィルターの所有者。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public override object Owner { get; set; }

        /// <summary>
        /// 常に true を返します。
        /// </summary>
        /// <param name="item">無視されます。</param>
        /// <param name="inquirySource">無視されます。</param>
        /// <returns>常に true。</returns>
        public override bool Match(T item, object inquirySource)
        {
            return true;
        }
    }
}
