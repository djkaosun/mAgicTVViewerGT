using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using mAgicTVViewerGT.Model.FilterCriteria;
using mAgicTVViewerGT.Model.TvProgramWatcher;

namespace mAgicTVViewerGT.Model.TvProgramFilter
{
    /// <summary>
    /// TvProgram に特化した階層フィルターです。
    /// </summary>
    public class TvProgramHierarchicalFilter : HierarchicalFilter<TvProgram>, ITvProgramHierarchicalFilter
    {
        /// <summary>
        /// 階層フィルター名。所有するフィルターの名前も同時に更新されます。
        /// </summary>
        public string Name
        {
            get {
                if (base.Filter == null) return null;
                return ((ITvProgramFilter)base.Filter).Name;
            }
            set
            {
                if (base.Filter != null)
                {
                    ((TvProgramStandardFilter)base.Filter).Name = value;
                    base.OnPropertyChanged(this, new PropertyChangedEventArgs("Name"));
                }
                else
                {
                    throw new NullReferenceException("Filter が設定されていません。");
                }
            }
        }

        /// <summary>
        /// フィルターを示すアイコン。所有するフィルターのアイコンも同時に更新されます。
        /// </summary>
        public string Icon
        {
            get
            {
                if (base.Filter == null) return null;
                return ((ITvProgramFilter)base.Filter).Icon;
            }
            set
            {
                if (base.Filter != null)
                {
                    ((ITvProgramFilter)base.Filter).Icon = value;
                    base.OnPropertyChanged(this, new PropertyChangedEventArgs("Icon"));
                }
                else
                {
                    throw new NullReferenceException("Filter が設定されていません。");
                }
            }
        }

        private int _UnviewedNumber;
        /// <summary>
        /// カウントされた未視聴番組数
        /// </summary>
        public int UnviewedNumber
        {
            get
            {
                return this._UnviewedNumber;
            }
        }

        /// <summary>
        /// カウントされた未視聴番組数を示す文字列
        /// </summary>
        public string UnviewedNumberString
        {
            get
            {
                if (this._UnviewedNumber == 0) return String.Empty;

                return "(" + this._UnviewedNumber + ")";
            }
        }

        /// <summary>
        /// 階層フィルターに適合する番組であり、かつ、未視聴であった場合、カウント アップします。
        /// 子フィルターの UnviewedCountUp メソッドも呼び出します。
        /// </summary>
        /// <param name="tvPrg">カウントする番組</param>
        public void UnviewedCountUp(TvProgram tvPrg)
        {
            if (tvPrg.IsUnviewed && base.Match(tvPrg))
            {
                this._UnviewedNumber++;
                base.OnPropertyChanged(this, new PropertyChangedEventArgs("UnviewedNumber"));
                base.OnPropertyChanged(this, new PropertyChangedEventArgs("UnviewedNumberString"));
                foreach (IHierarchicalFilter<TvProgram> child in base.Children)
                {
                    ((TvProgramHierarchicalFilter)child).UnviewedCountUp(tvPrg);
                }
            }
        }

        /// <summary>
        /// 階層フィルターに適合する番組であり、かつ、未視聴であった場合、カウント ダウンします。
        /// 子フィルターの UnviewedCountDown メソッドも呼び出します。
        /// </summary>
        /// <param name="tvPrg">カウントする番組</param>
        public void UnviewedCountDown(TvProgram tvPrg)
        {
            if (tvPrg.IsUnviewed && base.Match(tvPrg))
            {
                if (this._UnviewedNumber > 0)
                {
                    this._UnviewedNumber--;
                    base.OnPropertyChanged(this, new PropertyChangedEventArgs("UnviewedNumber"));
                    base.OnPropertyChanged(this, new PropertyChangedEventArgs("UnviewedNumberString"));
                }
                foreach (IHierarchicalFilter<TvProgram> child in base.Children)
                {
                    ((TvProgramHierarchicalFilter)child).UnviewedCountDown(tvPrg);
                }
            }
        }

        /// <summary>
        /// 未視聴番組数のカウントを 0 にします。
        /// 子フィルターの UnviewedCountReset メソッドも呼び出します。
        /// </summary>
        public void UnviewedCountClear()
        {
            this._UnviewedNumber = 0;
            base.OnPropertyChanged(this, new PropertyChangedEventArgs("UnviewedNumber"));
            base.OnPropertyChanged(this, new PropertyChangedEventArgs("UnviewedNumberString"));
            foreach (IHierarchicalFilter<TvProgram> child in base.Children)
            {
                ((TvProgramHierarchicalFilter)child).UnviewedCountClear();
            }
        }

        /// <summary>
        /// 配列に含まれる番組のうち、階層フィルターに適合する番組数をカウントします。
        /// これまでのカウントはリセットされます。
        /// 子フィルターの UnviewedCount メソッドも呼び出します。
        /// </summary>
        /// <param name="tvPrg">カウントする番組のリスト</param>
        public void UnviewedCount(TvProgram[] tvPrgs)
        {
            this._UnviewedNumber = 0;

            List<TvProgram> resultList = new List<TvProgram>(4096);
            foreach (TvProgram tvPrg in tvPrgs)
            {
                if (tvPrg.IsUnviewed && base.Filter.Match(tvPrg))
                {
                    this._UnviewedNumber++;
                    resultList.Add(tvPrg);
                }
            }
            base.OnPropertyChanged(this, new PropertyChangedEventArgs("UnviewedNumber"));
            base.OnPropertyChanged(this, new PropertyChangedEventArgs("UnviewedNumberString"));

            TvProgram[] resultArray = new TvProgram[resultList.Count];
            resultList.CopyTo(resultArray);
            foreach (IHierarchicalFilter<TvProgram> child in base.Children)
            {
                ((TvProgramHierarchicalFilter)child).UnviewedCount(resultArray);
            }
            /*
            Parallel.ForEach(base.Children, child =>
            {
                ((ITvProgramHierarchicalFilter)child).UnviewedCount(resultArray);
            });
            */
        }
    }
}
