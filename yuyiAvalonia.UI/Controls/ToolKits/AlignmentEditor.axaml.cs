using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using System;

namespace yuyiAvalonia.UI.Controls;

public partial class AlignmentEditor : UserControl
{
    public static readonly StyledProperty<HorizontalAlignment> HorizontalAlignmentProperty =
        AvaloniaProperty.Register<AlignmentEditor, HorizontalAlignment>(
            nameof(HorizontalAlignment),
            defaultValue: HorizontalAlignment.Left,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<VerticalAlignment> VerticalAlignmentProperty =
        AvaloniaProperty.Register<AlignmentEditor, VerticalAlignment>(
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

    public AlignmentEditor()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            if (PART_HorizontalGroup != null)
            {
                PART_HorizontalGroup.SelectionChanged += (sender, args) =>
                {
                    HorizontalAlignment = (HorizontalAlignment)PART_HorizontalGroup.SelectedIndex;
                };
            }

            if (PART_VerticalGroup != null)
            {
                PART_VerticalGroup.SelectionChanged += (sender, args) =>
                {
                    VerticalAlignment = (VerticalAlignment)PART_VerticalGroup.SelectedIndex;
                };
            }

            UpdateButtonSelection();
        };

        HorizontalAlignmentProperty.Changed.AddClassHandler<AlignmentEditor>((control, e) =>
        {
            control.UpdateButtonSelection();
        });

        VerticalAlignmentProperty.Changed.AddClassHandler<AlignmentEditor>((control, e) =>
        {
            control.UpdateButtonSelection();
        });
    }

    private void UpdateButtonSelection()
    {
        if (PART_HorizontalGroup != null)
        {
            PART_HorizontalGroup.SelectedIndex = (int)HorizontalAlignment;
        }
        if (PART_VerticalGroup != null)
        {
            PART_VerticalGroup.SelectedIndex = (int)VerticalAlignment;
        }
    }
}