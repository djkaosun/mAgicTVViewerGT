using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;

namespace mAgicTVViewerGT
{
    /// <summary>Popup の表示時イベントを RoutedEvent にするためのクラス</summary>
    /// <remarks>
    /// RibbonApplicationMenu の PART_Popup の Popup.Opend イベントは他でも処理されるので、
    /// 先に Popup の位置を動かす処理をしないと Popup に表示されるボタンが変な位置に表示されます。
    /// そのため Popup のスタイルでイベント登録することで先に処理できるようにしておく必要があります。
    /// </remark>
    public class RibbonApplicationMenuPopupEventManager
    {
        public static bool GetFixPopup(DependencyObject obj)
        {
            return (bool)obj.GetValue(FixPopupProperty);
        }

        public static void SetFixPopup(DependencyObject obj, bool value)
        {
            obj.SetValue(FixPopupProperty, value);
        }

        /// <summary>Popup の表示時に位置を直すかを指定するプロパティ</summary>
        public static DependencyProperty FixPopupProperty
            = DependencyProperty.RegisterAttached
            ("FixPopup"
            , typeof(bool)
            , typeof(RibbonApplicationMenuPopupEventManager)
            , new PropertyMetadata
                (false
                , new PropertyChangedCallback(OnPopupPropertyChanged)));

        public static void OnPopupPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            Popup target = dpo as Popup;
            if (target != null)
            {
                bool newValue = (bool)e.NewValue;
                bool oldValue = (bool)e.OldValue;

                if (oldValue)
                {
                    target.Opened -= new EventHandler(target_Opened);
                }

                if (newValue)
                {
                    target.Opened += new EventHandler(target_Opened);
                }
            }
        }


        static void target_Opened(object sender, EventArgs e)
        {
            Popup popup = (Popup)sender;
            if (popup.PlacementTarget != null)
            {
                if (popup.TemplatedParent is RibbonApplicationMenu)
                {
                    //関係ない Popup まで Routed イベントを発生させてしまうので、RibbonApplicationMenu の PART_Popup だけ処理する

                    //RibbonApplicationMenu を基準にした位置でうまく表示されるようにする
                    popup.CustomPopupPlacementCallback = CustomPopupPlacementCallback;
                    popup.Placement = PlacementMode.Custom;
                }
            }
        }

        private static CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            CustomPopupPlacement cpp = new CustomPopupPlacement(new Point(0, 0), PopupPrimaryAxis.Horizontal);
            return new CustomPopupPlacement[] { cpp };
        }
    }
}
