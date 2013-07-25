using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml.Data;

namespace BasicApp.Common
{
    /// <summary>
    /// 모델은 단순화하기 위한 <see cref="INotifyPropertyChanged"/>의 구현입니다.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// 속성 변경 알림을 위한 멀티캐스트 이벤트입니다.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 속성이 이미 원하는 값과 일치하는지 확인합니다. 속성을 설정하고
        /// 필요한 경우 수신기에 알립니다.
        /// </summary>
        /// <typeparam name="T">속성의 형식입니다.</typeparam>
        /// <param name="storage">getter 및 setter를 모두 갖고 있는 속성에 대한 참조입니다.</param>
        /// <param name="value">속성에 대한 원하는 값입니다.</param>
        /// <param name="propertyName">수신기에 알리는 데 사용된 속성 이름입니다. 이
        /// 값은 생략 가능하며 CallerMemberName을 지원하는 컴파일러에서 호출할 때
        /// 자동으로 제공될 수 있습니다.</param>
        /// <returns>값이 변경되면 true, 기존 값이 원하는 값과 일치하면
        /// false입니다.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 수신기에 속성 값이 변경되었음을 알립니다.
        /// </summary>
        /// <param name="propertyName">수신기에 알리는 데 사용된 속성 이름입니다. 이
        /// 값은 생략 가능하며 <see cref="CallerMemberNameAttribute"/>를 지원하는 컴파일러에서 호출할 때
        /// 자동으로 제공될 수 있습니다.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
