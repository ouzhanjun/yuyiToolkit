
using System;
using Avalonia.Media.Fonts;

namespace yuyiAvaloniaUI.FontCollections
{
    public sealed class HarmonyOSFontCollection : EmbeddedFontCollection
    {
        public HarmonyOSFontCollection() : base(
            new Uri("fonts:HarmonyOS Sans", UriKind.Absolute), 
            new Uri("avares://yuyiAvaloniaUI/Assets/Fonts",UriKind.Absolute))
        {
            
        }
    }
}