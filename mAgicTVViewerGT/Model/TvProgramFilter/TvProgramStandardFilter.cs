using System;
using System.ComponentModel;
using mAgicTVViewerGT.Model.FilterCriteria;
using mAgicTVViewerGT.Model.TvProgramWatcher;

namespace mAgicTVViewerGT.Model.TvProgramFilter
{
    public class TvProgramStandardFilter : AbstractFilter<TvProgram>, ITvProgramFilter
    {
        /// <summary>
        /// このフィルターの所有者。
        /// </summary>
        [System.Xml.Serialization.XmlIgnore]
        public override object Owner { get; set; }

        private string _Name;
        public string Name
        {
            get { return this._Name; }
            set
            {
                this._Name = value;
            }
        }

        private string _Icon;
        public string Icon
        {
            get { return this._Icon; }
            set
            {
                this._Icon = value;
            }
        }

        private StringFilter _Title;
        public string Title
        {
            get { return this._Title.FilterString; }
            set
            {
                this._Title.FilterString = value;
            }
        }
        private int startYearMin;
        private int startMonthMin;
        private int startDayMin;
        public string StartDateMin
        {
            get
            {
                return this.startYearMin.ToString("0000")
                        + "-" + this.startMonthMin.ToString("00")
                        + "-" + this.startDayMin.ToString("00")
                        ;
            }
            set
            {
                string[] values = value.Split('-');

                if (values.Length != 3
                        || values[0].Length != 4
                        || values[1].Length != 2
                        || values[2].Length != 2
                        )
                {
                    throw new FormatException();
                }

                int y = Int32.Parse(values[0]);
                int m = Int32.Parse(values[1]);
                int d = Int32.Parse(values[2]);

                try
                {
                    DateTime temp = new DateTime(y,m,d);
                }
                catch (Exception e) { throw e; }

                this.startYearMin = y;
                this.startMonthMin = m;
                this.startDayMin = d;

            }
        }

        private int startMonthMax;
        private int startYearMax;
        private int startDayMax;
        public string StartDateMax
        {
            get
            {
                return this.startYearMax.ToString("0000")
                        + "-" + this.startMonthMax.ToString("00")
                        + "-" + this.startDayMax.ToString("00")
                        ;
            }
            set
            {
                string[] values = value.Split('-');

                if (values.Length != 3
                        || values[0].Length != 4
                        || values[1].Length != 2
                        || values[2].Length != 2
                        )
                {
                    throw new FormatException();
                }

                int y = Int32.Parse(values[0]);
                int m = Int32.Parse(values[1]);
                int d = Int32.Parse(values[2]);

                try
                {
                    DateTime temp = new DateTime(y, m, d);
                }
                catch (Exception e) { throw e; }

                this.startYearMax = y;
                this.startMonthMax = m;
                this.startDayMax = d;
            }
        }

        private int startHourMin;
        private int startMinuteMin;
        public string StartTimeMin
        {
            get
            {
                return this.startHourMin.ToString("00")
                        + ":" + this.startMinuteMin.ToString("00")
                        ;
            }
            set
            {
                string[] values = value.Split(':');

                if (values.Length != 2
                        || values[0].Length != 2
                        || values[1].Length != 2
                        )
                {
                    throw new FormatException();
                }

                int h = Int32.Parse(values[0]);
                int m = Int32.Parse(values[1]);

                try
                {
                    DateTime temp = new DateTime(
                            DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, h, m, 0);
                }
                catch (Exception e) { throw e; }

                this.startHourMin = h;
                this.startMinuteMin = m;

            }
        }

