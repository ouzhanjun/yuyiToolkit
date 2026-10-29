using Avalonia.Controls;
using ExCSS;
using yuyiAvalonia.UI.ElementControls;
using yuyiAvalonia.UI.ElementControls.Controls;
using yuyiAvalonia.UI.ElementControls.Elements;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Renders
{
    public class RenderControl
    {
        private RenderBackground _renderBackground;

        public RenderControl()
        {
            _renderBackground = new RenderBackground();
        }

        public void RenderBackground(Border? control, CanvasProperty canvasProperty)
        {
            if (control == null || canvasProperty == null)
            {
                return;
            }
            _renderBackground.UpdateBackground(control, canvasProperty);
        }

        public void RenderBackgroundImage(Border? control, CanvasProperty canvasProperty)
        {
            if (control == null || canvasProperty == null)
            {
                return;
            }
            _renderBackground.UpdateImageBackground(control, canvasProperty);
        }

        /// <summary>
        /// 处理画布属性变化
        /// </summary>
        public void OnCanvasPropertyChanged(
            string propertyName,
            CanvasProperty? canvasProperty,
            Action<CanvasProperty>? renderBackgroundAction,
            Action<bool>? updateAllElementsShowIdLabelAction,
            IUIControlCreator? controlCreator)
        {
            switch (propertyName)
            {
                case nameof(CanvasProperty.FillType):
                case nameof(CanvasProperty.CanvasBackgroundColor):
                case nameof(CanvasProperty.CanvasBackgroundImage):
                case nameof(CanvasProperty.IsGradientBackground):
                    renderBackgroundAction?.Invoke(canvasProperty!);
                    break;
                case nameof(CanvasProperty.ShowAllElementIds):
                    updateAllElementsShowIdLabelAction?.Invoke(canvasProperty!.ShowAllElementIds);
                    controlCreator!.ShowElementId = canvasProperty!.ShowAllElementIds;
                    break;
                case nameof(CanvasProperty.ShowRulers):
                    // 由 View 处理
                    break;
                case nameof(CanvasProperty.ShowGrid):
                    // 由 View 处理
                    break;
            }
        }

        /// <summary>
        /// 处理画布属性变化（重载版本，使用已有引用）
        /// </summary>
        public void OnCanvasPropertyChanged(
            Border? canvasBackgroundBorder,
            string propertyName,
            CanvasProperty? canvasProperty,
            Action<bool>? updateAllElementsShowIdLabelAction,
            IUIControlCreator? controlCreator)
        {
            OnCanvasPropertyChanged(
                propertyName,
                canvasProperty,
                cp => RenderBackground(canvasBackgroundBorder, cp),
                updateAllElementsShowIdLabelAction,
                controlCreator);
        }

        public void UpdateLeft(CanvasElement? element, Control? control)
        {
            if (control == null || element == null)
            {
                return;
            }

            if (control is IElementControlBase _control && control.Parent != null && control.Parent is Canvas)
            {
                _control.UpdateLeft(element!.Left);
            }
        }

        public void UpdateTop(CanvasElement? element, Control? control)
        {
            if (control == null || element == null)
            {
                return;
            }

            if (control is IElementControlBase _control && control.Parent != null && control.Parent is Canvas)
            {
                _control.UpdateTop(element!.Top);
            }
        }

        public void UpdateX(CanvasElement? element, Control? control)
        {
            if (control == null || element == null)
            {
                return;
            }

            if (control is IElementControlBase _control && control.Parent != null && control.Parent is Canvas)
            {
                _control.UpdateLeft(element!.X);
            }
        }

        public void UpdateY(CanvasElement? element, Control? control)
        {
            if (control == null || element == null)
            {
                return;
            }

            if (control is IElementControlBase _control && control.Parent != null && control.Parent is Canvas)
            {
                _control.UpdateTop(element!.Y);
            }
        }

        public void UpdateElementControl(CanvasElement? element, Control? control, string? propertyName)
        {
            if (control is not IElementControlBase contentBorder || element == null || string.IsNullOrEmpty(propertyName))
                return;

            //Task.Run(() =>
            //{
            //Dispatcher.UIThread.Post(() =>
            //{
            // 根据控件类型更新对应的属性
            switch (control)
            {
                case CardControl cardControl when element is CardElement cardElement:
                    UpdateCardElementControl(cardControl, cardElement, propertyName);
                    break;

                case ButtonControl buttonControl when element is ButtonElement buttonElement:
                    UpdateButtonElementControl(buttonControl, buttonElement, propertyName);
                    break;

                case TextControl textControl when element is TextElement textElement:
                    UpdateTextElementControl(textControl, textElement, propertyName);
                    break;

                case StackPanelControl stackPanelControl when element is StackPanelElement stackPanelElement:
                    UpdateStackPanelElementControl(stackPanelControl, stackPanelElement, propertyName);
                    break;

                case PanelControl panelControl when element is PanelElement panelElement:
                    UpdatePanelElementControl(panelControl, panelElement, propertyName);
                    break;

                case GridControl gridControl when element is GridElement gridElement:
                    UpdateGridElementControl(gridControl, gridElement, propertyName);
                    break;

                case CanvasContainerControl canvasControl when element is CanvasPanelElement canvasElement:
                    UpdateCanvasElementControl(canvasControl, canvasElement, propertyName);
                    break;

                case ImageControl imageControl when element is ImageElement imageElement:
                    UpdateImageElementControl(imageControl, imageElement, propertyName);
                    break;

                case RoundedPolygonControl roundedPolygonControl when element is ElementControls.Elements.RoundedPolygonElement polygonElement:
                    UpdateRoundedPolygonControl(roundedPolygonControl, polygonElement, propertyName);
                    break;

                case RoundedStarControl roundedStarControl when element is ElementControls.Elements.RoundedStarElement starElement:
                    UpdateRoundedStarControl(roundedStarControl, starElement, propertyName);
                    break;

                case RoundedRectangleControl roundedRectControl when element is ElementControls.Elements.RoundedRectangleElement rectElement:
                    UpdateRoundedRectangleControl(roundedRectControl, rectElement, propertyName);
                    break;

                case RoundEllipseControl sectorControl when element is ElementControls.Elements.RoundEllipseElement sectorElement:
                    UpdateSectorControl(sectorControl, sectorElement, propertyName);
                    break;

                case ShapeFlowControlBase shapeFlowControl when element is ShapeElement flowShapeElement:
                    UpdateShapeFlowControl(shapeFlowControl, flowShapeElement, propertyName);
                    break;

                default:
                    if (control is Border border)
                    {
                        UpdateCommonProperties(border, element, propertyName);
                    }
                    break;
            }

            //});
            //});
        }

        /// <summary>
        /// 更新TextElement控件属性
        /// </summary>
        public void UpdateTextElementControl(TextControl control, TextElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                control.UpdateElement(element);
                return;
            }

            switch (propertyName)
            {
                case nameof(TextElement.X):
                case nameof(TextElement.Y):
                case nameof(TextElement.Width):
                case nameof(TextElement.Height):
                case nameof(TextElement.Left):
                case nameof(TextElement.Top):
                case nameof(TextElement.Bottom):
                case nameof(TextElement.Right):
                case nameof(TextElement.Padding):
                case nameof(TextElement.Margin):
                case nameof(TextElement.PaddingLeft):
                case nameof(TextElement.PaddingTop):
                case nameof(TextElement.PaddingRight):
                case nameof(TextElement.PaddingBottom):
                case nameof(TextElement.MarginLeft):
                case nameof(TextElement.MarginTop):
                case nameof(TextElement.MarginRight):
                case nameof(TextElement.MarginBottom):
                case nameof(TextElement.PaddingIsUniform):
                case nameof(TextElement.MarginIsUniform):
                case nameof(TextElement.HorizontalAlignment):
                case nameof(TextElement.VerticalAlignment):
                case nameof(TextElement.ZIndex):
                case nameof(TextElement.IsVisible):
                    UpdateCommonProperties(control, element, propertyName);
                    break;

                case nameof(TextElement.Text):
                    control.UpdateText(element.Text);
                    break;
                case nameof(TextElement.FontSize):
                    control.UpdateFontSize(element.FontSize);
                    break;
                case nameof(TextElement.FontFamily):
                    control.UpdateFontFamily(element.FontFamily);
                    break;
                case nameof(TextElement.ForeColor):
                    control.UpdateForeColor(element.ForeColor);
                    break;
                case nameof(TextElement.BorderColor):
                    control.UpdateBorderColor(element.BorderColor);
                    break;
                case nameof(TextElement.BorderWidth):
                    control.UpdateBorderWidth(element.BorderWidth);
                    break;
                case nameof(TextElement.CornerRadius):
                    control.UpdateCornerRadius(element.CornerRadius);
                    break;
                case nameof(TextElement.IsBold):
                    control.UpdateFontWeight(element.IsBold);
                    break;
                case nameof(TextElement.IsItalic):
                    control.UpdateFontStyle(element.IsItalic);
                    break;
                case nameof(TextElement.IsUnderline):
                    control.UpdateTextDecorations(element.IsUnderline, element.IsStrikethrough);
                    break;
                case nameof(TextElement.IsStrikethrough):
                    control.UpdateTextDecorations(element.IsUnderline, element.IsStrikethrough);
                    break;
                case nameof(TextElement.TextAlignment):
                    control.UpdateTextAlignment(element.TextAlignment);
                    break;
                case nameof(TextElement.VerticalContentAlignment):
                    control.UpdateVerticalContentAlignment(element.VerticalContentAlignment);
                    break;
                case nameof(TextElement.LineHeight):
                    control.UpdateLineHeight(element.LineHeight, element.FontSize);
                    break;
                case nameof(TextElement.LetterSpacing):
                    control.UpdateLetterSpacing(element.LetterSpacing);
                    break;

                case nameof(TextElement.BackgroundFill):
                case nameof(TextElement.BackgroundFills):
                    control.ApplyMultiFill(element);
                    break;
                case nameof(TextElement.Opacity):
                    control.UpdateOpacity(element.Opacity);
                    break;

                // 渐变文字属性
                case nameof(TextElement.HasTextGradient):
                    control.UpdateHasTextGradient(element.HasTextGradient, element);
                    break;
                case nameof(TextElement.TextGradientStartColor):
                    control.UpdateTextGradientStartColor(element.TextGradientStartColor, element);
                    break;
                case nameof(TextElement.TextGradientEndColor):
                    control.UpdateTextGradientEndColor(element.TextGradientEndColor, element);
                    break;
                case nameof(TextElement.TextGradientAngle):
                    control.UpdateTextGradientAngle(element.TextGradientAngle, element);
                    break;

                // Id标签属性
                case nameof(TextElement.ShowIdLabel):
                case nameof(TextElement.Id):
                    control.UpdateIdLabel(element);
                    break;

                default:
                    UpdateCommonProperties(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新CardElement控件属性
        /// </summary>
        public void UpdateCardElementControl(CardControl control, CardElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                control.UpdateElement(element);
                return;
            }

            switch (propertyName)
            {
                case nameof(CardElement.X):
                case nameof(CardElement.Y):
                case nameof(CardElement.Left):
                case nameof(CardElement.Top):
                case nameof(CardElement.Bottom):
                case nameof(CardElement.Right):
                case nameof(CardElement.Width):
                case nameof(CardElement.Height):
                case nameof(CardElement.Padding):
                case nameof(CardElement.Margin):
                case nameof(CardElement.PaddingLeft):
                case nameof(CardElement.PaddingTop):
                case nameof(CardElement.PaddingRight):
                case nameof(CardElement.PaddingBottom):
                case nameof(CardElement.MarginLeft):
                case nameof(CardElement.MarginTop):
                case nameof(CardElement.MarginRight):
                case nameof(CardElement.MarginBottom):
                case nameof(CardElement.PaddingIsUniform):
                case nameof(CardElement.MarginIsUniform):
                case nameof(CardElement.HorizontalAlignment):
                case nameof(CardElement.VerticalAlignment):
                case nameof(CardElement.ZIndex):
                case nameof(CardElement.IsVisible):
                    UpdateCommonProperties(control, element, propertyName);
                    break;

                case nameof(CardElement.BorderColor):
                    control.UpdateBorderColor(element.BorderColor);
                    break;
                case nameof(CardElement.BorderWidth):
                    control.UpdateBorderWidth(element.BorderWidth);
                    break;
                case nameof(CardElement.CornerRadius):
                    control.UpdateCornerRadius(element.CornerRadius);
                    break;

                case nameof(CardElement.BackgroundFill):
                case nameof(CardElement.BackgroundFills):
                    control.ApplyMultiFill(element);
                    break;
                case nameof(CardElement.Spacing):
                    control.UpdateSpacing(element.Spacing);
                    break;
                case nameof(CardElement.Opacity):
                    control.UpdateOpacity(element.Opacity);
                    break;

                // Id标签属性
                case nameof(CardElement.ShowIdLabel):
                case nameof(CardElement.Id):
                    control.UpdateIdLabel(element);
                    break;

                default:
                    UpdateCommonProperties(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新ButtonElement控件属性
        /// </summary>
        public void UpdateButtonElementControl(ButtonControl control, ButtonElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                control.UpdateElement(element);
                return;
            }

            switch (propertyName)
            {

                case nameof(ButtonElement.X):
                case nameof(ButtonElement.Y):
                case nameof(ButtonElement.Width):
                case nameof(ButtonElement.Height):
                case nameof(ButtonElement.Top):
                case nameof(ButtonElement.Bottom):
                case nameof(ButtonElement.Left):
                case nameof(ButtonElement.Right):
                case nameof(ButtonElement.Padding):
                case nameof(ButtonElement.Margin):
                case nameof(ButtonElement.PaddingLeft):
                case nameof(ButtonElement.PaddingTop):
                case nameof(ButtonElement.PaddingRight):
                case nameof(ButtonElement.PaddingBottom):
                case nameof(ButtonElement.MarginLeft):
                case nameof(ButtonElement.MarginTop):
                case nameof(ButtonElement.MarginRight):
                case nameof(ButtonElement.MarginBottom):
                case nameof(ButtonElement.PaddingIsUniform):
                case nameof(ButtonElement.MarginIsUniform):
                case nameof(ButtonElement.HorizontalAlignment):
                case nameof(ButtonElement.VerticalAlignment):
                case nameof(ButtonElement.ZIndex):
                case nameof(ButtonElement.IsVisible):
                    UpdateCommonProperties(control, element, propertyName);
                    break;

                case nameof(ButtonElement.Text):
                    control.UpdateText(element.Text);
                    break;
                case nameof(ButtonElement.FontSize):
                    control.UpdateFontSize(element.FontSize);
                    break;
                case nameof(ButtonElement.FontFamily):
                    control.UpdateFontFamily(element.FontFamily);
                    break;
                case nameof(ButtonElement.ForeColor):
                    control.UpdateForeColor(element.ForeColor);
                    break;
                case nameof(ButtonElement.BorderColor):
                    control.UpdateBorderColor(element.BorderColor);
                    break;
                case nameof(ButtonElement.BorderWidth):
                    control.UpdateBorderWidth(element.BorderWidth);
                    break;
                case nameof(ButtonElement.CornerRadius):
                    control.UpdateCornerRadius(element.CornerRadius);
                    break;
                case nameof(ButtonElement.IsBold):
                    control.UpdateFontWeight(element.IsBold);
                    break;
                case nameof(ButtonElement.IsItalic):
                    control.UpdateFontStyle(element.IsItalic);
                    break;
                case nameof(ButtonElement.IsUnderline):
                    control.UpdateTextDecorations(element.IsUnderline, element.IsStrikethrough);
                    break;
                case nameof(ButtonElement.IsStrikethrough):
                    control.UpdateTextDecorations(element.IsUnderline, element.IsStrikethrough);
                    break;
                case nameof(ButtonElement.TextAlignment):
                    control.UpdateTextAlignment(element.TextAlignment);
                    break;
                case nameof(ButtonElement.VerticalContentAlignment):
                    control.UpdateVerticalContentAlignment(element.VerticalContentAlignment);
                    break;
                case nameof(ButtonElement.LineHeight):
                    control.UpdateLineHeight(element.LineHeight, element.FontSize);
                    break;
                case nameof(ButtonElement.LetterSpacing):
                    control.UpdateLetterSpacing(element.LetterSpacing);
                    break;
                case nameof(ButtonElement.BackgroundFill):
                case nameof(ButtonElement.BackgroundFills):
                    control.ApplyMultiFill(element);
                    break;

                // 渐变文字属性
                case nameof(ButtonElement.HasTextGradient):
                case nameof(ButtonElement.TextGradientStartColor):
                case nameof(ButtonElement.TextGradientEndColor):
                case nameof(ButtonElement.TextGradientAngle):
                    control.UpdateTextGradient(element.HasTextGradient, element.TextGradientStartColor, element.TextGradientEndColor, element.TextGradientAngle);
                    break;

                // Id标签属性
                case nameof(ButtonElement.ShowIdLabel):
                case nameof(ButtonElement.Id):
                    control.UpdateIdLabel(element);
                    break;

                default:
                    UpdateCommonProperties(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新StackPanelElement控件属性
        /// </summary>
        public void UpdateStackPanelElementControl(StackPanelControl control, StackPanelElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                control.UpdateElement(element);
                return;
            }

            switch (propertyName)
            {
                case nameof(StackPanelElement.X):
                case nameof(StackPanelElement.Y):
                case nameof(StackPanelElement.Width):
                case nameof(StackPanelElement.Height):
                case nameof(StackPanelElement.Top):
                case nameof(StackPanelElement.Bottom):
                case nameof(StackPanelElement.Left):
                case nameof(StackPanelElement.Right):
                case nameof(StackPanelElement.Padding):
                case nameof(StackPanelElement.Margin):
                case nameof(StackPanelElement.PaddingLeft):
                case nameof(StackPanelElement.PaddingTop):
                case nameof(StackPanelElement.PaddingRight):
                case nameof(StackPanelElement.PaddingBottom):
                case nameof(StackPanelElement.MarginLeft):
                case nameof(StackPanelElement.MarginTop):
                case nameof(StackPanelElement.MarginRight):
                case nameof(StackPanelElement.MarginBottom):
                case nameof(StackPanelElement.PaddingIsUniform):
                case nameof(StackPanelElement.MarginIsUniform):
                case nameof(StackPanelElement.HorizontalAlignment):
                case nameof(StackPanelElement.VerticalAlignment):
                case nameof(StackPanelElement.ZIndex):
                case nameof(StackPanelElement.IsVisible):
                    UpdateCommonProperties(control, element, propertyName);
                    break;

                case nameof(StackPanelElement.BorderColor):
                    control.UpdateBorderColor(element.BorderColor);
                    break;
                case nameof(StackPanelElement.BorderWidth):
                    control.UpdateBorderWidth(element.BorderWidth);
                    break;
                case nameof(StackPanelElement.CornerRadius):
                    control.UpdateCornerRadius(element.CornerRadius);
                    break;
                case nameof(StackPanelElement.Orientation):
                    control.UpdateOrientation(element.Orientation);
                    break;
                case nameof(StackPanelElement.Spacing):
                    control.UpdateSpacing(element.Spacing);
                    break;

                case nameof(StackPanelElement.BackgroundFill):
                case nameof(StackPanelElement.BackgroundFills):
                    control.ApplyMultiFill(element);
                    break;
                case nameof(StackPanelElement.Opacity):
                    control.UpdateOpacity(element.Opacity);
                    break;

                // Id标签属性
                case nameof(StackPanelElement.ShowIdLabel):
                case nameof(StackPanelElement.Id):
                    control.UpdateIdLabel(element);
                    break;

                default:
                    UpdateCommonProperties(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新PanelElement控件属性
        /// </summary>
        public void UpdatePanelElementControl(PanelControl control, PanelElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                control.UpdateElement(element);
                return;
            }

            switch (propertyName)
            {
                case nameof(PanelElement.X):
                case nameof(PanelElement.Y):
                case nameof(PanelElement.Width):
                case nameof(PanelElement.Height):
                case nameof(PanelElement.Top):
                case nameof(PanelElement.Bottom):
                case nameof(PanelElement.Left):
                case nameof(PanelElement.Right):
                case nameof(PanelElement.Padding):
                case nameof(PanelElement.Margin):
                case nameof(PanelElement.PaddingLeft):
                case nameof(PanelElement.PaddingTop):
                case nameof(PanelElement.PaddingRight):
                case nameof(PanelElement.PaddingBottom):
                case nameof(PanelElement.MarginLeft):
                case nameof(PanelElement.MarginTop):
                case nameof(PanelElement.MarginRight):
                case nameof(PanelElement.MarginBottom):
                case nameof(PanelElement.PaddingIsUniform):
                case nameof(PanelElement.MarginIsUniform):
                case nameof(PanelElement.HorizontalAlignment):
                case nameof(PanelElement.VerticalAlignment):
                case nameof(PanelElement.ZIndex):
                case nameof(PanelElement.IsVisible):
                    UpdateCommonProperties(control, element, propertyName);
                    break;

                case nameof(PanelElement.BorderColor):
                    control.UpdateBorderColor(element.BorderColor);
                    break;
                case nameof(PanelElement.BorderWidth):
                    control.UpdateBorderWidth(element.BorderWidth);
                    break;
                case nameof(PanelElement.CornerRadius):
                    control.UpdateCornerRadius(element.CornerRadius);
                    break;

                case nameof(PanelElement.BackgroundFill):
                case nameof(PanelElement.BackgroundFills):
                    control.ApplyMultiFill(element);
                    break;
                case nameof(PanelElement.Opacity):
                    control.UpdateOpacity(element.Opacity);
                    break;

                // Id标签属性
                case nameof(PanelElement.ShowIdLabel):
                case nameof(PanelElement.Id):
                    control.UpdateIdLabel(element);
                    break;

                default:
                    UpdateCommonProperties(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新所有元素类型的公共属性
        /// </summary>
        public void UpdateCommonProperties(Border border, CanvasElement element, string? propertyName)
        {
            if (propertyName == null) return;

            if (border is BorderControlBase control)
            {
                switch (propertyName)
                {
                    case nameof(CanvasElement.Left):
                        if (border.Parent != null && border.Parent is Canvas)
                        {
                            control.UpdateLeft(element.Left);
                        }
                        break;
                    case nameof(CanvasElement.Top):
                        if (border.Parent != null && border.Parent is Canvas)
                        {
                            control.UpdateTop(element.Top);
                        }
                        break;
                    case nameof(CanvasElement.Bottom):
                        if (border.Parent != null && border.Parent is Canvas)
                        {
                            //control.UpdateBottom(element.Bottom);
                        }
                        break;
                    case nameof(CanvasElement.Right):
                        if (border.Parent != null && border.Parent is Canvas)
                        {
                            //control.UpdateRight(element.Right);
                        }
                        break;
                    case nameof(CanvasElement.X):
                        UpdateElementPosition(border, element);
                        break;
                    case nameof(CanvasElement.Y):
                        UpdateElementPosition(border, element);
                        break;
                    case nameof(CanvasElement.IsVisible):
                        control.UpdateIsVisible(element.IsVisible);
                        break;
                    case nameof(CanvasElement.Width):
                        control.UpdateWidth(element.Width);
                        break;
                    case nameof(CanvasElement.Height):
                        control.UpdateHeight(element.Height);
                        break;
                    case nameof(CanvasElement.Padding):
                    case nameof(CanvasElement.PaddingLeft):
                    case nameof(CanvasElement.PaddingTop):
                    case nameof(CanvasElement.PaddingRight):
                    case nameof(CanvasElement.PaddingBottom):
                    case nameof(CanvasElement.PaddingIsUniform):
                        control.UpdatePadding(element);
                        break;
                    case nameof(CanvasElement.Margin):
                    case nameof(CanvasElement.MarginLeft):
                    case nameof(CanvasElement.MarginTop):
                    case nameof(CanvasElement.MarginRight):
                    case nameof(CanvasElement.MarginBottom):
                    case nameof(CanvasElement.MarginIsUniform):
                        control.UpdateMargin(element);
                        break;
                    //case nameof(CanvasElement.FixBottom):
                    //case nameof(CanvasElement.FixLeft):
                    //case nameof(CanvasElement.FixRight):
                    //case nameof(CanvasElement.FixTop):
                    //    break;
                    case nameof(CanvasElement.HorizontalAlignment):
                        control.UpdateHorizontalAlignment(element);
                        break;
                    case nameof(CanvasElement.VerticalAlignment):
                        control.UpdateVerticalAlignment(element);
                        break;
                    case nameof(CanvasElement.ZIndex):
                        control.UpdateZIndex(element.ZIndex);
                        break;
                    case nameof(CanvasElement.Opacity):
                        control.UpdateOpacity(element.Opacity);
                        break;
                    case nameof(CanvasElement.BackgroundFill):
                    case nameof(CanvasElement.BackgroundFills):
                        control.ApplyMultiFill(element);
                        break;

                    // 边线属性
                    case nameof(CanvasElement.StrokeStyle):
                    case nameof(CanvasElement.StrokeStyleIndex):
                    case nameof(CanvasElement.StrokeFillType):
                    case nameof(CanvasElement.StrokeColor):
                    case nameof(CanvasElement.StrokeWidth):
                    case nameof(CanvasElement.StrokeOpacity):
                    //case nameof(CanvasElement.StrokeGradientStartColor):
                    //case nameof(CanvasElement.StrokeGradientEndColor):
                    //case nameof(CanvasElement.StrokeGradientAngle):
                    case nameof(CanvasElement.StrokeWidthMode):
                    case nameof(CanvasElement.StrokeWidthLeft):
                    case nameof(CanvasElement.StrokeWidthTop):
                    case nameof(CanvasElement.StrokeWidthRight):
                    case nameof(CanvasElement.StrokeWidthBottom):
                    //case nameof(CanvasElement.StrokeImage):
                    case nameof(CanvasElement.StrokeDashOffset):
                    case nameof(CanvasElement.StrokeDashArrayString):
                    //case nameof(CanvasElement.StrokeImageStretchIndex):
                    //case nameof(CanvasElement.StrokeImageTileModeIndex):
                    case nameof(CanvasElement.CornerRadius):
                    case nameof(CanvasElement.StrokeJoin):
                    case nameof(CanvasElement.StrokeEndpoint):
                    case nameof(CanvasElement.StrokePosition):
                        control.UpdateStroke(element);
                        break;
                    case nameof(CanvasElement.StrokeMiterLimit):
                        control.UpdateStroke(element);
                        break;

                    // 旋转属性
                    case nameof(CanvasElement.Rotation):
                        control.UpdateRotation(element.Rotation);
                        break;

                    // Id标签属性
                    case nameof(CanvasElement.ShowIdLabel):
                    case nameof(CanvasElement.Id):
                        control.UpdateIdLabel(element);
                        break;
                }
            }
        }

        /// <summary>
        /// 更新元素位置（X, Y）- 同步更新 Canvas 面板的附加属性
        /// </summary>
        public void UpdateElementPosition(Border border, CanvasElement element)
        {
            if (border != null && border.Parent is Canvas)
            {
                Canvas.SetLeft(border, element.X);
                Canvas.SetTop(border, element.Y);
            }
        }

        /// <summary>
        /// 更新GridElement控件属性
        /// </summary>
        public void UpdateGridElementControl(GridControl control, GridElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                control.UpdateElement(element);
                return;
            }

            switch (propertyName)
            {
                case nameof(GridElement.X):
                case nameof(GridElement.Y):
                case nameof(GridElement.Left):
                case nameof(GridElement.Top):
                case nameof(GridElement.Right):
                case nameof(GridElement.Bottom):
                case nameof(GridElement.Width):
                case nameof(GridElement.Height):
                case nameof(GridElement.Padding):
                case nameof(GridElement.Margin):
                case nameof(GridElement.PaddingLeft):
                case nameof(GridElement.PaddingTop):
                case nameof(GridElement.PaddingRight):
                case nameof(GridElement.PaddingBottom):
                case nameof(GridElement.MarginLeft):
                case nameof(GridElement.MarginTop):
                case nameof(GridElement.MarginRight):
                case nameof(GridElement.MarginBottom):
                case nameof(GridElement.PaddingIsUniform):
                case nameof(GridElement.MarginIsUniform):
                case nameof(GridElement.HorizontalAlignment):
                case nameof(GridElement.VerticalAlignment):
                case nameof(GridElement.ZIndex):
                case nameof(GridElement.IsVisible):
                    UpdateCommonProperties(control, element, propertyName);
                    break;

                case nameof(GridElement.BorderColor):
                    control.UpdateBorderColor(element.BorderColor);
                    break;
                case nameof(GridElement.BorderWidth):
                    control.UpdateBorderWidth(element.BorderWidth);
                    break;
                case nameof(GridElement.CornerRadius):
                    control.UpdateCornerRadius(element.CornerRadius);
                    break;
                case nameof(GridElement.Rows):
                    control.UpdateRows(element.Rows);
                    break;
                case nameof(GridElement.Columns):
                    control.UpdateColumns(element.Columns);
                    break;
                case nameof(GridElement.RowSpacing):
                    control.UpdateRowSpacing(element.RowSpacing);
                    break;
                case nameof(GridElement.ColumnSpacing):
                    control.UpdateColumnSpacing(element.ColumnSpacing);
                    break;
                case nameof(GridElement.RowDefinitions):
                    control.UpdateRowDefinitions(element);
                    break;
                case nameof(GridElement.ColumnDefinitions):
                    control.UpdateColumnDefinitions(element);
                    break;

                case nameof(GridElement.BackgroundFill):
                case nameof(GridElement.BackgroundFills):
                    control.ApplyMultiFill(element);
                    break;
                case nameof(GridElement.Opacity):
                    control.UpdateOpacity(element.Opacity);
                    break;

                // Id标签属性
                case nameof(GridElement.ShowIdLabel):
                case nameof(GridElement.Id):
                    control.UpdateIdLabel(element);
                    break;

                default:
                    UpdateCommonProperties(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新Canvas容器控件属性
        /// </summary>
        public void UpdateCanvasElementControl(CanvasContainerControl control, CanvasElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                control.UpdateElement(element);
                return;
            }

            switch (propertyName)
            {
                case nameof(CanvasElement.X):
                case nameof(CanvasElement.Y):
                case nameof(CanvasElement.Left):
                case nameof(CanvasElement.Top):
                case nameof(CanvasElement.Right):
                case nameof(CanvasElement.Bottom):
                case nameof(CanvasElement.Width):
                case nameof(CanvasElement.Height):
                case nameof(CanvasElement.Padding):
                case nameof(CanvasElement.Margin):
                case nameof(CanvasElement.PaddingLeft):
                case nameof(CanvasElement.PaddingTop):
                case nameof(CanvasElement.PaddingRight):
                case nameof(CanvasElement.PaddingBottom):
                case nameof(CanvasElement.MarginLeft):
                case nameof(CanvasElement.MarginTop):
                case nameof(CanvasElement.MarginRight):
                case nameof(CanvasElement.MarginBottom):
                case nameof(CanvasElement.PaddingIsUniform):
                case nameof(CanvasElement.MarginIsUniform):
                case nameof(CanvasElement.HorizontalAlignment):
                case nameof(CanvasElement.VerticalAlignment):
                case nameof(CanvasElement.ZIndex):
                case nameof(CanvasElement.IsVisible):
                    UpdateCommonProperties(control, element, propertyName);
                    break;

                case nameof(CanvasElement.BorderColor):
                    control.UpdateBorderColor(element.BorderColor);
                    break;
                case nameof(CanvasElement.BorderWidth):
                    control.UpdateBorderWidth(element.BorderWidth);
                    break;
                case nameof(CanvasElement.CornerRadius):
                    control.UpdateCornerRadius(element.CornerRadius);
                    break;

                case "FillType":
                case "BackgroundColor":
                case "GradientStartColor":
                case "GradientEndColor":
                case "GradientAngle":
                case "BackgroundImage":
                case "EnableBackgroundImage":
                case "Fills":
                case "BackgroundFill":
                    control.ApplyMultiFill(element);
                    break;

                case nameof(CanvasElement.Opacity):
                    control.UpdateOpacity(element.Opacity);
                    break;

                // Id标签属性
                case nameof(CanvasElement.ShowIdLabel):
                case nameof(CanvasElement.Id):
                    control.UpdateIdLabel(element);
                    break;

                default:
                    UpdateCommonProperties(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新ImageElement控件属性
        /// </summary>
        public void UpdateImageElementControl(ImageControl control, ImageElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                control.UpdateElement(element);
                return;
            }

            switch (propertyName)
            {
                case nameof(ImageElement.X):
                case nameof(ImageElement.Y):
                case nameof(ImageElement.Left):
                case nameof(ImageElement.Top):
                case nameof(ImageElement.Right):
                case nameof(ImageElement.Bottom):
                case nameof(ImageElement.Width):
                case nameof(ImageElement.Height):
                case nameof(ImageElement.Padding):
                case nameof(ImageElement.Margin):
                case nameof(ImageElement.PaddingLeft):
                case nameof(ImageElement.PaddingTop):
                case nameof(ImageElement.PaddingRight):
                case nameof(ImageElement.PaddingBottom):
                case nameof(ImageElement.MarginLeft):
                case nameof(ImageElement.MarginTop):
                case nameof(ImageElement.MarginRight):
                case nameof(ImageElement.MarginBottom):
                case nameof(ImageElement.PaddingIsUniform):
                case nameof(ImageElement.MarginIsUniform):
                case nameof(ImageElement.HorizontalAlignment):
                case nameof(ImageElement.VerticalAlignment):
                case nameof(ImageElement.ZIndex):
                case nameof(ImageElement.IsVisible):
                    UpdateCommonProperties(control, element, propertyName);
                    break;

                case nameof(ImageElement.ImagePath):
                    control.UpdateImagePath(element.ImagePath);
                    break;
                case nameof(ImageElement.ImageStretch):
                    control.UpdateImageStretch(element.ImageStretch);
                    break;
                case nameof(ImageElement.ImageStretchDirection):
                    control.UpdateImageStretchDirection(element.ImageStretchDirection);
                    break;

                case nameof(ImageElement.BackgroundFill):
                case nameof(ImageElement.BackgroundFills):
                    control.ApplyMultiFill(element);
                    break;
                case nameof(ImageElement.BorderColor):
                    control.UpdateBorderColor(element.BorderColor);
                    break;
                case nameof(ImageElement.BorderWidth):
                    control.UpdateBorderWidth(element.BorderWidth);
                    break;
                case nameof(ImageElement.CornerRadius):
                    control.UpdateCornerRadius(element.CornerRadius);
                    break;
                case nameof(ImageElement.Opacity):
                    control.UpdateOpacity(element.Opacity);
                    break;

                // Id标签属性
                case nameof(ImageElement.ShowIdLabel):
                case nameof(ImageElement.Id):
                    control.UpdateIdLabel(element);
                    break;

                default:
                    UpdateCommonProperties(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新圆角矩形控件属性
        /// </summary>
        public void UpdateRoundedRectangleControl(RoundedRectangleControl control, ElementControls.Elements.RoundedRectangleElement element, string? propertyName)
        {
            if (propertyName == null)
                return;

            switch (propertyName)
            {
                case nameof(ElementControls.Elements.RoundedRectangleElement.R0):
                case nameof(ElementControls.Elements.RoundedRectangleElement.R1):
                case nameof(ElementControls.Elements.RoundedRectangleElement.R2):
                case nameof(ElementControls.Elements.RoundedRectangleElement.R3):
                    control.UpdateShape(element);
                    break;

                default:
                    UpdateCommonPropertiesForShapeFlow(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新圆角星形控件属性
        /// </summary>
        public void UpdateRoundedStarControl(RoundedStarControl control, ElementControls.Elements.RoundedStarElement element, string? propertyName)
        {
            if (propertyName == null)
                return;

            switch (propertyName)
            {
                case nameof(ElementControls.Elements.RoundedStarElement.PointCount):
                    control.InitialPoints();
                    control.UpdateShape(element);
                    break;
                case nameof(ElementControls.Elements.RoundedPolygonElement.RotDeg):
                    control.InitialPoints();
                    control.UpdateShape(element);
                    break;
                case nameof(ElementControls.Elements.RoundedStarElement.OuterRatio):
                case nameof(ElementControls.Elements.RoundedStarElement.InnerRatio):
                    control.InitialPoints();
                    control.UpdateShape(element);
                    break;

                case nameof(ElementControls.Elements.RoundedStarElement.CornerRadius):
                case nameof(ElementControls.Elements.RoundedStarElement.Radii):
                    control.UpdateShape(element);
                    break;

                default:
                    UpdateCommonPropertiesForShapeFlow(control, element, propertyName);
                    break;
            }
        }

    /// <summary>
    /// 更新圆角多边形控件属性
    /// </summary>
    public void UpdateRoundedPolygonControl(RoundedPolygonControl control, ElementControls.Elements.RoundedPolygonElement element, string? propertyName)
    {
        if (propertyName == null)
            return;

        switch (propertyName)
        {
            case nameof(ElementControls.Elements.RoundedPolygonElement.PointCount):
                control.InitialPoints();
                control.UpdateShape(element);
                break;

            case nameof(ElementControls.Elements.RoundedPolygonElement.RotDeg):
                control.InitialPoints();
                control.UpdateShape(element);
                break;

            case nameof(ElementControls.Elements.RoundedPolygonElement.CornerRadius):
            case nameof(ElementControls.Elements.RoundedPolygonElement.Radii):
                control.UpdateShape(element);
                break;

            default:
                UpdateCommonPropertiesForShapeFlow(control, element, propertyName);
                break;
        }
    }

    /// <summary>
    /// 更新扇形 / 椭圆控件属性
    /// </summary>
    public void UpdateSectorControl(RoundEllipseControl control, ElementControls.Elements.RoundEllipseElement element, string? propertyName)
    {
        if (propertyName == null) return;

        switch (propertyName)
        {
            case nameof(ElementControls.Elements.RoundEllipseElement.StartAngle):
            case nameof(ElementControls.Elements.RoundEllipseElement.SweepAngle):
            case nameof(ElementControls.Elements.RoundEllipseElement.InnerRadius):
            case nameof(ElementControls.Elements.RoundEllipseElement.CornerRadius):
                control.UpdateShape(element);
                break;

            default:
                UpdateCommonPropertiesForShapeFlow(control, element, propertyName);
                break;
        }
    }

        /// <summary>
        /// 更新ShapeFlowElement控件属性
        /// </summary>
        public void UpdateShapeFlowControl(ShapeFlowControlBase control, ShapeElement element, string? propertyName)
        {
            if (propertyName == null)
            {
                return;
            }

            switch (propertyName)
            {
                default:
                    UpdateCommonPropertiesForShapeFlow(control, element, propertyName);
                    break;
            }
        }

        /// <summary>
        /// 更新ShapeFlowControl的通用属性
        /// </summary>
        public void UpdateCommonPropertiesForShapeFlow(ShapeFlowControlBase control, ShapeElement element, string? propertyName)
        {
            if (propertyName == null) 
                return;

            switch (propertyName)
            {
                case nameof(ShapeElement.X):
                    control.UpdateLeft(element.X);
                    break;
                case nameof(ShapeElement.Y):
                    control.UpdateTop(element.Y);
                    break;
                case nameof(ShapeElement.Width):
                    control.UpdateWidth(element.Width);
                    break;
                case nameof(ShapeElement.Height):
                    control.UpdateHeight(element.Height);
                    break;
                case nameof(ShapeElement.IsVisible):
                    control.UpdateIsVisible(element.IsVisible);
                    break;
                case nameof(ShapeElement.Opacity):
                    control.UpdateOpacity(element.Opacity);
                    break;
                case nameof(ShapeElement.ZIndex):
                    control.UpdateZIndex(element.ZIndex);
                    break;
                case nameof(ShapeElement.Shadows):
                    control.UpdateShadows(element.Shadows.ToList());
                    break;
                case nameof(ShapeElement.BackgroundFill):
                case nameof(ShapeElement.BackgroundFills):
                    control.ApplyMultiFill(element);
                    break;
                case nameof(ShapeElement.ShowIdLabel):
                case nameof(ShapeElement.Id):
                    control.UpdateIdLabel(element);
                    break;
                case nameof(ShapeElement.Rotation):
                    control.UpdateRotation(element.Rotation);
                    break;
                case nameof(CanvasElement.Padding):
                case nameof(CanvasElement.PaddingLeft):
                case nameof(CanvasElement.PaddingTop):
                case nameof(CanvasElement.PaddingRight):
                case nameof(CanvasElement.PaddingBottom):
                case nameof(CanvasElement.PaddingIsUniform):
                    control.UpdatePadding(element);
                    break;
                case nameof(CanvasElement.Margin):
                    control.UpdateMargin(element);
                    break;
                case nameof(CanvasElement.MarginLeft):
                case nameof(CanvasElement.MarginTop):
                case nameof(CanvasElement.MarginRight):
                case nameof(CanvasElement.MarginBottom):
                case nameof(CanvasElement.MarginIsUniform):
                    control.UpdateMargin(element);
                    break;
                //case nameof(CanvasElement.FixBottom):
                //case nameof(CanvasElement.FixLeft):
                //case nameof(CanvasElement.FixTop):
                //case nameof(CanvasElement.FixRight):
                //    break;
                case nameof(CanvasElement.HorizontalAlignment):
                    control.UpdateHorizontalAlignment(element);
                    break;
                case nameof(CanvasElement.VerticalAlignment):
                    control.UpdateVerticalAlignment(element);
                    break;
                case nameof(ShapeElement.StrokeColor):
                case nameof(ShapeElement.StrokeWidth):
                case nameof(ShapeElement.StrokeStyle):
                case nameof(ShapeElement.StrokeStyleIndex):
                case nameof(ShapeElement.StrokeFillType):
                case nameof(ShapeElement.StrokeOpacity):
                //case nameof(ShapeElement.StrokeGradientStartColor):
                //case nameof(ShapeElement.StrokeGradientEndColor):
                //case nameof(ShapeElement.StrokeGradientAngle):
                //case nameof(ShapeElement.StrokeImage):
                //case nameof(ShapeElement.StrokeImageStretchIndex):
                //case nameof(ShapeElement.StrokeImageTileModeIndex):
                case nameof(ShapeElement.StrokeDashOffset):
                case nameof(ShapeElement.StrokeDashArrayString):
                case nameof(ShapeElement.CornerRadius):
                case nameof(ShapeElement.StrokeJoin):
                case nameof(ShapeElement.StrokeEndpoint):
                case nameof(ShapeElement.StrokeMiterLimit):
                case nameof(ShapeElement.StrokePosition):
                case nameof(ShapeElement.StrokeFills):
                case nameof(ShapeElement.StrokeFill):
                    control.UpdateStroke(element);
                    break;
            }
        }

    }
}