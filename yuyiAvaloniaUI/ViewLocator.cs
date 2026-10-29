using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using yuyiAvalonia.Base.ViewModels.Pages;
using yuyiAvaloniaUI.ViewModels;

namespace yuyiAvaloniaUI
{
    /// <summary>
    /// Given a view model, returns the corresponding view if possible.
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var dataType = param.GetType();
            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            //var type = Type.GetType(name);

            var type = Type.GetType($"{name}, {GetAssemblyName(dataType)}");
            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        private string GetAssemblyName(Type type)
        {
            string? input = type.AssemblyQualifiedName;
            if (input is null)
            {
                return string.Empty;
            }

            var match = Regex.Match(input, ",(.*?),");
            return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
        }
        public bool Match(object? data)
        {
            return data is ViewModelBase || data is PageViewModelBase;
        }
    }
}
