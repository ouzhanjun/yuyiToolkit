using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.ViewModels.Pages;
using yuyiAvalonia.UI.Models;

namespace yuyiAvalonia.UI.ViewModels
{
    public  partial class ElementEditViewModelBase : PageViewModelBase
    {

        #region 画布元素

        [ObservableProperty]
        private ObservableCollection<CanvasElement> _canvasElements = new();

        [ObservableProperty]
        private CanvasElement? _selectedElement;

        [ObservableProperty]
        private bool _hasSelectedElement;

        #endregion

        partial void OnSelectedElementChanged(CanvasElement? oldValue, CanvasElement? newValue)
        {
            HasSelectedElement = newValue != null;
            SelectedElementChangedHandle(oldValue, newValue);
        }

        public virtual void SelectedElementChangedHandle(CanvasElement? oldValue, CanvasElement? newValue)
        { }

        public override void UpdateLanguageTexts()
        {
        }

        public override void Dispose()
        {
        }

        public override void Initialize(SystemContext? context)
        {
        }

    }
}