        private int startHourMax;
        private int startMinuteMax;
        public string StartTimeMax
        {
            get
            {
                return this.startHourMax.ToString("00")
                        + ":" + this.startMinuteMax.ToString("00")
                        ;
            }
            set
            {
                string[] values = value.Split(':');

                if (values.Length != 2
                        || values[0].Length != 2
                        || values[1].Length != 2
                        )
                {
                    throw new FormatException();
                }

                int h = Int32.Parse(values[0]);
                int m = Int32.Parse(values[1]);

                try
                {
                    DateTime temp = new DateTime(
                            DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, h, m, 0);
                }
                catch (Exception e) { throw e; }

                this.startHourMax = h;
                this.startMinuteMax = m;
            }
        }
        private bool _Sunday;
        public bool Sunday
        {
            get { return this._Sunday; }
            set
            {
                this._Sunday = value;
            }
        }
        private bool _Monday;
        public bool Monday
        {
            get { return this._Monday; }
            set
            {
                this._Monday = value;
            }
        }
        private bool _Tuesday;
        public bool Tuesday
        {
            get { return this._Tuesday; }
            set
            {
                this._Tuesday = value;
            }
        }
        private bool _Wednesday;
        public bool Wednesday
        {
            get { return this._Wednesday; }
            set
            {
                this._Wednesday = value;
            }
        }
        private bool _Thursday;
        public bool Thursday
        {
            get { return this._Thursday; }
            set
            {
                this._Thursday = value;
            }
        }
        private bool _Friday;
        public bool Friday
        {
            get { return this._Friday; }
            set
            {
                this._Friday = value;
            }
        }
        private bool _Saturday;
        public bool Saturday
        {
            get { return this._Saturday; }
            set
            {
                this._Saturday = value;
            }
        }
        private bool _Viewed;
        public bool Viewed
        {
            get { return this._Viewed; }
            set
            {
                this._Viewed = value;
            }
        }
        private bool _Unviewed;
        public bool Unviewed
        {
            get { return this._Unviewed; }
            set
            {
                this._Unviewed = value;
            }
        }

        public TvProgramStandardFilter()
        {
            this._Name = Properties.Resources.General_NewFilter;
            this._Icon = "Resources/Filtrate.png";
            this._Title = new StringFilter();
            this.startYearMin = DateTime.MinValue.Year;
            this.startMonthMin = DateTime.MinValue.Month;
            this.startDayMin = DateTime.MinValue.Day;
            this.startYearMax = DateTime.MaxValue.Year;
            this.startMonthMax = DateTime.MaxValue.Month;
            this.startDayMax = DateTime.MaxValue.Day;
            this.startHourMin = 0;
            this.startMinuteMin = 0;
            this.startHourMax = 23;
            this.startMinuteMax = 59;
            this._Sunday = true;
            this._Monday = true;
            this._Tuesday = true;
            this._Wednesday = true;
            this._Thursday = true;
            this._Friday = true;
            this._Saturday = true;
            this._Viewed = true;
            this._Unviewed = true;
        }
        public override bool Match(TvProgram tvp, object inquirySource)
        {
            return isMatchedTitle(tvp.Contents_Title)
                    && isBetweenDates(tvp.Program_StartDateTime)
                    && isTimeInBand(tvp.Program_StartDateTime)
                    && isMatchedDayOfWeek(tvp.Program_StartDateTime)
                    && isMatchedUnviewed(tvp.IsUnviewed)
                    ;
        }
        public Object Clone()
        {
            TvProgramStandardFilter result = (TvProgramStandardFilter)this.MemberwiseClone();
            result._Title = (StringFilter)this._Title.Clone();
            return result;
        }
        
        #region PrivateMethod
        private bool isMatchedTitle(string title)
        {
            return this._Title.Match(title);
        }
        private bool isBetweenDates(DateTime startDateTime)
        {
            DateTime startDateMin = new DateTime(this.startYearMin, this.startMonthMin, this.startDayMin, 0, 0, 0);
            DateTime startDateMax = new DateTime(this.startYearMax, this.startMonthMax, this.startDayMax, 0, 0, 0);
            return startDateMin <= startDateTime && startDateTime <= startDateMax;
        }
        private bool isTimeInBand(DateTime startDateTime)
        {
            DateTime startTime = new DateTime(
                    DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, startDateTime.Hour, startDateTime.Minute, 0);
            DateTime startTimeMin = new DateTime(
                    DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, this.startHourMin, this.startMinuteMin, 0);
            DateTime startTimeMax = new DateTime(
                    DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, this.startHourMax, this.startMinuteMax, 0);

            if (startTimeMin <= startTimeMax)
            {
                return startTimeMin <= startTime && startTime <= startTimeMax;
            }
            else
            {
                return startTime <= startTimeMax || startTimeMin <= startTime;
            }
        }
        private bool isMatchedDayOfWeek(DateTime startDate)
        {
            bool result;
            switch (startDate.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    result = this.Sunday;
                    break;
                case DayOfWeek.Monday:
                    result = this.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    result = this.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    result = this.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    result = this.Thursday;
                    break;
                case DayOfWeek.Friday:
                    result = this.Friday;
                    break;
                case DayOfWeek.Saturday:
                    result = this.Saturday;
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        private bool isMatchedUnviewed(bool isUnviewed)
        {
            if (isUnviewed) return this.Unviewed;
            else return this.Viewed;
        }
        #endregion PrivateMethod
    }
}
