using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ViewModels
{
    public partial class CanvasProperty : ObservableObject
    {
        [ObservableProperty]
        private double _canvasWidth = 375;

        [ObservableProperty]
        private double _canvasHeight = 667;

        [ObservableProperty]
        private string _canvasBackgroundColor = "#F7F7F7";

        [ObservableProperty]
        private double _canvasOpacity = 1;

        [ObservableProperty]
        private string? _canvasBackgroundImage;

        [ObservableProperty]
        private bool _isGradientBackground;

        [ObservableProperty]
        private string _gradientStartColor = "#FFFFFF";

        [ObservableProperty]
        private string _gradientEndColor = "#E0E0E0";

        [ObservableProperty]
        private double _gradientAngle = 90;

        [ObservableProperty]
        private BackgroundType _fillType = BackgroundType.SolidColor;

        [ObservableProperty]
        private bool _enableBackgroundImage = false;

        #region 显示设置

        [ObservableProperty]
        private bool _showAllElementIds = false;

        partial void OnShowAllElementIdsChanged(bool value)
        {
            WhenShowAllElementIdsChanged?.Invoke(this, value);
        }

        public event EventHandler<bool>? WhenShowAllElementIdsChanged;

        [ObservableProperty]
        private bool _showRulers = true;

        partial void OnShowRulersChanged(bool value)
        {
            WhenShowRulersChanged?.Invoke(this, value);
        }

        public event EventHandler<bool>? WhenShowRulersChanged;

        [ObservableProperty]
        private bool _showGrid = true;

        partial void OnShowGridChanged(bool value)
        {
            WhenShowGridChanged?.Invoke(this, value);
        }

        public event EventHandler<bool>? WhenShowGridChanged;

        #endregion

        #region 背景图属性

        [ObservableProperty]
        private Stretch _backgroundImageStretch = Stretch.UniformToFill;

        [ObservableProperty]
        private TileMode _backgroundImageTileMode = TileMode.None;

        [ObservableProperty]
        private AlignmentX _backgroundImageAlignmentX = AlignmentX.Left;

        [ObservableProperty]
        private AlignmentY _backgroundImageAlignmentY = AlignmentY.Top;

        #endregion

        partial void OnEnableBackgroundImageChanged(bool value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnBackgroundImageStretchChanged(Stretch value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnBackgroundImageTileModeChanged(TileMode value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnBackgroundImageAlignmentXChanged(AlignmentX value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnBackgroundImageAlignmentYChanged(AlignmentY value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        public void OnSelectedCanvasSizeChanged(CanvasSize? value)
        {
            if (value != null)
            {
                this.CanvasWidth = value.Width;
                this.CanvasHeight = value.Height;
            }
        }

        partial void OnCanvasBackgroundColorChanged(string? oldValue, string newValue)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnIsGradientBackgroundChanged(bool value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnGradientStartColorChanged(string value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnGradientEndColorChanged(string value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnGradientAngleChanged(double value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnCanvasBackgroundImageChanged(string? value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        partial void OnCanvasOpacityChanged(double value)
        {
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? WhenBackgroundChanged;

        public event EventHandler<double>? WhenCanvasWidthChanged;
        partial void OnCanvasWidthChanged(double value)
        {
            WhenCanvasWidthChanged?.Invoke(this,value);
        }

        public event EventHandler<double>? WhenCanvasHeightChanged;
        partial void OnCanvasHeightChanged(double value)
        {
            WhenCanvasHeightChanged?.Invoke(this, value);
        }

        public event EventHandler<BackgroundType>? WhenFillTypeChanged;
        partial void OnFillTypeChanged(BackgroundType value)
        {
            WhenFillTypeChanged?.Invoke(this, value);
            WhenBackgroundChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 从另一个 CanvasProperty 复制属性值
        /// </summary>
        public void CopyFrom(CanvasProperty source)
        {
            if (source == null) return;

            CanvasWidth = source.CanvasWidth;
            CanvasHeight = source.CanvasHeight;
            CanvasBackgroundColor = source.CanvasBackgroundColor;
            CanvasOpacity = source.CanvasOpacity;
            CanvasBackgroundImage = source.CanvasBackgroundImage;
            EnableBackgroundImage = source.EnableBackgroundImage;
            IsGradientBackground = source.IsGradientBackground;
            GradientStartColor = source.GradientStartColor;
            GradientEndColor = source.GradientEndColor;
            GradientAngle = source.GradientAngle;
            FillType = source.FillType;
            BackgroundImageStretch = source.BackgroundImageStretch;
            BackgroundImageTileMode = source.BackgroundImageTileMode;
            BackgroundImageAlignmentX = source.BackgroundImageAlignmentX;
            BackgroundImageAlignmentY = source.BackgroundImageAlignmentY;
            ShowAllElementIds = source.ShowAllElementIds;
            ShowRulers = source.ShowRulers;
            ShowGrid = source.ShowGrid;
        }
    }
}
