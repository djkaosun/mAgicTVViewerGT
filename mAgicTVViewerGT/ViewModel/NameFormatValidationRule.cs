using System;
using System.Windows.Controls;

namespace mAgicTVViewerGT.ViewModel
{
    public class NameFormatValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (!(value is String)) return new ValidationResult(false, "文字列ではありません。");

            if(((string)value).Length < 1)
                    return new ValidationResult(false, "名前が設定されていません。");

            return new ValidationResult(true, null);
        }
    }
}
