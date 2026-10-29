using Avalonia.Collections;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace yuyiAvalonia.UI.Models;

/// <summary>
/// 线条位置（Inside=内侧，Outside=外侧，Center=居中）
/// </summary>
public enum StrokePosition
{
    Center,
    Inside,
    Outside,
}

/// <summary>
/// 拐角连接类型
/// </summary>
public enum StrokeJoin
{
    Bevel,
    Miter, 
    Round 
}

/// <summary>
/// 端点样式
/// </summary>
public enum StrokeEndpoints
{
    Butt = 0,
    Round = 1,
    Square = 2
}

public enum StrokeType
{
    Solid = 0,
    Dashed = 1,
    Custom = 2,
}

public enum StrokeStyleType
{
    Solid = 0,
    Dashed = 1,
    Dot = 2,
    DashDot = 3,
    DashDotDot = 4,
    Custom
}

public enum StrokeWidthMode
{
    Uniform = 0,
    Individual = 1
}

public enum StrokeFillType
{
    None = 0,
    Solid = 1,
    Gradient = 2,
    Image = 3
}

/// <summary>
/// 阴影类型
/// </summary>
public enum ShadowType
{
    Outer,
    Inner
}
