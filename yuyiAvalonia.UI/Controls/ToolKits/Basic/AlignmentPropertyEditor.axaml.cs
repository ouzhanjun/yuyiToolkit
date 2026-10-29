using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;

namespace yuyiAvalonia.UI.Controls;

public partial class AlignmentPropertyEditor : UserControl
{
    public static readonly StyledProperty<HorizontalAlignment> HorizontalAlignmentProperty =
        AvaloniaProperty.Register<AlignmentPropertyEditor, HorizontalAlignment>(
            nameof(HorizontalAlignment),
            defaultValue: HorizontalAlignment.Left,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<VerticalAlignment> VerticalAlignmentProperty =
        AvaloniaProperty.Register<AlignmentPropertyEditor, VerticalAlignment>(
            nameof(VerticalAlignment),
            defaultValue: VerticalAlignment.Top,
            defaultBindingMode: BindingMode.TwoWay);

    public HorizontalAlignment HorizontalAlignment
    {
        get => GetValue(HorizontalAlignmentProperty);
        set => SetValue(HorizontalAlignmentProperty, value);
    }

    public VerticalAlignment VerticalAlignment
    {
        get => GetValue(VerticalAlignmentProperty);
        set => SetValue(VerticalAlignmentProperty, value);
    }

    private bool _isUpdating; // 防止循环更新

    public AlignmentPropertyEditor()
    {
        InitializeComponent();

        // 监听属性变化，当绑定值改变时更新按钮选择状态
        HorizontalAlignmentProperty.Changed.AddClassHandler<AlignmentPropertyEditor>((control, e) =>
        {
            if (!_isUpdating)
            {
                control.UpdateHorizontalButtonSelection();
            }
        });

        VerticalAlignmentProperty.Changed.AddClassHandler<AlignmentPropertyEditor>((control, e) =>
        {
            if (!_isUpdating)
            {
                control.UpdateVerticalButtonSelection();
            }
        });

        Loaded += (s, e) =>
        {
            UpdateButtonSelection();

            if (PART_HorizontalGroup != null)
            {
                PART_HorizontalGroup.SelectionChanged += OnHorizontalSelectionChanged;
            }

            if (PART_VerticalGroup != null)
            {
                PART_VerticalGroup.SelectionChanged += OnVerticalSelectionChanged;
            }
        };
    }

    private void OnHorizontalSelectionChanged(object? sender, int index)
    {
        if (_isUpdating) return;

        _isUpdating = true;
        // 索引 0->Left(1), 1->Center(2), 2->Right(3)
        // Stretch(0) 无法通过按钮选择，需要通过绑定
        var alignment = index switch
        {
            0 => HorizontalAlignment.Stretch,
            1 => HorizontalAlignment.Left,
            2 => HorizontalAlignment.Center,
            3 => HorizontalAlignment.Right,
            _ => HorizontalAlignment.Left
        };

        SetValue(HorizontalAlignmentProperty, alignment);
        _isUpdating = false;
    }

    private void OnVerticalSelectionChanged(object? sender, int index)
    {
        if (_isUpdating) return;

        _isUpdating = true;
        // 索引 0->Top(1), 1->Center(2), 2->Bottom(3)
        // Stretch(0) 无法通过按钮选择，需要通过绑定
        var alignment = index switch
        {
            0 => VerticalAlignment.Stretch,
            1 => VerticalAlignment.Top,
            2 => VerticalAlignment.Center,
            3 => VerticalAlignment.Bottom,
            _ => VerticalAlignment.Top
        };

        SetValue(VerticalAlignmentProperty, alignment);
        _isUpdating = false;
    }

    private void UpdateButtonSelection()
    {
        UpdateHorizontalButtonSelection();
        UpdateVerticalButtonSelection();
    }

    private void UpdateHorizontalButtonSelection()
    {
        if (PART_HorizontalGroup != null)
        {
            var index = HorizontalAlignment switch
            {
                HorizontalAlignment.Stretch => 0,
                HorizontalAlignment.Left => 1,
                HorizontalAlignment.Center => 2,
                HorizontalAlignment.Right => 3,
                _ => 0
            };
            PART_HorizontalGroup.SelectedIndex = index;
        }
    }

    private void UpdateVerticalButtonSelection()
    {
        if (PART_VerticalGroup != null)
        {
            var index = VerticalAlignment switch
            {
                VerticalAlignment.Stretch => 0,
                VerticalAlignment.Top => 1,
                VerticalAlignment.Center => 2,
                VerticalAlignment.Bottom => 3,
                _ => 0
            };
            PART_VerticalGroup.SelectedIndex = index;
        }
    }
}