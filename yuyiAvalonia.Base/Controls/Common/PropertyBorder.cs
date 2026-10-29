using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using yuyiAvalonia.Base.ControlStyles;
using yuyiAvalonia.Base.Core;

namespace yuyiAvalonia.Base.Controls.Common
{
    public class PropertyBorder : Border
    {
        private IBrush? _oldBrush;
        private IBrush? _oldBackground;
        private Thickness _oldThickness;
        private SystemTheme _systemTheme;
        public PropertyBorder()
        {
            this.ClipToBounds = true;
            this.BorderThickness = new Thickness(1);
            this.BorderBrush = new SolidColorBrush(GlobalProvider.SystemTheme.BorderNormal);
            this._systemTheme = GlobalProvider.SystemTheme;
        }

        public void Attach(Control userControl)
        {
            userControl.GotFocus += UserControl_GotFocus;
            userControl.LostFocus += UserControl_LostFocus;
            userControl.PointerEntered += UserControl_PointerMoved;
            userControl.PointerExited += UserControl_PointerExited;
        }

        private void UserControl_PointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            this.Background = _oldBackground;
        }

        private void UserControl_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            SetHover();
        }

        public void Deattach(UserControl userControl)
        {
            userControl.GotFocus -= UserControl_GotFocus;
            userControl.LostFocus -= UserControl_LostFocus;
            userControl.PointerMoved -= UserControl_PointerMoved;
        }

        private void UserControl_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.SetLostFocus();
        }

        private void UserControl_GotFocus(object? sender, Avalonia.Input.GotFocusEventArgs e)
        {
            this.SetFocus();
        }

        public void SetFocus()
        {
            if (_oldBrush == null)
                _oldBrush = this.BorderBrush;
            this.BorderBrush = new SolidColorBrush(_systemTheme.BorderFocus);
        }

        public void SetLostFocus()
        {
            this.BorderBrush = _oldBrush;
        }

        public void SetHover()
        {
            if (_oldBackground == null)
                _oldBackground = this.Background;
            this.Background = new SolidColorBrush(_systemTheme.BackgroundHoverBody, _systemTheme.BackgroundHoverBodyOpacity);
        }
    }
}
