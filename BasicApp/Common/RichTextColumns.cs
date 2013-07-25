using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;

namespace BasicApp.Common
{
    /// <summary>
    /// 사용 가능한 콘텐츠에 맞게 필요한 만큼 추가 오버플로 열을 만드는
    /// <see cref="RichTextBlock"/>의 래퍼입니다.
    /// </summary>
    /// <example>
    /// 다음은 데이터 바인딩된 임의의 콘텐츠를 포함하기 위해 50픽셀의
    /// 간격을 두고 400픽셀 너비의 열 컬렉션을 만듭니다.
    /// <code>
    /// <RichTextColumns>
    ///     <RichTextColumns.ColumnTemplate>
    ///         <DataTemplate>
    ///             <RichTextBlockOverflow Width="400" Margin="50,0,0,0"/>
    ///         </DataTemplate>
    ///     </RichTextColumns.ColumnTemplate>
    ///     
    ///     <RichTextBlock Width="400">
    ///         <Paragraph>
    ///             <Run Text="{Binding Content}"/>
    ///         </Paragraph>
    ///     </RichTextBlock>
    /// </RichTextColumns>
    /// </code>
    /// </example>
    /// <remarks>일반적으로 바인딩되지 않은 공간에 필요한 모든 열이 만들어질 것으로
    /// 간주되는 가로 스크롤 영역에 사용됩니다. 세로 스크롤 공간에
    /// 사용되는 경우에는 추가 열이 없습니다.</remarks>
    [Windows.UI.Xaml.Markup.ContentProperty(Name = "RichTextContent")]
    public sealed class RichTextColumns : Panel
    {
        /// <summary>
        /// <see cref="RichTextContent"/> 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty RichTextContentProperty =
            DependencyProperty.Register("RichTextContent", typeof(RichTextBlock),
            typeof(RichTextColumns), new PropertyMetadata(null, ResetOverflowLayout));

        /// <summary>
        /// <see cref="ColumnTemplate"/> 종속성 속성을 식별합니다.
        /// </summary>
        public static readonly DependencyProperty ColumnTemplateProperty =
            DependencyProperty.Register("ColumnTemplate", typeof(DataTemplate),
            typeof(RichTextColumns), new PropertyMetadata(null, ResetOverflowLayout));

        /// <summary>
        /// <see cref="RichTextColumns"/> 클래스의 새 인스턴스를 초기화합니다.
        /// </summary>
        public RichTextColumns()
        {
            this.HorizontalAlignment = HorizontalAlignment.Left;
        }

        /// <summary>
        /// 첫 번째 열로 사용할 초기 서식 있는 텍스트 콘텐츠를 가져오거나 설정합니다.
        /// </summary>
        public RichTextBlock RichTextContent
        {
            get { return (RichTextBlock)GetValue(RichTextContentProperty); }
            set { SetValue(RichTextContentProperty, value); }
        }

        /// <summary>
        /// 추가 <see cref="RichTextBlockOverflow"/> 인스턴스를
        /// 만드는 데 사용되는 템플릿을 가져오거나 설정합니다.
        /// </summary>
        public DataTemplate ColumnTemplate
        {
            get { return (DataTemplate)GetValue(ColumnTemplateProperty); }
            set { SetValue(ColumnTemplateProperty, value); }
        }

        /// <summary>
        /// 콘텐츠 또는 오버플로 템플릿이 변경되어 열 레이아웃이 다시 만들어질 때 호출됩니다.
        /// </summary>
        /// <param name="d">변경이 발생한 <see cref="RichTextColumns"/>의
        /// 인스턴스입니다.</param>
        /// <param name="e">특정 변경을 설명하는 이벤트 데이터입니다.</param>
        private static void ResetOverflowLayout(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 큰 변경이 발생하면 열 레이아웃을 처음부터 다시 작성합니다.
            var target = d as RichTextColumns;
            if (target != null)
            {
                target._overflowColumns = null;
                target.Children.Clear();
                target.InvalidateMeasure();
            }
        }

