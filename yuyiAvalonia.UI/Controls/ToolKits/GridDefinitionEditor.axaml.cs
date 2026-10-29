using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace yuyiAvalonia.UI.Controls
{
    /// <summary>
    /// 网格行列定义编辑器 - 提供可视化编辑和自动生成功能
    /// </summary>
    public partial class GridDefinitionEditor : UserControl
    {
        #region 依赖属性

        /// <summary>
        /// 数量属性（行数或列数）
        /// </summary>
        public static readonly StyledProperty<int> CountProperty =
            AvaloniaProperty.Register<GridDefinitionEditor, int>(
                nameof(Count),
                defaultValue: 3,
                defaultBindingMode: BindingMode.OneWay);

        public int Count
        {
            get => GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        /// <summary>
        /// 定义值属性（格式如 "*,*,*" 或 "2*,*,*"）
        /// </summary>
        public static readonly StyledProperty<string> DefinitionProperty =
            AvaloniaProperty.Register<GridDefinitionEditor, string>(
                nameof(Definition),
                defaultValue: string.Empty,
                defaultBindingMode: BindingMode.TwoWay);

        public string Definition
        {
            get => GetValue(DefinitionProperty);
            set => SetValue(DefinitionProperty, value);
        }

        /// <summary>
        /// 提示文本（用于区分行/列）
        /// </summary>
        public static readonly StyledProperty<string> HintTextProperty =
            AvaloniaProperty.Register<GridDefinitionEditor, string>(
                nameof(HintText),
                defaultValue: "设置各行/列比例",
                defaultBindingMode: BindingMode.OneWay);

        public string HintText
        {
            get => GetValue(HintTextProperty);
            set => SetValue(HintTextProperty, value);
        }

        #endregion

        #region 控件引用

        private TextBox? _definitionTextBox;
        private Button? _autoGenerateButton;
        private Button? _applyButton;
        private TextBlock? _errorTextBlock;

        #endregion

        #region 事件

        /// <summary>
        /// 应用事件 - 当用户点击应用按钮时触发
        /// </summary>
        public event EventHandler<string>? Applied;

        #endregion

        public GridDefinitionEditor()
        {
            InitializeComponent();

            _definitionTextBox = this.FindControl<TextBox>("PART_DefinitionTextBox");
            _autoGenerateButton = this.FindControl<Button>("PART_AutoGenerateButton");
            _applyButton = this.FindControl<Button>("PART_ApplyButton");
            _errorTextBlock = this.FindControl<TextBlock>("PART_ErrorTextBlock");

            // 自动生成按钮点击
            if (_autoGenerateButton != null)
            {
                _autoGenerateButton.Click += OnAutoGenerateClick;
            }

            // 应用按钮点击
            if (_applyButton != null)
            {
                _applyButton.Click += OnApplyClick;
            }

            // 监听 Definition 属性变化，确保 TextBox 正确同步
            DefinitionProperty.Changed.AddClassHandler<GridDefinitionEditor>((sender, e) =>
            {
                sender.SyncDefinitionToTextBox();
            });

            // 初始化时验证并更新UI
            ValidateAndUpdateUI();
        }

        /// <summary>
        /// 同步 Definition 到 TextBox
        /// </summary>
        private void SyncDefinitionToTextBox()
        {
            if (_definitionTextBox != null)
            {
                var currentText = _definitionTextBox.Text;
                if (currentText != Definition)
                {
                    _definitionTextBox.Text = Definition;
                }
            }
            ValidateAndUpdateUI();
        }

        /// <summary>
        /// 自动生成按钮点击
        /// </summary>
        private void OnAutoGenerateClick(object? sender, RoutedEventArgs e)
        {
            // 设置 Definition 会触发 DefinitionProperty.Changed 事件
            // 该事件会同步到 TextBox 并验证
            Definition = GenerateDefaultDefinition(Count);
        }

        /// <summary>
        /// 应用按钮点击 - 触发应用事件
        /// </summary>
        private void OnApplyClick(object? sender, RoutedEventArgs e)
        {
            // 先同步 TextBox 的值到 Definition
            if (_definitionTextBox != null)
            {
                Definition = _definitionTextBox.Text ?? string.Empty;
            }

            // 如果验证通过，触发应用事件
            if (IsDefinitionValid())
            {
                Applied?.Invoke(this, Definition);
            }
        }

        /// <summary>
        /// 检查当前定义是否有效
        /// </summary>
        private bool IsDefinitionValid()
        {
            var (isValid, _) = ValidateDefinition(Definition, Count);
            return isValid;
        }

        /// <summary>
        /// 生成默认定义字符串
        /// </summary>
        public string GenerateDefaultDefinition(int count)
        {
            if (count <= 0) return string.Empty;

            var parts = new string[count];
            for (int i = 0; i < count; i++)
            {
                parts[i] = "*";
            }
            return string.Join(",", parts);
        }

        /// <summary>
        /// 验证定义并更新UI
        /// </summary>
        private void ValidateAndUpdateUI()
        {
            var (isValid, errorMessage) = ValidateDefinition(Definition, Count);

            if (_errorTextBlock != null)
            {
                _errorTextBlock.Text = errorMessage;
                _errorTextBlock.IsVisible = !isValid;
            }

            if (_definitionTextBox != null)
            {
                _definitionTextBox.BorderBrush = isValid
                    ? Avalonia.Media.Brushes.Gray
                    : new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.Parse("#FF5555"));
            }
        }

        /// <summary>
        /// 验证定义字符串
        /// </summary>
        /// <param name="definition">定义字符串</param>
        /// <param name="expectedCount">期望的数量</param>
        /// <returns>(是否有效, 错误消息)</returns>
        public static (bool IsValid, string ErrorMessage) ValidateDefinition(string? definition, int expectedCount)
        {
            // 空定义视为有效（将使用默认平均分配）
            if (string.IsNullOrWhiteSpace(definition))
            {
                return (true, string.Empty);
            }

            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // 如果部分数量大于期望数量，给出警告但仍视为有效
            if (parts.Length > expectedCount)
            {
                return (true, $"警告: 定义数量({parts.Length})大于({expectedCount})，多余部分将被忽略");
            }

            // 验证每个部分
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                var (partValid, partError) = ValidatePart(part, i + 1);
                if (!partValid)
                {
                    return (false, $"第{i + 1}个定义错误: {partError}");
                }
            }

            return (true, string.Empty);
        }

        /// <summary>
        /// 验证单个定义部分
        /// </summary>
        private static (bool IsValid, string ErrorMessage) ValidatePart(string part, int index)
        {
            if (string.IsNullOrWhiteSpace(part))
            {
                return (false, "不能为空");
            }

            part = part.Trim();

            // * 或 1* - 平均分配
            if (part == "*" || part == "1*")
            {
                return (true, string.Empty);
            }

            // Auto - 自动尺寸
            if (part.Equals("Auto", StringComparison.OrdinalIgnoreCase))
            {
                return (true, string.Empty);
            }

            // N* 格式 - 比例值
            if (part.EndsWith("*"))
            {
                var numPart = part.TrimEnd('*').Trim();
                if (string.IsNullOrEmpty(numPart))
                {
                    return (true, string.Empty); // * 等同于 1*
                }
                if (double.TryParse(numPart, out double value))
                {
                    if (value <= 0)
                    {
                        return (false, "比例值必须大于0");
                    }
                    if (value > 1000)
                    {
                        return (false, "比例值不能超过1000");
                    }
                    return (true, string.Empty);
                }
                return (false, $"'{numPart}' 不是有效的数字");
            }

            // 纯数字 - 固定像素值
            if (double.TryParse(part, out double pixelValue))
            {
                if (pixelValue < 0)
                {
                    return (false, "像素值不能为负数");
                }
                if (pixelValue > 10000)
                {
                    return (false, "像素值不能超过10000");
                }
                return (true, string.Empty);
            }

            return (false, $"'{part}' 不是有效的定义格式");
        }

        /// <summary>
        /// 解析定义字符串，返回解析后的数量和定义数组
        /// </summary>
        public static (int Count, string[] Definitions) ParseDefinition(string? definition, int expectedCount)
        {
            if (string.IsNullOrWhiteSpace(definition))
            {
                // 返回默认的平均分配
                var defaultDefs = new string[expectedCount];
                for (int i = 0; i < expectedCount; i++)
                {
                    defaultDefs[i] = "*";
                }
                return (expectedCount, defaultDefs);
            }

            var parts = definition.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            // 清理和验证每个部分，无效部分替换为 *
            var validDefs = new List<string>();
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i].Trim();
                var (isValid, _) = ValidatePart(part, i + 1);

                if (isValid)
                {
                    validDefs.Add(NormalizePart(part));
                }
                else
                {
                    validDefs.Add("*"); // 无效部分替换为默认
                }
            }

            // 如果有效定义数量少于期望数量，补充 *
            while (validDefs.Count < expectedCount)
            {
                validDefs.Add("*");
            }

            return (validDefs.Count, validDefs.ToArray());
        }

        /// <summary>
        /// 标准化单个定义部分
        /// </summary>
        private static string NormalizePart(string part)
        {
            part = part.Trim();

            // * 或 1* 标准化为 *
            if (part == "*" || part == "1*")
            {
                return "*";
            }

            // Auto 标准化为首字母大写
            if (part.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return "Auto";
            }

            // N* 格式保留原样
            if (part.EndsWith("*"))
            {
                return part;
            }

            // 数字格式保留原样
            return part;
        }
    }
}
