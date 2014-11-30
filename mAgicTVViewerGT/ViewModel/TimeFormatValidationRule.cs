using System;
using System.Windows.Controls;

namespace mAgicTVViewerGT.ViewModel
{
    public class TimeFormatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (!(value is String)) return new ValidationResult(false, "文字列ではありません。");

            string[] values = ((string)value).Split(':');

            if (values.Length != 3
                    || values[0].Length != 2
                    || values[1].Length != 2
                    )
            {
                return new ValidationResult(false, "時刻の書式が間違っています。(hh:mm 形式)");
            }

            int h, m;

            try
            {
                h = Int32.Parse(values[0]);
                m = Int32.Parse(values[1]);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "時刻に数字と : 以外が含まれています。");
            }

            try
            {
                DateTime temp = new DateTime(DateTime.MinValue.Year, DateTime.MinValue.Month, DateTime.MinValue.Day, h, m, 0);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new ValidationResult(false, "正しい時刻ではありません。");
            }

            return new ValidationResult(true, null);
        }
    }
}
