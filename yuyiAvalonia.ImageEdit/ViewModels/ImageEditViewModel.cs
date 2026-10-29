using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using yuyiAvalonia.Base.Core;
using yuyiAvalonia.Base.Helpers;
using yuyiAvalonia.Base.Models.Menu;
using yuyiAvalonia.Base.ViewModels.Menu;
using yuyiAvalonia.Base.ViewModels.Pages;
using yuyiAvalonia.ImageEdit.Common;
using yuyiAvalonia.ImageEdit.Controls;
using yuyiAvalonia.ImageEdit.Helpers;
using yuyiAvalonia.ImageEdit.Models;
using yuyiAvalonia.ImageEdit.Views;

namespace yuyiAvalonia.ImageEdit.ViewModels;


public partial class ImageEditViewModel : PageViewModelBase, INotifyPropertyChanged
{
    [ObservableProperty]
    private Bitmap? _imageSource;

    private ImageEditHistoryItem currentImageEditItem;

    public ImageEditHistoryItem CurrentImageEditItem { get => currentImageEditItem; set => currentImageEditItem = value; }

    // 缓存的 SKBitmap，避免频繁编码/解码
    private SKBitmap? _cachedSkBitmap;

    // 用于取消正在进行的图像处理操作
    private CancellationTokenSource? _previewCts;

    // 锁对象，用于保护图像处理操作
    private readonly object _imageProcessingLock = new();

    public SelectionHelper? SelectionHelper;

    /// <summary>
    /// 选择框变化事件（用于通知 UI 更新）
    /// </summary>
    public event Action<Rect>? SelectionRectChanged;

    /// <summary>
    /// 是否有图像
    /// </summary>
    [ObservableProperty]
    public bool _hasImage = false;

    /// <summary>
    /// 顶部工具栏
    /// </summary>
    [ObservableProperty]
    public ObservableCollection<MenuItemViewModel> _toolbarItems;

    [ObservableProperty]
    public UIImageEditIntensityList _imageEditIntensityList;

    [ObservableProperty]
    public ObservableCollection<ImageIntensity> _imageEditIntensityBasic;

    [ObservableProperty]
    public ObservableCollection<ImageIntensity> _imageEditIntensityEffect;

    [ObservableProperty]
    public ObservableCollection<ImageIntensity> _imageEditIntensityStylize;

    [ObservableProperty]
    public ObservableCollection<ImageIntensity> _imageEditIntensityColor;

    [ObservableProperty]
    public UserControl _rightControl;

    /// <summary>
    /// 图片列表
    /// </summary>
    [ObservableProperty]
    public ObservableCollection<ImageFileItem> _imageList = new();

    /// <summary>
    /// 当前选中的图片
    /// </summary>
    [ObservableProperty]
    private ImageFileItem? _selectedImageItem;

    /// <summary>
    /// 当前文件夹路径
    /// </summary>
    [ObservableProperty]
    private string? _currentFolderPath;

    /// <summary>
    /// 是否有多个图片（显示左侧面板）
    /// </summary>
    public bool HasMultipleImages => ImageList != null && ImageList.Count > 1;

    /// <summary>
    /// 缩放级别 (1.0 = 100%)
    /// </summary>
    [ObservableProperty]
    public double _zoomLevel = 1.0;

    /// <summary>
    /// 将屏幕坐标（缩放后）转换为原始图像坐标
    /// </summary>
    public Point ScreenToImage(Point screenPoint)
    {
        return new Point(
            screenPoint.X / ZoomLevel,
            screenPoint.Y / ZoomLevel
        );
    }

    /// <summary>
    /// 最小缩放级别
    /// </summary>
    public double MinZoom => 0.1;

    /// <summary>
    /// 最大缩放级别
    /// </summary>
    public double MaxZoom => 10.0;

    /// <summary>
    /// 缩放级别列表（用于下拉菜单）
    /// </summary>
    public double[] ZoomLevels => [0.1, 0.25, 0.5, 0.75, 1.0, 1.5, 2.0, 3.0, 4.0, 5.0, 8.0, 10.0];

    /// <summary>
    /// 水平平移量
    /// </summary>
    [ObservableProperty]
    public double _panX;

    /// <summary>
    /// 垂直平移量
    /// </summary>
    [ObservableProperty]
    public double _panY;

    /// <summary>
    /// 容器大小（用于适应窗口）
    /// </summary>
    [ObservableProperty]
    private Size _containerSize = new Size(800, 600);

    /// <summary>
    /// 缩放后的图片尺寸
    /// </summary>
    public Size ScaledImageSize
    {
        get
        {
            if (ImageSource == null)
                return new Size(0, 0);

            var pixelSize = ImageSource.PixelSize;
            return new Size(
                pixelSize.Width * ZoomLevel,
                pixelSize.Height * ZoomLevel);
        }
    }

    /// <summary>
    /// 当前文件路径（用于保存）
    /// </summary>
    private string? _currentFilePath;

    /// <summary>
    /// 当前工具模式（选择/裁剪/移动）
    /// </summary>
    [ObservableProperty]
    private EditToolMode _currentToolMode = EditToolMode.Normal;

    /// <summary>
    /// 画笔设置
    /// </summary>
    [ObservableProperty]
    private BrushSettings _currentBrushSettings = new BrushSettings();

    /// <summary>
    /// 文字设置
    /// </summary>
    [ObservableProperty]
    private TextSettings _currentTextSettings = new TextSettings();

    /// <summary>
    /// 字体名称列表（用于下拉选择）
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<string> _fontNameList = new ObservableCollection<string>(FontNames.Common);

    /// <summary>
    /// 是否正在绘制画笔
    /// </summary>
    [ObservableProperty]
    private bool _isDrawingBrush;

    /// <summary>
    /// 画笔路径列表（用于绘制）
    /// </summary>
    [ObservableProperty]
    private List<List<Point>> _brushStrokes = new List<List<Point>>();

    /// <summary>
    /// 当前绘制的路径
    /// </summary>
    [ObservableProperty]
    private List<Point>? _currentStroke;

    /// <summary>
    /// 上次绘制的点（用于增量绘制）
    /// </summary>
    private Point? _lastDrawPoint;

    /// <summary>
    /// 增量绘制的路径段
    /// </summary>
    private List<Point>? _incrementalStroke;

    /// <summary>
    /// 防抖定时器
    /// </summary>
    private DispatcherTimer? _debounceTimer;

    /// <summary>
    /// 防抖时间间隔（毫秒）
    /// </summary>
    private const int DebounceInterval = 1;

    /// <summary>
    /// 选择框起始点（图像坐标系）
    /// </summary>
    [ObservableProperty]
    private Point? _selectionStart;

    /// <summary>
    /// 选择框结束点（图像坐标系）
    /// </summary>
    [ObservableProperty]
    private Point? _selectionEnd;

    /// <summary>
    /// 是否有有效的选择区域
    /// </summary>
    public bool HasSelection => SelectionRect.Width > 5 && SelectionRect.Height > 5;

    /// <summary>
    /// 当前选择区域（用于绑定到视图）
    /// </summary>
    [ObservableProperty]
    private Rect _selectionRect = new Rect(0, 0, 0, 0);

    /// <summary>
    /// 当前选择区域（显示用：屏幕坐标 = 图像坐标 * ZoomLevel）
    /// </summary>
    public Rect SelectionRectDisplay => new Rect(
        SelectionRect.X * ZoomLevel,
        SelectionRect.Y * ZoomLevel,
        SelectionRect.Width * ZoomLevel,
        SelectionRect.Height * ZoomLevel);

    /// <summary>
    /// 是否正在绘制选择框
    /// </summary>
    [ObservableProperty]
    private bool _isSelecting;

    /// <summary>
    /// 是否正在绘制选择框
    /// </summary>
    [ObservableProperty]
    private bool _useSelection;

    /// <summary>
    /// 编辑历史列表
    /// </summary>
    private HistoryHelper _history ;

    public HistoryHelper History => _history;

    private SystemContext _systemContext;

    public override void Initialize(SystemContext? context)
    {
        if (context == null) 
            return;

        _systemContext= context;

        #region 顶部菜单

        _systemContext.TopMenus.MenuOpenFile.Command= new RelayCommand(async () =>
        {
            this.OpenFile();
        });

        _systemContext.TopMenus.MenuOpenDirectory.Command = new RelayCommand(async () =>
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;

            // 从当前控件获取 TopLevel。或者，您也可以使用 Window 引用。
            var topLevel = TopLevel.GetTopLevel(window);

            // 启动异步操作以打开对话框。
            var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = "Open Directory",
                AllowMultiple = false
            });

