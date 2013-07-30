using BasicApp.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BasicApp.Common
{
    /// <summary>
    /// 여러 가지 중요한 편의를 제공하는 페이지의 일반 구현입니다.
    /// <list type="bullet">
    /// <item>
    /// <description>시각적 상태 매핑에 대한 응용 프로그램 뷰 상태</description>
    /// </item>
    /// <item>
    /// <description>GoBack, GoForward 및 GoHome 이벤트 핸들러</description>
    /// </item>
    /// <item>
    /// <description>탐색을 위한 마우스 및 바로 가기 키</description>
    /// </item>
    /// <item>
    /// <description>탐색 및 프로세스 수명 관리를 위한 상태 관리</description>
    /// </item>
    /// <item>
    /// <description>기본 뷰 모델</description>
    /// </item>
    /// </list>
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public partial class LayoutAwarePage : Page
    {
        /// <summary>
        /// <see cref="DefaultViewModel"/> 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty DefaultViewModelProperty =
            DependencyProperty.Register("DefaultViewModel", typeof(IObservableMap<String, Object>),
            typeof(LayoutAwarePage), null);

        private List<Control> _layoutAwareControls;

        /// <summary>
        /// <see cref="LayoutAwarePage"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public LayoutAwarePage()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled) return;

            // 비어 있는 기본 뷰 모델을 만듭니다.
            this.DefaultViewModel = new ObservableDictionary<String, Object>();

            // 이 페이지가 시각적 트리의 일부인 경우 두 가지를 변경합니다.
            // 1) 응용 프로그램 뷰 상태를 페이지의 시각적 상태에 매핑
            // 2) 키보드 및 마우스 탐색 요청 처리
            this.Loaded += (sender, e) =>
            {
                this.StartLayoutUpdates(sender, e);

                // 키보드 및 마우스 탐색은 전체 창 크기인 경우에만 적용됩니다.
                if (this.ActualHeight == Window.Current.Bounds.Height &&
                    this.ActualWidth == Window.Current.Bounds.Width)
                {
                    // 포커스가 필요하지 않도록 창을 직접 수신합니다.
                    Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated +=
                        CoreDispatcher_AcceleratorKeyActivated;
                    Window.Current.CoreWindow.PointerPressed +=
                        this.CoreWindow_PointerPressed;
                }
            };

            // 페이지가 더 이상 표시되지 않는 경우 동일한 변경을 취소합니다.
            this.Unloaded += (sender, e) =>
            {
                this.StopLayoutUpdates(sender, e);
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -=
                    CoreDispatcher_AcceleratorKeyActivated;
                Window.Current.CoreWindow.PointerPressed -=
                    this.CoreWindow_PointerPressed;
            };
        }

        /// <summary>
        /// trivial 뷰 모델로 사용하기 위해 제작된 <see cref="IObservableMap&lt;String, Object&gt;"/>의
        /// 구현을 가져옵니다.
        /// </summary>
        protected IObservableMap<String, Object> DefaultViewModel
        {
            get
            {
                return this.GetValue(DefaultViewModelProperty) as IObservableMap<String, Object>;
            }

            set
            {
                this.SetValue(DefaultViewModelProperty, value);
            }
        }

        #region 탐색 지원

        /// <summary>
        /// 이벤트 핸들러로 호출되어 페이지의 연결된 <see cref="Frame"/>에서
        /// 탐색 스택의 맨 위에 도달할 때까지 뒤로 탐색합니다.
        /// </summary>
        /// <param name="sender">이벤트를 트리거하는 인스턴스입니다.</param>
        /// <param name="e">이벤트의 발생 조건을 설명하는 이벤트 데이터입니다.</param>
        protected virtual void GoHome(object sender, RoutedEventArgs e)
        {
            // 탐색 프레임을 사용하여 최상위 페이지로 돌아갑니다.
            if (this.Frame != null)
            {
                while (this.Frame.CanGoBack) this.Frame.GoBack();
            }
        }

        /// <summary>
        /// 이벤트 핸들러로 호출되어 이 페이지의 <see cref="Frame"/>과 연결된
        /// 탐색 스택에서 뒤로 탐색합니다.
        /// </summary>
        /// <param name="sender">이벤트를 트리거하는 인스턴스입니다.</param>
        /// <param name="e">이벤트의 발생 조건을 설명하는 이벤트
        /// 데이터입니다.</param>
        protected virtual void GoBack(object sender, RoutedEventArgs e)
        {
            // 탐색 프레임을 사용하여 이전 페이지로 돌아갑니다.
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }

        /// <summary>
        /// 이벤트 핸들러로 호출되어 이 페이지의 <see cref="Frame"/>과 연결된
        /// 탐색 스택에서 뒤로 탐색합니다.
        /// </summary>
        /// <param name="sender">이벤트를 트리거하는 인스턴스입니다.</param>
        /// <param name="e">이벤트의 발생 조건을 설명하는 이벤트
        /// 데이터입니다.</param>
        protected virtual void GoForward(object sender, RoutedEventArgs e)
        {
            // 탐색 프레임을 사용하여 다음 페이지로 이동합니다.
            if (this.Frame != null && this.Frame.CanGoForward) this.Frame.GoForward();
        }

        /// <summary>
        /// 이 페이지가 활성화되고 전체 창 크기로 표시된 경우 Alt 키 조합 등
        /// 시스템 키를 포함한 모든 키 입력에서 호출됩니다. 페이지에 포커스가 없으면
        /// 페이지 간 키보드 탐색을 검색하는 데 사용됩니다.
        /// </summary>
        /// <param name="sender">이벤트를 트리거하는 인스턴스입니다.</param>
        /// <param name="args">이벤트의 발생 조건을 설명하는 이벤트 데이터입니다.</param>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender,
            AcceleratorKeyEventArgs args)
        {
            var virtualKey = args.VirtualKey;

            // 왼쪽 화살표, 오른쪽 화살표 또는 전용 이전 또는 다음 키를 눌렀을 때만 더
            // 조사합니다.
            if ((args.EventType == CoreAcceleratorKeyEventType.SystemKeyDown ||
                args.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
                (virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right ||
                (int)virtualKey == 166 || (int)virtualKey == 167))
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;
                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
                bool noModifiers = !menuKey && !controlKey && !shiftKey;
                bool onlyAlt = menuKey && !controlKey && !shiftKey;

                if (((int)virtualKey == 166 && noModifiers) ||
                    (virtualKey == VirtualKey.Left && onlyAlt))
                {
                    // 이전 키 또는 Alt+왼쪽 화살표를 누르면 뒤로 탐색
                    args.Handled = true;
                    this.GoBack(this, new RoutedEventArgs());
                }
                else if (((int)virtualKey == 167 && noModifiers) ||
                    (virtualKey == VirtualKey.Right && onlyAlt))
                {
                    // 다음 키 또는 Alt+오른쪽 화살표를 누르면 앞으로 탐색
                    args.Handled = true;
                    this.GoForward(this, new RoutedEventArgs());
                }
            }
        }

        /// <summary>
        /// 이 페이지가 활성화되고 전체 창 크기로 표시된 경우 모든 마우스 클릭, 터치 스크린 탭
        /// 또는 이와 같은 상호 작용에 대해 호출됩니다. 브라우저 스타일의 다음 및 이전 마우스 버튼 클릭을
        /// 검색하여 페이지 간에 탐색하는 데 사용됩니다.
        /// </summary>
        /// <param name="sender">이벤트를 트리거하는 인스턴스입니다.</param>
        /// <param name="args">이벤트의 발생 조건을 설명하는 이벤트 데이터입니다.</param>
        private void CoreWindow_PointerPressed(CoreWindow sender,
            PointerEventArgs args)
        {
            var properties = args.CurrentPoint.Properties;

            // 왼쪽 화살표, 오른쪽 화살표 및 가운데 화살표 단추와 함께 누르는 단추를 무시합니다.
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed) return;

            // 뒤로 또는 앞으로를 누르면(동시 아님) 해당 방향으로 탐색합니다.
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                args.Handled = true;
                if (backPressed) this.GoBack(this, new RoutedEventArgs());
                if (forwardPressed) this.GoForward(this, new RoutedEventArgs());
            }
        }

        #endregion

        #region 시각적 상태 전환

        /// <summary>
        /// 이벤트 핸들러로 호출되어 일반적으로 페이지 내에서 <see cref="Control"/>의
        /// <see cref="FrameworkElement.Loaded"/> 이벤트에 대해 보낸 사람이
        /// 응용 프로그램 뷰 상태 변경에 해당하는 시각적 상태 관리 변경을 수신해야 함을
        /// 나타냅니다.
        /// </summary>
        /// <param name="sender">뷰 상태에 해당하는 시각적 상태 관리를 지원하는
        /// <see cref="Control"/> 인스턴스입니다.</param>
        /// <param name="e">요청이 생성된 방법을 설명하는 이벤트 데이터입니다.</param>
        /// <remarks>현재 뷰 상태는 레이아웃 업데이트가 요청될 때 해당
        /// 시각적 상태를 설정하는 데 즉시 사용됩니다. <see cref="StopLayoutUpdates"/>에
        /// 연결된 해당 <see cref="FrameworkElement.Unloaded"/> 이벤트
        /// 핸들러를 사용하는 것이 좋습니다. <see cref="LayoutAwarePage"/>의 인스턴스는
        /// Loaded 및 Unloaded 이벤트에서 이러한 핸들러를
        /// 자동으로 호출합니다.</remarks>
        /// <seealso cref="DetermineVisualState"/>
        /// <seealso cref="InvalidateVisualState"/>
        public void StartLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null) return;
            if (this._layoutAwareControls == null)
            {
                // 업데이트해야 하는 컨트롤이 있을 경우 뷰 상태 변경을 수신하기 시작합니다.
                Window.Current.SizeChanged += this.WindowSizeChanged;
                this._layoutAwareControls = new List<Control>();
            }
            this._layoutAwareControls.Add(control);

            // 컨트롤의 초기 시각적 상태를 설정합니다.
            VisualStateManager.GoToState(control, DetermineVisualState(ApplicationView.Value), false);
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            this.InvalidateVisualState();
        }

        /// <summary>
        /// 일반적으로 <see cref="Control"/>의 <see cref="FrameworkElement.Unloaded"/>
        /// 이벤트에 대해 이벤트 핸들러로 호출되어 보낸 사람이 응용 프로그램 뷰 상태 변경에
        /// 해당하는 시각적 상태 관리 변경을 수신해야 함을 나타냅니다.
        /// </summary>
        /// <param name="sender">뷰 상태에 해당하는 시각적 상태 관리를 지원하는
        /// <see cref="Control"/> 인스턴스입니다.</param>
        /// <param name="e">요청이 생성된 방법을 설명하는 이벤트 데이터입니다.</param>
        /// <remarks>현재 뷰 상태는 레이아웃 업데이트가 요청될 때 해당
        /// 해당하는 시각적 상태 관리 변경을 수신해야 함을 나타냅니다.</remarks>
        /// <seealso cref="StartLayoutUpdates"/>
        public void StopLayoutUpdates(object sender, RoutedEventArgs e)
        {
            var control = sender as Control;
            if (control == null || this._layoutAwareControls == null) return;
            this._layoutAwareControls.Remove(control);
            if (this._layoutAwareControls.Count == 0)
            {
                // 업데이트할 컨트롤이 없는 경우 뷰 상태 변경에 대한 수신을 중지합니다.
                this._layoutAwareControls = null;
                Window.Current.SizeChanged -= this.WindowSizeChanged;
            }
        }

        /// <summary>
        /// <see cref="ApplicationViewState"/> 값을 페이지 내의 시각적 상태 관리를 위한
        /// 문자열로 변환합니다. 기본 구현은 열거형 값의 이름을 사용합니다.
        /// 서브클래스는 이 메서드를 재정의하여 사용된 매핑 구성표를 재정의할 수 있습니다.
        /// </summary>
        /// <param name="viewState">시각적 상태가 필요한 뷰 상태입니다.</param>
        /// <returns><see cref="VisualStateManager"/>를 실행하는 데 사용되는
        /// 시각적 상태 이름입니다.</returns>
        /// <seealso cref="InvalidateVisualState"/>
        protected virtual string DetermineVisualState(ApplicationViewState viewState)
        {
            return viewState.ToString();
        }

        /// <summary>
        /// 올바른 시각적 상태의 시각적 상태 변경을 수신하는 모든 컨트롤을
        /// 업데이트합니다.
        /// </summary>
        /// <remarks>
        /// 일반적으로 재정의하는 <see cref="DetermineVisualState"/>와 함께
        /// 사용되어 뷰 상태가 변경되지 않았어도 다른 값이 반환될 수 있음을
        /// 알립니다.
        /// </remarks>
        public void InvalidateVisualState()
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame.CurrentSourcePageType != typeof(SnappedViewPage ) && DetermineVisualState(ApplicationView.Value) == ApplicationViewState.Snapped.ToString())
            {
                rootFrame.Navigate(typeof(SnappedViewPage));
                return;
            }
            else
            {
                if( rootFrame.CurrentSourcePageType == typeof(SnappedViewPage )) {
                    rootFrame.GoBack();
                }
            }
            if (this._layoutAwareControls != null)
            {
                string visualState = DetermineVisualState(ApplicationView.Value);
                foreach (var layoutAwareControl in this._layoutAwareControls)
                {
                    VisualStateManager.GoToState(layoutAwareControl, visualState, false);
                }
            }
        }

        #endregion

        #region 프로세스 수명 관리

        private String _pageKey;

        /// <summary>
        /// 이 페이지가 프레임에 표시되려고 할 때 호출됩니다.
        /// </summary>
        /// <param name="e">페이지에 도달한 방법을 설명하는 이벤트 데이터입니다. Parameter
        /// 속성은 표시할 그룹을 지정합니다.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // 탐색을 통해 캐시된 페이지로 돌아가도 상태 로딩이 발생하지 않아야 합니다.
            if (this._pageKey != null) return;

            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            this._pageKey = "Page-" + this.Frame.BackStackDepth;

            if (e.NavigationMode == NavigationMode.New)
            {
                // 탐색 스택에 새 페이지를 추가할 때 앞으로 탐색에 대한 기존 상태를
                // 지웁니다.
                var nextPageKey = this._pageKey;
                int nextPageIndex = this.Frame.BackStackDepth;
                while (frameState.Remove(nextPageKey))
                {
                    nextPageIndex++;
                    nextPageKey = "Page-" + nextPageIndex;
                }

                // 탐색 매개 변수를 새 페이지에 전달합니다.
                this.LoadState(e.Parameter, null);
            }
            else
            {
                // 일시 중단된 상태를 로드하고 캐시에서 삭제된 페이지를 다시 만드는 것과
                // 같은 전략을 사용하여 탐색 매개 변수와 유지된 페이지 상태를 페이지로
                // 전달합니다.
                this.LoadState(e.Parameter, (Dictionary<String, Object>)frameState[this._pageKey]);
            }
        }

        /// <summary>
        /// 이 페이지가 프레임에 더 이상 표시되지 않을 때 호출됩니다.
        /// </summary>
        /// <param name="e">페이지에 도달한 방법을 설명하는 이벤트 데이터입니다. Parameter
        /// 속성은 표시할 그룹을 지정합니다.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
            var pageState = new Dictionary<String, Object>();
            this.SaveState(pageState);
            frameState[_pageKey] = pageState;
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
        protected virtual void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// 응용 프로그램이 일시 중지되거나 탐색 캐시에서 페이지가 삭제된 경우
        /// 이 페이지와 관련된 상태를 유지합니다. 값은
        /// <see cref="SuspensionManager.SessionState"/>의 serialization 요구 사항을 만족해야 합니다.
        /// </summary>
        /// <param name="pageState">serializable 상태로 채워질 빈 사전입니다.</param>
        protected virtual void SaveState(Dictionary<String, Object> pageState)
        {
        }

        #endregion

        /// <summary>
        /// 기본 뷰 모델로 사용하기 위한 재입력을 지원하는 IObservableMap의
        /// 구현입니다.
        /// </summary>
        private class ObservableDictionary<K, V> : IObservableMap<K, V>
        {
            private class ObservableDictionaryChangedEventArgs : IMapChangedEventArgs<K>
            {
                public ObservableDictionaryChangedEventArgs(CollectionChange change, K key)
                {
                    this.CollectionChange = change;
                    this.Key = key;
                }

                public CollectionChange CollectionChange { get; private set; }
                public K Key { get; private set; }
            }

            private Dictionary<K, V> _dictionary = new Dictionary<K, V>();
            public event MapChangedEventHandler<K, V> MapChanged;

            private void InvokeMapChanged(CollectionChange change, K key)
            {
                var eventHandler = MapChanged;
                if (eventHandler != null)
                {
                    eventHandler(this, new ObservableDictionaryChangedEventArgs(change, key));
                }
            }

            public void Add(K key, V value)
            {
                this._dictionary.Add(key, value);
                this.InvokeMapChanged(CollectionChange.ItemInserted, key);
            }

            public void Add(KeyValuePair<K, V> item)
            {
                this.Add(item.Key, item.Value);
            }

            public bool Remove(K key)
            {
                if (this._dictionary.Remove(key))
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                    return true;
                }
                return false;
            }

            public bool Remove(KeyValuePair<K, V> item)
            {
                V currentValue;
                if (this._dictionary.TryGetValue(item.Key, out currentValue) &&
                    Object.Equals(item.Value, currentValue) && this._dictionary.Remove(item.Key))
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
                    return true;
                }
                return false;
            }

            public V this[K key]
            {
                get
                {
                    return this._dictionary[key];
                }
                set
                {
                    this._dictionary[key] = value;
                    this.InvokeMapChanged(CollectionChange.ItemChanged, key);
                }
            }

            public void Clear()
            {
                var priorKeys = this._dictionary.Keys.ToArray();
                this._dictionary.Clear();
                foreach (var key in priorKeys)
                {
                    this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                }
            }

            public ICollection<K> Keys
            {
                get { return this._dictionary.Keys; }
            }

            public bool ContainsKey(K key)
            {
                return this._dictionary.ContainsKey(key);
            }

            public bool TryGetValue(K key, out V value)
            {
                return this._dictionary.TryGetValue(key, out value);
            }

            public ICollection<V> Values
            {
                get { return this._dictionary.Values; }
            }

            public bool Contains(KeyValuePair<K, V> item)
            {
                return this._dictionary.Contains(item);
            }

            public int Count
            {
                get { return this._dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
            {
                return this._dictionary.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this._dictionary.GetEnumerator();
            }

            public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
            {
                int arraySize = array.Length;
                foreach (var pair in this._dictionary)
                {
                    if (arrayIndex >= arraySize) break;
                    array[arrayIndex++] = pair;
                }
            }
        }
    }
}
