using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System.Windows.Input;

namespace yuyiAvalonia.ImageEdit.Controls;

public partial class ZoomSelectorControl : UserControl
{
    public static readonly StyledProperty<double> ZoomLevelProperty =
        AvaloniaProperty.Register<ZoomSelectorControl, double>(nameof(ZoomLevel), 1.0);

    public static readonly StyledProperty<double[]> ZoomLevelsProperty =
        AvaloniaProperty.Register<ZoomSelectorControl, double[]>(nameof(ZoomLevels), [0.1, 0.25, 0.5, 0.75, 1.0, 1.5, 2.0, 3.0, 4.0, 5.0, 8.0, 10.0]);

    public static readonly StyledProperty<ICommand?> ZoomCommandProperty =
        AvaloniaProperty.Register<ZoomSelectorControl, ICommand?>(nameof(ZoomCommand));

    private Button? _zoomButton;

    public double ZoomLevel
    {
        get => GetValue(ZoomLevelProperty);
        set => SetValue(ZoomLevelProperty, value);
    }

    public double[] ZoomLevels
    {
        get => GetValue(ZoomLevelsProperty);
        set => SetValue(ZoomLevelsProperty, value);
    }

    public ICommand? ZoomCommand
    {
        get => GetValue(ZoomCommandProperty);
        set => SetValue(ZoomCommandProperty, value);
    }

    public ZoomSelectorControl()
    {
        InitializeComponent();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _zoomButton = this.FindControl<Button>("ZoomButton");
        UpdateButtonContent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ZoomLevelProperty)
        {
            UpdateButtonContent();
        }
    }

    private void UpdateButtonContent()
    {
        if (_zoomButton != null)
        {
            _zoomButton.Content = $"{ZoomLevel:P0}";
        }
    }
}
