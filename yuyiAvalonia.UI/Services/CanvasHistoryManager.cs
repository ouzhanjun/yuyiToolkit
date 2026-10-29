using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json;
using yuyiAvalonia.UI.Models;
using yuyiAvalonia.UI.ViewModels;

namespace yuyiAvalonia.UI.Services;

/// <summary>
/// 画布历史记录管理器 - 简化版
/// 使用单一集合配合索引实现撤销/重做
/// </summary>
public partial class CanvasHistoryManager : ObservableObject
{
    private readonly List<HistoryRecord> _history = new();
    private int _currentIndex = -1;
    private const int MaxHistorySize = 50;

    [ObservableProperty]
    private bool _canUndo;

    [ObservableProperty]
    private bool _canRedo;

    [ObservableProperty]
    private string _undoTooltip = "撤销 (Ctrl+Z)";

    [ObservableProperty]
    private string _redoTooltip = "重做 (Ctrl+Y)";

    [ObservableProperty]
    private int _undoCount;

    [ObservableProperty]
    private int _redoCount;

    private bool _stopRecord = false;

    public event EventHandler<HistoryRecord>? StateRestored;

    public CanvasHistoryManager()
    {
        UpdateState();
    }

    /// <summary>
    /// 记录当前状态
    /// </summary>
    /// <param name="actionType">操作类型</param>
    /// <param name="selectedElementLabelIds">当前选中的元素LabelId列表</param>
    /// <param name="canvasElements">画布元素集合</param>
    /// <param name="canvasProperty">画布属性</param>
    public void Record(
        HistoryActionType actionType,
        IReadOnlyList<string>? selectedElementLabelIds,
        ObservableCollection<CanvasElement>? canvasElements,
        CanvasProperty? canvasProperty)
    {
        if (_stopRecord)
            return;

        // 如果当前不是最后一条记录，删除重做部分
        if (_currentIndex < _history.Count - 1)
        {
            _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
        }

        var record = new HistoryRecord
        {
            ActionType = actionType,
            SelectedElementIds = selectedElementLabelIds != null
                ? new List<string>(selectedElementLabelIds)
                : new List<string>(),
            CanvasElementsJson = SerializeCanvasElements(canvasElements),
            CanvasPropertyJson = SerializeCanvasProperty(canvasProperty),
            Timestamp = DateTime.Now
        };

        _history.Add(record);
        _currentIndex = _history.Count - 1;

        // 限制历史记录数量
        while (_history.Count > MaxHistorySize)
        {
            _history.RemoveAt(0);
            _currentIndex--;
        }

        UpdateState();
    }

    /// <summary>
    /// 记录画布属性变更
    /// </summary>
    public void RecordCanvasProperty(CanvasProperty? canvasProperty, HistoryActionType actionType = HistoryActionType.CanvasPropertyChanged)
    {
        AddRecord(actionType, null, null, SerializeCanvasProperty(canvasProperty));
    }

    /// <summary>
    /// 记录元素变更
    /// </summary>
    public void RecordElements(ObservableCollection<CanvasElement>? canvasElements, IReadOnlyList<string>? selectedElementLabelIds, HistoryActionType actionType = HistoryActionType.ElementModified)
    {
        AddRecord(actionType, selectedElementLabelIds, SerializeCanvasElements(canvasElements), null);
    }

    /// <summary>
    /// 记录选中状态变更
    /// </summary>
    public void RecordSelection(IReadOnlyList<string>? selectedElementLabelIds)
    {
        AddRecord(HistoryActionType.SelectionChanged, selectedElementLabelIds, null, null);
    }

    /// <summary>
    /// 记录单个元素变更（仅记录该元素的 JSON，适用于元素属性变化等细粒度操作）
    /// </summary>
    /// <param name="element">变化的元素</param>
    /// <param name="selectedElementLabelIds">当前选中的元素ID列表</param>
    public void RecordSingleElement(CanvasElement? element, IReadOnlyList<string>? selectedElementLabelIds)
    {
        if (_stopRecord)
            return;

        if (_currentIndex < _history.Count - 1)
        {
            _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
        }

        var record = new HistoryRecord
        {
            ActionType = HistoryActionType.SingleElementChanged,
            SelectedElementIds = selectedElementLabelIds != null
                ? new List<string>(selectedElementLabelIds)
                : new List<string>(),
            SingleElementJson = SerializeSingleElement(element),
            Timestamp = DateTime.Now
        };

        _history.Add(record);
        _currentIndex = _history.Count - 1;

        while (_history.Count > MaxHistorySize)
        {
            _history.RemoveAt(0);
            _currentIndex--;
        }

        UpdateState();
    }

