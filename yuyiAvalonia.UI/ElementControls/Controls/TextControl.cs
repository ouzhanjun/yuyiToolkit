using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public class TextControl : BorderControlBase
    {

        public override TextBlock? Control
        {
            get { return control as TextBlock; }
        }

        public TextControl(TextElement element) : base(element)
        {
            var lineHeight = double.IsNaN(element.LineHeight)
                ? double.NaN
                : element.FontSize * element.LineHeight;

            control = new TextBlock
            {
                Text = element.Text,
                FontSize = element.FontSize,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = element.VerticalContentAlignment,
                TextAlignment = element.TextAlignment,
                Padding = new Thickness(element.FontSize * 0.3, 0),
                LineHeight = lineHeight,
                LetterSpacing = element.LetterSpacing > 0 ? element.LetterSpacing : 0
            };

            if (element.IsBold)
            {
                ((TextBlock)control).FontWeight = Avalonia.Media.FontWeight.Bold;
            }
            if (element.IsItalic)
            {
                ((TextBlock)control).FontStyle = Avalonia.Media.FontStyle.Italic;
            }
            if (element.IsUnderline)
            {
                ((TextBlock)control).TextDecorations = new TextDecorationCollection
                {
                    new TextDecoration { Location = TextDecorationLocation.Underline }
                };
            }
            if (element.IsStrikethrough)
            {
                ((TextBlock)control).TextDecorations = new TextDecorationCollection
                {
                    new TextDecoration { Location = TextDecorationLocation.Strikethrough }
                };
            }
            if (element.IsUnderline && element.IsStrikethrough)
            {
                ((TextBlock)control).TextDecorations = new TextDecorationCollection
                {
                    new TextDecoration { Location = TextDecorationLocation.Underline },
                    new TextDecoration { Location = TextDecorationLocation.Strikethrough }
                };
            }
            if(!string.IsNullOrWhiteSpace(element.FontFamily))
            {
                ((TextBlock)control).FontFamily = new FontFamily(element.FontFamily);
            }

            // 应用渐变文字或普通颜色
            if (element.HasTextGradient)
            {
                ApplyTextGradient(element);
            }
            else
            {
                ((TextBlock)control).Foreground = PropertyValidator.CreateColorBrush(element.ForeColor, Brushes.Black);
            }

            _contentPanel!.Children.Add(control);

            this.Child = _contentPanel;

            // 初始化描边控件
            InitStrokes();
        }

        public override void UpdateElement(CanvasElement element)
        {
            TextElement textElement = (TextElement)element;

            base.UpdateElement(textElement);

            if (control is TextBlock textBlock)
            {
                textBlock.Background = Brushes.Transparent;

                this.UpdateText(textElement.Text);

                this.UpdateFontSize(textElement.FontSize);

                this.UpdateFontWeight(textElement.IsBold);

                this.UpdateFontStyle(textElement.IsItalic);

                this.UpdateTextDecorations(textElement.IsUnderline, textElement.IsStrikethrough);

                this.UpdateFontFamily(textElement.FontFamily);

                this.UpdateTextAlignment(textElement.TextAlignment);

                this.UpdateVerticalContentAlignment(textElement.VerticalContentAlignment);

                this.UpdateLineHeight(textElement.LineHeight, textElement.FontSize);

                this.UpdateLetterSpacing(textElement.LetterSpacing);

                // 应用渐变或普通颜色
                if (textElement.HasTextGradient)
                {
                    ApplyTextGradient(textElement);
                }
                else
                {
                    textBlock.Foreground = PropertyValidator.CreateColorBrush(textElement.ForeColor, Brushes.Black);
                }

            }
        }

        #region 行间距和字间距

        /// <summary>
        /// 更新行高倍数
        /// </summary>
        public void UpdateLineHeight(double lineHeight, double fontSize)
        {
            if (control is TextBlock textBlock)
            {
                if (double.IsNaN(lineHeight))
                {
                    textBlock.LineHeight = double.NaN;
                }
                else
                {
                    textBlock.LineHeight = fontSize * lineHeight;
                }
            }
        }

        /// <summary>
        /// 更新字间距
        /// </summary>
        public void UpdateLetterSpacing(double letterSpacing)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.LetterSpacing = letterSpacing;
            }
        }

        #endregion

        // 单独更新属性方法
        public void UpdateText(string text)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.Text = text ?? string.Empty;
            }
        }

        public void UpdateFontSize(double fontSize)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.FontSize = PropertyValidator.ValidateFontSize(fontSize);
                textBlock.Padding = new Thickness(fontSize * 0.3, 0);
            }
        }

        public void UpdateForeColor(string color)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.Foreground = PropertyValidator.CreateColorBrush(color, Brushes.Black);
            }
        }

        public void UpdateFontWeight(bool isBold)

        {
            if (control is TextBlock textBlock)
            {
                textBlock.FontWeight = isBold ? Avalonia.Media.FontWeight.Bold : Avalonia.Media.FontWeight.Normal;
            }
        }

        public void UpdateFontStyle(bool isItalic)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.FontStyle = isItalic ? Avalonia.Media.FontStyle.Italic : Avalonia.Media.FontStyle.Normal;
            }
        }

        public void UpdateTextDecorations(bool isUnderline)
        {
            if (control is TextBlock textBlock)
            {
                var element = Tag as TextElement;
                bool isStrikethrough = element?.IsStrikethrough ?? false;

                if (isUnderline && isStrikethrough)
                {
                    textBlock.TextDecorations = new TextDecorationCollection
                    {
                        new TextDecoration { Location = TextDecorationLocation.Underline },
                        new TextDecoration { Location = TextDecorationLocation.Strikethrough }
                    };
                }
                else if (isUnderline)
                {
                    textBlock.TextDecorations = new TextDecorationCollection
                    {
                        new TextDecoration { Location = TextDecorationLocation.Underline }
                    };
                }
                else if (isStrikethrough)
                {
                    textBlock.TextDecorations = new TextDecorationCollection
                    {
                        new TextDecoration { Location = TextDecorationLocation.Strikethrough }
                    };
                }
                else
                {
                    textBlock.TextDecorations = null;
                }
            }
        }

        public void UpdateTextDecorations(bool isUnderline, bool isStrikethrough)
        {
            if (control is TextBlock textBlock)
            {
                if (isUnderline && isStrikethrough)
                {
                    textBlock.TextDecorations = new TextDecorationCollection
                    {
                        new TextDecoration { Location = TextDecorationLocation.Underline },
                        new TextDecoration { Location = TextDecorationLocation.Strikethrough }
                    };
                }
                else if (isUnderline)
                {
                    textBlock.TextDecorations = new TextDecorationCollection
                    {
                        new TextDecoration { Location = TextDecorationLocation.Underline }
                    };
                }
                else if (isStrikethrough)
                {
                    textBlock.TextDecorations = new TextDecorationCollection
                    {
                        new TextDecoration { Location = TextDecorationLocation.Strikethrough }
                    };
                }
                else
                {
                    textBlock.TextDecorations = null;
                }
            }
        }

        public void UpdateFontFamily(string fontFamily)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.FontFamily = new FontFamily(fontFamily);
            }
        }

        public void UpdateTextAlignment(TextAlignment alignment)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.TextAlignment = alignment;
            }
        }

        public void UpdateVerticalContentAlignment(Avalonia.Layout.VerticalAlignment alignment)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.VerticalAlignment = alignment;
            }
        }

        #region 文本渐变属性

        /// <summary>
        /// 应用文本渐变效果
        /// </summary>
        public void ApplyTextGradient(TextElement element)
        {
            if (control is TextBlock textBlock)
            {
                UpdateTextGradientBrush(textBlock, element);
            }
        }

        /// <summary>
        /// 更新文本渐变开关
        /// </summary>
        public void UpdateHasTextGradient(bool hasTextGradient, TextElement element)
        {
            if (control is TextBlock textBlock)
            {
                UpdateTextGradientBrush(textBlock, element);
            }
        }

        /// <summary>
        /// 更新文本渐变起始颜色
        /// </summary>
        public void UpdateTextGradientStartColor(string color, TextElement element)
        {
            if (control is TextBlock textBlock)
            {
                UpdateTextGradientBrush(textBlock, element);
            }
        }

        /// <summary>
        /// 更新文本渐变结束颜色
        /// </summary>
        public void UpdateTextGradientEndColor(string color, TextElement element)
        {
            if (control is TextBlock textBlock)
            {
                UpdateTextGradientBrush(textBlock, element);
            }
        }

        /// <summary>
        /// 更新文本渐变角度
        /// </summary>
        public void UpdateTextGradientAngle(double angle, TextElement element)
        {
            if (control is TextBlock textBlock)
            {
                UpdateTextGradientBrush(textBlock, element);
            }
        }

        /// <summary>
        /// 更新文本块渐变画刷
        /// </summary>
        private void UpdateTextGradientBrush(TextBlock textBlock, TextElement element)
        {
            if (element.HasTextGradient)
            {
                var gradientBrush = new LinearGradientBrush();
                gradientBrush.StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative);
                gradientBrush.EndPoint = new RelativePoint(1, 0, RelativeUnit.Relative);

                // 根据角度调整渐变方向
                double angleRad = element.TextGradientAngle * Math.PI / 180;
                gradientBrush.StartPoint = new RelativePoint(
                    (float)(0.5 - 0.5 * Math.Cos(angleRad)),
                    (float)(0.5 - 0.5 * Math.Sin(angleRad)),
                    RelativeUnit.Relative);
                gradientBrush.EndPoint = new RelativePoint(
                    (float)(0.5 + 0.5 * Math.Cos(angleRad)),
                    (float)(0.5 + 0.5 * Math.Sin(angleRad)),
                    RelativeUnit.Relative);

                gradientBrush.GradientStops.Add(new GradientStop
                {
                    Color = ParseColor(element.TextGradientStartColor, Colors.Black),
                    Offset = 0.0
                });
                gradientBrush.GradientStops.Add(new GradientStop
                {
                    Color = ParseColor(element.TextGradientEndColor, Colors.White),
                    Offset = 1.0
                });

                textBlock.Foreground = gradientBrush;
            }
            else
            {
                textBlock.Foreground = PropertyValidator.CreateColorBrush(element.ForeColor, Brushes.Black);
            }
        }

        /// <summary>
        /// 解析颜色字符串
        /// </summary>
        private static Color ParseColor(string? color, Color fallback)
        {
            if (string.IsNullOrEmpty(color))
                return fallback;

            try
            {
                // 支持 #RRGGBB 和 #AARRGGBB 格式
                if (color.StartsWith("#"))
                {
                    return Color.Parse(color);
                }
                // 其他格式
                return Color.Parse(color);
            }
            catch
            {
                return fallback;
            }
        }

        #endregion

        public override void ShowWorkspaceOverlay()
        {
        }

        public override void HideWorkspaceOverlay()
        {
        }
    }
}
