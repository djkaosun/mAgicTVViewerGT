using System;
using System.Windows.Controls;

namespace mAgicTVViewerGT.ViewModel
{
    public class DateFormatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (!(value is String)) return new ValidationResult(false, "文字列ではありません。");

            string[] values = ((string)value).Split('-');

            if (values.Length != 3
                    || values[0].Length != 4
                    || values[1].Length != 2
                    || values[2].Length != 2
                    )
            {
                return new ValidationResult(false, "日付の書式が間違っています。(YYYY-MM-DD 形式)");
            }

            int y, m, d;
            try
            {
                y = Int32.Parse(values[0]);
                m = Int32.Parse(values[1]);
                d = Int32.Parse(values[2]);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, "日付に数字と - 以外が含まれています。");
            }

            try
            {
                DateTime temp = new DateTime(y, m, d);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new ValidationResult(false, "正しい日付ではありません。");
            }

            return new ValidationResult(true, null);
        }
    }
}