    /// <summary>
    /// 记录完整状态
    /// </summary>
    public void RecordFull(
        IReadOnlyList<string>? selectedElementLabelIds,
        ObservableCollection<CanvasElement>? canvasElements,
        CanvasProperty? canvasProperty,
        HistoryActionType actionType = HistoryActionType.Unspecified)
    {
        AddRecord(actionType, selectedElementLabelIds, SerializeCanvasElements(canvasElements), SerializeCanvasProperty(canvasProperty));
    }

    private void AddRecord(
        HistoryActionType actionType,
        IReadOnlyList<string>? selectedElementLabelIds,
        string? elementsJson,
        string? propertyJson)
    {
        if (_stopRecord)
            return;

        if (_currentIndex < _history.Count - 1)
        {
            _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
        }

        var record = new HistoryRecord
        {
            ActionType = actionType,
            SelectedElementIds = selectedElementLabelIds != null
                ? new List<string>(selectedElementLabelIds)
                : new List<string>(),
            CanvasElementsJson = elementsJson,
            CanvasPropertyJson = propertyJson,
            Timestamp = DateTime.Now
        };

        _history.Add(record);
        _currentIndex = _history.Count - 1;

        while (_history.Count > MaxHistorySize)
        {
            _history.RemoveAt(0);
            _currentIndex--;
        }

        UpdateState();
    }

    /// <summary>
    /// 撤销
    /// </summary>
    public bool Undo()
    {
        if (!CanUndo)
            return false;

        _stopRecord = true;

        _currentIndex--;
        UpdateState();

        StateRestored?.Invoke(this, _history[_currentIndex]);

        _stopRecord = false;
        return true;
    }

    /// <summary>
    /// 重做
    /// </summary>
    public bool Redo()
    {
        if (!CanRedo)
            return false;

        _stopRecord = true;

        _currentIndex++;
        UpdateState();
        StateRestored?.Invoke(this, _history[_currentIndex]);

        _stopRecord = false;
        return true;
    }

    /// <summary>
    /// 获取当前记录
    /// </summary>
    public HistoryRecord? GetCurrentRecord()
    {
        if (_currentIndex >= 0 && _currentIndex < _history.Count)
            return _history[_currentIndex];
        return null;
    }

    /// <summary>
    /// 清空历史记录
    /// </summary>
    public void Clear()
    {
        _currentIndex = -1;
        UpdateState();
    }

    private void UpdateState()
    {
        CanUndo = _currentIndex > 0;
        CanRedo = _currentIndex < _history.Count - 1;
        UndoCount = _currentIndex;
        RedoCount = _history.Count - _currentIndex - 1;
    }

    private static string? SerializeCanvasElements(ObservableCollection<CanvasElement>? elements)
    {
        if (elements == null || elements.Count == 0)
            return JsonSerializer.Serialize(new List<CanvasElement>(), CanvasElementSerializer.DeserializationOptions);

        try
        {
            return JsonSerializer.Serialize(new List<CanvasElement>(elements), CanvasElementSerializer.DeserializationOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializeCanvasProperty(CanvasProperty? property)
    {
        if (property == null)
            return null;

        try
        {
            return JsonSerializer.Serialize(property, CanvasElementSerializer.DeserializationOptions);
        }
        catch
        {
            return null;
        }
    }

    private static string? SerializeSingleElement(CanvasElement? element)
    {
        if (element == null)
            return null;

        try
        {
            return JsonSerializer.Serialize(element, CanvasElementSerializer.DeserializationOptions);
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// 历史记录类型
/// </summary>
public enum HistoryActionType
{
    Unspecified = 0,
    ElementAdded = 1,
    ElementDeleted = 2,
    ElementModified = 3,
    SelectionChanged = 4,
    CanvasPropertyChanged = 5,
    ElementMoved = 6,
    ElementResized = 7,
    ElementStyled = 8,
    SingleElementChanged = 9
}

/// <summary>
/// 历史记录数据结构
/// </summary>
public class HistoryRecord
{
    public HistoryActionType ActionType { get; set; } = HistoryActionType.Unspecified;
    public List<string> SelectedElementIds { get; set; } = new();
    public string? CanvasElementsJson { get; set; }
    public string? CanvasPropertyJson { get; set; }
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 单个元素变化时的 JSON 数据
    /// </summary>
    public string? SingleElementJson { get; set; }

    public override string ToString()
    {
        return $"[{ActionType}] {Timestamp:HH:mm:ss}";
    }
}