        /// <summary>
        /// 이미 만들어진 오버플로 열을 나열합니다. 초기 RichTextBlock 자식
        /// 다음의 <see cref="Panel.Children"/> 컬렉션의 인스턴스와 1:1 관계를
        /// 유지해야 합니다.
        /// </summary>
        private List<RichTextBlockOverflow> _overflowColumns = null;

        /// <summary>
        /// 추가 오버플로 열이 필요한지와 기존 열을 제거할 수 있는지 여부를
        /// 결정합니다.
        /// </summary>
        /// <param name="availableSize">생성할 수 있는 추가 열의 개수를 제한하는
        /// 사용 가능한 공간 크기입니다.</param>
        /// <returns>원본 콘텐츠에 여분의 열을 더한 결과 크기입니다.</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.RichTextContent == null) return new Size(0, 0);

            // 추가 열 목록의 부족을 아직 수행되지 않았다는 표시로
            // 사용하여 RichTextBlock이 자식인지
            // 확인합니다.
            if (this._overflowColumns == null)
            {
                Children.Add(this.RichTextContent);
                this._overflowColumns = new List<RichTextBlockOverflow>();
            }

            // 원본 RichTextBlock 콘텐츠를 측정하여 시작합니다.
            this.RichTextContent.Measure(availableSize);
            var maxWidth = this.RichTextContent.DesiredSize.Width;
            var maxHeight = this.RichTextContent.DesiredSize.Height;
            var hasOverflow = this.RichTextContent.HasOverflowContent;

            // 오버플로 열이 충분한지 확인합니다.
            int overflowIndex = 0;
            while (hasOverflow && maxWidth < availableSize.Width && this.ColumnTemplate != null)
            {
                // 기존 오버플로 열을 모두 사용한 다음 제공된 템플릿에서
                // 추가로 만듭니다.
                RichTextBlockOverflow overflow;
                if (this._overflowColumns.Count > overflowIndex)
                {
                    overflow = this._overflowColumns[overflowIndex];
                }
                else
                {
                    overflow = (RichTextBlockOverflow)this.ColumnTemplate.LoadContent();
                    this._overflowColumns.Add(overflow);
                    this.Children.Add(overflow);
                    if (overflowIndex == 0)
                    {
                        this.RichTextContent.OverflowContentTarget = overflow;
                    }
                    else
                    {
                        this._overflowColumns[overflowIndex - 1].OverflowContentTarget = overflow;
                    }
                }

                // 새 열을 측정하고 준비하여 필요에 따라 반복합니다.
                overflow.Measure(new Size(availableSize.Width - maxWidth, availableSize.Height));
                maxWidth += overflow.DesiredSize.Width;
                maxHeight = Math.Max(maxHeight, overflow.DesiredSize.Height);
                hasOverflow = overflow.HasOverflowContent;
                overflowIndex++;
            }

            // 오버플로 체인에서 여분의 열에 대한 연결을 끊고 개인 열 목록에서 제공한 다음
            // 자식으로 제거합니다.
            if (this._overflowColumns.Count > overflowIndex)
            {
                if (overflowIndex == 0)
                {
                    this.RichTextContent.OverflowContentTarget = null;
                }
                else
                {
                    this._overflowColumns[overflowIndex - 1].OverflowContentTarget = null;
                }
                while (this._overflowColumns.Count > overflowIndex)
                {
                    this._overflowColumns.RemoveAt(overflowIndex);
                    this.Children.RemoveAt(overflowIndex + 1);
                }
            }

            // 최종적으로 결정된 크기를 보고합니다.
            return new Size(maxWidth, maxHeight);
        }

        /// <summary>
        /// 원본 콘텐츠 및 모든 여분의 열을 정렬합니다.
        /// </summary>
        /// <param name="finalSize">자식을 정렬해야 하는 영역 크기를
        /// 정의합니다.</param>
        /// <returns>자식이 실제로 필요한 영역 크기입니다.</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            double maxWidth = 0;
            double maxHeight = 0;
            foreach (var child in Children)
            {
                child.Arrange(new Rect(maxWidth, 0, child.DesiredSize.Width, finalSize.Height));
                maxWidth += child.DesiredSize.Width;
                maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
            }
            return new Size(maxWidth, maxHeight);
        }
    }
}
