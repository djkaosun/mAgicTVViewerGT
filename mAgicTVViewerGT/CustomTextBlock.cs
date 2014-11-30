using System;
using System.Windows;
using System.Windows.Controls;

namespace mAgicTVViewerGT
{
    public class CustomTextBlock : TextBlock
    {
        public bool IsTrimmed
        {
            get { return (bool)base.GetValue(CustomTextBlock.IsTrimmedProperty); }
            private set { SetValue(IsTrimmedPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsTrimmedPropertyKey;
        public static readonly DependencyProperty IsTrimmedProperty;

        static CustomTextBlock()
        {
            CustomTextBlock.IsTrimmedPropertyKey = DependencyProperty.RegisterReadOnly(
                    "IsTrimmed",
                    typeof(bool),
                    typeof(CustomTextBlock),
                    new UIPropertyMetadata(false)
                    );
            CustomTextBlock.IsTrimmedProperty = CustomTextBlock.IsTrimmedPropertyKey.DependencyProperty;
        }

        public CustomTextBlock()
        {
            this.Loaded += this.loadedEventHandler;
            this.SizeChanged += this.sizeChangedEventHandler;
        }

        public void loadedEventHandler(object sender, RoutedEventArgs e)
        {
            this.IsTrimmed = this.isTextTrimmed();
        }

        private void sizeChangedEventHandler(object sender, SizeChangedEventArgs e)
        {
            this.IsTrimmed = this.isTextTrimmed();
        }

        private bool isTextTrimmed()
        {
            if (!this.IsVisible) return false;
            if (this.ActualWidth == 0) return false;

            var textEndScrPt = this.PointToScreen(new Point(this.ActualWidth - 1, 1));
            var textEndRelPt = this.PointFromScreen(textEndScrPt);

            return System.Windows.Media.VisualTreeHelper.HitTest(this, textEndRelPt) == null;
        }
    }
}
