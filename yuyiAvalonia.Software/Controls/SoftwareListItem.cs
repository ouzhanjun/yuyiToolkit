using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using yuyiAvalonia.Software.Models;

namespace yuyiAvalonia.Software.Controls;

/// <summary>
/// 软件列表项控件 - 用于在列表中显示单个软件项
/// </summary>
public partial class SoftwareListItem : TemplatedControl
{
    public static readonly StyledProperty<SoftwareItem?> SoftwareProperty =
        AvaloniaProperty.Register<SoftwareListItem, SoftwareItem?>(nameof(Software));

    public static readonly StyledProperty<bool> IsSelectedProperty =
        AvaloniaProperty.Register<SoftwareListItem, bool>(nameof(IsSelected), false);

    public static readonly DirectProperty<SoftwareListItem, IBrush?> StatusBrushProperty =
        AvaloniaProperty.RegisterDirect<SoftwareListItem, IBrush?>(
            nameof(StatusBrush),
            o => o.StatusBrush);

    private IBrush? _statusBrush;

    public SoftwareItem? Software
    {
        get => GetValue(SoftwareProperty);
        set => SetValue(SoftwareProperty, value);
    }

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public IBrush? StatusBrush => _statusBrush;

    public event EventHandler<SoftwareItem?>? SoftwareClicked;
    public event EventHandler<SoftwareItem?>? FavoriteClicked;
    public event EventHandler<SoftwareItem?>? DownloadClicked;

    public SoftwareListItem()
    {
        PointerPressed += OnPointerPressed;
        UpdateStatusBrush();
    }

    static SoftwareListItem()
    {
        SoftwareProperty.Changed.AddClassHandler<SoftwareListItem>((sender, e) =>
        {
            sender.UpdateStatusBrush();
        });
    }

    private void UpdateStatusBrush()
    {
        if (Software == null)
        {
            _statusBrush = new SolidColorBrush(Color.Parse("#8C8C8C"));
            return;
        }

        _statusBrush = Software.Status switch
        {
            SoftwareStatus.Downloaded => new SolidColorBrush(Color.Parse("#52C41A")),
            SoftwareStatus.Downloading => new SolidColorBrush(Color.Parse("#1890FF")),
            SoftwareStatus.Error => new SolidColorBrush(Color.Parse("#FF4D4F")),
            SoftwareStatus.UpdateAvailable => new SolidColorBrush(Color.Parse("#FAAD14")),
            _ => new SolidColorBrush(Color.Parse("#8C8C8C"))
        };

        RaisePropertyChanged(StatusBrushProperty, null, _statusBrush);
    }

    private void OnPointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            SoftwareClicked?.Invoke(this, Software);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var favoriteButton = e.NameScope.Find<Button>("PART_FavoriteButton");
        var downloadButton = e.NameScope.Find<Button>("PART_DownloadButton");

        if (favoriteButton != null)
        {
            favoriteButton.Click += (s, args) => FavoriteClicked?.Invoke(this, Software);
        }

        if (downloadButton != null)
        {
            downloadButton.Click += (s, args) => DownloadClicked?.Invoke(this, Software);
        }
    }
}
