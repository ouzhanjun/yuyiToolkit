using SkiaSharp;
using yuyiAvalonia.ImageEdit.ViewModels;
using yuyiAvalonia.ImageEdit.Models;

namespace yuyiAvalonia.ImageEdit.Helpers
{
    public class HistoryHelper
    {
        public HistoryHelper(ImageEditViewModel model)
        {
            _model = model;
            _historyIndex = 0;
        }

        ImageEditViewModel _model;
        /// <summary>
        /// 是否有修改
        /// </summary>
        private bool _isModified;

        /// <summary>
        /// 是否可以撤销
        /// </summary>
        private bool _canUndo;

        /// <summary>
        /// 是否可以重做
        /// </summary>
        private bool _canRedo;

        /// <summary>
        /// 编辑历史列表
        /// </summary>
        private readonly List<ImageEditHistoryItem> _history = new();

        /// <summary>
        /// 当前历史索引
        /// </summary>
        private int _historyIndex = -1;

        public bool IsModified { get => _isModified; set => _isModified = value; }
        public bool CanUndo { get => _canUndo; set => _canUndo = value; }
        public bool CanRedo { get => _canRedo; set => _canRedo = value; }

        public void ClearHistory( )
        {
            // 释放所有历史项中的 SKBitmap 资源
            foreach (var item in _history)
            {
                item.Image?.Dispose();
            }
            _history.Clear();
            _historyIndex = -1;
            CanUndo = false;
            CanRedo = false;
            IsModified = false;
        }

        public void AddHistory(ImageEditHistoryItem historyItem)
        {
            // 清除当前索引之后的所有历史，并释放 SKBitmap 资源
            while (_history.Count > _historyIndex + 1)
            {
                var item = _history[^1];
                _history.RemoveAt(_history.Count - 1);
                // 释放被移除的 SKBitmap 资源
                item.Image?.Dispose();
            }

            // 添加新历史
            _history.Add(historyItem);

            _historyIndex = _history.Count - 1;
            UpdateCanUndoRedo();
            IsModified = true;
        }

        // 撤销命令
        public void Undo()
        {
            if (_historyIndex > 0)
            {
                _historyIndex--;
                var item = _history[_historyIndex];

                // 释放之前的图片并清理缓存
                ChangeImageSource(item);
                IsModified = _historyIndex > 0;
            }
        }

        // 重做命令
        public void Redo()
        {
            if (_historyIndex < _history.Count - 1)
            {
                _historyIndex++;
                var item = _history[_historyIndex];

                // 释放之前的图片并清理缓存
                ChangeImageSource(item);
                IsModified = true;
            }
        }

        private void ChangeImageSource(ImageEditHistoryItem item)
        {
            // 先清理 ViewModel 中的缓存位图
            _model.ClearCachedSkBitmap();

            _model.CurrentImageEditItem = item;
            _model.ImageEditIntensityList =(UIImageEditIntensityList) item.Properties.IntensityList.Clone();
            _model.ResetImageEditIntensity();
            _model.PreviewImageAdjust();
            UpdateCanUndoRedo();
        }

        private void UpdateCanUndoRedo()
        {
            CanUndo = _historyIndex > 0;
            CanRedo = _historyIndex < _history.Count - 1;
        }

    }
}
