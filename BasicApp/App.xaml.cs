using BasicApp.Common;
using BasicApp.Common;
using Callisto.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 구매 기능의 로컬 테스트를 위한 설정.
// 스토어 제출 시에 이 부분을 주석 처리해야 합니다.
using CurrentAppProxy = Windows.ApplicationModel.Store.CurrentAppSimulator;

// 구매 기능의 실제 서비스를 위한 설정.
// 스토어 제출 시에 이 부분을 주석 해제해야 합니다.
//using CurrentAppProxy = Windows.ApplicationModel.Store.CurrentApp;

// 표 형태 응용 프로그램 템플릿에 대한 설명은 http://go.microsoft.com/fwlink/?LinkId=234226에 나와 있습니다.

namespace BasicApp
{
    /// <summary>
    /// 기본 응용 프로그램 클래스를 보완하는 응용 프로그램별 동작을 제공합니다.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// 앱의 기본 설정 부분입니다.
        /// 앱 제출 전에 적절히 수정해야 합니다.
        /// </summary>
       
        // 설정에서 개인정보 보호 정책을 클릭하였을 때 연결되는 웹페이지입니다.
        // 스토어에 제출할 때 적절한 개인정보 보호 정책을 공지한 웹페이지가 준비되어 있어야 합니다.
        private string PRIVACY_POLICY_URL = "about:blank;";
        
        public static ResourceLoader LangRes = new ResourceLoader();
        Color SettingsPaneHeaderColor = Windows.UI.Color.FromArgb(255, 0, 0xA2, 0xE8);
        private string STORE_PROXY_FILENAME = "WindowsStoreProxy.xml";

        /// <summary>
        /// Singleton 응용 프로그램 개체를 초기화합니다. 이것은 실행되는 작성 코드의 첫 번째
        /// 줄이며 따라서 main() 또는 WinMain()과 논리적으로 동일합니다.
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// 최종 사용자가 응용 프로그램을 정상적으로 시작할 때 호출됩니다. 다른 진입점은
        /// 특정 파일을 열거나, 검색 결과를 표시하는 등 응용 프로그램을 시작할 때
        /// 사용됩니다.
        /// </summary>
        /// <param name="args">시작 요청 및 프로세스에 대한 정보입니다.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            // 설정 메뉴 초기화
            SettingsPane.GetForCurrentView().CommandsRequested += SettingPane_CommandsRequested;

            // 구매 기능 시뮬레이터 초기화
            if (typeof(CurrentAppProxy) == typeof(CurrentAppSimulator))
            {
                var file = await Package.Current.InstalledLocation.GetFileAsync(STORE_PROXY_FILENAME);
                await CurrentAppSimulator.ReloadSimulatorAsync(file);
                licenseInformation = CurrentAppProxy.LicenseInformation;
                listingInformation = await CurrentAppProxy.LoadListingInformationAsync();
            }
            else
            {
                licenseInformation = CurrentApp.LicenseInformation;
                listingInformation = await CurrentApp.LoadListingInformationAsync();
            }
            
            Frame rootFrame = Window.Current.Content as Frame;

            // 창에 콘텐츠가 이미 있는 경우 앱 초기화를 반복하지 말고,
            // 창이 활성화되어 있는지 확인하십시오.
            
            if (rootFrame == null)
            {
                // 탐색 컨텍스트로 사용할 프레임을 만들고 첫 페이지로 이동합니다.
                rootFrame = new Frame();
                //프레임을 SuspensionManager 키에 연결                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // 해당하는 경우에만 저장된 세션 상태를 복원합니다.
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                    }
                    catch (SuspensionManagerException)
                    {
                        //상태를 복원하는 중에 오류가 발생했습니다.
                        //상태가 없는 것으로 가정하고 계속합니다.
                    }
                }

                // 현재 창에 프레임 넣기
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content == null)
            {
                // 탐색 스택이 복원되지 않으면 첫 번째 페이지로 돌아가고
                // 필요한 정보를 탐색 매개 변수로 전달하여 새 페이지를
                // 구성합니다.
                if (!rootFrame.Navigate(typeof(MainPage)))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // 현재 창이 활성 창인지 확인
            Window.Current.Activate();
        }
        internal static LicenseInformation licenseInformation;
        internal static ListingInformation listingInformation;

        internal async static void PurchaseApp()
        {
            await CurrentAppProxy.RequestAppPurchaseAsync(false);
        }


        private void SettingPane_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Add(new SettingsCommand("about", LangRes.GetString("About"), OpenAbout));
            args.Request.ApplicationCommands.Add(new SettingsCommand("options", LangRes.GetString("Options"), OpenOptions));
            args.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", LangRes.GetString("PrivacyPolicy"), OpenPrivacyPolicy));
        }


        private void OpenOptions(Windows.UI.Popups.IUICommand command)
        {
            var settings = new SettingsFlyout();
            settings.Content = new OptionsUserControl();
            settings.HeaderBrush = new SolidColorBrush(SettingsPaneHeaderColor);
            settings.HeaderText = LangRes.GetString("Options");
            settings.IsOpen = true;
        }

        private void OpenAbout(Windows.UI.Popups.IUICommand command)
        {
            var settings = new SettingsFlyout();
            settings.Content = new AboutUserControl();
            settings.HeaderBrush = new SolidColorBrush(SettingsPaneHeaderColor);
            settings.HeaderText = LangRes.GetString("About");
            settings.IsOpen = true;
        }

        private async void OpenPrivacyPolicy(Windows.UI.Popups.IUICommand command)
        {
            Uri uri = new Uri(PRIVACY_POLICY_URL);
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        /// <summary>
        /// 응용 프로그램 실행이 일시 중지된 경우 호출됩니다. 응용 프로그램이 종료될지
        /// 또는 메모리 콘텐츠를 변경하지 않고 다시 시작할지 여부를 결정하지 않은 채
        /// 응용 프로그램 상태가 저장됩니다.
        /// </summary>
        /// <param name="sender">일시 중지된 요청의 소스입니다.</param>
        /// <param name="e">일시 중지된 요청에 대한 세부 정보입니다.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

    }
}
