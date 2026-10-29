using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ElementControls.Controls
{
    public class ButtonControl : BorderControlBase
    {
        public override TextBlock? Control
        {
            get { return control as TextBlock; }
        }

        public ButtonControl(ButtonElement element):base(element)
        {
            var lineHeight = double.IsNaN(element.LineHeight)
                ? double.NaN
                : element.FontSize * element.LineHeight;

            control = new TextBlock
            {
                Text = element.Text,
                FontSize = element.FontSize,
                Foreground = PropertyValidator.CreateColorBrush(element.ForeColor, Brushes.White),
                Background = Brushes.Transparent,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = element.VerticalContentAlignment,
                TextAlignment = element.TextAlignment,
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
            if (!string.IsNullOrWhiteSpace(element.FontFamily))
            {
                ((TextBlock)control).FontFamily = new FontFamily(element.FontFamily);
            }

            // 应用渐变文字
            ApplyTextGradient(element.HasTextGradient, element.TextGradientStartColor, element.TextGradientEndColor, element.TextGradientAngle);

            _contentPanel!.Children.Add(control);

            this.Child = _contentPanel;

            // 初始化描边控件
            InitStrokes();

        }

        public override void UpdateElement(CanvasElement element)
        {
            ButtonElement btnElement = (ButtonElement)element;
            base.UpdateElement(element);

            if (this.control != null && this.control is TextBlock textBlock)
            {
                textBlock.Background = Brushes.Transparent;

                this.UpdateText(btnElement.Text);
                this.UpdateFontSize(btnElement.FontSize);
                this.UpdateFontFamily(btnElement.FontFamily);
                this.UpdateForeColor(btnElement.ForeColor);

                this.UpdateTextAlignment(btnElement.TextAlignment);

                this.UpdateVerticalContentAlignment(btnElement.VerticalContentAlignment);

                this.UpdateFontWeight(btnElement.IsBold);
                this.UpdateFontStyle(btnElement.IsItalic);
                this.UpdateTextDecorations(btnElement.IsUnderline, btnElement.IsStrikethrough);

                this.UpdateLineHeight(btnElement.LineHeight, btnElement.FontSize);
                this.UpdateLetterSpacing(btnElement.LetterSpacing);

                // 更新渐变文字
                this.UpdateTextGradient(btnElement.HasTextGradient, btnElement.TextGradientStartColor, btnElement.TextGradientEndColor, btnElement.TextGradientAngle);
            }
        }

        public void UpdateText(string text)
        {
            if (this.control != null && this.control is TextBlock textBlock)
            {
                textBlock.Text = text ?? string.Empty;
            }
        }

        public void UpdateFontSize(double fontSize)
        {
            if (this.control != null && this.control is TextBlock textBlock)
            {
                textBlock.FontSize = PropertyValidator.ValidateFontSize(fontSize);
            }
        }

        public void UpdateFontFamily(string fontFamily)
        {
            if (this.control != null && this.control is TextBlock textBlock)
            {
                textBlock.FontFamily = new FontFamily(fontFamily);
            }
        }
        
        public void UpdateForeColor(string color)
        {
            if (this.control != null && this.control is TextBlock textBlock)
            {
                textBlock.Foreground = PropertyValidator.CreateColorBrush(color, Brushes.White);
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
                var element = Tag as ButtonElement;
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

        public void UpdateTextAlignment(TextAlignment textAlignment)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.TextAlignment = textAlignment;
            }
        }

        public void UpdateVerticalContentAlignment(Avalonia.Layout.VerticalAlignment alignment)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.VerticalAlignment = alignment;
            }
        }

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

        public void UpdateLetterSpacing(double letterSpacing)
        {
            if (control is TextBlock textBlock)
            {
                textBlock.LetterSpacing = letterSpacing;
            }
        }

        #region 渐变文字方法

        /// <summary>
        /// 应用渐变文字效果
        /// </summary>
        public void ApplyTextGradient(bool hasTextGradient, string startColor, string endColor, double angle)
        {
            if (control is TextBlock textBlock)
            {
                if (hasTextGradient)
                {
                    var start = Avalonia.Media.Color.Parse(startColor);
                    var end = Avalonia.Media.Color.Parse(endColor);

                    var radians = angle * Math.PI / 180;
                    var cos = Math.Cos(radians);
                    var sin = Math.Sin(radians);

                    var startX = (1 - cos) / 2;
                    var startY = (1 - sin) / 2;
                    var endX = 1 - startX;
                    var endY = 1 - startY;

                    textBlock.Foreground = new LinearGradientBrush
                    {
                        StartPoint = new RelativePoint(startX, startY, RelativeUnit.Relative),
                        EndPoint = new RelativePoint(endX, endY, RelativeUnit.Relative),
                        GradientStops = new GradientStops
                        {
                            new GradientStop(start, 0),
                            new GradientStop(end, 1)
                        }
                    };
                }
            }
        }

        /// <summary>
        /// 更新渐变文字
        /// </summary>
        public void UpdateTextGradient(bool hasTextGradient, string startColor, string endColor, double angle)
        {
            if (control is TextBlock textBlock)
            {
                if (hasTextGradient)
                {
                    ApplyTextGradient(true, startColor, endColor, angle);
                }
                else
                {
                    textBlock.Foreground = PropertyValidator.CreateColorBrush(((ButtonElement)Tag).ForeColor, Brushes.White);
                }
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
