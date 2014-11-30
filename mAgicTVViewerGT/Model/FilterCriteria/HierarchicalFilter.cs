﻿using System;
using System.ComponentModel;
using System.Collections.Specialized;

namespace mAgicTVViewerGT.Model.FilterCriteria
{
    /// <summary>
    /// 階層的なフィルターを実現します。
    /// </summary>
    /// <typeparam name="T">所有する IFilter が受け入れる型</typeparam>
    public class HierarchicalFilter<T> : IHierarchicalFilter<T>
    {
        private IFilter<T> _Filter;
        /// <summary>
        /// 階層フィルターが所有するフィルター。
        /// 内部的に、Match メソッドで利用されます。
        /// 階層フィルターとしての結果ではなく、このノードのみでの適合性が必要な場合は、
        /// この Filter プロパティに設定されている IFilter の Match メソッドを使用します。
        /// </summary>
        public virtual IFilter<T> Filter
        {
            get
            {
                return this._Filter;
            }
            set
            {
                value.Owner = this;
                this._Filter = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("Filter"));
            }
        }

        private IHierarchicalFilter<T> _Parent;
        /// <summary>
        /// 親フィルター。
        /// </summary>
        public IHierarchicalFilter<T> Parent {
            get { return this._Parent; }
            private set {
                this._Parent = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("Parent"));
            }
        }
        
        private HierarchicalFilterChildren<T> _Children;
        /// <summary>
        /// 子フィルターを入れるコレクション。
        /// </summary>
        public HierarchicalFilterChildren<T> Children
        {
            get { return this._Children; }
            private set { this._Children = value; }
        }

        /// <summary>
        /// ルート フィルターかどうか。
        /// </summary>
        public bool IsRoot
        {
            get { return this.Parent == null; }
        }

        private bool _IsSelected;
        /// <summary>
        /// この階層フィルターが選択状態かどうか。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public bool IsSelected
        {
            get { return this._IsSelected; }
            set
            {
                this._IsSelected = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
                if (value) this.SelectedItem = this;
            }
        }

        private IHierarchicalFilter<T> _SelectedItem;
        /// <summary>
        /// 自分の子孫の中で最後に IsSelected が true になった階層フィルター。
        /// </summary>
        public IHierarchicalFilter<T> SelectedItem
        {
            get {
                return this._SelectedItem;
            }
            private set
            {
                this._SelectedItem = value;
                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("SelectedItem"));
            }
        }

        /// <summary>
        /// この階層フィルターのプロパティーが変更された場合に発生するイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// この階層フィルターおよび子孫のプロパティーが変更された場合に発生するイベント。
        /// </summary>
        public event PropertyChangedEventHandler SubFilterPropertyChanged;

        /// <summary>
        /// コンストラクター。
        /// </summary>
        public HierarchicalFilter()
        {
            //this.Filter = new NotAFilter<T>();
            HierarchicalFilterChildren<T> children = new HierarchicalFilterChildren<T>();
            children.CollectionChanged += this.collectionChangeEventHandler;
            this.Children = children;
        }

        /// <summary>
        /// このノードでの階層フィルターとしての適合性を判断します。
        /// Match(T item, this) の結果を返します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public bool Match(T item)
        {
            return this.Match(item, this);
        }

        /// <summary>
        /// このノードでの階層フィルターとしての適合性を判断します。
        /// 自身の Filter の Match(T item, object inquirySource) と、親の Match(T item, object inquirySource) の、両方に適合するかを返します。
        /// 親が設定されていない場合は、自身の Filter の Match(T item, object inquirySource) の結果を返します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合</returns>
        public bool Match(T item, object inquirySource)
        {
            if (this.IsRoot)
            {
                return this.Filter.Match(item, inquirySource);
            }
            else
            {
                return this.Filter.Match(item, inquirySource) && this.Parent.Match(item, inquirySource);
            }
        }

        /// <summary>
        /// ある階層フィルターが、この階層フィルターの子孫かどうかチェックします。
        /// </summary>
        /// <param name="item">子孫かどうかチェックする階層フィルター。</param>
        /// <returns>子孫の場合 true、子孫ではない場合 false。</returns>
        public bool IsSubFilter(IHierarchicalFilter<T> item)
        {
            return this.checkSubFilter(item, this);
        }
        private bool checkSubFilter(IHierarchicalFilter<T> item, IHierarchicalFilter<T> checkAt)
        {
            foreach (IHierarchicalFilter<T> child in checkAt.Children)
            {
                if (child == item) return true;
            }
            foreach (IHierarchicalFilter<T> child in checkAt.Children)
            {
                return this.checkSubFilter(item, child);
            }
            return false;
        }

        /// <summary>
        /// ある階層フィルターが、この階層フィルターの祖先かどうかチェックします。
        /// </summary>
        /// <param name="item">祖先かどうかチェックする階層フィルター。</param>
        /// <returns>祖先の場合 true、祖先ではない場合 false。</returns>
        public bool IsSuperFilter(IHierarchicalFilter<T> item)
        {
            if (this.IsRoot) return false;
            return this.checkSuperFilter(item, this);
        }
        private bool checkSuperFilter(IHierarchicalFilter<T> item, IHierarchicalFilter<T> checkAt)
        {
            if (checkAt.Parent == item) return true;
            if (checkAt.Parent.IsRoot) return false;
            return this.checkSuperFilter(item, checkAt.Parent);
        }

        /// <summary>
        /// Children コレクションの CollectionChange イベント用のイベント ハンドラー。
        /// コンストラクター内で Children のイベントに設定されます。
        /// コレクションに追加されたときに、追加した階層フィルターの PropertyChanged および SubFilterPropertyChanged イベントのハンドラーを設定、および、Parent を更新します。
        /// コレクションから削除されたときに、削除された階層フィルターの PropertyChanged および SubFilterPropertyChanged イベントのハンドラーを設定解除、および、Parent を更新します。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void collectionChangeEventHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            // null は考慮していない。
            if (e.Action == NotifyCollectionChangedAction.Add
                    || e.Action == NotifyCollectionChangedAction.Replace
                    //|| e.Action == NotifyCollectionChangedAction.Reset // Reset って Clear, ClearItem のとき？
                    )
            {
                HierarchicalFilter<T> castedItem;
                foreach (IHierarchicalFilter<T> item in e.NewItems)
                {
                    if (item == this || this.IsSuperFilter(item))
                    {
                        throw new InvalidOperationException("自分自身、および、上位フィルターは子にできません。");
                    }
                    if (!(item is HierarchicalFilter<T>)) throw new InvalidOperationException("HierarchicalFilter クラスでは、HierarchicalFilter 以外の子を持てません。");

                    castedItem = (HierarchicalFilter<T>)item;
                    castedItem.Parent = this;
                    castedItem.PropertyChanged += propertyChangedEventHandler;
                    castedItem.SubFilterPropertyChanged += subFilterPropertyChangedEventHandler;
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Remove
                    || e.Action == NotifyCollectionChangedAction.Replace
                    || e.Action == NotifyCollectionChangedAction.Reset
                    )
            {
                HierarchicalFilter<T> castedItem;
                foreach (IHierarchicalFilter<T> item in e.OldItems)
                {
                    if (!(item is HierarchicalFilter<T>)) throw new InvalidOperationException("HierarchicalFilter クラスでは、HierarchicalFilter 以外の子を持てません。");

                    castedItem = (HierarchicalFilter<T>)item;
                    castedItem.PropertyChanged -= propertyChangedEventHandler;
                    castedItem.SubFilterPropertyChanged -= subFilterPropertyChangedEventHandler;
                    if (castedItem.SubFilterPropertyChanged == null)
                    {
                        castedItem.Parent = null;
                    }
                }
            }
        }

        /// <summary>
        /// 階層フィルターの PropertyChanged イベント用ハンドラー。
        /// collectionChangeEventHandler メソッドにより、階層フィルターに設定されます。
        /// IsSelected プロパティが変更された場合、SelectedItem を更新します。
        /// このハンドラー内部で、SubFilterPropertyChanged イベントを発生します。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void propertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IHierarchicalFilter<T>)
            {
                IHierarchicalFilter<T> item = (IHierarchicalFilter<T>)sender;
                switch (e.PropertyName)
                {
                    case "IsSelected":
                        if (item.IsSelected) this.SelectedItem = item;
                        break;
                }

                if (this.SubFilterPropertyChanged != null) this.SubFilterPropertyChanged(sender, e);
            }
        }

        /// <summary>
        /// 階層フィルターの SubFilterPropertyChanged イベント用ハンドラー。
        /// collectionChangeEventHandler メソッドにより、階層フィルターに設定されます。
        /// IsSelected プロパティが変更された場合、SelectedItem を更新します。
        /// このハンドラー内部で、SubFilterPropertyChanged イベントを発生します。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        private void subFilterPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IHierarchicalFilter<T>)
            {
                IHierarchicalFilter<T> item = (IHierarchicalFilter<T>)sender;
                switch (e.PropertyName)
                {
                    case "IsSelected":
                        if (item.IsSelected) this.SelectedItem = item;
                        break;
                }

                if (this.SubFilterPropertyChanged != null) this.SubFilterPropertyChanged(sender, e);
            }
        }

        /// <summary>
        /// 派生クラスで PropertyChanged イベントを発生させたい場合に使用します。
        /// 引数の sender および e でイベントを発生させます。
        /// </summary>
        /// <param name="sender">イベントの発生元</param>
        /// <param name="e">イベントの引数</param>
        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this.PropertyChanged != null) this.PropertyChanged(sender, e);
        }
    }
}