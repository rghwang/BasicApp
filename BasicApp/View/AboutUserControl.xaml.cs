using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 사용자 정의 컨트롤 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234236에 나와 있습니다.

namespace BasicApp.View
{
    public sealed partial class AboutUserControl : UserControl
    {
        public AboutUserControl()
        {
            this.InitializeComponent();
            UpdateVersionInfo();
        }
        internal void UpdateVersionInfo()
        {
            if (App.licenseInformation.IsTrial)
            {
                licenseText.Text = App.LangRes.GetString("TrialVersion");
                licenseDetail.Text = string.Format(App.LangRes.GetString("ExpirationDateInfo"), App.licenseInformation.ExpirationDate.LocalDateTime.ToString(), App.listingInformation.FormattedPrice);
                PurchaseButton.IsEnabled = true;
            }
            else
            {
                licenseText.Text = App.LangRes.GetString("FullVersion");
                licenseDetail.Text = "Thank you for purchasing.";
                PurchaseButton.IsEnabled = false;
            }
        }

        private void PurchaseButton_Click(object sender, RoutedEventArgs e)
        {
            App.PurchaseApp();
            UpdateVersionInfo();
        }
    }
}
