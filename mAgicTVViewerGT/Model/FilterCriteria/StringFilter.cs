using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace mAgicTVViewerGT.Model.FilterCriteria
{
    public class StringFilter : AbstractFilter<string>, ICloneable, INotifyPropertyChanged 
    {
        /// <summary>
        /// このフィルターの所有者。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public override object Owner { get; set; }

        private ExpressionElement FilterTree;
        private string _FilterString;
        /// <summary>
        /// フィルターを示す文字列。
        /// </summary>
        public string FilterString {
            get { return this._FilterString; }
            set
            {
                this._FilterString = value;

                if (String.IsNullOrEmpty(value))
                {
                    this.FilterTree = new OperandElement();
                }
                else
                {
                    List<ExpressionElement> elements = tokenize(value);
//System.Console.WriteLine("================================================================");
//foreach (ExpressionElement elem in elements) System.Console.WriteLine(elem);
//System.Console.WriteLine("================================================================");
                    this.FilterTree = parse(elements);
                }

                if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("FilterString"));
            }
        }

        /// <summary>
        /// プロパティが変更された場合に発生するイベント。
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// コンストラクター。
        /// </summary>
        public StringFilter()
        {
            this.FilterTree = new OperandElement();
            this.FilterString = String.Empty;
        }

        #region privateMethod
        private static List<ExpressionElement> tokenize(string titleFilter)
        {
            List<ExpressionElement> result = new List<ExpressionElement>();

            bool isSpaceAtPrev = true;
            bool isSpaceOAtPrev = false;
            bool isSpaceORAtPrev = false;
            bool isOrTokenAtPrev = false;
            bool isNotTokenAtPrev = false;
            bool isOperandAtPrev = false;
            bool isMinusAtPrev = false;
            bool isQuoted = false;
            string token = String.Empty;
            int nest = 0;
            foreach (char c in titleFilter.ToCharArray())
            {
                if (isQuoted)
                {
                    if (c == '"')
                    {
                        if (isOrTokenAtPrev)
                        {
                            result.Add(new OrOperatorElement());
                        }
                        else if (isOperandAtPrev)
                        {
                            result.Add(new AndOperatorElement());
                        }

                        if (isNotTokenAtPrev)
                        {
                            result.Add(new NotOperatorElement());
                            isNotTokenAtPrev = false;
                        }

                        result.Add(new OperandElement(token, true));
                        token = String.Empty;

                        isSpaceAtPrev = true;
                        isQuoted = false;
                        isOperandAtPrev = true;
                    }
                    else
                    {
                        token += c;
                    }
                }
                else if (isMinusAtPrev)
                {
                    // "～" でなく、かつ、「 -」の後の場合
                    switch (c)
                    {
                        case ' ':
                            if (isOrTokenAtPrev)
                            {
                                result.Add(new OrOperatorElement());
                                isOrTokenAtPrev = false;
                            }
                            else if (isOperandAtPrev)
                            {
                                result.Add(new AndOperatorElement());
                            }

                            result.Add(new OperandElement(token));
                            token = String.Empty;
                            isSpaceAtPrev = true;
                            isOperandAtPrev = true;
                            break;
                        case '"':
                            if (isOrTokenAtPrev)
                            {
                                result.Add(new OrOperatorElement());
                                isOrTokenAtPrev = false;
                            }
                            else if (isOperandAtPrev)
                            {
                                result.Add(new AndOperatorElement());
                            }

                            result.Add(new NotOperatorElement());
                            token = String.Empty;
                            isQuoted = true;
                            isOperandAtPrev = false; // この時点で演算子を吐き出しているため
                            break;
                        case '(':
                            if (isOrTokenAtPrev)
                            {
                                result.Add(new OrOperatorElement());
                                isOrTokenAtPrev = false;
                            }
                            else if (isOperandAtPrev)
                            {
                                result.Add(new AndOperatorElement());
                            }

                            result.Add(new NotOperatorElement());
                            result.Add(new NestBeginOperatorElement());
                            nest++;
                            token = String.Empty;

                            isSpaceAtPrev = true;
                            isOperandAtPrev = false;
                            break;
                        case ')':
                            if (isOrTokenAtPrev)
                            {
                                result.Add(new OrOperatorElement());
                                isOrTokenAtPrev = false;
                            }
                            else if (isOperandAtPrev)
                            {
                                result.Add(new AndOperatorElement());
                            }

                            if (isNotTokenAtPrev)
                            {
                                result.Add(new NotOperatorElement());
                                isNotTokenAtPrev = false;
                            }

                            result.Add(new OperandElement(token));
                            token = String.Empty;

                            result.Add(new NestEndOperatorElement());
                            nest--;
                            
                            isSpaceAtPrev = true;
                            isOperandAtPrev = true;
                            break;
                        default:
                            token = String.Empty + c;
                            isNotTokenAtPrev = true;
                            break;
                    }
                    isMinusAtPrev = false;
                }
                else
                {
                    if (isSpaceAtPrev)
                    {
                        // "～" でなく、かつ、空白の直後の場合
                        switch (c)
                        {
                            case ' ':
                                // 「 」は読み飛ばす
                                break;
                            case '"':
                                isSpaceAtPrev = false; // いらないかも
                                isQuoted = true;
                                break;
                            case '-':
                                token += c;
                                isSpaceAtPrev = false;
                                isMinusAtPrev = true;
                                break;
                            case '(':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }
                                result.Add(new NestBeginOperatorElement());
                                nest++;
                                isOperandAtPrev = false;
                                break;
                            case ')':
                                if (isOrTokenAtPrev)
                                {
                                    if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                    result.Add(new OperandElement("OR"));
                                    isOrTokenAtPrev = false;
                                }
                                result.Add(new NestEndOperatorElement());
                                nest--;
                                isOperandAtPrev = true;
                                break;
                            case 'O':
                                if (isOperandAtPrev) isSpaceOAtPrev = true;
                                token += c;
                                isSpaceAtPrev = false;
                                break;
                            default:
                                token += c;
                                isSpaceAtPrev = false;
                                break;
                        }
                    }
                    else if (isSpaceOAtPrev)
                    {
                        // "～" でなく、かつ、OR 演算子の可能性のある「 O」の後の場合
                        switch (c)
                        {
                            case ' ':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            case '"':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                isQuoted = true;
                                isOperandAtPrev = true;
                                break;
                            case '(':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new NestBeginOperatorElement());
                                nest++;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case ')':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new NestEndOperatorElement());
                                nest--;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            case 'R':
                                token += c;
                                isSpaceORAtPrev = true;
                                break;
                            default:
                                token += c;
                                break;
                        }
                        isSpaceOAtPrev = false;
                    }
                    else if (isSpaceORAtPrev)
                    {
                        // "～" でなく、かつ、OR 演算子の可能性のある「 OR」の後の場合
                        switch (c)
                        {
                            case ' ':
                                token = String.Empty;
                                isSpaceAtPrev = true;
                                isOrTokenAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case '"':
                                token = String.Empty;
                                isQuoted = true;
                                isOrTokenAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case '(':
                                result.Add(new OrOperatorElement());
                                token = String.Empty;

                                result.Add(new NestBeginOperatorElement());
                                nest++;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case ')':
                                if (isOperandAtPrev) result.Add(new AndOperatorElement());
                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new NestEndOperatorElement());
                                nest--;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            default:
                                token += c;
                                break;
                        }
                        isSpaceORAtPrev = false;
                    }
                    else
                    {
                        // OR にも NOT にも関係ない、かつ、空白の直後でもない場合
                        switch (c)
                        {
                            case ' ':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }

                                if (isNotTokenAtPrev)
                                {
                                    result.Add(new NotOperatorElement());
                                    isNotTokenAtPrev = false;
                                }

                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            case '"':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }

                                if (isNotTokenAtPrev)
                                {
                                    result.Add(new NotOperatorElement());
                                    isNotTokenAtPrev = false;
                                }

                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                isQuoted = true;
                                isOperandAtPrev = true; // いらないかも
                                break;
                            case '(':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }

                                if (isNotTokenAtPrev)
                                {
                                    result.Add(new NotOperatorElement());
                                    isNotTokenAtPrev = false;
                                }

                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new AndOperatorElement());
                                result.Add(new NestBeginOperatorElement());
                                nest++;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = false;
                                break;
                            case ')':
                                if (isOrTokenAtPrev)
                                {
                                    result.Add(new OrOperatorElement());
                                    isOrTokenAtPrev = false;
                                }
                                else if (isOperandAtPrev)
                                {
                                    result.Add(new AndOperatorElement());
                                }

                                if (isNotTokenAtPrev)
                                {
                                    result.Add(new NotOperatorElement());
                                    isNotTokenAtPrev = false;
                                }

                                result.Add(new OperandElement(token));
                                token = String.Empty;

                                result.Add(new NestEndOperatorElement());
                                nest--;

                                isSpaceAtPrev = true;
                                isOperandAtPrev = true;
                                break;
                            default:
                                token += c;
                                break;
                        }
                    }
                }
            }

            // 最後のトークンの吐き出し
            if (token != String.Empty)
            {
                if (isOrTokenAtPrev)
                {
                    result.Add(new OrOperatorElement());
                }
                else if (isOperandAtPrev)
                {
                    result.Add(new AndOperatorElement());
                }

                if (isNotTokenAtPrev)
                {
                    result.Add(new NotOperatorElement());
                    isNotTokenAtPrev = false;
                }

                if (isQuoted)
                {
                    // "～" が閉じられなかった場合の辻褄合わせ
                    result.Add(new OperandElement(token, true));
                }
                else
                {
                    result.Add(new OperandElement(token));
                }
            }

            // 括弧の辻褄合わせ
            for (int i = 0; i < nest; i++)
            {
                result.Add(new NestEndOperatorElement());
            }

            for (int i = 0; i > nest; i--)
            {
                result.Insert(0, new NestBeginOperatorElement());
            }

            // 「()」については間に空文字列があるとする。恒真になる。
            bool isNestBeginAtPrev = false;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] is NestBeginOperatorElement)
                {
                    isNestBeginAtPrev = true;
                }
                else
                {
                    if (isNestBeginAtPrev && result[i] is NestEndOperatorElement)
                    {
                        result.Insert(i, new OperandElement());
                    }
                    isNestBeginAtPrev = false;
                }
            }

            return result;
        }
        
        private static ExpressionElement parse(List<ExpressionElement> elements)
        {
            // 要素がないなら空の項を返す
            if (elements.Count == 0) return new OperandElement();

            // 配列化
            ExpressionElement[] elemArray = elements.ToArray();

            // 左項の括弧の除去
            int startIndex = 0;
            int endIndex = elemArray.Length;
            while (isNested(elemArray, startIndex, endIndex))
            {
                startIndex++;
                endIndex--;
            }

            // 括弧をとると要素がなくなるなら空の項を返す
            if (startIndex == endIndex) return new OperandElement();

            // 再帰的に処理
            return parseRecurse(elemArray, startIndex, endIndex);
        }

        private static ExpressionElement parseRecurse(ExpressionElement[] elemArray, int startIndex, int endIndex)
        {
            // 演算子の位置の取得
            int pos = getOperatorPos(elemArray, startIndex, endIndex);

            if (pos < 0)
            {
                if ((endIndex - startIndex) != 1) throw new ArgumentException("構文解析に失敗しました。");
                return elemArray[startIndex];
            }

            int leftEnd = pos;
            int rightStart = pos + 1;


            // 左項の処理
            if (((OperatorElement)elemArray[pos]).Operator == OperatorElement.OperatorType.Not)
            {
                if (startIndex != leftEnd) throw new ArgumentException("構文解析に失敗しました。");
            }
            else
            {
                // 左項の括弧の除去
                while (isNested(elemArray, startIndex, leftEnd))
                {
                    startIndex++;
                    leftEnd--;
                }
                // 左項に結び付け
                if (startIndex == leftEnd) throw new ArgumentException("構文解析に失敗しました。");
                ((OperatorElement)elemArray[pos]).Left = parseRecurse(elemArray, startIndex, leftEnd);
            }

            // 右項の処理
            // 右項の括弧の除去
            while (isNested(elemArray, rightStart, endIndex))
            {
                rightStart++;
                endIndex--;
            }
            // 右項に結び付け
            if (rightStart == endIndex) throw new ArgumentException("構文解析に失敗しました。");
            ((OperatorElement)elemArray[pos]).Right = parseRecurse(elemArray, rightStart, endIndex);

            return elemArray[pos];
        }

        private static bool isNested(ExpressionElement[] elemArray, int startIndex, int endIndex)
        {
            // 最初と最後のいずれかが項なら false
            if (elemArray[startIndex].Type == ExpressionElementType.Operand || elemArray[endIndex - 1].Type == ExpressionElementType.Operand)
            {
                return false;
            }

            // 最初が「(」でないか、または、最後が「)」でないなら false
            if (((OperatorElement)elemArray[startIndex]).Operator != OperatorElement.OperatorType.NestBegin || ((OperatorElement)elemArray[endIndex - 1]).Operator != OperatorElement.OperatorType.NestEnd)
            {
                return false;
            }

            // 最初と最後をとってみて、ネストに不具合があったら false
            int nest = 0;
            for (int i = startIndex + 1; i < endIndex - 1; i++)
            {
                if (elemArray[i].Type == ExpressionElementType.Operator)
                {
                    if (((OperatorElement)elemArray[i]).Operator == OperatorElement.OperatorType.NestBegin)
                    {
                        nest++;
                    }
                    else if (((OperatorElement)elemArray[i]).Operator == OperatorElement.OperatorType.NestBegin)
                    {
                        nest--;
                    }

                    if (nest < 0) return false;
                }
            }

            // すべてのチェックを抜けたら true
            return true;
        }

        private static int getOperatorPos(ExpressionElement[] elemArray, int startIndex, int endIndex)
        {
//System.Windows.MessageBox.Show("引数 // Start: " + startIndex + " / End: " + endIndex);
            ExpressionElement elem;
            int pos = -1;
            int nest = 0;
            int priority = Int32.MaxValue;
            int lowestPriority = Int32.MinValue;
            for (int i = startIndex; i < endIndex; i++)
            {
                elem = elemArray[i];
                
                if (elem.Type == ExpressionElementType.Operator)
                {
                    if (((OperatorElement)elem).Operator == OperatorElement.OperatorType.NestBegin)
                    {
                        nest++;
                    }
                    else if (((OperatorElement)elem).Operator == OperatorElement.OperatorType.NestEnd)
                    {
                        nest--;
                    }
                    else
                    {
                        priority = ((OperatorElement)elem).Priority;

//System.Windows.MessageBox.Show("" + elem + " // " + priority + "/" + lowestPriority);
                        if (nest == 0 && priority >= lowestPriority)
                        {
                            lowestPriority = priority;
                            pos = i;
                        }
                    }
                }
            }

//System.Windows.MessageBox.Show("戻り値　// " + pos + " / " + elemArray[(pos == -1)?startIndex:pos]);
            return pos;
        }
        #endregion privateMethod

        /// <summary>
        /// このフィルターへの適合性を判断します。
        /// </summary>
        /// <param name="item">適合するか確認する対象</param>
        /// <param name="inquirySource">問い合わせ元</param>
        /// <returns>適合する場合 true、しない場合 false</returns>
        public override bool Match(string title, object inquirySource)
        {
            return this.FilterTree.Match(title);
        }

        /// <summary>
        /// このオブジェクトをディープ コピーします。
        /// </summary>
        /// <returns>コピーされたオブジェクト</returns>
        public Object Clone()
        {
            StringFilter result = (StringFilter)this.MemberwiseClone();
            if(this.FilterTree != null) result.FilterTree = (ExpressionElement)this.FilterTree.Clone();
            return result;
        }

        #region InternalClass
        public interface ExpressionElement : ICloneable
        {
            ExpressionElementType Type { get; }
            string ToString();
            bool Match(string title);
            int Priority { get; }
        }

        public enum ExpressionElementType
        {
            Operator, Operand
        }

        [Serializable]
        public abstract class OperatorElement : ExpressionElement
        {
            public ExpressionElementType Type { get; set; }
            public OperatorElement.OperatorType Operator { get; set; }
            public int Priority { get; set; }
            public ExpressionElement Right { get; set; }
            public ExpressionElement Left { get; set; }
            public enum OperatorType { And, Or, Not, NestBegin, NestEnd }
            public OperatorElement()
            {
                this.Type = ExpressionElementType.Operator;
                this.Priority = Int32.MaxValue;
            }
            public new string ToString()
            {
                return base.GetType().Name + " | L[ " + this.Left + " ] | R[ " + this.Right + " ]";
            }
            public abstract bool Match(string title);
            public Object Clone()
            {
                OperatorElement result = (OperatorElement)this.MemberwiseClone();
                if (this.Left != null) result.Left = (ExpressionElement)this.Left.Clone();
                if (this.Right != null) result.Right = (ExpressionElement)this.Right.Clone();
                return result;
            }
        }

        [Serializable]
        public class AndOperatorElement : OperatorElement
        {
            public AndOperatorElement() : base()
            {
                base.Operator = OperatorElement.OperatorType.And;
                base.Priority = 2;
            }

            public override bool Match(string title)
            {
                if (base.Right == null || base.Left == null) return true;
                return base.Right.Match(title) && base.Left.Match(title);
            }
        }

        [Serializable]
        public class OrOperatorElement : OperatorElement
        {
            public OrOperatorElement()
                : base()
            {
                base.Operator = OperatorElement.OperatorType.Or;
                base.Priority = 3;
            }

            public override bool Match(string title)
            {
                if (base.Right == null || base.Left == null) return false;
                return base.Right.Match(title) || base.Left.Match(title);
            }
        }

        [Serializable]
        public class NotOperatorElement : OperatorElement
        {
            public NotOperatorElement()
                : base()
            {
                base.Operator = OperatorElement.OperatorType.Not;
                base.Priority = 1;
            }

            public override bool Match(string title)
            {
                if (base.Right == null)
                {
                    if (base.Left == null) return false;
                    else base.Right = base.Left;
                }
                else
                {
                    base.Left = Right;
                }
                return !base.Right.Match(title);
            }
            public new string ToString()
            {
                if (base.Left != null && base.Right == null)
                {
                    base.Right = base.Left;
                }
                else
                {
                    base.Left = base.Right;
                }
                return base.GetType().Name + " | L[ " + this.Left + " ]";
            }
        }

        [Serializable]
        public class NestBeginOperatorElement : OperatorElement
        {
            public NestBeginOperatorElement()
                : base()
            {
                base.Operator = OperatorElement.OperatorType.NestBegin;
            }

            public override bool Match(string title)
            {
                throw new InvalidCastException();
            }

            public new string ToString()
            {
                return base.GetType().Name;
            }
        }

        [Serializable]
        public class NestEndOperatorElement : OperatorElement
        {
            public NestEndOperatorElement()
                : base()
            {
                base.Operator = OperatorElement.OperatorType.NestEnd;
            }

            public override bool Match(string title)
            {
                throw new InvalidCastException();
            }

            public new string ToString()
            {
                return base.GetType().Name;
            }
        }

        [Serializable]
        public class OperandElement : ExpressionElement
        {
            public ExpressionElementType Type { get; set; }
            public int Priority { get; set; }
            public string Keyword { get; set; }
            public bool CaseSense { get; set; }
            public OperandElement()
            {
                this.Type = ExpressionElementType.Operand;
                this.Priority = Int32.MinValue;
                this.Keyword = String.Empty;
                this.CaseSense = false;
            }
            public OperandElement(string keyword)
            {
                this.Type = ExpressionElementType.Operand;
                this.Priority = Int32.MinValue;
                this.Keyword = keyword;
                this.CaseSense = false;
            }

            public OperandElement(string keyword, bool casesense)
            {
                this.Type = ExpressionElementType.Operand;
                this.Keyword = keyword;
                this.CaseSense = casesense;
            }
            public new string ToString()
            {
                return this.Keyword + " | " + this.CaseSense + " (" + base.GetType().Name + ")";
            }
            public Object Clone()
            {
                return (OperandElement)this.MemberwiseClone();
            }
            public bool Match(string title)
            {
                if (String.IsNullOrEmpty(this.Keyword)) return true;
                if (this.CaseSense) return title.Contains(this.Keyword);
                else return title.ToLower().Contains(this.Keyword.ToLower());
            }
        }

        #region FilterClass
        private interface FilterNode
        {
            FilterNode ParentNode { get; set; }
            bool IsRootNode { get; }
            bool Match(string title);
        }

        private class AndNode : FilterNode
        {
            private List<FilterNode> NodeList;
            public FilterNode ParentNode { get; set; }
            public bool IsRootNode
            {
                get
                {
                    return this.ParentNode == null;
                }
            }

            internal AndNode()
            {
                this.ParentNode = null;
                this.NodeList = new List<FilterNode>();
            }

            public bool Match(string title)
            {
                foreach (FilterNode node in this.NodeList)
                {
                    if (!node.Match(title)) return false;
                }
                return true;
            }

            internal void AddNode(FilterNode node)
            {
                this.NodeList.Add(node);
                node.ParentNode = this;
            }
        }

        private class OrNode : FilterNode
        {
            private List<FilterNode> NodeList;
            public FilterNode ParentNode { get; set; }
            public bool IsRootNode
            {
                get
                {
                    return this.ParentNode == null;
                }
            }

            internal OrNode()
            {
                this.ParentNode = null;
                this.NodeList = new List<FilterNode>();
            }

            public bool Match(string title)
            {
                if (this.NodeList.Count == 0) return true;
                foreach (FilterNode node in this.NodeList)
                {
                    if (node.Match(title)) return true;
                }
                return false;
            }

            internal void AddNode(FilterNode node)
            {
                this.NodeList.Add(node);
                node.ParentNode = this;
            }
        }

        private class NotNode : FilterNode
        {
            private FilterNode ChildNode;
            public FilterNode ParentNode { get; set; }
            public bool IsRootNode
            {
                get
                {
                    return this.ParentNode == null;
                }
            }

            internal NotNode()
            {
                this.ParentNode = null;
                this.ChildNode = null;
            }

            public bool Match(string title)
            {
                if (this.ChildNode == null) return true;
                return !this.ChildNode.Match(title);
            }

            internal void SetChildNode(FilterNode node)
            {
                this.ChildNode = node;
                node.ParentNode = this;
            }
        }

        private class LeafNode : FilterNode
        {
            private string keyword = String.Empty;
            public string Keyword
            {
                get
                {
                    return this.keyword;
                }
                set
                {
                    if (value == null) this.keyword = String.Empty;
                    this.keyword = value;
                }
            }
            public bool CaseSense { get; set; }
            public FilterNode ParentNode { get; set; }
            public bool IsRootNode
            {
                get
                {
                    return this.ParentNode == null;
                }
            }

            internal LeafNode()
            {
                this.ParentNode = null;
                this.CaseSense = false;
            }

            public bool Match(string title)
            {
                if (this.keyword == String.Empty) return true;
                if (this.CaseSense) return title.Contains(this.keyword);
                else return title.ToLower().Contains(this.keyword.ToLower());
            }
        }
        #endregion FilterClass
        #endregion InternalClass
    }
}
