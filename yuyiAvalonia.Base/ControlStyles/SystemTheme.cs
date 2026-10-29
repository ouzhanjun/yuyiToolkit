using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;

namespace yuyiAvalonia.Base.ControlStyles
{
    public partial class SystemTheme : ObservableObject
    {
        public static ThemeVariant? CurrentTheme = null;
        public SystemTheme()
        {
            CurrentTheme = Application.Current!.ActualThemeVariant;
            ChangeTheme(CurrentTheme);
        }

        [ObservableProperty]
        private Color _textPrimary = Color.Parse("#F1F5F9");
        [ObservableProperty]
        private Color _textSecondary = Color.Parse("#F1F5F9");
        [ObservableProperty]
        private Color _textForeground = Color.Parse("#F1F5F9");
        
        [ObservableProperty]
        private Color _backgroundBody = Color.Parse("#F1F5F9");
        [ObservableProperty]
        private Color _backgroundContent = Color.Parse("#192435");
        [ObservableProperty]
        private Color _backgroundHoverBody = Color.Parse("#192435");
        [ObservableProperty]
        private double _backgroundHoverBodyOpacity = 0.8;

        [ObservableProperty]
        private Color _borderDefault = Color.Parse("#374151");
        [ObservableProperty]
        private Color _borderNormal = Color.Parse("#374151");
        [ObservableProperty]
        private Color _borderHover = Color.Parse("#374151");
        [ObservableProperty]
        private Color _borderFocus = Color.Parse("#374151");
        [ObservableProperty]
        private Color _borderPress = Color.Parse("#374151");

        public void ChangeTheme(ThemeVariant? themeVariant)
        {
            if (themeVariant == null)
            {
                return;
            }
            CurrentTheme = themeVariant;
            ThemeStyleBase? theme = null;

            if (CurrentTheme == ThemeVariant.Dark)
            {
                theme = new DarkThemeStyles();
            }
            else
            {
                theme = new LightThemeStyles();
            }

            TextPrimary = theme!.TextPrimary;
            TextSecondary = theme.TextSecondary;
            TextForeground = theme.TextForeground;

            BackgroundBody = theme.BackgroundBody;
            BackgroundContent = theme.BackgroundContent;
            BackgroundHoverBody = theme.BackgroundHoverBody;
            BackgroundHoverBodyOpacity = theme.BackgroundHoverBodyOpacity;

            BorderDefault = theme.BorderDefault;
            BorderNormal = theme.BorderNormal;
            BorderHover = theme.BorderHover;
            BorderFocus = theme.BorderFocus;
            BorderPress = theme.BorderPress;
        }
    }
}