            if (folders.Count > 0)
            {
                // 从文件路径获取文件夹并加载图片列表
                var folderPath = System.IO.Path.GetDirectoryName(folders[0].Path.LocalPath);
                if (!string.IsNullOrEmpty(folderPath))
                {
                    await LoadImageListFromFolderAsync(folderPath);
                }
            }
        });

        _systemContext.TopMenus.MenuSaveAs.Command = new RelayCommand(async () =>
        {
            _ = this.SaveImageAs();
        });
        #endregion

        #region 主框架顶部工具栏


        MenuItemViewModel itemSeperator = new MenuItemViewModel("Seperator");
        itemSeperator.Text = "-";
        itemSeperator.IsSeparator = true;

        MenuItemViewModel itemOpenFile = new MenuItemViewModel("File");
        itemOpenFile.Icon = Icon.Image;
        itemOpenFile.Text = "打开图片";
        itemOpenFile.MenuClickCommand = new RelayCommand(async () =>
        {
            this.OpenFile();
        });

        MenuItemViewModel itemUndo = new MenuItemViewModel("Undo");
        itemUndo.Icon = Icon.Undo;
        itemUndo.Text = "撤销";
        itemUndo.MenuClickCommand = this.UndoCommand;

        MenuItemViewModel itemRedo = new MenuItemViewModel("Redo");
        itemRedo.Icon = Icon.Redo;
        itemRedo.Text = "重做";
        itemRedo.MenuClickCommand = this.RedoCommand;

        MenuItemViewModel itemZoomIn = new MenuItemViewModel("ZoomIn");
        itemZoomIn.Icon = Icon.ZoomIn;
        itemZoomIn.Text = "放大";
        itemZoomIn.MenuClickCommand = this.ZoomInCommand;

        MenuItemViewModel itemZoomOut = new MenuItemViewModel("ZoomOut");
        itemZoomOut.Icon = Icon.ZoomOut;
        itemZoomOut.Text = "缩小";
        itemZoomOut.MenuClickCommand = this.ZoomOutCommand;

        MenuItemViewModel itemFit = new MenuItemViewModel("FitToWindow");
        itemFit.Icon = Icon.Fit;
        itemFit.Text = "适应窗口";
        itemFit.MenuClickCommand = this.FitToWindowCommand;

        MenuItemViewModel itemRotateL_90 = new MenuItemViewModel("RotateL_90");
        itemRotateL_90.Icon = Icon.RotateLF_90;
        itemRotateL_90.Text = "逆时针旋转90";
        itemRotateL_90.MenuClickCommand = this.RotateLeftCommand;

        MenuItemViewModel itemRotateR_90 = new MenuItemViewModel("RotateR_90");
        itemRotateR_90.Icon = Icon.RotateRT_90;
        itemRotateR_90.Text = "顺时针旋转90";
        itemRotateR_90.MenuClickCommand = this.RotateRightCommand;

        MenuItemViewModel itemFlipH = new MenuItemViewModel("FlipHorizontal");
        itemFlipH.Icon = Icon.FlipHorizontal;
        itemFlipH.Text = "水平翻转";
        itemFlipH.MenuClickCommand = this.FlipHorizontalCommand;

        MenuItemViewModel itemFlipV = new MenuItemViewModel("FlipVertical");
        itemFlipV.Icon = Icon.FlipVertical;
        itemFlipV.Text = "垂直翻转";
        itemFlipV.MenuClickCommand = this.FlipVerticalCommand;

        MenuItemViewModel itemPrint = new MenuItemViewModel("Print");
        itemPrint.Icon = Icon.Print;
        itemPrint.Text = "打印";
        itemPrint.MenuClickCommand = this.PrintImageCommand;

        MenuItemViewModel itemSelect = new MenuItemViewModel("Select");
        itemSelect.Icon = Icon.Select;
        itemSelect.Text = "选择";
        itemSelect.MenuClickCommand = this.SetNormalModeCommand;

        MenuItemViewModel itemSelection = new MenuItemViewModel("Selection");
        itemSelection.Icon = Icon.Selection;
        itemSelection.Text = "框选";
        itemSelection.MenuClickCommand = this.SetSelectModeCommand;

        MenuItemViewModel itemCrop = new MenuItemViewModel("Crop");
        itemCrop.Icon = Icon.Crop;
        itemCrop.Text = "裁剪";
        itemCrop.MenuClickCommand = this.CropCommand;

        MenuItemViewModel itemBrush = new MenuItemViewModel("Brush");
        itemBrush.Icon = Icon.Pen;
        itemBrush.Text = "画笔";
        itemBrush.MenuClickCommand = this.SetBrushModeCommand;

        MenuItemViewModel itemText = new MenuItemViewModel("Text");
        itemText.Icon = Icon.Text;
        itemText.Text = "文字";
        itemText.MenuClickCommand = this.SetTextModeCommand;

        MenuItemViewModel itemSave = new MenuItemViewModel("Save");
        itemSave.Icon = Icon.Save;
        itemSave.Text = "保存";
        itemSave.MenuClickCommand = this.SaveImageCommand;

        MenuItemViewModel itemDelete = new MenuItemViewModel("Delete");
        itemDelete.Icon = Icon.Delete;
        itemDelete.Text = "删除";
        itemDelete.MenuClickCommand = this.DeleteCommand;

        MenuItemViewModel itemOriginal = new MenuItemViewModel("original");
        itemOriginal.Icon = Icon.Original;
        itemOriginal.Text = "原图";
        itemOriginal.MenuClickCommand = this.ActualSizeCommand;

        // 创建缩放选择器（复杂控件）
        MenuItemViewModel itemZoomSelector = new MenuItemViewModel("ZoomSelector");
        itemZoomSelector.IsComplex = true;
        var zoomSelector = new ZoomSelectorControl
        {
            ZoomLevel = this.ZoomLevel,
            ZoomLevels = this.ZoomLevels,
            ZoomCommand = this.ZoomSizeCommand
        };

        // 绑定 ZoomLevel 变化
        this.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ZoomLevel))
            {
                zoomSelector.ZoomLevel = this.ZoomLevel;
            }
        };
        itemZoomSelector.ComplexContent = zoomSelector;

        ChildToolbar childToolbar1 = new ChildToolbar("toolbar_2",
            [
                itemOpenFile,
                itemSave,
                itemDelete,
                itemPrint,
                itemSeperator,
                itemUndo,
                itemRedo,
                itemSeperator,
                itemZoomIn,
                itemZoomOut,
                itemFit,
                itemOriginal,
                itemZoomSelector,
                itemSeperator,
                itemRotateL_90,
                itemRotateR_90,
                itemFlipH,
                itemFlipV,
                itemSeperator,
                itemSelect,
                itemSelection,
                itemCrop,
                itemBrush,
                itemText,
            ]);

        var childTool = _systemContext.TopToolbar.ChildTools.FirstOrDefault(x => x.Name == childToolbar1.Name);
        if (childTool != null)
        {
            _systemContext.TopToolbar.ChildTools.Remove(childTool);
        }

        _systemContext.TopToolbar.ChildTools.Add(childToolbar1);

        _systemContext.NotifyPropertyChange(true, "TopToolbar");

        #endregion

        ToolbarItems = new ObservableCollection<MenuItemViewModel>();
        ToolbarItems.Add(itemOpenFile);
        ToolbarItems.Add(itemSave);
        ToolbarItems.Add(itemDelete);
        ToolbarItems.Add(itemUndo);
        ToolbarItems.Add(itemRedo);
        ToolbarItems.Add(itemZoomIn);
        ToolbarItems.Add(itemZoomOut);
        ToolbarItems.Add(itemFit);
        ToolbarItems.Add(itemRotateL_90);
        ToolbarItems.Add(itemRotateR_90);
        ToolbarItems.Add(itemFlipH);
        ToolbarItems.Add(itemFlipV);
        ToolbarItems.Add(itemPrint);
        ToolbarItems.Add(itemSelect);
        ToolbarItems.Add(itemSelection);
        ToolbarItems.Add(itemCrop);
        ToolbarItems.Add(itemBrush);
        ToolbarItems.Add(itemText);
        ToolbarItems.Add(itemOriginal);
        ToolbarItems.Add(itemZoomSelector);
    }

    public override void Dispose()
    {
        var childTool = _systemContext.TopToolbar.ChildTools.FirstOrDefault(x => x.Name == "toolbar_2");
        if (childTool != null)
        {
            _systemContext.TopToolbar.ChildTools.Remove(childTool);
            _systemContext.NotifyPropertyChange(true, "TopToolbar");
        }
    }

    public ImageEditViewModel()
    {
        RightControl = new ImageEditControlPanel();

        _history = new HistoryHelper(this);
        ImageEditIntensityList = new UIImageEditIntensityList();
        ResetImageEditIntensity();

        // 订阅字号变化事件，用于更新输入框尺寸（需要应用 ZoomLevel）
        CurrentTextSettings.FontSizeChanged += () =>
        {
            // 应用当前缩放级别
            CurrentTextSettings.FontSizeDisplay = CurrentTextSettings.FontSize * ZoomLevel;
            CurrentTextSettings.NotifyFontSizeDisplayChanged();
            OnPropertyChanged(nameof(TextInputBoxSize));
        };

        // 初始更新显示字号和输入框尺寸
        CurrentTextSettings.FontSizeDisplay = CurrentTextSettings.FontSize;
        OnPropertyChanged(nameof(TextInputBoxSize));

        #region 注释

        //MenuItemViewModel itemOpenFile = new MenuItemViewModel("File");
        //itemOpenFile.Icon = Icon.Image;
        //itemOpenFile.Text = "打开图片";
        //itemOpenFile.MenuClickCommand = new RelayCommand(async () =>
        //{
        //    OpenFile();
        //});

        //MenuItemViewModel itemUndo = new MenuItemViewModel("Undo");
        //itemUndo.Icon = Icon.Undo;
        //itemUndo.Text = "撤销";
        //itemUndo.MenuClickCommand = UndoCommand;

        //MenuItemViewModel itemRedo = new MenuItemViewModel("Redo");
        //itemRedo.Icon = Icon.Redo;
        //itemRedo.Text = "重做";
        //itemRedo.MenuClickCommand = RedoCommand;

        //MenuItemViewModel itemZoomIn = new MenuItemViewModel("ZoomIn");
        //itemZoomIn.Icon = Icon.ZoomIn;
        //itemZoomIn.Text = "放大";
        //itemZoomIn.MenuClickCommand = ZoomInCommand;

        //MenuItemViewModel itemZoomOut = new MenuItemViewModel("ZoomOut");
        //itemZoomOut.Icon = Icon.ZoomOut;
        //itemZoomOut.Text = "缩小";
        //itemZoomOut.MenuClickCommand = ZoomOutCommand;

        //MenuItemViewModel itemFit = new MenuItemViewModel("FitToWindow");
        //itemFit.Icon = Icon.Fit;
        //itemFit.Text = "适应窗口";
        //itemFit.MenuClickCommand = FitToWindowCommand;

        //MenuItemViewModel itemRotateL_90 = new MenuItemViewModel("RotateL_90");
        //itemRotateL_90.Icon = Icon.RotateLF_90;
        //itemRotateL_90.Text = "逆时针旋转90";
        //itemRotateL_90.MenuClickCommand = RotateLeftCommand;

        //MenuItemViewModel itemRotateR_90 = new MenuItemViewModel("RotateR_90");
        //itemRotateR_90.Icon = Icon.RotateRT_90;
        //itemRotateR_90.Text = "顺时针旋转90";
        //itemRotateR_90.MenuClickCommand = RotateRightCommand;

        //MenuItemViewModel itemFlipH = new MenuItemViewModel("FlipHorizontal");
        //itemFlipH.Icon = Icon.FlipHorizontal;
        //itemFlipH.Text = "水平翻转";
        //itemFlipH.MenuClickCommand = FlipHorizontalCommand;

        //MenuItemViewModel itemFlipV = new MenuItemViewModel("FlipVertical");
        //itemFlipV.Icon = Icon.FlipVertical;
        //itemFlipV.Text = "垂直翻转";
        //itemFlipV.MenuClickCommand = FlipVerticalCommand;

        //MenuItemViewModel itemPrint = new MenuItemViewModel("Print");
        //itemPrint.Icon = Icon.Print;
        //itemPrint.Text = "打印";
        //itemPrint.MenuClickCommand = PrintImageCommand;

        //MenuItemViewModel itemSelect = new MenuItemViewModel("Select");
        //itemSelect.Icon = Icon.Select;
        //itemSelect.Text = "选择";
        //itemSelect.MenuClickCommand = SetSelectModeCommand;

        //MenuItemViewModel itemSelection = new MenuItemViewModel("Selection");
        //itemSelection.Icon = Icon.Selection;
        //itemSelection.Text = "框选";
        //itemSelection.MenuClickCommand = SetSelectModeCommand;

        //MenuItemViewModel itemCrop = new MenuItemViewModel("Crop");
        //itemCrop.Icon = Icon.Crop;
        //itemCrop.Text = "裁剪";
        //itemCrop.MenuClickCommand = CropCommand;

        //MenuItemViewModel itemBrush = new MenuItemViewModel("Brush");
        //itemBrush.Icon = Icon.Pen;
        //itemBrush.Text = "画笔";
        //itemBrush.MenuClickCommand = SetBrushModeCommand;

        //MenuItemViewModel itemText = new MenuItemViewModel("Text");
        //itemText.Icon = Icon.Text;
        //itemText.Text = "文字";
        //itemText.MenuClickCommand = SetTextModeCommand;

        //MenuItemViewModel itemSave = new MenuItemViewModel("Save");
        //itemSave.Icon = Icon.Save;
        //itemSave.Text = "保存";
        //itemSave.MenuClickCommand = SaveImageCommand;

        //MenuItemViewModel itemDelete = new MenuItemViewModel("Delete");
        //itemDelete.Icon = Icon.Delete;
        //itemDelete.Text = "删除";
        //itemDelete.MenuClickCommand = DeleteCommand;

        //MenuItemViewModel itemDelete = new MenuItemViewModel("Delete");
        //itemDelete.Icon = Icon.Delete;
        //itemDelete.Text = "删除";
        //itemDelete.MenuClickCommand = DeleteCommand;

        //ToolbarItems = new ObservableCollection<MenuItemViewModel>();
        //ToolbarItems.Add(itemOpenFile);
        //ToolbarItems.Add(itemSave);
        //ToolbarItems.Add(itemDelete);
        //ToolbarItems.Add(itemUndo);
        //ToolbarItems.Add(itemRedo);
        //ToolbarItems.Add(itemZoomIn);
        //ToolbarItems.Add(itemZoomOut);
        //ToolbarItems.Add(itemFit);
        //ToolbarItems.Add(itemRotateL_90);
        //ToolbarItems.Add(itemRotateR_90);
        //ToolbarItems.Add(itemFlipH);
        //ToolbarItems.Add(itemFlipV);
        //ToolbarItems.Add(itemPrint);
        //ToolbarItems.Add(itemSelect); 
        //ToolbarItems.Add(itemSelection);
        //ToolbarItems.Add(itemCrop);
        //ToolbarItems.Add(itemBrush);
        //ToolbarItems.Add(itemText);

        #endregion
    }

    public void ResetImageEditIntensity()
    {
        ImageEditIntensityBasic = new ObservableCollection<ImageIntensity>(ImageEditIntensityList.Where(x => x.Category == ImageIntensityCategory.Basic).ToArray());
        ImageEditIntensityEffect = new ObservableCollection<ImageIntensity>(ImageEditIntensityList.Where(x => x.Category == ImageIntensityCategory.Effect).ToArray());
        ImageEditIntensityColor = new ObservableCollection<ImageIntensity>(ImageEditIntensityList.Where(x => x.Category == ImageIntensityCategory.Color).ToArray());
        ImageEditIntensityStylize = new ObservableCollection<ImageIntensity>(ImageEditIntensityList.Where(x => x.Category == ImageIntensityCategory.Stylize).ToArray());
    }

    private void SetImageSource(Bitmap? bitmap)
    {
        // 注意：不要在这里清除 _cachedSkBitmap，因为它可能已经被赋值给 CurrentImageEditItem.Image
        // 缓存清理应该只在历史切换时进行（见 HistoryHelper.ChangeImageSource）

        ImageSource = bitmap;
        HasImage = ImageSource != null;
        RefreshComputedProperties();

        // 通知图像大小变化（用于选择框边界检查）
        if (bitmap != null)
        {
            ImageSizeChanged?.Invoke(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
        }
        else
        {
            ImageSizeChanged?.Invoke(0, 0);
        }
    }

    #region 事件

    /// <summary>
    /// 选择框缩放变化事件（供 View 订阅）
    /// </summary>
    public event Action<double>? SelectionRectZoomChanged;

    /// <summary>
    /// 图像大小变化事件（供 View 订阅，用于更新选择框边界检查）
    /// </summary>
    public event Action<double, double>? ImageSizeChanged;

    #endregion

    #region 命令

    // 打印命令
    [RelayCommand]
    private async Task PrintImage()
    {
        if (!HasImage || ImageSource == null) return;

        _ = UIHelper.PrintImage(ImageSource);

    }

    // 保存命令
    [RelayCommand]
    private async Task SaveImage()
    {
        if (!HasImage || ImageSource == null) return;

        // 如果没有文件路径，直接另存为
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            await SaveImageAs();
            return;
        }

        // 如果文件未被修改，直接保存
        if (!History.IsModified)
        {
            await ImageHelper.SaveToFile(ImageSource, _currentFilePath);
            return;
        }

        // 文件已修改，显示对话框让用户选择
        var result = await ShowSaveDialogAsync();

        if (result == SaveImageResult.Overwrite)
        {
            // 覆盖源文件
            await ImageHelper.SaveToFile(ImageSource, _currentFilePath);
            History.IsModified = false;
        }
        else if (result == SaveImageResult.SaveAs)
        {
            await SaveImageAs();
        }
        // 用户取消，不做任何操作
    }

    [RelayCommand]
    public async void OpenFile()
    {
        var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;
        var topLevel = TopLevel.GetTopLevel(window);
        if (topLevel == null) return;
        var imageFileTypes = new FilePickerFileType("图片文件")
        {
            MimeTypes = new List<string>
                {
                    ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"
                }
        };

        var filePickerOptions = new FilePickerOpenOptions
        {
            Title = "打开图片",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType> { imageFileTypes }
        };

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(filePickerOptions);

        if (files.Count >= 1)
        {
            // 清除旧的历史
            History.ClearHistory();

            var selectedFile = files[0];
            await LoadSingleImageFileAsync(selectedFile);

            ImageList.Clear();
            OnPropertyChanged(nameof(HasMultipleImages));

            // 从文件路径获取文件夹并加载图片列表
            //var folderPath = System.IO.Path.GetDirectoryName(selectedFile.Path.LocalPath);

            //if (!string.IsNullOrEmpty(folderPath))
            //{
            //    await LoadImageListFromFolderAsync(folderPath, selectedFile.Path.LocalPath);
            //}
        }
    }

    /// <summary>
    /// 加载单个图片文件
    /// </summary>
    private async Task LoadSingleImageFileAsync(IStorageFile selectedFile)
    {
        await using var stream = await selectedFile.OpenReadAsync();
        var bitmap = new Bitmap(stream);
        SKBitmap skBitmap = ImageHelper.AvaloniaToSkBitmap(bitmap);

        Bitmap displayBitmap = ImageHelper.ConvertToAvaloniaBitmap(skBitmap, true, ZoomLevel);
        SetImageSource(displayBitmap);

        // 创建初始历史项，转换原图为 SKBitmap
        CurrentImageEditItem = new ImageEditHistoryItem()
        {
            Image = skBitmap,
            Action = new HistoryAction() { Edit = ImageEditAction.None, RotationAngle = 0 }
        };

        // 添加到历史
        History.AddHistory(CurrentImageEditItem);
        // 保存文件路径
        _currentFilePath = selectedFile.Path.LocalPath;

        // 重置状态
        //ZoomLevel = 1.0;

        PanX = 0;
        PanY = 0;

        ZoomLevel = Math.Min(this.ContainerSize.Width > skBitmap.Width ? 1 : this.ContainerSize.Width / skBitmap.Width, this.ContainerSize.Height / skBitmap.Height);

    }

    /// <summary>
    /// 从文件夹加载图片列表
    /// </summary>
    public async Task LoadImageListFromFolderAsync(string folderPath, string? selectFilePath = null)
    {
        if (string.IsNullOrEmpty(folderPath) || !System.IO.Directory.Exists(folderPath))
            return;

        CurrentFolderPath = folderPath;
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tiff", ".tif" };

        var files = System.IO.Directory.GetFiles(folderPath)
            .Where(f => imageExtensions.Contains(System.IO.Path.GetExtension(f).ToLower()))
            .OrderBy(f => f)
            .ToList();

        ImageList.Clear();

        foreach (var file in files)
        {
            var item = new ImageFileItem(file, new Bitmap(file));
            ImageList.Add(item);

            // 如果是当前选中的文件，标记选中
            if (!string.IsNullOrEmpty(selectFilePath) && file.Equals(selectFilePath, StringComparison.OrdinalIgnoreCase))
            {
                SelectedImageItem = item;
            }
        }

        OnPropertyChanged(nameof(HasMultipleImages));
    }

    /// <summary>
    /// 选中图片项时加载该图片
    /// </summary>
    [RelayCommand]
    public async Task SelectImageFromListAsync(ImageFileItem? item)
    {
        if (item == null || string.IsNullOrEmpty(item.FilePath))
            return;

        if (!System.IO.File.Exists(item.FilePath))
            return;

        // 清除旧的历史
        History.ClearHistory();

        await LoadImageFromPath(item.FilePath);
    }

    public async Task LoadImageFromPath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            return;

        try
        {
            // 清除旧的历史
            History.ClearHistory();

            using var stream = System.IO.File.OpenRead(filePath);//去掉using
            var bitmap = new Bitmap(stream);
            SKBitmap skBitmap = ImageHelper.AvaloniaToSkBitmap(bitmap);

            Bitmap displayBitmap = ImageHelper.ConvertToAvaloniaBitmap(skBitmap, true, ZoomLevel);
            SetImageSource(displayBitmap);

            // 创建初始历史项
            CurrentImageEditItem = new ImageEditHistoryItem()
            {
                Image = skBitmap,
                Action = new HistoryAction() { Edit = ImageEditAction.None, RotationAngle = 0 }
            };

            // 添加到历史
            History.AddHistory(CurrentImageEditItem);

            // 保存文件路径
            _currentFilePath = filePath;

            // 重置状态
            //ZoomLevel = 1.0;
            PanX = 0;
            PanY = 0;

            ZoomLevel = Math.Min(this.ContainerSize.Width > skBitmap.Width ? 1 : this.ContainerSize.Width / skBitmap.Width, this.ContainerSize.Height / skBitmap.Height);

        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading image from path: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Delete()
    {
        ZoomLevel = 1.0;
        SetImageSource(null);
        History.ClearHistory();
        ImageEditIntensityList = new UIImageEditIntensityList();
        ImageEditIntensityBasic = new ObservableCollection<ImageIntensity>(ImageEditIntensityList.Where(x => x.Category == ImageIntensityCategory.Basic).ToArray());
        ImageEditIntensityEffect = new ObservableCollection<ImageIntensity>(ImageEditIntensityList.Where(x => x.Category == ImageIntensityCategory.Effect).ToArray());
        ImageEditIntensityColor = new ObservableCollection<ImageIntensity>(ImageEditIntensityList.Where(x => x.Category == ImageIntensityCategory.Color).ToArray());
        ImageEditIntensityStylize = new ObservableCollection<ImageIntensity>(ImageEditIntensityList.Where(x => x.Category == ImageIntensityCategory.Stylize).ToArray());

        ClearSelection();
    }

    // 缩放命令
    [RelayCommand]
    private void ZoomIn()
    {
        if (!HasImage) return;
        var newZoom = ZoomLevel + 0.1;
        ZoomLevel = System.Math.Min(newZoom, MaxZoom);
    }

    [RelayCommand]
    private void ZoomOut()
    {
        if (!HasImage) return;
        var newZoom = ZoomLevel - 0.1;
        ZoomLevel = System.Math.Max(newZoom, MinZoom);
    }

    [RelayCommand]
    private void FitToWindow()
    {
        if (!HasImage || ImageSource == null) return;
        if (ContainerSize.Width <= 0 || ContainerSize.Height <= 0) return;

        var imageWidth = ImageSource.PixelSize.Width;
        var imageHeight = ImageSource.PixelSize.Height;

        double scaleX = ContainerSize.Width / imageWidth;
        double scaleY = ContainerSize.Height / imageHeight;
        double fitScale = System.Math.Min(scaleX, scaleY);

        ZoomLevel = System.Math.Max(fitScale, MinZoom);

        PanX = 0;
        PanY = 0;
    }

    [RelayCommand]
    private void ActualSize()
    {
        if (!HasImage) return;
        ZoomLevel = 1.0;
        PanX = 0;
        PanY = 0;
    }

    [RelayCommand]
    private void ZoomSize(double zoom)
    {
        if (!HasImage) return;
        ZoomLevel = System.Math.Clamp(zoom, MinZoom, MaxZoom);
    }

    // 撤销命令
    [RelayCommand]
    private void Undo()
    {
        Task.Run(() => History.Undo());
    }

    // 重做命令
    [RelayCommand]
    private void Redo()
    {
        Task.Run(() => History.Redo());
    }

    // 顺时针90度旋转
    [RelayCommand]
    private void RotateRight()
    {
        if (!HasImage || ImageSource == null) return;

        // 通过重新加载图片来实现旋转效果（使用内存流）
        var rotated = ImageHelper.CreateRotatedBitmap(CurrentImageEditItem.Image, 90);

        CurrentImageEditItem = new ImageEditHistoryItem()
        {
            Image = rotated,
            Action = new HistoryAction() { Edit = ImageEditAction.RotateRight, RotationAngle = 90 },
            Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
        };

        // 添加到历史
        History.AddHistory(CurrentImageEditItem);

        PreviewImageAdjust();
    }

    // 逆时针90度旋转
    [RelayCommand]
    private void RotateLeft()
    {
        if (!HasImage || ImageSource == null) return;

        var rotated = ImageHelper.CreateRotatedBitmap(CurrentImageEditItem.Image, -90);

        CurrentImageEditItem = new ImageEditHistoryItem()
        {
            Image = rotated,
            Action = new HistoryAction() { Edit = ImageEditAction.RotateLeft, RotationAngle = 90 },
            Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
        };

        // 添加到历史
        History.AddHistory(CurrentImageEditItem);

        PreviewImageAdjust();
    }

    // 水平翻转
    [RelayCommand]
    private void FlipHorizontal()
    {
        if (!HasImage || ImageSource == null) return;

        var flipped = ImageHelper.CreateFlippedBitmap(CurrentImageEditItem.Image, FlipMode.Horizontal);

        CurrentImageEditItem = new ImageEditHistoryItem()
        {
            Image = flipped,
            Action = new HistoryAction() { Edit = ImageEditAction.FlipHorizontal, FlipMode = FlipMode.Horizontal },
            Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
        };

        // 添加到历史
        History.AddHistory(CurrentImageEditItem);

        PreviewImageAdjust();
    }

    // 垂直翻转
    [RelayCommand]
    private void FlipVertical()
    {
        if (!HasImage || ImageSource == null) return;

        var flipped = ImageHelper.CreateFlippedBitmap(CurrentImageEditItem.Image, FlipMode.Vertical);

        CurrentImageEditItem = new ImageEditHistoryItem()
        {
            Image = flipped,
            Action = new HistoryAction() { Edit = ImageEditAction.FlipVertical, FlipMode = FlipMode.Vertical },
            Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
        };

        // 添加到历史
        History.AddHistory(CurrentImageEditItem);

        PreviewImageAdjust();

    }

    // 切换到选择工具
    [RelayCommand]
    private void SetNormalMode()
    {
        CurrentToolMode = EditToolMode.Normal;
        RightControl = new ImageEditControlPanel();
        ClearSelection();
    }

    // 切换到选择工具
    [RelayCommand]
    private void SetSelectMode()
    {
        CurrentToolMode = EditToolMode.Select;
        RightControl = new ImageEditControlPanel();
        ClearSelection();
    }

    // 切换到画笔工具
    [RelayCommand]
    private void SetBrushMode()
    {
        CurrentToolMode = EditToolMode.Brush;
        RightControl = new ImageEditBrushPanel();
        ClearSelection();
    }

    // 切换到文字工具
    [RelayCommand]
    private void SetTextMode()
    {
        CurrentToolMode = EditToolMode.Text;
        RightControl = new ImageEditTextPanel();
        ClearSelection();
    }
    
    // 选择画笔颜色命令
    [RelayCommand]
    public void SelectColor(Avalonia.Media.Color color)
    {
        CurrentBrushSettings.StrokeColor = color;
        CurrentBrushSettings.HasColor = true;
    }

    // 选择文字颜色命令
    [RelayCommand]
    public void SelectTextColor(Avalonia.Media.Color color)
    {
        CurrentTextSettings.FontColor = color;
        CurrentTextSettings.FontColorDisplay = new SolidColorBrush(color);
        CurrentTextSettings.HasColor = true;
    }

    // 裁剪命令
    [RelayCommand]
    private void Crop()
    {
        if (!HasImage || ImageSource == null || !HasSelection) return;

        var cropRect = GetNormalizedSelectionRect();
        var cropped = ImageHelper.CreateCroppedBitmap(CurrentImageEditItem.Image, cropRect);

        CurrentImageEditItem = new ImageEditHistoryItem()
        {
            Image = cropped,
            Action = new HistoryAction() { Edit = ImageEditAction.Crop },
            Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
        };

        // 添加到历史
        History.AddHistory(CurrentImageEditItem);

        PreviewImageAdjust();

        ClearSelection();
    }

    #endregion

    #region 画笔

    // 开始画笔绘制
    public void StartBrushStroke(Point screenPoint)
    {
        if (!HasImage || CurrentToolMode != EditToolMode.Brush)
            return;

        // 初始化防抖定时器
        _debounceTimer?.Stop();
        _debounceTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(DebounceInterval)
        };
        _debounceTimer.Tick += (sender, e) =>
        {
            _debounceTimer?.Stop();
            if (_incrementalStroke != null && _incrementalStroke.Count > 1)
            {
                IncrementalDrawBrushStroke();
                _lastDrawPoint = _incrementalStroke.Last();
                _incrementalStroke.Clear();
                _incrementalStroke.Add(_lastDrawPoint.Value);
            }
        };

        // 将屏幕坐标转换为原始图像坐标
        var imagePoint = ScreenToImage(screenPoint);
        IsDrawingBrush = true;
        CurrentStroke = new List<Point> { imagePoint };
        _lastDrawPoint = imagePoint;
        _incrementalStroke = new List<Point> { imagePoint };
    }

    // 继续画笔绘制
    public void ContinueBrushStroke(Point screenPoint)
    {
        if (!IsDrawingBrush || CurrentStroke == null || _incrementalStroke == null)
            return;

        // 将屏幕坐标转换为原始图像坐标
        var imagePoint = ScreenToImage(screenPoint);
        CurrentStroke.Add(imagePoint);
        _incrementalStroke.Add(imagePoint);

        // 使用防抖定时器控制绘制频率
        _debounceTimer?.Stop();
        _debounceTimer?.Start();
    }

    // 结束画笔绘制
    public void EndBrushStroke()
    {
        if (!IsDrawingBrush)
            return;

        IsDrawingBrush = false;

        // 停止防抖定时器
        _debounceTimer?.Stop();
        _debounceTimer = null;

        // 完成最后的增量绘制
        if (_incrementalStroke != null && _incrementalStroke.Count > 1)
        {
            IncrementalDrawBrushStroke();
        }

        if (ImageSource != null && _cachedSkBitmap != null)
        {
            // 复制缓存位图（避免共享引用导致释放问题）
            var finalBitmap = _cachedSkBitmap.Copy();

            // 更新历史记录
            CurrentImageEditItem = new ImageEditHistoryItem()
            {
                Image = finalBitmap,  // 使用副本
                Action = new HistoryAction() { Edit = ImageEditAction.Brush },
                Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
            };

            // 添加到历史
            History.AddHistory(CurrentImageEditItem);

            // 清理缓存（不再需要）
            ClearCachedSkBitmap();

            History.IsModified = true;
        }

        // 清理变量
        BrushStrokes.Clear();
        CurrentStroke = null;
        _lastDrawPoint = null;
        _incrementalStroke = null;
    }

    // 增量绘制画笔路径
    public void IncrementalDrawBrushStroke()
    {
        if (!HasImage || _incrementalStroke == null || _incrementalStroke.Count < 2)
            return;

        // 检查是否已经有缓存的位图
        if (_cachedSkBitmap == null)
        {
            // 如果没有缓存，从当前历史项复制一个（避免直接引用，防止被其他操作释放）
            var sourceBitmap = CurrentImageEditItem?.Image;
            if (sourceBitmap == null) return;

            // 使用 Copy() 创建独立副本，这样即使源被释放也不受影响
            _cachedSkBitmap = sourceBitmap.Copy();
        }

        // 直接在已有的 SKBitmap 上绘制新笔画（避免叠加加深）
        var scaledStrokeWidth = (float)(CurrentBrushSettings.StrokeWidth * ZoomLevel);
        var success = DrawingHelper.DrawStrokeOnSkBitmap(
            _cachedSkBitmap,
            _incrementalStroke,
            CurrentBrushSettings,
            scaledStrokeWidth);

        if (success)
        {
            // 转换为 Avalonia Bitmap 更新显示
            var newBitmap = DrawingHelper.SkBitmapToAvaloniaBitmap(_cachedSkBitmap, ZoomLevel);
            if (newBitmap != null)
            {
                ImageSource = newBitmap;
            }
        }

        // 记录已绘制的点，为最终提交做准备
        if (CurrentStroke != null)
        {
            CurrentStroke.AddRange(_incrementalStroke.Skip(1));
        }
        _lastDrawPoint = _incrementalStroke.Last();
        _incrementalStroke.Clear();
        _incrementalStroke.Add(_lastDrawPoint.Value);
    }

    // 将画笔绘制应用到图像
    private void ApplyBrushToImage()
    {
        if (ImageSource == null || BrushStrokes.Count == 0)
            return;

        try
        {
            // 使用当前显示的图像（包含所有增量绘制结果）作为源图像
            var sourceBitmap = ImageSource;

            // 获取或创建缓存的 SKBitmap（使用高效的直接像素复制）
            if (_cachedSkBitmap == null )
            {
                // 释放旧的缓存
                _cachedSkBitmap?.Dispose();

                // 使用高效方法：直接复制像素，避免编码/解码
                _cachedSkBitmap = ImageHelper.AvaloniaToSkBitmap(sourceBitmap);
            }

            if (_cachedSkBitmap == null)
                return;

            // 使用 DrawingHelper 应用画笔（使用缩放后的线宽：设置线宽 * 缩放级别）
            // 传递起始索引 BrushStrokes.Count - 1，只绘制新添加的笔画，避免重复绘制导致透明度叠加加深
            var scaledStrokeWidth = CurrentBrushSettings.StrokeWidth * ZoomLevel;
            var startIndex = BrushStrokes.Count > 0 ? BrushStrokes.Count - 1 : 0;
            var newBitmap = DrawingHelper.ApplyBrushToImage(
                _cachedSkBitmap,
                BrushStrokes,
                CurrentBrushSettings,
                scaledStrokeWidth,
                startIndex,
                ZoomLevel);

            if (newBitmap == null) return;

            // 更新显示
            SetImageSource(newBitmap);

            // 更新缓存（使用高效方法）
            _cachedSkBitmap?.Dispose();
            _cachedSkBitmap = ImageHelper.AvaloniaToSkBitmap(newBitmap);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyBrushToImage error: {ex.Message}");
        }
    }

    // 清除 SKBitmap 缓存
    public void ClearCachedSkBitmap()
    {
        _cachedSkBitmap?.Dispose();
        _cachedSkBitmap = null;
    }

    #endregion
    
    #region 文字

    // 文字相关属性
    [ObservableProperty]
    private Point _textInsertPoint;

    /// <summary>
    /// 文字插入点（显示用：屏幕坐标 = 图像坐标 * ZoomLevel）
    /// </summary>
    public Point TextInsertPointDisplay => new Point(TextInsertPoint.X * ZoomLevel, TextInsertPoint.Y * ZoomLevel);

    /// <summary>
    /// 文字输入框尺寸（根据显示字号动态调整）
    /// </summary>
    public Size TextInputBoxSize => new Size(
        Math.Max(120, CurrentTextSettings.FontSizeDisplay * 4),  // 宽度至少120，约4个字符宽度
        CurrentTextSettings.FontSizeDisplay + 8);  // 高度为显示字号加边距

    [ObservableProperty]
    private bool _isAddingText;

    // 开始添加文字
    public void StartAddText(Point screenPoint)
    {
        if (!HasImage || CurrentToolMode != EditToolMode.Text)
            return;

        // 将屏幕坐标转换为原始图像坐标
        var imagePoint = ScreenToImage(screenPoint);
        TextInsertPoint = imagePoint;
        OnPropertyChanged(nameof(TextInsertPointDisplay));
        IsAddingText = true;
    }

    // 结束添加文字（使用预处理的 Bitmap）
    public void EndAddText(string text, SKBitmap? processedBitmap = null)
    {
        if (!IsAddingText || string.IsNullOrWhiteSpace(text))
        {
            IsAddingText = false;
            return;
        }

        IsAddingText = false;

        // 如果已经处理好了 Bitmap，直接使用
        if (processedBitmap != null)
        {
            ApplyProcessedTextToImage(processedBitmap);
        }
        else
        {
            ApplyTextToImage(TextInsertPoint, text);
        }
    }

    /// <summary>
    /// 应用已处理的文字图像
    /// </summary>
    private void ApplyProcessedTextToImage(SKBitmap newBitmap)
    {
        try
        {
            // 更新历史记录
            CurrentImageEditItem = new ImageEditHistoryItem()
            {
                Image = newBitmap,
                Action = new HistoryAction() { Edit = ImageEditAction.AddText },
                Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
            };

            // 添加到历史
            History.AddHistory(CurrentImageEditItem);

            // 更新显示（使用当前缩放级别重新绘制棋格）
            SetImageSource(ImageHelper.ConvertToAvaloniaBitmap(newBitmap, true, ZoomLevel));

            History.IsModified = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyProcessedTextToImage error: {ex.Message}");
        }
    }

    // 结束添加文字（原始方法，保留兼容）
    public void EndAddText(string text)
    {
        if (!IsAddingText || string.IsNullOrWhiteSpace(text))
        {
            IsAddingText = false;
            return;
        }

        IsAddingText = false;
        ApplyTextToImage(TextInsertPoint, text);
    }

    // 取消添加文字
    public void CancelAddText()
    {
        IsAddingText = false;
    }

    // 将文字应用到图像
    private void ApplyTextToImage(Point position, string text)
    {
        if (ImageSource == null || CurrentImageEditItem?.Image == null || string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            var srcBitmap = CurrentImageEditItem.Image;

            var width = srcBitmap.Width;
            var height = srcBitmap.Height;

            // 创建目标 SkiaSharp 位图
            var destBitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(destBitmap);

            // 绘制原始图像
            canvas.DrawBitmap(srcBitmap, 0, 0);

            // 创建文字画笔
            var fontStyle = SKFontStyle.Normal;
            if (CurrentTextSettings.IsBold && CurrentTextSettings.IsItalic)
                fontStyle = SKFontStyle.BoldItalic;
            else if (CurrentTextSettings.IsBold)
                fontStyle = SKFontStyle.Bold;
            else if (CurrentTextSettings.IsItalic)
                fontStyle = SKFontStyle.Italic;

            // 创建 Typeface
            using var typeface = SKTypeface.FromFamilyName(
                CurrentTextSettings.FontFamilyName ?? "Arial",
                fontStyle);

            // 使用原始字号直接绘制到图像（不乘以 ZoomLevel）
            // FontSize 已经是用户设置的原始像素大小
            var scaledFontSize = (float)CurrentTextSettings.FontSize;

            using var font = new SKFont
            {
                Size = scaledFontSize,
                Typeface = typeface
            };

            var textPaint = new SKPaint
            {
                Color = new SKColor(
                    CurrentTextSettings.FontColor.R,
                    CurrentTextSettings.FontColor.G,
                    CurrentTextSettings.FontColor.B,
                    CurrentTextSettings.FontColor.A),
                IsAntialias = true,
                TextSize = scaledFontSize,
                Typeface = typeface,
                Style = SKPaintStyle.Fill
            };

            // 处理下划线
            if (CurrentTextSettings.IsUnderline)
            {
                textPaint.TextSkewX = 0;
            }

            // 绘制文字
            var x = (float)position.X;
            var y = (float)position.Y;

            // 处理多行文字（回车换行）
            string[] lines = text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            float lineHeight = textPaint.TextSize * 1.2f; // 行高为文字大小的1.2倍

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                float lineY = y + textPaint.TextSize + (i * lineHeight);
                canvas.DrawText(line, x, lineY, textPaint);

                // 如果需要下划线，绘制下划线
                if (CurrentTextSettings.IsUnderline)
                {
                    var textBounds = new SKRect();
                    textPaint.MeasureText(line, ref textBounds);

                    var underlinePaint = new SKPaint
                    {
                        Color = textPaint.Color,
                        StrokeWidth = Math.Max(1, scaledFontSize / 12),
                        Style = SKPaintStyle.Stroke
                    };

                    canvas.DrawLine(
                        x,
                        lineY + 3,
                        x + textBounds.Width,
                        lineY + 3,
                        underlinePaint);

                    underlinePaint.Dispose();
                }
            }

            textPaint.Dispose();

            // 将结果转换为 Avalonia Bitmap
            var newBitmap = ImageHelper.ConvertToAvaloniaBitmap(destBitmap, true, ZoomLevel);
            if (newBitmap == null) return;

            // 更新历史记录
            CurrentImageEditItem = new ImageEditHistoryItem()
            {
                Image = destBitmap,
                Action = new HistoryAction() { Edit = ImageEditAction.AddText },
                Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
            };

            // 添加到历史
            History.AddHistory(CurrentImageEditItem);

            // 更新显示
            SetImageSource(newBitmap);

            History.IsModified = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ApplyTextToImage error: {ex.Message}");
        }
    }

    #endregion

    #region 选择

    // 获取归一化的选择矩形（确保 x,y 为左上角，width,height 为正）
    // 注意：返回的是原始图像坐标，用于裁剪
    private Rect GetNormalizedSelectionRect()
    {
        if (SelectionRect.Width <= 0 || SelectionRect.Height <= 0)
        {
            return new Rect(0, 0, 0, 0);
        }
        return SelectionRect;
    }

    // 清除选择框
    [RelayCommand]
    private void ClearSelection()
    {
        SelectionStart = null;
        SelectionEnd = null;
        SelectionRect = new Rect(0, 0, 0, 0);
        IsSelecting = false;
        UseSelection = false;
        // 触发选择框变化事件
        SelectionRectChanged?.Invoke(SelectionRect);
    }

    // 开始选择
    public void StartSelection(Point screenPoint)
    {
        if (!HasImage || (EditToolMode.Select != CurrentToolMode))
        {
            return;
        }

        // 将屏幕坐标转换为原始图像坐标
        var imagePoint = ScreenToImage(screenPoint);
        SelectionStart = imagePoint;
        SelectionEnd = imagePoint;
        IsSelecting = true;
        UseSelection = true;
        UpdateSelectionRect();
    }

    // 更新选择
    public void UpdateSelection(Point screenPoint)
    {
        if (!IsSelecting || SelectionStart == null)
            return;
        // 将屏幕坐标转换为原始图像坐标
        var imagePoint = ScreenToImage(screenPoint);
        SelectionEnd = imagePoint;
        UpdateSelectionRect();
    }

    // 结束选择
    public void EndSelection()
    {
        IsSelecting = false;
    }

    // 更新选择矩形
    public void UpdateSelectionRect()
    {
        if (SelectionStart == null || SelectionEnd == null || ImageSource == null)
        {
            SelectionRect = new Rect(0, 0, 0, 0);
            // 触发选择框变化事件
            SelectionRectChanged?.Invoke(SelectionRect);
            return;
        }

        // SelectionStart/SelectionEnd 已经是原始图像坐标
        var imgWidth = ImageSource.PixelSize.Width;
        var imgHeight = ImageSource.PixelSize.Height;

        var x = Math.Max(0, Math.Min(SelectionStart.Value.X, SelectionEnd.Value.X));
        var y = Math.Max(0, Math.Min(SelectionStart.Value.Y, SelectionEnd.Value.Y));
        var width = Math.Min(imgWidth - x, Math.Abs(SelectionEnd.Value.X - SelectionStart.Value.X));
        var height = Math.Min(imgHeight - y, Math.Abs(SelectionEnd.Value.Y - SelectionStart.Value.Y));

        // SelectionRect 存储为原始图像坐标（显示时用 SelectionRectDisplay）
        SelectionRect = new Rect(x, y, width, height);
        OnPropertyChanged(nameof(SelectionRectDisplay));

        // 通知选择框变化
        SelectionRectChanged?.Invoke(SelectionRect);
    }

    #endregion

    /// <summary>
    /// 保存图像结果
    /// </summary>
    public enum SaveImageResult
    {
        Overwrite,  // 覆盖源文件
        SaveAs,     // 另存为
        Cancel      // 取消
    }

    /// <summary>
    /// 显示保存选项对话框
    /// </summary>
    private async Task<SaveImageResult> ShowSaveDialogAsync()
    {
        var result = SaveImageResult.Cancel;

        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                        ? desktop.MainWindow
                        : null;

            if (window == null)
            {
                result = SaveImageResult.SaveAs;
                return;
            }

            var dialog = new Window
            {
                Title = "保存图片",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                ShowInTaskbar = false,
                Padding = new Thickness(20)
            };

            var panel = new StackPanel { Spacing = 15 };

            var messageText = new TextBlock
            {
                Text = "图片已修改，请选择保存方式：",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var overwriteButton = new Button { Content = "覆盖源文件", MinWidth = 100 };
            var saveAsButton = new Button { Content = "另存为", MinWidth = 80 };
            var cancelButton = new Button { Content = "取消", MinWidth = 60 };

            overwriteButton.Click += (s, e) => { result = SaveImageResult.Overwrite; dialog.Close(); };
            saveAsButton.Click += (s, e) => { result = SaveImageResult.SaveAs; dialog.Close(); };
            cancelButton.Click += (s, e) => { result = SaveImageResult.Cancel; dialog.Close(); };

            buttonPanel.Children.Add(overwriteButton);
            buttonPanel.Children.Add(saveAsButton);
            buttonPanel.Children.Add(cancelButton);

            panel.Children.Add(messageText);
            panel.Children.Add(buttonPanel);

            dialog.Content = panel;

            await dialog.ShowDialog(window);
        });

        return result;
    }

    /// <summary>
    /// 另存为
    /// </summary>
    private async Task SaveImageAs()
    {
        var filePath = UIHelper.ShowSaveFilePicker(_currentFilePath ?? "");
        if (!string.IsNullOrEmpty(filePath))
        {
            await ImageHelper.SaveToFile(ImageSource, filePath);
            _currentFilePath = filePath;
            History.IsModified = false;
        }
    }

    /// <summary>
    /// 显示当前图像（从当前历史项）
    /// </summary>

    /// <summary>
    /// 应用所有图像调整效果（返回 SKBitmap，支持取消）
    /// </summary>
    private SKBitmap? ApplyAllAdjustmentsInternal(SKBitmap source, CancellationToken token)
    {
        if (source == null)
            return null;

        // 获取所有参数值
        // 基础调整
        int brightness = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Brightness)?.Value ?? 0;
        int contrast = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Contrast)?.Value ?? 0;
        int exposure = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Exposure)?.Value ?? 0;

        // 颜色调整
        int saturation = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Saturation)?.Value ?? 0;
        int vibrance = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Vibrance)?.Value ?? 0;
        int temperature = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Temperature)?.Value ?? 0;
        int hue = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Hue)?.Value ?? 0;

        // 效果
        int blur = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Blur)?.Value ?? 0;
        int sharpen = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Sharpen)?.Value ?? 0;
        int denoise = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Denoise)?.Value ?? 0;
        int depthOfField = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.DepthOfField)?.Value ?? 0;

        // 风格化
        int grayscale = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.GrayScale)?.Value ?? 0;
        int sepia = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Sepia)?.Value ?? 0;
        int invert = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Invert)?.Value ?? 0;
        int emboss = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Emboss)?.Value ?? 0;
        int comic = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Comic)?.Value ?? 0;
        // 新效果
        int edgeDetection = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.EdgeDetection)?.Value ?? 0;
        int oilPaint = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.OilPaint)?.Value ?? 0;
        int watercolor = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Watercolor)?.Value ?? 0;
        int sketch = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Sketch)?.Value ?? 0;
        int mosaic = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Mosaic)?.Value ?? 0;
        int fishEye = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.FishEye)?.Value ?? 0;
        int colorBalanceRed = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.ColorBalance && x.Channel == ImageHelper.ColorChannel.Red)?.Value ?? 0;
        int colorBalanceGreen = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.ColorBalance && x.Channel == ImageHelper.ColorChannel.Green)?.Value ?? 0;
        int colorBalanceBlue = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.ColorBalance && x.Channel == ImageHelper.ColorChannel.Blue)?.Value ?? 0;

        // 新增效果
        int vignette = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Vignette)?.Value ?? 0;
        int grain = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Grain)?.Value ?? 0;
        int defog = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Defog)?.Value ?? 0;
        int highlights = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Highlights)?.Value ?? 0;
        int shadows = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Shadows)?.Value ?? 0;

        // 用于追踪当前和之前的位图，以便正确释放
        SKBitmap? currentBitmap = source;
        SKBitmap? previousBitmap = null;

        try
        {
            // 检查取消
            if (token.IsCancellationRequested)
                return null;

            // 按顺序应用调整
            // === 效果 ===
            if (denoise > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateDenoiseBitmap(currentBitmap, denoise);
                previousBitmap?.Dispose();
            }

            if (blur > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateBlurBitmap(currentBitmap, blur);
                previousBitmap?.Dispose();
            }

            if (sharpen > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateSharpenBitmap(currentBitmap, sharpen);
                previousBitmap?.Dispose();
            }

            if (depthOfField > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateDepthOfFieldBitmap(currentBitmap, depthOfField);
                previousBitmap?.Dispose();
            }

            if (token.IsCancellationRequested)
                return null;

            // === 基础调整 ===
            if (brightness != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateBrightnessBitmap(currentBitmap, brightness);
                previousBitmap?.Dispose();
            }

            if (exposure != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateExposureBitmap(currentBitmap, exposure);
                previousBitmap?.Dispose();
            }

            if (contrast != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateContrastBitmap(currentBitmap, contrast);
                previousBitmap?.Dispose();
            }

            if (token.IsCancellationRequested)
                return null;

            // === 颜色调整 ===
            if (saturation != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateSaturationBitmap(currentBitmap, saturation);
                previousBitmap?.Dispose();
            }

            if (vibrance != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateVibranceBitmap(currentBitmap, vibrance);
                previousBitmap?.Dispose();
            }

            if (temperature != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateTemperatureBitmap(currentBitmap, temperature);
                previousBitmap?.Dispose();
            }

            if (hue != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateHueBitmap(currentBitmap, hue);
                previousBitmap?.Dispose();
            }

            if (token.IsCancellationRequested)
                return null;

            // === 风格化 ===
            if (sepia > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateSepiaBitmap(currentBitmap, sepia);
                previousBitmap?.Dispose();
            }

            if (grayscale > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateGrayscaleBitmap(currentBitmap, grayscale);
                previousBitmap?.Dispose();
            }

            if (invert > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateInvertBitmap(currentBitmap, invert);
                previousBitmap?.Dispose();
            }

            if (emboss > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateEmbossBitmap(currentBitmap, emboss);
                previousBitmap?.Dispose();
            }

            if (comic > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateComicBitmap(currentBitmap, comic);
                previousBitmap?.Dispose();
            }

            if (token.IsCancellationRequested)
                return null;

            // === 新效果 ===
            if (edgeDetection > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateEdgeDetectionBitmap(currentBitmap, edgeDetection);
                previousBitmap?.Dispose();
            }

            if (oilPaint > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateOilPaintBitmap(currentBitmap, oilPaint);
                previousBitmap?.Dispose();
            }

            if (watercolor > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateWatercolorBitmap(currentBitmap, watercolor);
                previousBitmap?.Dispose();
            }

            if (sketch > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateSketchBitmap(currentBitmap, sketch);
                previousBitmap?.Dispose();
            }

            if (mosaic > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateMosaicBitmap(currentBitmap, mosaic);
                previousBitmap?.Dispose();
            }

            if (fishEye > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateFishEyeBitmap(currentBitmap, fishEye);
                previousBitmap?.Dispose();
            }

            if (colorBalanceRed != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateColorBalanceBitmap(currentBitmap, colorBalanceRed, ImageHelper.ColorChannel.Red);
                previousBitmap?.Dispose();
            }

            if (colorBalanceGreen != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateColorBalanceBitmap(currentBitmap, colorBalanceGreen, ImageHelper.ColorChannel.Green);
                previousBitmap?.Dispose();
            }

            if (colorBalanceBlue != 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateColorBalanceBitmap(currentBitmap, colorBalanceBlue, ImageHelper.ColorChannel.Blue);
                previousBitmap?.Dispose();
            }

            if (token.IsCancellationRequested)
                return null;

            // 新增效果
            if (vignette > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateVignetteBitmap(currentBitmap, vignette);
                previousBitmap?.Dispose();
            }

            if (grain > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateGrainBitmap(currentBitmap, grain);
                previousBitmap?.Dispose();
            }

            if (defog > 0 && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateDefogBitmap(currentBitmap, defog);
                previousBitmap?.Dispose();
            }

            if ((highlights != 0 || shadows != 0) && !token.IsCancellationRequested)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateHighlightsShadowsBitmap(currentBitmap, highlights, shadows);
                previousBitmap?.Dispose();
            }

            if (token.IsCancellationRequested)
            {
                if (currentBitmap != source && currentBitmap != null)
                {
                    try { currentBitmap?.Dispose(); } catch { }
                }
                return null;
            }

            // 如果没有应用任何效果，返回原始图像的副本
            if (currentBitmap == source)
            {
                try
                {
                    var copy = source.Copy();
                    return copy;
                }
                catch
                {
                    return null;
                }
            }

            return currentBitmap;
        }
        catch
        {
            // 异常安全：如果在应用效果过程中发生异常，确保清理
            if (currentBitmap != source && currentBitmap != null)
            {
                try { currentBitmap?.Dispose(); } catch { }
            }
            return null;
        }
    }

    /// <summary>
    /// 应用所有图像调整效果（原有方法，保持兼容）
    /// </summary>
    private Bitmap ApplyAllAdjustments(SKBitmap source)
    {
        if (source == null)
            return null!;

        // 获取所有参数值
        // 基础调整
        int brightness = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Brightness)?.Value ?? 0;
        int contrast = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Contrast)?.Value ?? 0;
        int exposure = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Exposure)?.Value ?? 0;

        // 颜色调整
        int saturation = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Saturation)?.Value ?? 0;
        int vibrance = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Vibrance)?.Value ?? 0;
        int temperature = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Temperature)?.Value ?? 0;
        int hue = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Hue)?.Value ?? 0;

        // 效果
        int blur = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Blur)?.Value ?? 0;
        int sharpen = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Sharpen)?.Value ?? 0;
        int denoise = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Denoise)?.Value ?? 0;
        int depthOfField = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.DepthOfField)?.Value ?? 0;

        // 风格化
        int grayscale = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.GrayScale)?.Value ?? 0;
        int sepia = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Sepia)?.Value ?? 0;
        int invert = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Invert)?.Value ?? 0;
        int emboss = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Emboss)?.Value ?? 0;
        int comic = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Comic)?.Value ?? 0;
        // 新效果
        int edgeDetection = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.EdgeDetection)?.Value ?? 0;
        int oilPaint = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.OilPaint)?.Value ?? 0;
        int watercolor = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Watercolor)?.Value ?? 0;
        int sketch = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Sketch)?.Value ?? 0;
        int mosaic = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Mosaic)?.Value ?? 0;
        int fishEye = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.FishEye)?.Value ?? 0;
        int colorBalanceRed = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.ColorBalance && x.Channel == ImageHelper.ColorChannel.Red)?.Value ?? 0;
        int colorBalanceGreen = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.ColorBalance && x.Channel == ImageHelper.ColorChannel.Green)?.Value ?? 0;
        int colorBalanceBlue = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.ColorBalance && x.Channel == ImageHelper.ColorChannel.Blue)?.Value ?? 0;

        // 新增效果
        int vignette = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Vignette)?.Value ?? 0;
        int grain = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Grain)?.Value ?? 0;
        int defog = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Defog)?.Value ?? 0;
        int highlights = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Highlights)?.Value ?? 0;
        int shadows = ImageEditIntensityList.FirstOrDefault(x => x.Mode == ImageEditAction.Shadows)?.Value ?? 0;

        // 用于追踪当前和之前的位图，以便正确释放
        SKBitmap? currentBitmap = source;
        SKBitmap? previousBitmap = null;

        try
        {
            // 按顺序应用调整
            // === 效果 ===
            if (denoise > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateDenoiseBitmap(currentBitmap, denoise);
                previousBitmap?.Dispose();
            }

            if (blur > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateBlurBitmap(currentBitmap, blur);
                previousBitmap?.Dispose();
            }

            if (sharpen > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateSharpenBitmap(currentBitmap, sharpen);
                previousBitmap?.Dispose();
            }

            if (depthOfField > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateDepthOfFieldBitmap(currentBitmap, depthOfField);
                previousBitmap?.Dispose();
            }

            // === 基础调整 ===
            if (brightness != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateBrightnessBitmap(currentBitmap, brightness);
                previousBitmap?.Dispose();
            }

            if (exposure != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateExposureBitmap(currentBitmap, exposure);
                previousBitmap?.Dispose();
            }

            if (contrast != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateContrastBitmap(currentBitmap, contrast);
                previousBitmap?.Dispose();
            }

            // === 颜色调整 ===
            if (saturation != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateSaturationBitmap(currentBitmap, saturation);
                previousBitmap?.Dispose();
            }

            if (vibrance != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateVibranceBitmap(currentBitmap, vibrance);
                previousBitmap?.Dispose();
            }

            if (temperature != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateTemperatureBitmap(currentBitmap, temperature);
                previousBitmap?.Dispose();
            }

            if (hue != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateHueBitmap(currentBitmap, hue);
                previousBitmap?.Dispose();
            }

            // === 风格化 ===
            if (sepia > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateSepiaBitmap(currentBitmap, sepia);
                previousBitmap?.Dispose();
            }

            if (grayscale > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateGrayscaleBitmap(currentBitmap, grayscale);
                previousBitmap?.Dispose();
            }

            if (invert > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateInvertBitmap(currentBitmap, invert);
                previousBitmap?.Dispose();
            }

            if (emboss > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateEmbossBitmap(currentBitmap, emboss);
                previousBitmap?.Dispose();
            }

            if (comic > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateComicBitmap(currentBitmap, comic);
                previousBitmap?.Dispose();
            }

            // === 新效果 ===
            if (edgeDetection > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateEdgeDetectionBitmap(currentBitmap, edgeDetection);
                previousBitmap?.Dispose();
            }

            if (oilPaint > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateOilPaintBitmap(currentBitmap, oilPaint);
                previousBitmap?.Dispose();
            }

            if (watercolor > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateWatercolorBitmap(currentBitmap, watercolor);
                previousBitmap?.Dispose();
            }

            if (sketch > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateSketchBitmap(currentBitmap, sketch);
                previousBitmap?.Dispose();
            }

            if (mosaic > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateMosaicBitmap(currentBitmap, mosaic);
                previousBitmap?.Dispose();
            }

            if (fishEye > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateFishEyeBitmap(currentBitmap, fishEye);
                previousBitmap?.Dispose();
            }

            if (colorBalanceRed != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateColorBalanceBitmap(currentBitmap, colorBalanceRed, ImageHelper.ColorChannel.Red);
                previousBitmap?.Dispose();
            }

            if (colorBalanceGreen != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateColorBalanceBitmap(currentBitmap, colorBalanceGreen, ImageHelper.ColorChannel.Green);
                previousBitmap?.Dispose();
            }

            if (colorBalanceBlue != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateColorBalanceBitmap(currentBitmap, colorBalanceBlue, ImageHelper.ColorChannel.Blue);
                previousBitmap?.Dispose();
            }

            // 新增效果
            if (vignette > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateVignetteBitmap(currentBitmap, vignette);
                previousBitmap?.Dispose();
            }

            if (grain > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateGrainBitmap(currentBitmap, grain);
                previousBitmap?.Dispose();
            }

            if (defog > 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateDefogBitmap(currentBitmap, defog);
                previousBitmap?.Dispose();
            }

            if (highlights != 0 || shadows != 0)
            {
                previousBitmap = currentBitmap;
                currentBitmap = ImageHelper.CreateHighlightsShadowsBitmap(currentBitmap, highlights, shadows);
                previousBitmap?.Dispose();
            }

            // 转换为 Avalonia Bitmap
            var result = ImageHelper.ConvertToAvaloniaBitmap(currentBitmap, true, ZoomLevel);

            // 释放中间位图（注意：ConvertToAvaloniaBitmap 内部会复制 bitmap，所以原位图可以安全释放）
            // 只有当 currentBitmap 与 source 不同时才需要释放（如果相同，说明没有应用任何效果，不应释放）
            if (currentBitmap != source)
            {
                currentBitmap?.Dispose();
            }

            return result;
        }
        finally
        {
            // 异常安全：如果在应用效果过程中发生异常，确保清理
            // 正常流程下，currentBitmap 已在 return 前被释放
            if (currentBitmap != source && currentBitmap != null)
            {
                try
                {
                    currentBitmap?.Dispose();
                }
                catch { /* 忽略已释放异常 */ }
            }
        }
    }

    /// <summary>
    /// 临时预览图像调整效果（滑块拖动时调用）
    /// </summary>
    public void PreviewImageAdjust()
    {
        // 取消之前的预览操作
        _previewCts?.Cancel();
        _previewCts?.Dispose();
        _previewCts = new CancellationTokenSource();

        // 捕获当前状态，避免闭包引用问题
        var token = _previewCts.Token;
        var hasImage = HasImage;
        var currentItem = CurrentImageEditItem;

        // 在锁内获取图像数据
        SKBitmap? baseImage;
        lock (_imageProcessingLock)
        {
            if (!hasImage || currentItem?.Image == null)
                return;

            // 创建图像的深拷贝，避免并发修改问题
            try
            {
                baseImage = currentItem.Image.Copy();
            }
            catch
            {
                return;
            }
        }

        // 使用 Task.Run 在后台线程处理图像，避免阻塞 UI
        Task.Run(async () =>
        {
            try
            {
                // 检查是否已取消
                if (token.IsCancellationRequested)
                {
                    baseImage.Dispose();
                    return;
                }

                // 应用所有调整
                SKBitmap? processedBitmap = null;
                lock (_imageProcessingLock)
                {
                    try
                    {
                        processedBitmap = ApplyAllAdjustmentsInternal(baseImage, token);
                    }
                    catch
                    {
                        // 忽略处理错误
                    }
                    finally
                    {
                        baseImage.Dispose();
                    }
                }

                if (processedBitmap == null || token.IsCancellationRequested)
                {
                    processedBitmap?.Dispose();
                    return;
                }

                // 转换为 Avalonia Bitmap
                Bitmap? displayBitmap = null;
                try
                {
                    displayBitmap = ImageHelper.ConvertToAvaloniaBitmap(processedBitmap, true, ZoomLevel);
                }
                catch
                {
                    // 忽略转换错误
                }
                finally
                {
                    processedBitmap.Dispose();
                }

                if (displayBitmap == null || token.IsCancellationRequested)
                {
                    displayBitmap?.Dispose();
                    return;
                }

                // 在 UI 线程更新
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (token.IsCancellationRequested)
                    {
                        displayBitmap?.Dispose();
                        return;
                    }

                    try
                    {
                        SetImageSource(displayBitmap);
                    }
                    catch
                    {
                        // 忽略设置错误
                    }
                });
            }
            catch
            {
                // 忽略所有错误
            }
        }, token);
    }

    /// <summary>
    /// 应用图像调整效果（滑块释放时调用）
    /// </summary>
    public void ApplyImageAdjust(ImageIntensity intensity)
    {
        if (!HasImage || CurrentImageEditItem.Image == null)
            return;

        // 保存当前调整参数到历史项
        CurrentImageEditItem = new ImageEditHistoryItem()
        {
            Image = CurrentImageEditItem.Image,
            Action = new HistoryAction() { Edit = intensity?.Mode ?? ImageEditAction.GrayScale },
            Properties = new HistoryProperty() { IntensityList = (UIImageEditIntensityList)ImageEditIntensityList.Clone() }
        };

        // 添加到历史
        History.AddHistory(CurrentImageEditItem);
    }

    public override void UpdateLanguageTexts()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 刷新计算属性（ScaledImageSize, NeedVerticalScroll, NeedHorizontalScroll）
    /// </summary>
    public void RefreshComputedProperties()
    {
        OnPropertyChanged(nameof(ScaledImageSize));
    }

    /// <summary>
    /// 当 ZoomLevel 改变时刷新计算属性
    /// </summary>
    partial void OnZoomLevelChanged(double value)
    {
        RefreshComputedProperties();
        // 缩放变化时刷新显示用属性，避免框选/文字位置错位
        OnPropertyChanged(nameof(SelectionRectDisplay));
        OnPropertyChanged(nameof(TextInsertPointDisplay));

        // 通知选择框帮助类更新缩放
        SelectionRectZoomChanged?.Invoke(value);

        // 更新文字字号和画笔线宽的显示值
        CurrentTextSettings.FontSizeDisplay = CurrentTextSettings.FontSize * value;
        CurrentTextSettings.NotifyFontSizeDisplayChanged();

        // 更新输入框尺寸（依赖于 FontSizeDisplay）
        OnPropertyChanged(nameof(TextInputBoxSize));

        CurrentBrushSettings.StrokeWidthDisplay = CurrentBrushSettings.StrokeWidth * value;
        CurrentBrushSettings.NotifyStrokeWidthDisplayChanged();

        // 缩放变化时重新渲染棋格背景（保持视觉大小一致）
        RefreshCheckerboardDisplay();
    }

    /// <summary>
    /// 重新渲染棋格背景（使用当前缩放级别）
    /// </summary>
    private void RefreshCheckerboardDisplay()
    {
        if (!HasImage || CurrentImageEditItem?.Image == null)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshCheckerboardDisplay: HasImage={HasImage}, CurrentImageEditItem.Image is null");
            return;
        }

        try
        {
            System.Diagnostics.Debug.WriteLine($"RefreshCheckerboardDisplay: ZoomLevel={ZoomLevel}, ImageSize={CurrentImageEditItem.Image.Width}x{CurrentImageEditItem.Image.Height}");

            // 使用当前缩放级别重新渲染显示
            var displayBitmap = ImageHelper.ConvertToAvaloniaBitmap(
                CurrentImageEditItem.Image,
                true,
                ZoomLevel);
            SetImageSource(displayBitmap);
            System.Diagnostics.Debug.WriteLine($"RefreshCheckerboardDisplay: Done. DisplayBitmap size={displayBitmap?.PixelSize.Width}x{displayBitmap?.PixelSize.Height}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"RefreshCheckerboardDisplay error: {ex.Message}");
        }
    }

}
