using Avalonia.Media;

namespace yuyiAvalonia.Base.ControlStyles
{
    public abstract class ThemeStyleBase
    {
        public abstract Color TextPrimary { get; }
        public abstract Color TextSecondary { get; }
        public abstract Color TextForeground { get; }
        public abstract Color BackgroundBody { get; }
        public abstract Color BackgroundHoverBody { get; }
        public abstract double BackgroundHoverBodyOpacity { get; }
        public abstract Color BackgroundContent { get; }
        public abstract Color BorderDefault { get; }
        public abstract Color BorderNormal { get; }
        public abstract Color BorderHover { get; }
        public abstract Color BorderFocus { get; }
        public abstract Color BorderPress { get; }
    }

    public class DarkThemeStyles : ThemeStyleBase
    {
        public override Color TextPrimary => Color.Parse("#F1F5F9");
        public override Color TextSecondary => Color.Parse("#F1F5F9");
        public override Color TextForeground => Color.Parse("#F1F5F9");

        public override Color BackgroundBody => Color.Parse("#0F172A");
        public override Color BackgroundHoverBody => Color.Parse("#1A233A");
        public override double BackgroundHoverBodyOpacity => 0.9;
        public override Color BackgroundContent => Color.Parse("#192435");

        public override Color BorderDefault => Color.Parse("#0F172A");
        public override Color BorderNormal => Color.Parse("#0F172A");
        public override Color BorderHover => Color.Parse("#374151");
        public override Color BorderFocus => Color.Parse("#66A7E2");
        public override Color BorderPress => Color.Parse("#374151");
    }

    public class LightThemeStyles : ThemeStyleBase
    {
        public override Color TextPrimary => Color.Parse("#111827");
        public override Color TextSecondary => Color.Parse("#6B7280");
        public override Color TextForeground => Color.Parse("#111827");

        public override Color BackgroundBody => Color.Parse("#FEFEFE");
        public override Color BackgroundHoverBody => Color.Parse("#FEFEFE");
        public override double BackgroundHoverBodyOpacity => 0.9;
        public override Color BackgroundContent => Color.Parse("#F3F4F6");

        public override Color BorderDefault => Color.Parse("#FEFEFE");
        public override Color BorderNormal => Color.Parse("#FEFEFE");
        public override Color BorderHover => Color.Parse("#E5E7EB");
        public override Color BorderFocus => Color.Parse("#66A7E2");
        public override Color BorderPress => Color.Parse("#E5E7EB");
    }
}
