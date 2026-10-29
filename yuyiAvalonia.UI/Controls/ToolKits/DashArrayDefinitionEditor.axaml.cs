using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using yuyiAvalonia.UI.Helpers;

namespace yuyiAvalonia.UI.Controls
{
    /// <summary>
    /// 虚线数组编辑器 - 提供可视化编辑、自动生成和预览功能
    /// </summary>
    public partial class StrokeDashArrayEditor : UserControl
    {
        #region 依赖属性
        public static readonly StyledProperty<int> StrokeStyleIndexProperty =
       AvaloniaProperty.Register<StrokeDashArrayEditor, int>(
           nameof(StrokeStyleIndex),
           defaultValue: 0,
           defaultBindingMode: BindingMode.OneWay);

        public int StrokeStyleIndex
        {
            get => GetValue(StrokeStyleIndexProperty);
            set => SetValue(StrokeStyleIndexProperty, value);
        }


        /// <summary>
        /// 虚线数组定义值（格式如 "8,4" 或 "4,2,1,2"）
        /// </summary>
        public static readonly StyledProperty<string> DashArrayDefinitionProperty =
            AvaloniaProperty.Register<StrokeDashArrayEditor, string>(
                nameof(DashArrayDefinition),
                defaultValue: "8,4",
                defaultBindingMode: BindingMode.TwoWay);

        public string DashArrayDefinition
        {
            get => GetValue(DashArrayDefinitionProperty);
            set => SetValue(DashArrayDefinitionProperty, value);
        }

        /// <summary>
        /// 提示文本
        /// </summary>
        public static readonly StyledProperty<string> HintTextProperty =
            AvaloniaProperty.Register<StrokeDashArrayEditor, string>(
                nameof(HintText),
                defaultValue: "设置虚线数组",
                defaultBindingMode: BindingMode.OneWay);

        public string HintText
        {
            get => GetValue(HintTextProperty);
            set => SetValue(HintTextProperty, value);
        }

        /// <summary>
        /// 当前虚线数组的显示字符串
        /// </summary>
        public static readonly StyledProperty<string> CurrentDashArrayDisplayProperty =
            AvaloniaProperty.Register<StrokeDashArrayEditor, string>(
                nameof(CurrentDashArrayDisplay),
                defaultValue: "",
                defaultBindingMode: BindingMode.OneWay);

        public string CurrentDashArrayDisplay
        {
            get => GetValue(CurrentDashArrayDisplayProperty);
            set => SetValue(CurrentDashArrayDisplayProperty, value);
        }

        /// <summary>
        /// 用于预览的虚线数组
        /// </summary>
        public static readonly StyledProperty<AvaloniaList<double>> DashArrayForPreviewProperty =
            AvaloniaProperty.Register<StrokeDashArrayEditor, AvaloniaList<double>>(
                nameof(DashArrayForPreview),
                defaultValue: new AvaloniaList<double> { 8, 4 },
                defaultBindingMode: BindingMode.OneWay);

        public AvaloniaList<double> DashArrayForPreview
        {
            get => GetValue(DashArrayForPreviewProperty);
            set => SetValue(DashArrayForPreviewProperty, value);
        }

        #endregion

        #region 控件引用

        private TextBox? _dashArrayTextBox;
        private Button? _autoGenerateButton;
        private Button? _applyButton;
        private TextBlock? _errorTextBlock;
        private Line? _previewLine;

        #endregion

        #region 事件

        /// <summary>
        /// 应用事件 - 当用户点击应用按钮时触发
        /// </summary>
        public event EventHandler<string>? Applied;

        #endregion

        public StrokeDashArrayEditor()
        {
            InitializeComponent();

            _dashArrayTextBox = this.FindControl<TextBox>("PART_DashArrayTextBox");
            _autoGenerateButton = this.FindControl<Button>("PART_AutoGenerateButton");
            _applyButton = this.FindControl<Button>("PART_ApplyButton");
            _errorTextBlock = this.FindControl<TextBlock>("PART_ErrorTextBlock");
            _previewLine = this.FindControl<Line>("PART_PreviewLine");

            if (_autoGenerateButton != null)
            {
                _autoGenerateButton.Click += OnAutoGenerateClick;
            }

            if (_applyButton != null)
            {
                _applyButton.Click += OnApplyClick;
            }

            DashArrayDefinitionProperty.Changed.AddClassHandler<StrokeDashArrayEditor>((sender, e) =>
            {
                sender.SyncFromProperty();
            });

            StrokeStyleIndexProperty.Changed.AddClassHandler<StrokeDashArrayEditor>((sender, e) =>
            {
                // 当样式变化时，自动生成新的默认值
                if (sender.StrokeStyleIndex > 0)
                {
                    sender.DashArrayDefinition = GetDefaultDashArrayForStyle(sender.StrokeStyleIndex);
                }
            });

            Loaded += (s, e) =>
            {
                SyncFromProperty();
            };
        }

