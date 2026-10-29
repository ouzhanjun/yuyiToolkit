using CommunityToolkit.Mvvm.ComponentModel;
using yuyiAvalonia.Base.Core;

namespace yuyiAvalonia.Base.ViewModels.Pages;

/// <summary>
/// 视图页面模型基类
/// </summary>
public abstract class PageViewModelBase : ObservableObject,IDisposable
{
    /// <summary>
    /// 页面标题
    /// </summary>
    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Name { get; set; }

    public abstract void UpdateLanguageTexts();

    public abstract void Dispose();

    public abstract void Initialize(SystemContext? context);
}

