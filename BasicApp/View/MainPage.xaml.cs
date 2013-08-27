using BasicApp.View;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 기본 페이지 항목 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234237에 나와 있습니다.

namespace BasicApp.Common
{
    /// <summary>
    /// 대부분의 응용 프로그램에 공통되는 특성을 제공하는 기본 페이지입니다.
    /// </summary>
    public sealed partial class MainPage : BasicApp.Common.LayoutAwarePage
    {
        Windows.UI.Core.CoreDispatcher dispatcher;
        public MainPage()
        {
            this.InitializeComponent();
            dispatcher = Windows.UI.Core.CoreWindow.GetForCurrentThread().Dispatcher;

            loadSettings();

        }

        private async void loadSettings()
        {
            Uri SettingsUri = new Uri("ms-appx:///Settings/settings.xml");
            StorageFile SettingsFile = await StorageFile.GetFileFromApplicationUriAsync(SettingsUri);
            var SettingsXml = await XmlDocument.LoadFromFileAsync(SettingsFile);

            var result = SettingsXml.SelectSingleNode("url");
            mainWebView.Navigate(new Uri(result.FirstChild.NodeValue.ToString()));
        }

        /// <summary>
        /// 탐색 중 전달된 콘텐츠로 페이지를 채웁니다. 이전 세션의 페이지를
        /// 다시 만들 때 저장된 상태도 제공됩니다.
        /// </summary>
        /// <param name="navigationParameter">이 페이지가 처음 요청될 때
        /// <see cref="Frame.Navigate(Type, Object)"/>에 전달된 매개 변수 값입니다.
        /// </param>
        /// <param name="pageState">이전 세션 동안 이 페이지에 유지된
        /// 사전 상태입니다. 페이지를 처음 방문할 때는 이 값이 null입니다.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            App.licenseInformation.LicenseChanged += licenseInformation_LicenseChanged;
        }

        void licenseInformation_LicenseChanged()
        {
            if (!App.licenseInformation.IsTrial) dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, RemoveAd);
        }

        /// <summary>
        /// 응용 프로그램이 일시 중지되거나 탐색 캐시에서 페이지가 삭제된 경우
        /// 이 페이지와 관련된 상태를 유지합니다. 값은
        /// <see cref="SuspensionManager.SessionState"/>의 serialization 요구 사항을 만족해야 합니다.
        /// </summary>
        /// <param name="pageState">serializable 상태로 채워질 빈 사전입니다.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }
        void RemoveAd()
        {
            RightAd.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            AdCol.Width = new GridLength(0);
            BottomAd.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            AdRow.Height = new GridLength(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AdSamples));
        }
    }
}