        /// <summary>
        /// 从属性同步到UI
        /// </summary>
        private void SyncFromProperty()
        {
            if (_dashArrayTextBox != null)
            {
                var currentText = _dashArrayTextBox.Text;
                if (currentText != DashArrayDefinition)
                {
                    _dashArrayTextBox.Text = DashArrayDefinition;
                }
            }
            UpdateDisplay();
            UpdatePreview();
            ValidateAndUpdateUI();
        }

        /// <summary>
        /// 更新当前值显示
        /// </summary>
        private void UpdateDisplay()
        {
            var def = DashArrayDefinition;
            if (string.IsNullOrWhiteSpace(def))
            {
                CurrentDashArrayDisplay = "(空)";
            }
            else
            {
                CurrentDashArrayDisplay = $"[{def}]";
            }
        }

        /// <summary>
        /// 更新预览
        /// </summary>
        private void UpdatePreview()
        {
            var parsed = ParseDashArray(DashArrayDefinition);
            DashArrayForPreview = new AvaloniaList<double>(parsed);
        }

        /// <summary>
        /// 自动生成按钮点击
        /// </summary>
        private void OnAutoGenerateClick(object? sender, RoutedEventArgs e)
        {
            var defaultDef = GetDefaultDashArrayForCurrentStyle();
            DashArrayDefinition = defaultDef;
        }

        /// <summary>
        /// 根据样式索引获取默认虚线数组
        /// </summary>
        public static string GetDefaultDashArrayForStyle(int styleIndex)
        {
            return styleIndex switch
            {
                0 => "",      // 实线
                1 => "8,4",   // 虚线
                2 => "2,4",   // 点线
                3 => "8,4,2,4", // 点画线
                4 => "8,4,2,4,2,4", // 双点画线
                _ => "8,4"
            };
        }

        /// <summary>
        /// 获取当前样式对应的默认虚线数组（子类可重写）
        /// </summary>
        protected virtual string GetDefaultDashArrayForCurrentStyle()
        {
            return "8,4";
        }

        /// <summary>
        /// 应用按钮点击 - 触发应用事件
        /// </summary>
        private void OnApplyClick(object? sender, RoutedEventArgs e)
        {
            if (_dashArrayTextBox != null)
            {
                DashArrayDefinition = _dashArrayTextBox.Text ?? string.Empty;
            }

            if (IsDefinitionValid())
            {
                Applied?.Invoke(this, DashArrayDefinition);
            }
        }

        /// <summary>
        /// 检查当前定义是否有效
        /// </summary>
        private bool IsDefinitionValid()
        {
            var (isValid, _) = ValidateDefinition(DashArrayDefinition);
            return isValid;
        }

        /// <summary>
        /// 解析虚线数组字符串
        /// </summary>
        public static double[] ParseDashArray(string? definition)
        {
            if (string.IsNullOrWhiteSpace(definition))
                return Array.Empty<double>();

            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var result = new List<double>();

            foreach (var part in parts)
            {
                if (double.TryParse(part.Trim(), out double value) && value >= 0)
                {
                    result.Add(value);
                }
            }

            return result.ToArray();
        }

        /// <summary>
        /// 验证定义字符串
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateDefinition(string? definition)
        {
            if (string.IsNullOrWhiteSpace(definition))
            {
                return (true, "(空 - 将使用默认虚线)");
            }

            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (parts.Length == 0)
            {
                return (true, "(空 - 将使用默认虚线)");
            }

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                if (!double.TryParse(part, out double value))
                {
                    return (false, $"第{i + 1}个值 '{part}' 不是有效数字");
                }
                if (value < 0)
                {
                    return (false, $"第{i + 1}个值不能为负数");
                }
                if (value > 1000)
                {
                    return (false, $"第{i + 1}个值不能超过1000");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 验证定义并更新UI
        /// </summary>
        private void ValidateAndUpdateUI()
        {
            var (isValid, errorMessage) = ValidateDefinition(DashArrayDefinition);

            if (_errorTextBlock != null)
            {
                _errorTextBlock.Text = errorMessage;
                _errorTextBlock.IsVisible = !isValid;
            }

            if (_dashArrayTextBox != null)
            {
                _dashArrayTextBox.BorderBrush = isValid
                    ? Brushes.Gray
                    : new SolidColorBrush(Color.Parse("#FF5555"));
            }
        }
    }
}
