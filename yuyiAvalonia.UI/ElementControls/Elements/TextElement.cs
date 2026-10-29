using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Elements
{

    /// <summary>
    /// 文本元素
    /// </summary>
    public partial class TextElement : CanvasElement
    {
        public TextElement() : base()
        {
            ElementType = ComponentType.Text;
            HorizontalAlignment = HorizontalAlignment.Center;
            VerticalAlignment = VerticalAlignment.Center;
            IsContainer = false;
        }


        [ObservableProperty]
        private string _text = "双击编辑文本";

        [ObservableProperty]
        private double _fontSize = 14;

        [ObservableProperty]
        private string _fontFamily = "Microsoft YaHei";

        [ObservableProperty]
        private bool _isBold;

        [ObservableProperty]
        private bool _isItalic;

        [ObservableProperty]
        private bool _isUnderline;

        [ObservableProperty]
        private bool _isStrikethrough;

        [ObservableProperty]
        private string _foreColor = "#000000";

        [ObservableProperty]
        private TextAlignment _textAlignment = TextAlignment.Center;

        /// <summary>
        /// 文本垂直对齐方式
        /// </summary>
        [ObservableProperty]
        private Avalonia.Layout.VerticalAlignment _verticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center;

        /// <summary>
        /// 文本垂直对齐索引（用于UI绑定）
        /// 0=顶部, 1=居中, 2=底部
        /// </summary>
        public int VerticalContentAlignmentIndex
        {
            get => VerticalContentAlignment switch
            {
                Avalonia.Layout.VerticalAlignment.Top => 0,
                Avalonia.Layout.VerticalAlignment.Center => 1,
                Avalonia.Layout.VerticalAlignment.Bottom => 2,
                _ => 1
            };
            set
            {
                VerticalContentAlignment = value switch
                {
                    0 => Avalonia.Layout.VerticalAlignment.Top,
                    1 => Avalonia.Layout.VerticalAlignment.Center,
                    2 => Avalonia.Layout.VerticalAlignment.Bottom,
                    _ => Avalonia.Layout.VerticalAlignment.Center
                };
                OnPropertyChanged(nameof(VerticalContentAlignment));
            }
        }

        [ObservableProperty]
        private double _lineHeight = double.NaN;

        [ObservableProperty]
        private double _letterSpacing = 0;

        #region 渐变文字属性

        [ObservableProperty]
        private bool _hasTextGradient = false;

        [ObservableProperty]
        private string _textGradientStartColor = "#00f5d4";

        [ObservableProperty]
        private string _textGradientEndColor = "#7b2fff";

        [ObservableProperty]
        private double _textGradientAngle = 0;

        #endregion


        public override CanvasElement Clone()
        {
            var element = (TextElement)base.Clone();

            element.Text = Text;
            element.FontSize = FontSize;
            element.FontFamily = FontFamily;
            element.IsBold = IsBold;
            element.IsItalic = IsItalic;
            element.IsUnderline = IsUnderline;
            element.IsStrikethrough = IsStrikethrough;
            element.ForeColor = ForeColor;
            element.TextAlignment = TextAlignment;
            element.VerticalContentAlignment = VerticalContentAlignment;
            element.LineHeight = LineHeight;
            element.LetterSpacing = LetterSpacing;
            element.HasTextGradient = HasTextGradient;
            element.TextGradientStartColor = TextGradientStartColor;
            element.TextGradientEndColor = TextGradientEndColor;
            element.TextGradientAngle = TextGradientAngle;

            return element;
        }

    }

}
